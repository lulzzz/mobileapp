using MvvmCross.Droid.Support.V7.RecyclerView.ItemTemplates;

namespace Toggl.Giskard.TemplateSelectors
{
    public sealed class CalendarShortcutsTemplateSelector : MvxDefaultTemplateSelector
    {
        public CalendarShortcutsTemplateSelector()
            : base(Resource.Layout.ReportsCalendarShortcutCell) { }
    }
}
