using System;
using System.Collections.Generic;
using System.IO;
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
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class PhxLogActivity : AppCompatActivity
    {
        ToggleButton PhxLogEnable;
        TextView tvLogFile;
        EditText etLogTime;
        Button btnClearLog;
        string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LDARAPP6/LDARtools");
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PhxLog);
            var Registertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.PhxLogtoolbar);
            if (Registertoolbar != null)
            {
                Registertoolbar.Title = "检测设备日志";
                Registertoolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(Registertoolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            Registertoolbar.NavigationClick += (s, e) =>
            {
                if (etLogTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "日志间隔不能为空!", ToastLength.Short).Show();
                    return;
                }
                Finish();
            };
            PhxLogEnable = FindViewById<ToggleButton>(Resource.Id.PhxLogEnable);
            string value = UserPreferences.GetString("PhxLogEnable");
            if (value != null && value != string.Empty)
            {
                PhxLogEnable.Checked = bool.Parse(value);
            }
            else
            {
                PhxLogEnable.Checked = false;
            }
            PhxLogEnable.CheckedChange -= PhxLogEnable_CheckedChange;
            PhxLogEnable.CheckedChange += PhxLogEnable_CheckedChange;
            tvLogFile = FindViewById<TextView>(Resource.Id.tvLogFile);
            btnClearLog = FindViewById<Button>(Resource.Id.btnClearLog);
            btnClearLog.Click += BtnClearLog_Click;
            double size = 0;
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    size += fi.Length;
                }
            }
            size = Math.Round(size / 1024D / 1024D, 3, MidpointRounding.AwayFromZero);
            tvLogFile.Text = "日志缓存文件" + size.ToString() + "M";
            FindViewById<TextView>(Resource.Id.tvPhxLogPath).Text = "日志存储路径：" + "内部存储/LDARAPP6/LDARtools";
            etLogTime = FindViewById<EditText>(Resource.Id.etLogTime);
            string param = UserPreferences.GetString("PhxLogTime");
            if (param != null && param != string.Empty)
            {
                etLogTime.Text = param;
            }
            else
            {
                etLogTime.Text = "1000";
            }
            etLogTime.TextChanged -= EtLogTime_TextChanged;
            etLogTime.TextChanged += EtLogTime_TextChanged;
        }

        private void EtLogTime_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (e.Text.Count() == 0)
            {
                Toast.MakeText(this, "日志间隔不能为空", ToastLength.Short).Show();
            }
            else
            {
                UserPreferences.SetString("PhxLogTime", e.Text.ToString());
            }
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            long size = 0;
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }
                foreach (FileInfo fi in dir.GetFiles())
                {
                    size += fi.Length;
                }
            }
            tvLogFile.Text = "日志缓存文件" + (size / 1024D / 1024D).ToString("F3") + "M";
        }

        private void PhxLogEnable_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            string value = PhxLogEnable.Checked.ToString();
            UserPreferences.SetString("PhxLogEnable", value);
            if (App.phxDeviceService != null)
            {
                Toast.MakeText(this, "断开设备重新连接才可起作用", ToastLength.Short).Show();
            }
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (etLogTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "日志间隔不能为空!", ToastLength.Short).Show();
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}