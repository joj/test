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
    public class StayTimeActivity : AppCompatActivity
    {
        ToggleButton tbEnable;
        EditText etStayTime;

        ToggleButton tbContinueEnable;
        EditText etIntervalTime;
        LinearLayout tvInterval;
        ToggleButton tbDouble;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.StayTime);
            tbDouble = FindViewById<ToggleButton>(Resource.Id.tbDoubles);
            tbEnable = FindViewById<ToggleButton>(Resource.Id.tbEnable);
            etStayTime = FindViewById<EditText>(Resource.Id.etStayTime);
            tbContinueEnable = FindViewById<ToggleButton>(Resource.Id.tbContinueEnable);
            etIntervalTime = FindViewById<EditText>(Resource.Id.etIntervalTime);
            tvInterval = FindViewById<LinearLayout>(Resource.Id.tvInterval);

            tbDouble.CheckedChange -= TbDouble_CheckedChange;
            tbDouble.CheckedChange += TbDouble_CheckedChange;
            tbEnable.CheckedChange -= TbEnable_CheckedChange;
            tbEnable.CheckedChange += TbEnable_CheckedChange;
            etStayTime.TextChanged -= EtStayTime_TextChanged;
            etStayTime.TextChanged += EtStayTime_TextChanged;
            tbContinueEnable.CheckedChange -= TbContinueEnable_CheckedChange;
            tbContinueEnable.CheckedChange += TbContinueEnable_CheckedChange;
            etIntervalTime.TextChanged -= EtIntervalTime_TextChanged;
            etIntervalTime.TextChanged += EtIntervalTime_TextChanged;

            string doubleCheck = UserPreferences.GetString("DoubleCheck");
            if (doubleCheck != null && doubleCheck != string.Empty)
            {
                tbDouble.Checked = bool.Parse(doubleCheck);
            }
            else
            {
                tbDouble.Checked = false;
            }
            string value = UserPreferences.GetString("StayTime");
            if (value != null && value != string.Empty)
            {
                string[] param = value.Split(',');
                tbEnable.Checked = bool.Parse(param[0]);
                etStayTime.Text = param[1];
                if (param.Length == 4)
                {
                    tbContinueEnable.Checked = bool.Parse(param[2]);
                    etIntervalTime.Text = param[3];
                }
                else
                {
                    tbContinueEnable.Checked = false;
                    etIntervalTime.Text = "2";
                }
            }
            etStayTime.Enabled = tbEnable.Checked;
            tbContinueEnable.Enabled = tbEnable.Checked;
            etIntervalTime.Enabled = tbEnable.Checked;
            if (tbEnable.Checked == true)
            {
                etIntervalTime.Enabled = tbContinueEnable.Checked;
            }
            else
            {
                tvInterval.Visibility = ViewStates.Invisible;
            }
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.STtoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "工单检测设置";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                if (etStayTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "自动检测停留时间不能为空!", ToastLength.Short).Show();
                    return;
                }
                if (etIntervalTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "连续监测间隔时间不能为空!", ToastLength.Short).Show();
                    return;
                }
                Finish();
            };
        }

        private void TbDouble_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            string value = tbDouble.Checked.ToString();
            UserPreferences.SetString("DoubleCheck", value);
        }

        private void TbContinueEnable_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            etIntervalTime.Enabled = e.IsChecked;
            SaveStayTime();
        }
        private void EtIntervalTime_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            SaveStayTime();
        }

        private void TbEnable_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            etStayTime.Enabled = e.IsChecked;
            tbContinueEnable.Enabled = e.IsChecked;
            etIntervalTime.Enabled = e.IsChecked;
            if (e.IsChecked == true)
            {
                etIntervalTime.Enabled = tbContinueEnable.Checked;
                tvInterval.Visibility = ViewStates.Visible;
            }
            else
            {
                tvInterval.Visibility = ViewStates.Invisible;
            }
            SaveStayTime();
        }
        private void EtStayTime_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            SaveStayTime();
        }

        private void SaveStayTime()
        {
            string value = tbEnable.Checked.ToString() + "," + etStayTime.Text + "," + tbContinueEnable.Checked.ToString() + "," + etIntervalTime.Text;
            UserPreferences.SetString("StayTime", value);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (etStayTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "自动检测停留时间不能为空!", ToastLength.Short).Show();
                    return true;
                }
                if (etIntervalTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "连续监测间隔时间不能为空!", ToastLength.Short).Show();
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}