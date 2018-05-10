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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            EmailTextField.BecomeFirstResponder();
        }

        protected override void KeyboardWillShow(object sender, UIKeyboardEventArgs e)
        {
            ResetPasswordButtonBottomConstraint.Constant += e.FrameEnd.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        protected override void KeyboardWillHide(object sender, UIKeyboardEventArgs e)
        {
            ResetPasswordButtonBottomConstraint.Constant -= e.FrameBegin.Height;
            UIView.Animate(Animation.Timings.EnterTiming, () => View.LayoutIfNeeded());
        }

        private void prepareViews()
        {
            TopConstraint.AdaptForIos10(NavigationController.NavigationBar);

            ResetPasswordButton.SetTitleColor(
                Color.Login.DisabledButtonColor.ToNativeColor(),
                UIControlState.Disabled
            );

            ActivityIndicator.StartAnimation();
        }
    }
}
