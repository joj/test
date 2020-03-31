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
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class DetectionActivity : AppCompatActivity
    {
        AndroidWorkOrder workOrder;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Detection);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Detoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "已检测密封点记录";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };

            string info = this.Intent.GetStringExtra("AndroidWorkOrder");
            workOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidWorkOrder>(info);

            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(workOrder.DataPath);
                var list = connection.Table<AndroidSealPoint>().Where(c=>c.StartTime!=null).OrderByDescending(k=>k.StartTime).ToList();
                FindViewById<ListView>(Resource.Id.lvDe).Adapter = new DetectionAdapter(this, list);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("已检密封点记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }
    }
}