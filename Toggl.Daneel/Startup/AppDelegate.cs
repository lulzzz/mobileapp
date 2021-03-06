using System;
using System.Reactive.Linq;
using Foundation;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using MvvmCross.iOS.Platform;
using MvvmCross.Platform;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Foundation.Shortcuts;
using UIKit;

namespace Toggl.Daneel
{
    [Register(nameof(AppDelegate))]
    public sealed class AppDelegate : MvxApplicationDelegate
    {
        private IAnalyticsService analyticsService;
        private IBackgroundService backgroundService;
        private IMvxNavigationService navigationService;

        public override UIWindow Window { get; set; }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            var setup = new Setup(this, Window);
            setup.Initialize();

            var startup = Mvx.Resolve<IMvxAppStart>();
            startup.Start();

            analyticsService = Mvx.Resolve<IAnalyticsService>();
            backgroundService = Mvx.Resolve<IBackgroundService>();
            navigationService = Mvx.Resolve<IMvxNavigationService>();

            Window.MakeKeyAndVisible();

            #if ENABLE_TEST_CLOUD
            Xamarin.Calabash.Start();
            #endif
            #if USE_ANALYTICS
            Microsoft.AppCenter.AppCenter.Start(
                "{TOGGL_APP_CENTER_ID_IOS}", 
                typeof(Microsoft.AppCenter.Crashes.Crashes),
                typeof(Microsoft.AppCenter.Analytics.Analytics));
            Firebase.Core.App.Configure();
            Google.SignIn.SignIn.SharedInstance.ClientID =
                Firebase.Core.App.DefaultInstance.Options.ClientId;
            #endif

            return true;
        }

        #if USE_ANALYTICS
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var openUrlOptions = new UIApplicationOpenUrlOptions(options);
            return Google.SignIn.SignIn.SharedInstance.HandleUrl(url, openUrlOptions.SourceApplication, openUrlOptions.Annotation);
        }
        #endif

        public override void WillEnterForeground(UIApplication application)
        {
            base.WillEnterForeground(application);
            backgroundService.EnterForeground();
        }

        public override void DidEnterBackground(UIApplication application)
        {
            base.DidEnterBackground(application);
            backgroundService.EnterBackground();
        }

        public override void PerformActionForShortcutItem(UIApplication application, UIApplicationShortcutItem shortcutItem, UIOperationHandler completionHandler)
        {
            analyticsService.TrackAppShortcut(shortcutItem.LocalizedTitle);

            var shortcutType = (ShortcutType)(int)(NSNumber)shortcutItem.UserInfo[nameof(ApplicationShortcut.Type)];

            switch (shortcutType)
            {
                case ShortcutType.ContinueLastTimeEntry:
                    var interactorFactory = Mvx.Resolve<IInteractorFactory>();
                    if (interactorFactory == null) return;
                    IDisposable subscription = null;
                    subscription = interactorFactory
                        .ContinueMostRecentTimeEntry()
                        .Execute()
                        .Subscribe(_ =>
                        {
                            subscription.Dispose();
                            subscription = null;
                        });
                    break;

                case ShortcutType.Reports:
                    navigationService.Navigate<ReportsViewModel>();
                    break;

                case ShortcutType.StartTimeEntry:
                    navigationService.Navigate<StartTimeEntryViewModel>();
                    break;
            }
        }
    }
}
