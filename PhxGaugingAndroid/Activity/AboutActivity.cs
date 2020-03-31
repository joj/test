using Android.App;
using Android.OS;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Webkit;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class AboutActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.About);
            var Registertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Registertoolbar);
            if (Registertoolbar != null)
            {
                Registertoolbar.Title = "关于";
                Registertoolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(Registertoolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            Registertoolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
            WebView webView = FindViewById<WebView>(Resource.Id.wv_help);
            webView.Settings.JavaScriptEnabled = true;// 开启javascript支持  
            webView.Settings.SetSupportZoom(false);
            webView.Settings.SetAppCacheEnabled(false);
            webView.Settings.AllowContentAccess = true;
            //webView.AddJavascriptInterface(this, "changeVersionJs");
            webView.LoadUrl("file:///android_asset/about.html");
        }
    }
}