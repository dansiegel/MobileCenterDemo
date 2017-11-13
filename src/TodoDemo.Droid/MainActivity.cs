using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using TodoDemo.Helpers;
using Microsoft.Azure.Mobile.Push;
using Xamarin.Forms.Platform.Android;

namespace TodoDemo.Droid
{
    [Activity(Label = "@string/ApplicationName",
              Icon = "@mipmap/ic_launcher",
              Theme = "@style/MyTheme",
              ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::FFImageLoading.Forms.Droid.CachedImageRenderer.Init();
            global::FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration()
            {
                Logger = new Services.DebugLogger()
            });

            LoadApplication(new App(new AndroidInitializer()));
        }

        protected override void OnNewIntent(Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);
            Push.CheckLaunchedFromNotification(this, intent);
        }
    }
}
