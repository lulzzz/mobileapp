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
		UIKit.UIButton CTAButton { get; set; }

		[Outlet]
		UIKit.UILabel CTADescription { get; set; }

		[Outlet]
		UIKit.UILabel CTATitle { get; set; }

		[Outlet]
		UIKit.UIView CTAView { get; set; }

		[Outlet]
		UIKit.UIButton DismissButton { get; set; }

		[Outlet]
		UIKit.NSLayoutConstraint HeightConstraint { get; set; }

		[Outlet]
		UIKit.UILabel NotReallyLabel { get; set; }

		[Outlet]
		UIKit.UIView NotReallyView { get; set; }

		[Outlet]
		UIKit.UIView QuestionView { get; set; }

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

			if (HeightConstraint != null) {
				HeightConstraint.Dispose ();
				HeightConstraint = null;
			}

			if (QuestionView != null) {
				QuestionView.Dispose ();
				QuestionView = null;
			}

			if (CTAView != null) {
				CTAView.Dispose ();
				CTAView = null;
			}

			if (NotReallyLabel != null) {
				NotReallyLabel.Dispose ();
				NotReallyLabel = null;
			}

			if (NotReallyView != null) {
				NotReallyView.Dispose ();
				NotReallyView = null;
			}

			if (TitleLabel != null) {
				TitleLabel.Dispose ();
				TitleLabel = null;
			}

			if (YesLabel != null) {
				YesLabel.Dispose ();
				YesLabel = null;
			}

			if (YesView != null) {
				YesView.Dispose ();
				YesView = null;
			}

			if (CTATitle != null) {
				CTATitle.Dispose ();
				CTATitle = null;
			}

			if (CTADescription != null) {
				CTADescription.Dispose ();
				CTADescription = null;
			}

			if (CTAButton != null) {
				CTAButton.Dispose ();
				CTAButton = null;
			}

			if (DismissButton != null) {
				DismissButton.Dispose ();
				DismissButton = null;
			}
		}
	}
}
