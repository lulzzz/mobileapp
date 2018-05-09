// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.Views
{
	[Register ("SelectableTagViewCell")]
	partial class SelectableTagViewCell
	{
		[Outlet]
		[GeneratedCode ("iOS Designer", "1.0")]
		UIKit.UIImageView SelectedImage { get; set; }

		[Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UILabel TagLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (SelectedImage != null) {
				SelectedImage.Dispose ();
				SelectedImage = null;
			}

			if (TagLabel != null) {
				TagLabel.Dispose ();
				TagLabel = null;
			}
		}
	}
}
