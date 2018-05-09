using CoreGraphics;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Binding.iOS;
using MvvmCross.Core.ViewModels;
using ObjCRuntime;
using System;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel
{
    public partial class RatingView : MvxView
    {
        public IMvxCommand<bool> AnswerTappedCommand { get; set; }

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

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();

            var inverseBoolConverter = new BoolToConstantValueConverter<bool>(false, true);
            var boolToHeightConverter = new BoolToConstantValueConverter<nfloat>(209, 236);
            var bindingSet = this.CreateBindingSet<RatingView, RatingViewModel>();

            bindingSet.Bind(QuestionView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.GotAnswer);

            bindingSet.Bind(CTAView)
                      .For(v => v.BindVisibility())
                      .To(vm => vm.GotAnswer)
                      .WithConversion(inverseBoolConverter);

            bindingSet.Bind(HeightConstraint)
                      .For(v => v.BindConstant())
                      .To(vm => vm.GotAnswer)
                      .WithConversion(boolToHeightConverter);

            bindingSet.Apply();

            YesView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                AnswerTappedCommand?.Execute(true);
            }));

            NotReallyView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                AnswerTappedCommand?.Execute(false);
            }));
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
