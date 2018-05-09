// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel
{
	[Register ("RatingView")]
	partial class RatingView
	{
		[Outlet]
		UIKit.UIView CardView { get; set; }

		[Outlet]
		UIKit.UILabel NotReallyLabel { get; set; }

		[Outlet]
		UIKit.UIView NotReallyView { get; set; }

		[Outlet]
		UIKit.UILabel TitleLabel { get; set; }

		[Outlet]
		UIKit.UILabel YesLabel { get; set; }

		[Outlet]
		UIKit.UIView YesView { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CardView != null) {
				CardView.Dispose ();
				CardView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (YesView != null) {
				YesView.Dispose ();
				YesView = null;
			}

			if (NotReallyView != null) {
				NotReallyView.Dispose ();
				NotReallyView = null;
			}

			if (YesLabel != null) {
				YesLabel.Dispose ();
				YesLabel = null;
			}

			if (NotReallyLabel != null) {
				NotReallyLabel.Dispose ();
				NotReallyLabel = null;
			}
		}
	}
}
