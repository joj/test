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
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class ExepcModelActivity : AppCompatActivity
    {
        RadioGroup radioModel;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ExepcModel);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.ExepcModeltoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "检测模式设置";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
            string model = UserPreferences.GetString("ExepcModel");
            radioModel = FindViewById<RadioGroup>(Resource.Id.radioModel);
            switch (model)
            {
                case "FID":
                    radioModel.Check(Resource.Id.rbFID);
                    break;
                case "PID":
                    radioModel.Check(Resource.Id.rbPID);
                    break;
                case "FID&PID":
                    radioModel.Check(Resource.Id.rbFIDPID);
                    break;
                default:
                    UserPreferences.SetString("ExepcModel", "FID");
                    radioModel.Check(Resource.Id.rbFID);
                    break;
            }
            radioModel.CheckedChange += RadioModel_CheckedChange;
        }

        private void RadioModel_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var rb = FindViewById<RadioButton>(e.CheckedId);
            UserPreferences.SetString("ExepcModel", rb.Text.Trim());
        }
    }
}