﻿using System;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using static Toggl.Foundation.Helper.Constants;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class EditDurationViewModel : MvxViewModel<DurationParameter, DurationParameter>
    {
        private readonly ITimeService timeService;
        private readonly IMvxNavigationService navigationService;
        private readonly ITogglDataSource dataSource;

        private IDisposable runningTimeEntryDisposable;
        private IDisposable preferencesDisposable;

        private DurationParameter defaultResult;

        private DurationFormat durationFormat;

        private EditMode editMode;

        [DependsOn(nameof(IsRunning))]
        public DurationFormat DurationFormat => IsRunning ? DurationFormat.Improved : durationFormat;

        public DateFormat DateFormat { get; private set; }

        public TimeFormat TimeFormat { get; private set; }

        public bool IsRunning { get; private set; }

        public DateTimeOffset StartTime { get; private set; }

        public DateTimeOffset StopTime { get; private set; }

        [DependsOn(nameof(StartTime), nameof(StopTime))]
        public TimeSpan Duration
        {
            get => StopTime - StartTime;
            set
            {
                if (Duration == value) return;

                onDurationChanged(value);
            }
        }

        public bool IsEditingTime => IsEditingStopTime || IsEditingStartTime;

        public bool IsEditingStartTime => editMode == EditMode.StartTime;

        public bool IsEditingStopTime => editMode == EditMode.EndTime;

        public DateTimeOffset EditedTime
        {
            get
            {
                switch (editMode)
                {
                    case EditMode.StartTime:
                        return StartTime;

                    case EditMode.EndTime:
                        return StopTime;

                    default:
                        // any value between start and end time can be returned here
                        // this constraint is to avoid invalid dates with the date picker
                        return StartTime;
                }
            }

            set
            {
                if (!IsEditingTime) return;

                var valueInRange = value.Clamp(MinimumDateTime, MaximumDateTime);

                switch (editMode)
                {
                    case EditMode.StartTime:
                        StartTime = valueInRange;
                        break;

                    case EditMode.EndTime:
                        StopTime = valueInRange;
                        break;
                }
            }
        }

        public DateTime MinimumDateTime { get; private set; }

        public DateTime MaximumDateTime { get; private set; }

        public DateTimeOffset MinimumStartTime => StopTime.AddHours(-MaxTimeEntryDurationInHours);

        public DateTimeOffset MaximumStartTime => StopTime;

        public DateTimeOffset MinimumStopTime => StartTime;

        public DateTimeOffset MaximumStopTime => StartTime.AddHours(MaxTimeEntryDurationInHours);

        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxCommand EditStartTimeCommand { get; }

        public IMvxCommand EditStopTimeCommand { get; }

        public IMvxCommand StopEditingTimeCommand { get; }

        public EditDurationViewModel(IMvxNavigationService navigationService, ITimeService timeService, ITogglDataSource dataSource)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.timeService = timeService;
            this.navigationService = navigationService;
            this.dataSource = dataSource;

            SaveCommand = new MvxAsyncCommand(save);
            CloseCommand = new MvxAsyncCommand(close);

            EditStartTimeCommand = new MvxCommand(editStartTime);
            EditStopTimeCommand = new MvxCommand(editStopTime);
            StopEditingTimeCommand = new MvxCommand(stopEditingTime);
        }

        public override void Prepare(DurationParameter parameter)
        {
            defaultResult = parameter;
            IsRunning = defaultResult.Duration.HasValue == false;

            if (IsRunning)
            {
                runningTimeEntryDisposable = timeService.CurrentDateTimeObservable
                           .Subscribe(currentTime => StopTime = currentTime);
            }

            StartTime = parameter.Start;
            StopTime = parameter.Duration.HasValue
                ? StartTime + parameter.Duration.Value
                : timeService.CurrentDateTime;

            MinimumDateTime = StartTime.DateTime;
            MaximumDateTime = StopTime.DateTime;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            preferencesDisposable = dataSource.Preferences.Current
                .Subscribe(onPreferencesChanged);

            editMode = EditMode.None;
        }

        private Task close()
            => navigationService.Close(this, defaultResult);

        private Task save()
        {
            var result = DurationParameter.WithStartAndDuration(StartTime, IsRunning ? (TimeSpan?)null : Duration);
            return navigationService.Close(this, result);
        }

        private void editStartTime()
        {
            if (IsEditingStartTime)
            {
                editMode = EditMode.None;
            }
            else
            {
                MinimumDateTime = MinimumStartTime.LocalDateTime;
                MaximumDateTime = MaximumStartTime.LocalDateTime;

                editMode = EditMode.StartTime;
            }

            RaisePropertyChanged(nameof(IsEditingStartTime));
            RaisePropertyChanged(nameof(IsEditingStopTime));
            RaisePropertyChanged(nameof(IsEditingTime));
            RaisePropertyChanged(nameof(EditedTime));
        }

        private void editStopTime()
        {
            if (IsRunning)
            {
                runningTimeEntryDisposable?.Dispose();
                StopTime = timeService.CurrentDateTime;
                IsRunning = false;
            }

            if (IsEditingStopTime)
            {
                editMode = EditMode.None;
            }
            else
            {
                MinimumDateTime = MinimumStopTime.LocalDateTime;
                MaximumDateTime = MaximumStopTime.LocalDateTime;

                editMode = EditMode.EndTime;
            }

            RaisePropertyChanged(nameof(IsEditingStartTime));
            RaisePropertyChanged(nameof(IsEditingStopTime));
            RaisePropertyChanged(nameof(IsEditingTime));
            RaisePropertyChanged(nameof(EditedTime));
        }

        private void stopEditingTime()
        {
            editMode = EditMode.None;

            RaisePropertyChanged(nameof(IsEditingStartTime));
            RaisePropertyChanged(nameof(IsEditingStopTime));
            RaisePropertyChanged(nameof(IsEditingTime));
        }

        private void onDurationChanged(TimeSpan changedDuration)
        {
            if (IsRunning)
                StartTime = timeService.CurrentDateTime - changedDuration;

            StopTime = StartTime + changedDuration;
        }

        private void onPreferencesChanged(IDatabasePreferences preferences)
        {
            durationFormat = preferences.DurationFormat;
            DateFormat = preferences.DateFormat;
            TimeFormat = preferences.TimeOfDayFormat;

            RaisePropertyChanged(nameof(DurationFormat));
        }

        public override void ViewDestroy()
        {
            base.ViewDestroy();
            runningTimeEntryDisposable?.Dispose();
            preferencesDisposable?.Dispose();
        }

        private enum EditMode
        {
            None,
            StartTime,
            EndTime
        }
    }
}
