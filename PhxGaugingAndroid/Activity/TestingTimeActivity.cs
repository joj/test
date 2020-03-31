using Android.App;
using Android.OS;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class TestingTimeActivity : AppCompatActivity
    {
        ToggleButton tbEnable;
        EditText etStayTime;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.TestingTime);
            tbEnable = FindViewById<ToggleButton>(Resource.Id.TestingtbEnable);
            etStayTime = FindViewById<EditText>(Resource.Id.TestingetStayTime);
            tbEnable.CheckedChange -= TbEnable_CheckedChange;
            tbEnable.CheckedChange += TbEnable_CheckedChange;
            etStayTime.TextChanged -= EtStayTime_TextChanged;
            etStayTime.TextChanged += EtStayTime_TextChanged;
            string value = UserPreferences.GetString("DetectionSetting");
            if (value != null && value != string.Empty)
            {
                string[] param = value.Split(',');
                tbEnable.Checked = bool.Parse(param[0]);
                etStayTime.Text = param[1];
            }
            etStayTime.Enabled = tbEnable.Checked;
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.TestTtoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "检测设置";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                if (etStayTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "检测停留时间不能为空!", ToastLength.Short).Show();
                    return;
                }
                Finish();
            };
        }

        private void TbEnable_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            etStayTime.Enabled = e.IsChecked;
            SaveStayTime();
        }

        private void EtStayTime_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            SaveStayTime();
        }
        private void SaveStayTime()
        {
            string value = tbEnable.Checked.ToString() + "," + etStayTime.Text;
            UserPreferences.SetString("DetectionSetting", value);
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (etStayTime.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(this, "检测停留时间不能为空!", ToastLength.Short).Show();
                    return true;
                }
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}