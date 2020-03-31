using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class HelperActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Helper);
            var Helpertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Helpertoolbar);
            if (Helpertoolbar != null)
            {
                Helpertoolbar.Title = "帮助";
                Helpertoolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(Helpertoolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            Helpertoolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
            WebView webView = FindViewById<WebView>(Resource.Id.wv_help);
            webView.Settings.SetSupportZoom(false);
            webView.Settings.SetAppCacheEnabled(false);
            webView.Settings.AllowContentAccess = true;
            webView.LoadUrl("file:///android_asset/help.html");
        }
    }
}