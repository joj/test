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

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class BatchModeActivity : AppCompatActivity
    {
        AndroidWorkOrder workOrder;
        RadioGroup rdBMode;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BatchMode);
            rdBMode = FindViewById<RadioGroup>(Resource.Id.rdBMode);
            rdBMode.CheckedChange -= RdBMode_CheckedChange;
            rdBMode.CheckedChange += RdBMode_CheckedChange;

            string info = this.Intent.GetStringExtra("AndroidWorkOrder");
            workOrder = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidWorkOrder>(info);

            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.BMtoolbar);
            if (toolbar != null)
            {
                toolbar.Title = workOrder.WorkOrderName;
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };

            string values = UserPreferences.GetString(workOrder.ID + "GeneratePPM");
            if (values != null && values != string.Empty)
            {
                string[] param = values.Split(',');
                if (param.Count() > 6)
                {
                    if (param[6] == "0")
                        rdBMode.Check(Resource.Id.rbBModeWaite);
                    else
                        rdBMode.Check(Resource.Id.rbBModeNoWaite);
                }
                else
                {
                    rdBMode.Check(Resource.Id.rbBModeWaite);
                }
            }
            else
            {
                rdBMode.Check(Resource.Id.rbBModeWaite);
            }
        }

        /// <summary>
        /// 模式变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RdBMode_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            string mode = e.CheckedId == Resource.Id.rbBModeWaite ? "0" : "1";
            string values = UserPreferences.GetString(workOrder.ID + "GeneratePPM");
            if (values != null && values != string.Empty)
            {
                string[] param = values.Split(',');
                DateTime dt = DateTime.Parse(param[0] + " " + param[1]);
                if (e.CheckedId == Resource.Id.rbBModeWaite && dt > DateTime.Now)
                {
                    rdBMode.Check(Resource.Id.rbBModeNoWaite);
                    Toast.MakeText(this, "不等待模式最晚生成时间大于当前时间，不能修改成等待模式！", ToastLength.Short).Show();
                    return;
                }
                if (param.Count() > 6)
                {
                    param[6] = mode;
                    SaveParam(param);
                }
                else
                {
                    string[] paramNew = new string[7];
                    param.CopyTo(paramNew, 0);
                    paramNew[6] = mode;
                    SaveParam(paramNew);
                }
            }
            else
            {
                string[] param = new string[7] { DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("HH:mm:ss"), "", "", "", "", mode };
                SaveParam(param);
            }
        }

        private void SaveParam(string[] param)
        {
            string v = string.Empty;
            param.ToList().ForEach(c =>
            {
                if (v != string.Empty)
                {
                    v += ",";
                }
                v += c;
            });
            UserPreferences.SetString(workOrder.ID + "GeneratePPM", v);
        }
    }
}