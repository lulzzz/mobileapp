﻿using System;
using System.Collections.Generic;
using Toggl.Foundation.Shortcuts;

namespace Toggl.Foundation.Analytics
{
    public abstract class BaseAnalyticsService : IAnalyticsService
    {
        private const string originParameter = "Origin";
        private const string authenticationMethodParameter = "AuthenticationMethod";
        private const string pageParameter = "PageWhenSkipWasClicked";
        private const string viewModelNameParameter = "ViewModelName";

        private const string currentPageEventName = "CurrentPage";
        private const string onboardingSkipEventName = "OnboardingSkip";
        private const string startTimeEntryEventName = "TimeEntryStarted";

        private const string loginEventName = "Login";
        private const string signupEventName = "SignUp";
        private const string resetPasswordEventName = "ResetPassword";

        private const string logoutEventName = "Logout";
        private const string logoutSourceParameter = "Source";

        private const string passwordManagerButtonClicked = "PasswordManagerButtonClicked";
        private const string passwordManagerContainsValidEmail = "PasswordManagerContainsValidEmail";
        private const string passwordManagerContainsValidPassword = "PasswordManagerContainsValidPassword";

        private const string appShortcutEventName = "ApplicationShortcut";
        private const string appShortcutParameter = "ApplicationShortcutType";

        private const string editEntrySelectProjectEventName = "EditEntrySelectProject";
        private const string editEntrySelectTagEventName = "EditEntrySelectTag";

        private const string startEntrySelectProjectEventName = "StartEntrySelectProject";
        private const string startEntrySelectTagEventName = "StartEntrySelectTag";
        private const string suggestionSourceParameter = "Source";

        public void TrackOnboardingSkipEvent(string pageName)
        {
            track(onboardingSkipEventName, pageParameter, pageName);
        }

        public void TrackStartedTimeEntry(TimeEntryStartOrigin origin)
        {
            track(startTimeEntryEventName, originParameter, origin.ToString());
        }

        public void TrackCurrentPage(Type viewModelType)
        {
            var dict = new Dictionary<string, string> { { viewModelNameParameter, viewModelType.ToString() } };
            NativeTrackEvent(currentPageEventName, dict);
        }

        public void TrackLoginEvent(AuthenticationMethod authenticationMethod)
        {
            track(loginEventName, authenticationMethodParameter, authenticationMethod.ToString());
        }

        public void TrackSignUpEvent(AuthenticationMethod authenticationMethod)
        {
            track(signupEventName, authenticationMethodParameter, authenticationMethod.ToString());
        }

        public void TrackLogoutEvent(LogoutSource source)
        {
            track(loginEventName, logoutSourceParameter, source.ToString());
        }

        public void TrackResetPassword()
        {
            track(resetPasswordEventName);
        }

        public void TrackPasswordManagerButtonClicked()
        {
            track(passwordManagerButtonClicked);
        }

        public void TrackPasswordManagerContainsValidEmail()
        {
            track(passwordManagerContainsValidEmail);
        }

        public void TrackPasswordManagerContainsValidPassword()
        {
            track(passwordManagerContainsValidPassword);
        }

        public void TrackNonFatalException(Exception ex)
        {
        }

        public void TrackSyncError(Exception exception)
        {
            NativeTrackException(exception);
        }

        public void TrackAppShortcut(string shortcut)
        {
            track(appShortcutEventName, appShortcutParameter, shortcut);
        }

        public void TrackEditOpensProjectSelector()
        {
            track(editEntrySelectProjectEventName);
        }

        public void TrackEditOpensTagSelector()
        {
            track(editEntrySelectTagEventName);
        }

        public void TrackStartOpensProjectSelector(ProjectTagSuggestionSource source)
        {
            track(startEntrySelectProjectEventName, suggestionSourceParameter, source.ToString());

        }

        public void TrackStartOpensTagSelector(ProjectTagSuggestionSource source)
        {
            track(startEntrySelectTagEventName, suggestionSourceParameter, source.ToString());
        }

        private void track(string eventName)
        {
            var dict = new Dictionary<string, string>();
            NativeTrackEvent(eventName, dict);
        }

        private void track(string eventName, string parameterName, string parameterValue)
        {
            var dict = new Dictionary<string, string> { { parameterName, parameterValue } };
            NativeTrackEvent(eventName, dict);
        }

        protected abstract void NativeTrackEvent(string eventName, Dictionary<string, string> parameters);

        protected abstract void NativeTrackException(Exception exception);
    }
}
