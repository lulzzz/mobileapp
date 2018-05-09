using System;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform.Droid.Platform;
using Toggl.Giskard.Extensions;

namespace Toggl.Giskard.Views
{
    [Register("toggl.giskard.views.togglDroidDatePicker")]
    public class TogglDroidDatePicker
        : DatePicker
        , DatePicker.IOnDateChangedListener
    {
        private bool isInitialized;

        public TogglDroidDatePicker(Context context)
            : base(context)
        {
        }

        public TogglDroidDatePicker(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        protected TogglDroidDatePicker(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public event EventHandler ValueChanged;

		protected override void OnFinishInflate()
		{
            base.OnFinishInflate();

            var headerId = Resources.GetIdentifier("date_picker_header", "id", "android");
            var header = FindViewById(headerId);
            header.Visibility = ViewStates.Gone;
		}

		public DateTime Value
        {
            get
            {
                return MvxJavaDateUtils.DateTimeFromJava(Year, Month, DayOfMonth);
            }
            set
            {
                var javaYear = value.Year;
                var javaMonth = value.Month - 1;
                var javaDay = value.Day;

                if (!isInitialized)
                {
                    Init(javaYear, javaMonth, javaDay, this);
                    isInitialized = true;
                }
                else if (Year != javaYear || Month != javaMonth || DayOfMonth != javaDay)
                {
                    UpdateDate(javaYear, javaMonth, javaDay);
                }
            }
        }

        public void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            ValueChanged?.Invoke(this, null);
        }
    }
}