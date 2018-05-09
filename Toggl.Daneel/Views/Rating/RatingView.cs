using Foundation;
using CoreGraphics;
using MvvmCross.Binding.iOS.Views;
using ObjCRuntime;
using System;
using UIKit;

namespace Toggl.Daneel
{
    public partial class RatingView : MvxView
    {
        public RatingView (IntPtr handle) : base (handle)
        {
        }

        public static RatingView Create()
        {
            var arr = NSBundle.MainBundle.LoadNib(nameof(RatingView), null, null);
            var view = Runtime.GetNSObject<RatingView>(arr.ValueAt(0));
            view.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            view.TranslatesAutoresizingMaskIntoConstraints = true;
            return view;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var cardViewShadowPath = UIBezierPath.FromRect(CardView.Bounds);
            CardView.Layer.ShadowPath?.Dispose();
            CardView.Layer.ShadowPath = cardViewShadowPath.CGPath;

            CardView.Layer.CornerRadius = 8;
            CardView.Layer.ShadowRadius = 4;
            CardView.Layer.ShadowOpacity = 0.1f;
            CardView.Layer.MasksToBounds = false;
            CardView.Layer.ShadowOffset = new CGSize(0, 2);
            CardView.Layer.ShadowColor = UIColor.Black.CGColor;

            CTAButton.Layer.CornerRadius = 8;
            CTAButton.Layer.MasksToBounds = false;
        }
    }
}
