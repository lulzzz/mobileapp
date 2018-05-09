using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.Net;
using Android.Runtime;
using Java.Lang;
using Toggl.Foundation.Shortcuts;
using Toggl.Giskard.Helper;

namespace Toggl.Giskard
{
    public sealed class ApplicationShortcutCreator : BaseApplicationShortcutCreator
    {
        private readonly Context context;

        public ShortcutManager ShortcutManager
        {
            get
            {
                var shortcutManagerType = Class.FromType(typeof(ShortcutManager));
                var shortcutManager = context.GetSystemService(shortcutManagerType).JavaCast<ShortcutManager>();
                return shortcutManager;
            }
        }

        public ApplicationShortcutCreator(Context context)
        {
            this.context = context;
        }

        protected override void ClearAllShortCuts()
        {
            if (NougatApis.AreNotAvailable)
                return;

            ShortcutManager.RemoveAllDynamicShortcuts();
        }

        protected override void SetShortcuts(IEnumerable<ApplicationShortcut> shortcuts)
        {
            if (NougatApis.AreNotAvailable)
                return;

            var droidShortcuts = shortcuts.Select(androidShortcut).ToList();
            ShortcutManager.SetDynamicShortcuts(droidShortcuts);
        }

        private ShortcutInfo androidShortcut(ApplicationShortcut shortcut)
        {
            var droidShortcut = 
                new ShortcutInfo.Builder(context, shortcut.Title)
                    .SetLongLabel($"{shortcut.Title} {shortcut.Subtitle}")
                    .SetShortLabel(shortcut.Title)
                    .SetIcon(getIcon(shortcut.Type))
                    .SetIntent(new Intent(Intent.ActionView).SetData(Uri.Parse(shortcut.Url)))
                    .Build();

            return droidShortcut;
        }

        private Icon getIcon(ShortcutType type)
        {
            var resourceId = 0;
            switch (type)
            {
                case ShortcutType.ContinueLastTimeEntry:
                    resourceId = Resource.Drawable.play;
                    break;

                case ShortcutType.Reports:
                    resourceId = Resource.Drawable.reports_dark;
                    break;

                case ShortcutType.StartTimeEntry:
                    resourceId = Resource.Drawable.play;
                    break;

                case ShortcutType.StopTimeEntry:
                    resourceId = Resource.Drawable.stop_white;
                    break;
            }

            var icon = Icon.CreateWithResource(context, resourceId);
            return icon;
        }
    }
}
