using System;

using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    public partial class SelectCountryViewController : UIViewController
    {
        public SelectCountryViewController() : base("SelectCountryViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

