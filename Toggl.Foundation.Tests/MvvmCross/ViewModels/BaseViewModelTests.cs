﻿using MvvmCross.Core.ViewModels;
using NSubstitute;
using Toggl.Foundation.Login;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.Services;
using Toggl.Foundation.Suggestions;
using Toggl.PrimeRadiant;
using Toggl.PrimeRadiant.Settings;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Network;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public abstract class BaseViewModelTests<TViewModel> : BaseMvvmCrossTests
        where TViewModel : MvxViewModel
    {
        protected ITogglApi Api { get; } = Substitute.For<ITogglApi>();
        protected UserAgent UserAgent { get; } = new UserAgent("Foundation.Tests", "1.0");
        protected IMailService MailService { get; } = Substitute.For<IMailService>();
        protected ITogglDatabase Database { get; } = Substitute.For<ITogglDatabase>();
        protected ILoginManager LoginManager { get; } = Substitute.For<ILoginManager>();
        protected IDialogService DialogService { get; } = Substitute.For<IDialogService>();
        protected IBrowserService BrowserService { get; } = Substitute.For<IBrowserService>();
        protected ILicenseProvider LicenseProvider { get; } = Substitute.For<ILicenseProvider>();
        protected IPlatformConstants PlatformConstants { get; } = Substitute.For<IPlatformConstants>();
        protected IOnboardingStorage OnboardingStorage { get; } = Substitute.For<IOnboardingStorage>();
        protected IPasswordManagerService PasswordManagerService { get; } = Substitute.For<IPasswordManagerService>();
        protected IApiErrorHandlingService ApiErrorHandlingService { get; } = Substitute.For<IApiErrorHandlingService>();
        protected ISuggestionProviderContainer SuggestionProviderContainer { get; } = Substitute.For<ISuggestionProviderContainer>();

        protected TViewModel ViewModel { get; private set; }

        protected BaseViewModelTests()
        {
            Setup();
        }

        protected abstract TViewModel CreateViewModel();

        private void Setup()
        {
            AdditionalSetup();

            ViewModel = CreateViewModel();

            AdditionalViewModelSetup();
        }

        protected virtual void AdditionalSetup()
        {
        }

        protected virtual void AdditionalViewModelSetup()
        {
        }
    }
}