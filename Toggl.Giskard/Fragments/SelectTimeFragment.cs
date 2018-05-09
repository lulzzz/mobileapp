using System;
using System.ComponentModel;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using MvvmCross.Binding.Droid.BindingContext;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Droid.Views.Attributes;
using MvvmCross.Platform.WeakSubscription;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    using System.Collections.Generic;
    using MvvmCross.Binding.BindingContext;
    using static SelectTimeFragment.EditorMode;

    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed class SelectTimeFragment : MvxDialogFragment<SelectTimeViewModel>, TabLayout.IOnTabSelectedListener
    {
        private const int startTimeTab = 0;
        private const int stopTimeTab = 1;
        private const int durationTab = 2;

        internal enum EditorMode
        {
            Date,
            Time,
            Duration
        }

        private List<TabHeaderUpdateManager> tabManagers;

        private readonly int[] heights = { 450, 400, 224 };

        private int currentPosition;
        private EditorMode editorMode = Date;

        private IDisposable onModeChangedDisposable;
        private IDisposable onDurationChangedDisposable;
        private IDisposable onStartTimeChangedDisposable;
        private IDisposable onStopTimeChangedDisposable;

        private LinearLayout controlButtons;
        private TabLayout tabLayout;
        private ViewPager pager;


        public SelectTimeFragment()
        {
        }

        public SelectTimeFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(Resource.Layout.SelectTimeFragment, null);

            controlButtons = view.FindViewById<LinearLayout>(Resource.Id.SelectTimeFragmentControlButtons);
            pager = view.FindViewById<ViewPager>(Resource.Id.SelectTimeFragmentPager);
            tabLayout = view.FindViewById<TabLayout>(Resource.Id.SelectTimeTabView);

            pager.OffscreenPageLimit = 2;
            pager.Adapter = new SelectTimePagerAdapter(BindingContext);
            tabLayout.AddOnTabSelectedListener(this);
            tabLayout.SetupWithViewPager(pager, true);

            tabManagers = Enumerable.Range(0, tabLayout.TabCount)
                                    .Select(tabLayout.GetTabAt)
                                    .Select(TabHeaderUpdateManager.FromTab)
                                    .ToList();
            
            onModeChangedDisposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.IsCalendarView), onIsCalendarViewChanged);

            onDurationChangedDisposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.Duration), onDurationChanged);

            onStartTimeChangedDisposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.StartTime), onStartTimeChanged);

            onStopTimeChangedDisposable =
                ViewModel.WeakSubscribe<PropertyChangedEventArgs>(nameof(ViewModel.StopTime), onStopTimeChanged);

            pager.SetCurrentItem(ViewModel.StartingTabIndex, false);

            return view;
        }

        private void onIsCalendarViewChanged(object sender, PropertyChangedEventArgs args)
        {
            editorMode = ViewModel.IsCalendarView ? Date : Time;
            updateLayoutHeight();
        }

        private void onStartTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            if (tabLayout.SelectedTabPosition == startTimeTab)
                return;

            tabManagers[startTimeTab].Update(false, ViewModel.StartTimeText);
        }

        private void onStopTimeChanged(object sender, PropertyChangedEventArgs args)
        {
            if (tabLayout.SelectedTabPosition == stopTimeTab)
                return;

            tabManagers[stopTimeTab].Update(false, ViewModel.StopTimeText);
        }

        private void onDurationChanged(object sender, PropertyChangedEventArgs args)
        {
            if (tabLayout.SelectedTabPosition == durationTab)
                return;

            tabManagers[durationTab].Update(false, ViewModel.DurationText);
        }

        public void OnTabReselected(TabLayout.Tab tab)
            => onTabChange(tab.Position);

        public void OnTabSelected(TabLayout.Tab tab)
            => onTabChange(tab.Position);

        private void onTabChange(int tabPosition)
        {
            currentPosition = tabPosition;
            editorMode = calculateEditorMode();

            updateLayoutHeight();

            if (tabManagers == null)
                return;
            
            tabManagers[tabPosition].Update(true);
        }

        private EditorMode calculateEditorMode()
        {
            if (currentPosition == durationTab)
                return Duration;

            return ViewModel.IsCalendarView ? Date : Time;
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.CancelCommand.Execute();
        }

        public override void OnDismiss(IDialogInterface dialog)
        {
            if (ViewModel == null) return;

            ViewModel.SaveCommand.Execute();
        }

        private void updateLayoutHeight()
        {
            var newHeight = heights[(int)editorMode];

            tabLayout.RequestLayout();
            controlButtons.ForceLayout();

            Activity.RunOnUiThread(() =>
            {
                var heightInPixels = newHeight.DpToPixels(Context);

                var pagerLayout = pager.LayoutParameters;
                pagerLayout.Height = heightInPixels;
                pager.LayoutParameters = pagerLayout;

                Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: ViewGroup.LayoutParams.WrapContent);
            });
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            var text = "";

            if (tab.Position == startTimeTab)
                text = ViewModel.StartTimeText;
            else if (tab.Position == stopTimeTab)
                text = ViewModel.StopTimeText;
            else if (tab.Position == durationTab)
                text = ViewModel.DurationText;

            tabManagers[tab.Position].Update(false, text);
        }

		protected override void Dispose(bool disposing)
		{
            base.Dispose(disposing);

            if (disposing == false) return;

            onModeChangedDisposable.Dispose();
            onDurationChangedDisposable.Dispose();
            onStartTimeChangedDisposable.Dispose();
            onStopTimeChangedDisposable.Dispose();
		}
    }
}
