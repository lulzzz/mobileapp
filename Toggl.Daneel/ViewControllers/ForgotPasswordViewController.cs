using System;
using MvvmCross.Binding.BindingContext;
using MvvmCross.iOS.Views;
using MvvmCross.iOS.Views.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;
using Toggl.Daneel.Extensions;
using Toggl.Foundation.MvvmCross.Converters;
using MvvmCross.Binding.iOS;
using Toggl.Foundation.MvvmCross.Helper;
using MvvmCross.Plugins.Color.iOS;
using MvvmCross.Binding;
using Toggl.Foundation;

namespace Toggl.Daneel.ViewControllers
{
    [MvxChildPresentation]
    public sealed partial class ForgotPasswordViewController
        : KeyboardAwareViewController<ForgotPasswordViewModel>
    {
        private const int iPhoneSeScreenHeight = 568;
        private const int resetButtonBottomSpacing = 32;
        private const int distanceFromTop = 136;

        private bool viewInitialized;

        public ForgotPasswordViewController() : base(nameof(ForgotPasswordViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            prepareViews();

            var boolInverter = new BoolToConstantValueConverter<bool>(false, true);
            var resetPasswordButtonTitleConverter = new BoolToConstantValueConverter<string>("", Resources.GetPasswordResetLink);

            var bindingSet = this.CreateBindingSet<ForgotPasswordViewController, ForgotPasswordViewModel>();

            //Text
            bindingSet.Bind(ErrorLabel).To(vm => vm.ErrorMessage);
            bindingSet.Bind(EmailTextField)
                      .To(vm => vm.Email)
                      .WithConversion(new EmailToStringValueConverter());

            bindingSet.Bind(ResetPasswordButton)
                      .For(v => v.BindAnimatedTitle())
                      .To(vm => vm.IsLoading)
                      .WithConversion(resetPasswordButtonTitleConverter);

            //Visibility
            bindingSet.Bind(ErrorLabel)
                      .For(v => v.BindAnimatedVisibility())
                      .To(vm => vm.HasError);

            bindingSet.Bind(DoneCard)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.PasswordResetSuccessful);

            bindingSet.Bind(ResetPasswordButton)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.PasswordResetSuccessful)
                      .WithConversion(boolInverter);

            bindingSet.Bind(EmailTextField)
                      .For(v => v.BindFirstResponder())
                      .To(vm => vm.PasswordResetSuccessful)
                      .Mode(MvxBindingMode.OneWay)
                      .WithConversion(boolInverter);

            bindingSet.Bind(ActivityIndicator)
                      .For(v => v.BindVisibilityWithFade())
                      .To(vm => vm.IsLoading);

            //Commands
            bindingSet.Bind(ResetPasswordButton).To(vm => vm.ResetCommand);

            bindingSet.Apply();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (viewInitialized) return;

            viewInitialized = true;

            if (View.Frame.Height > iPhoneSeScreenHeight)
                TopConstraint.Constant = distanceFromTop;
            
            TopConstraint.AdaptForIos10(NavigationController?.NavigationBar);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            EmailTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            ResetPasswordButtonBottomConstraint.Constant = e.FrameEnd.Height + resetButtonBottomSpacing;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            ResetPasswordButtonBottomConstraint.Constant = resetButtonBottomSpacing;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        private void prepareViews()
        {
            ResetPasswordButton.SetTitleColor(
                Color.Login.DisabledButtonColor.ToNativeColor(),
                UIControlState.Disabled
            );

            EmailTextField.ShouldReturn = _ =>
            {
                ViewModel.ResetCommand.Execute();
                return false;
            };

            ActivityIndicator.StartAnimation();
        }
    }
}
