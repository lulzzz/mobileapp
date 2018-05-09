﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectTimeViewModel
        : MvxViewModel<SelectTimeParameters, SelectTimeResultsParameters>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ITimeService timeService;

        private readonly TimeSpanToDurationValueConverter durationConverter = new TimeSpanToDurationValueConverter();

        public DateTimeOffset CurrentDateTime { get; set; }

        public DateTimeOffset StartTime { get; set; }

        public int StartingTabIndex { get; private set; }

        [DependsOn(nameof(CurrentDateTime))]
        public DateTimeOffset? StopTime { get; set; }

        [DependsOn(nameof(StartTime))]
        public DateTime StartDatePart
        {
            get
            {
                return StartTime.Date;
            }
            set
            {
                var newYear = value.Year;
                var newMonth = value.Month;
                var newDay = value.Day;

                if (StartTime.Year == newYear && StartTime.Month == newMonth && StartTime.Day == newDay)
                    return;

                StartTime = new DateTimeOffset(
                    newYear, newMonth, newDay,
                    StartTime.Hour, StartTime.Minute, StartTime.Second, StartTime.Offset);

                RaisePropertyChanged(nameof(StartDatePart));
            }
        }

        [DependsOn(nameof(StartTime))]
        public TimeSpan StartTimePart
        {
            get
            {
                return StartTime.TimeOfDay;
            }
            set
            {
                var newTimeHours = value.Hours;
                var newTimeMinutes = value.Minutes;

                if (StartTime.TimeOfDay.Hours == newTimeHours && StartTime.TimeOfDay.Minutes == newTimeMinutes)
                    return;

                StartTime = new DateTimeOffset(
                    StartTime.Year, StartTime.Month, StartTime.Day,
                    newTimeHours, newTimeMinutes, 0, StartTime.Offset);

                RaisePropertyChanged(nameof(StartTimePart));
            }
        }

        [DependsOn(nameof(StopTime))]
        public DateTime StopDatePart
        {
            get
            {
                return StopTime?.Date ?? default(DateTime);
            }
            set
            {
                var startDateYear = StopTime?.Year;
                var startDateMonth = StopTime?.Month;
                var startDateDay = StopTime?.Day;

                var newYear = value.Year;
                var newMonth = value.Month;
                var newDay = value.Day;

                if (startDateYear == newYear && startDateMonth == newMonth && startDateDay == newDay)
                    return;

                var now = CurrentDateTime;
                var hour = StopTime?.Hour ?? now.Hour;
                var minute = StopTime?.Minute ?? now.Minute;
                var second = StopTime?.Second ?? now.Second;
                var offset = StopTime?.Offset ?? now.Offset;

                StopTime = new DateTimeOffset(
                    newYear, newMonth, newDay,
                    hour, minute, second, offset);

                RaisePropertyChanged(nameof(StopDatePart));
            }
        }

        [DependsOn(nameof(StopTime))]
        public TimeSpan StopTimePart
        {
            get
            {
                return StopTime?.TimeOfDay ?? default(TimeSpan);
            }
            set
            {
                var stopTimeHours = StopTime?.TimeOfDay.Hours;
                var stopTimeMinutes = StopTime?.TimeOfDay.Minutes;

                var newTimeHours = value.Hours;
                var newTimeMinutes = value.Minutes;

                if (stopTimeHours == newTimeHours && stopTimeMinutes == newTimeMinutes)
                    return;

                var now = CurrentDateTime;
                var year = StopTime?.Year ?? now.Year;
                var month = StopTime?.Month ?? now.Month;
                var day = StopTime?.Day ?? now.Day;
                var offset = StopTime?.Offset ?? now.Offset;

                StopTime = new DateTimeOffset(
                    year, month, day,
                    newTimeHours, newTimeMinutes, 0, offset);

                RaisePropertyChanged(nameof(StopTimePart));
            }
        }

        public bool IsEditingDuration { get; set; }

        [DependsOn(nameof(StopTime))]
        public bool IsTimeEntryStopped => StopTime.HasValue;

        public IMvxCommand StopTimeEntryCommand { get; }

        public IMvxAsyncCommand CancelCommand { get; }
        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxCommand ToggleClockCalendarModeCommand { get; }

        public IMvxCommand IncreaseDuration5MinCommand { get; }
        public IMvxCommand IncreaseDuration10MinCommand { get; }
        public IMvxCommand IncreaseDuration30MinCommand { get; }

        public IMvxCommand FocusDurationCommand { get; }
        public IMvxCommand UnfocusDurationCommand { get; }

        public bool IsCalendarView { get; set; }

        [DependsOn(nameof(StartTime))]
        public string StartTimeText => $"{StartTime:HH:mm}";

        [DependsOn(nameof(StartTime))]
        public string StartDateText => $"{StartTime:MM/dd}";

        [DependsOn(nameof(IsTimeEntryStopped))]
        public string StopTimeText
            => IsTimeEntryStopped
            ? $"{StopTime:HH:mm}"
            : $"{CurrentDateTime:HH:mm}";

        [DependsOn(nameof(IsTimeEntryStopped))]
        public string StopDateText
            => IsTimeEntryStopped
            ? $"{StopTime:MM/dd}"
            : $"{CurrentDateTime:MM/dd}";

        public bool IsDurationVisible { get; set; }

        [DependsOn(nameof(StartTime), nameof(StopTime))]
        public TimeSpan Duration
            => (StopTime ?? CurrentDateTime) - StartTime;

        [DependsOn(nameof(Duration))]
        public string DurationText
            => (string)durationConverter.Convert(Duration, typeof(TimeSpan), null, CultureInfo.CurrentCulture);

        private TimeSpan editingDuration;
        public TimeSpan EditingDuration
        {
            get => editingDuration;
            set
            {
                if (StopTime.HasValue)
                {
                    StopTime = StartTime + value;
                }
                else
                {
                    StartTime = CurrentDateTime - value;
                }

                editingDuration = (StopTime ?? CurrentDateTime) - StartTime;
            }
        }

        [DependsOn(nameof(CurrentDateTime))]
        public bool IsRunningTimeEntryAM => CurrentDateTime.Hour < 12;

        public SelectTimeViewModel(
            IMvxNavigationService navigationService,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.navigationService = navigationService;
            this.timeService = timeService;

            CancelCommand = new MvxAsyncCommand(cancel);
            SaveCommand = new MvxAsyncCommand(save);

            IncreaseDuration5MinCommand = new MvxCommand(increaseDuration(5));
            IncreaseDuration10MinCommand = new MvxCommand(increaseDuration(10));
            IncreaseDuration30MinCommand = new MvxCommand(increaseDuration(30));

            FocusDurationCommand = new MvxCommand(focusDurationCommand);
            UnfocusDurationCommand = new MvxCommand(unfocusDurationCommand);

            StopTimeEntryCommand = new MvxCommand(stopTimeEntry);

            ToggleClockCalendarModeCommand = new MvxCommand(togglClockCalendarMode);
        }

        public override void Prepare(SelectTimeParameters parameter)
        {
            StartTime = parameter.Start;
            StopTime = parameter.Stop;

            StartingTabIndex = parameter.StartingTabIndex;
            IsCalendarView = parameter.ShouldStartOnCalendar;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            timeService.CurrentDateTimeObservable
                       .Subscribe(currentDateTime => CurrentDateTime = currentDateTime);
        }

        private Action increaseDuration(int minutes)
        {
            return () => EditingDuration = EditingDuration + TimeSpan.FromMinutes(minutes);
        }

        private void focusDurationCommand()
        {
            IsEditingDuration = true;
        }

        private void unfocusDurationCommand()
        {
            IsEditingDuration = false;
        }

        private void togglClockCalendarMode()
        {
            IsCalendarView = !IsCalendarView;
        }

        private void stopTimeEntry()
        {
            StopTime = CurrentDateTime;
        }

        private async Task cancel()
            => await navigationService.Close(this, null);

        private async Task save()
        {
            var results = new SelectTimeResultsParameters(StartTime, StopTime);
            await navigationService.Close(this, results);
        }
    }
}