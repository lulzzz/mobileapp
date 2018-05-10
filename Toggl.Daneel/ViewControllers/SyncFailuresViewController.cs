using UIKit;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Daneel.ViewControllers
{    
    [MvxChildPresentation]
    public partial class SyncFailuresViewController : MvxViewController<SyncFailuresViewModel>
    {
        private UITableView tableView;

        public SyncFailuresViewController() : base()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            tableView = new UITableView(View.Frame, UITableViewStyle.Plain);
            View.Add(tableView);
        }
    }
}
