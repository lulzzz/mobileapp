using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;

namespace Toggl.Giskard.Extensions
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> GetChildren(this ViewGroup view) 
        {
            for (int i = 0; i < view.ChildCount; i++)
                yield return view.GetChildAt(i);
        }

        public static IEnumerable<T> GetChildren<T>(this ViewGroup view) 
            => view.GetChildren().OfType<T>();

        public static void SetFocus(this View view) 
        {
            view.RequestFocus();

            ContextThemeWrapper ctw = view.Context as ContextThemeWrapper;
            var activity = ctw.BaseContext as Activity;
            activity?.Window.SetSoftInputMode(SoftInput.StateVisible);

            var service = (InputMethodManager)view.Context.GetSystemService(Context.InputMethodService);
            service.ShowSoftInput(view, ShowFlags.Forced);
        }

        public static void RemoveFocus(this View view) 
        {
            view.ClearFocus();

            var service = (InputMethodManager)view.Context.GetSystemService(Context.InputMethodService);
            service.HideSoftInputFromWindow(view.WindowToken, 0);
        }
    }
}
