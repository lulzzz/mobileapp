using Android.Support.Design.Widget;
using Android.Widget;

namespace Toggl.Giskard.Fragments
{
    public class TabHeaderUpdateManager
    {
        private static readonly int[] resourceIcons =
        {
            Resource.Drawable.play_white,
            Resource.Drawable.stop_white,
            Resource.Drawable.timer_light
        };

        private readonly string[] headers =
        {
            "Start",
            "End",
            "Duration"
        };

        private TextView textView;
        private ImageView imageView;
        private int position;

        public static TabHeaderUpdateManager FromTab(TabLayout.Tab tab)
        {
            var manager = new TabHeaderUpdateManager();

            manager.position = tab.Position;

            tab.SetCustomView(Resource.Layout.SelectDateTimeTabHeader);

            manager.textView = tab.CustomView.FindViewById<TextView>(Resource.Id.text);
            manager.imageView = tab.CustomView.FindViewById<ImageView>(Resource.Id.icon);

            var iconResourceId = resourceIcons[manager.position];
            var icon = manager.imageView.Context.Resources.GetDrawable(iconResourceId, null);
            manager.imageView.SetImageDrawable(icon);

            return manager;
        }

        public void Update(bool isSelected, string text = null)
        {
            imageView.Alpha = isSelected ? 1 : 0.7f;

            if (isSelected)
            {
                textView.Text = headers[position];
            }
            else if (text != null)
            {
                textView.Text = text;
            }
        }
    }
}
