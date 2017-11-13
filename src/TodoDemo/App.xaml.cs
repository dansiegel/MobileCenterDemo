using System;
using System.Threading.Tasks;
using TodoDemo.Services;
using TodoDemo.Views;
using DryIoc;
using Prism.DryIoc;
using TodoDemo.Helpers;
using FFImageLoading.Helpers;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using Microsoft.Azure.Mobile.Distribute;
using Microsoft.Azure.Mobile.Push;
using Prism.Logging;
using Xamarin.Forms;
using DebugLogger = TodoDemo.Services.DebugLogger;

namespace TodoDemo
{
    public partial class App : PrismApplication
    {
        /* 
         * NOTE: 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() 
            : this(null)
        {
        }

        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
            // https://docs.microsoft.com/en-us/mobile-center/sdk/distribute/xamarin
            Distribute.ReleaseAvailable = OnReleaseAvailable;
            // https://docs.microsoft.com/en-us/mobile-center/sdk/push/xamarin-forms
            Push.PushNotificationReceived += OnPushNotificationReceived;
            // Handle when your app starts
            MobileCenter.Start(AppConstants.MobileCenterStart,
                               typeof(Analytics), typeof(Crashes), typeof(Distribute), typeof(Push));
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            LogUnobservedTaskExceptions();

            await NavigationService.NavigateAsync("SplashScreenPage");
        }

        protected override void RegisterTypes()
        {
            // Register the Popup Plugin Navigation Service
            Container.RegisterPopupNavigationService();

            if(!string.IsNullOrWhiteSpace(AppConstants.MobileCenterStart))
            {
                Container.Register<ILoggerFacade, MCAnalyticsLogger>(reuse: Reuse.Singleton,
                                                                     ifAlreadyRegistered: IfAlreadyRegistered.Replace);
            }

            // Navigating to "TabbedPage?createTab=ViewA&createTab=ViewB&createTab=ViewC will generate a TabbedPage
            // with three tabs for ViewA, ViewB, & ViewC
            // Adding `selectedTab=ViewB` will set the current tab to ViewB
            Container.RegisterTypeForNavigation<TabbedPage>();
            Container.RegisterTypeForNavigation<NavigationPage>();
            Container.RegisterTypeForNavigation<MainPage>();
            Container.RegisterTypeForNavigation<SplashScreenPage>();
            Container.RegisterTypeForNavigation<TodoItemDetail>();
        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            if (await Analytics.IsEnabledAsync())
            {
                System.Diagnostics.Debug.WriteLine("Analaytics is enabled");
                FFImageLoading.ImageService.Instance.Config.Logger = (IMiniLogger)Logger;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Analytics is disabled");
            }
        }

        protected override void OnSleep()
        {
            // Handle IApplicationLifecycle
            base.OnSleep();

            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle IApplicationLifecycle
            base.OnResume();

            // Handle when your app resumes
        }

        protected override ILoggerFacade CreateLogger()
        {
            switch (Xamarin.Forms.Device.RuntimePlatform)
            {
                case "Android":
                    if (!string.IsNullOrWhiteSpace(Secrets.MobileCenter_Android_Secret))
                        return CreateMobileCenterLogger();
                    break;
                case "iOS":
                    if (!string.IsNullOrWhiteSpace(Secrets.MobileCenter_iOS_Secret))
                        return CreateMobileCenterLogger();
                    break;
            }
            return new DebugLogger();
        }

        private MCAnalyticsLogger CreateMobileCenterLogger()
        {
            var logger = new MCAnalyticsLogger();
            FFImageLoading.ImageService.Instance.Config.Logger = (IMiniLogger)logger;
            return logger;
        }

        private void LogUnobservedTaskExceptions()
        {
            TaskScheduler.UnobservedTaskException += ( sender, e ) =>
            {
                Logger.Log(e.Exception);
            };
        }

        private void OnPushNotificationReceived(object sender, PushNotificationReceivedEventArgs e)
        {
            // Add the notification message and title to the message
            var summary = $"Push notification received:" +
                $"\n\tNotification title: {e.Title}" +
                $"\n\tMessage: {e.Message}";

            // If there is custom data associated with the notification,
            // print the entries
            if(e.CustomData != null)
            {
                summary += "\n\tCustom data:\n";
                foreach(var key in e.CustomData.Keys)
                {
                    summary += $"\t\t{key} : {e.CustomData[key]}\n";
                }
            }

            // Send the notification summary to debug output
            System.Diagnostics.Debug.WriteLine(summary);
            Logger.Log(summary);
        }

        private bool OnReleaseAvailable(ReleaseDetails releaseDetails)
        {
            // Look at releaseDetails public properties to get version information, release notes text or release notes URL
            string versionName = releaseDetails.ShortVersion;
            string versionCodeOrBuildNumber = releaseDetails.Version;
            string releaseNotes = releaseDetails.ReleaseNotes;
            Uri releaseNotesUrl = releaseDetails.ReleaseNotesUrl;

            // custom dialog
            var title = "Version " + versionName + " available!";
            Task answer;

            // On mandatory update, user cannot postpone
            if(releaseDetails.MandatoryUpdate)
            {
                answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install");
            }
            else
            {
                answer = Current.MainPage.DisplayAlert(title, releaseNotes, "Download and Install", "Maybe tomorrow...");
            }
            answer.ContinueWith((task) =>
            {
                // If mandatory or if answer was positive
                if(releaseDetails.MandatoryUpdate || (task as Task<bool>).Result)
                {
                    // Notify SDK that user selected update
                    Distribute.NotifyUpdateAction(UpdateAction.Update);
                }
                else
                {
                    // Notify SDK that user selected postpone (for 1 day)
                    // Note that this method call is ignored by the SDK if the update is mandatory
                    Distribute.NotifyUpdateAction(UpdateAction.Postpone);
                }
            });

            // Return true if you are using your own dialog, false otherwise
            return true;
        }
    }
}
