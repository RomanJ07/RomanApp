using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Webkit;

namespace RomanApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // On configure TOUTES les WebViews de l'app pour autoriser le chargement de fichiers
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AllowFileAccess", (handler, view) =>
        {
            if (handler.PlatformView is Android.Webkit.WebView nativeWebView)
            {
                nativeWebView.Settings.AllowFileAccess = true;
                nativeWebView.Settings.AllowContentAccess = true;
                nativeWebView.Settings.AllowFileAccessFromFileURLs = true;
                nativeWebView.Settings.AllowUniversalAccessFromFileURLs = true;
            }
        });
    }
}