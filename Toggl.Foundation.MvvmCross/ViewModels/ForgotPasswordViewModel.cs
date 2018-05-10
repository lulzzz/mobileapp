using System;
using System.Reactive.Linq;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class ForgotPasswordViewModel : MvxViewModel<EmailParameter>
    {
        private readonly ILoginManager loginManager;
        private readonly IAnalyticsService analyticsService;

        public Email Email { get; set; } = Email.Empty;

        public string ErrorMessage { get; private set; }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool IsLoading { get; private set; }

        public bool PasswordResetSuccessful { get; private set; }

        public IMvxCommand ResetCommand { get; }

        public ForgotPasswordViewModel(
            ILoginManager loginManager, IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(loginManager, nameof(loginManager));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.loginManager = loginManager;
            this.analyticsService = analyticsService;

            ResetCommand = new MvxCommand(reset, () => Email.IsValid);
        }

        public override void Prepare(EmailParameter parameter)
        {
            Email = parameter.Email;
        }

        private void reset()
        {
            ErrorMessage = "";
            IsLoading = true;
            PasswordResetSuccessful = false;

            loginManager
                .ResetPassword(Email)
                .Do(_ => analyticsService.TrackResetPassword())
                .Subscribe(onPasswordResetSuccess, onPasswordResetError);
        }

        private void OnEmailChanged()
        {
            ResetCommand.RaiseCanExecuteChanged();
        }

        private void onPasswordResetSuccess(string result)
        {
            IsLoading = false;
            PasswordResetSuccessful = true;
        }

        private void onPasswordResetError(Exception exception)
        {
            IsLoading = false;

            switch (exception)
            {
                case BadRequestException _:
                    ErrorMessage = Resources.PasswordResetEmailDoesNotExistError;
                    break;

                case OfflineException _:
                    ErrorMessage = Resources.PasswordResetOfflineError;
                    break;

                case ApiException apiException:
                    ErrorMessage = apiException.LocalizedApiErrorMessage;
                    break;

                default:
                    ErrorMessage = Resources.PasswordResetGeneralError;
                    break;
            }
        }
    }
}
