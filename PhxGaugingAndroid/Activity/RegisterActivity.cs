using Android.App;
using Android.Net.Wifi;
using Android.OS;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;
using System;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false,  NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class RegisterActivity : AppCompatActivity
    {
        ClearnEditText Company;
        ClearnEditText UserName;
        ClearnEditText Phone;
        ClearnEditText Pwd;
        ClearnEditText OldPwd;
        ClearnEditText Code;
        ClearnEditText MachineCode;
        Button btn_Code;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);
            var Registertoolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Registertoolbar);
            if (Registertoolbar != null)
            {
                Registertoolbar.Title = "用户注册";
                Registertoolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(Registertoolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            Registertoolbar.NavigationClick += (s, e) =>
            {
                //跳转主页面
                this.StartActivity(typeof(LoginActivity));
            };
            Company = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText01);
            UserName = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText02);
            Phone = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText03);
            Pwd = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText04);
            OldPwd = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText05);
            Code = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText06);
            MachineCode = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editText07);
            // 获取wifi
            WifiManager WifiState = (WifiManager)this.GetSystemService(Android.Content.Context.WifiService);
            var wifi = WifiState.WifiState;
            var iswificlos = WifiState.IsWifiEnabled;
            if (!iswificlos)
            {
                WifiState.SetWifiEnabled(true);
            }
            if (wifi == Android.Net.WifiState.Enabled)
            {
                MachineCode.Text = Utility.getMacAddress();
            }
            //if (int.Parse(Build.VERSION.Sdk) >= (int)Android.OS.BuildVersionCodes.M)
            //{
            //    MachineCode.Text = Utility.getMacAddress();
            //}
            //else
            //{
            //    MachineCode.Text = Utility.getMacAddress();
            //}
            Button register_in_button = FindViewById<Button>(Resource.Id.register_in_button);
            register_in_button.Click += register_click;
            btn_Code = FindViewById<Button>(Resource.Id.btn_Code);
            btn_Code.Click += btn_Code_click;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void register_click(object sender, EventArgs e)
        {
            var model = new LoginModel();
            model.CompanyName = Company.Text;
            model.LoginName = Phone.Text;
            model.UserName = UserName.Text;
            model.UserPass = OldPwd.Text;
            model.MakCardAddress = MachineCode.Text;
            model.VerificationCode = Code.Text;
            model.UserType = 2;
            if (string.IsNullOrEmpty(Company.Text))
            {
                Toast.MakeText(this, "公司名称不能为空!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(UserName.Text))
            {
                Toast.MakeText(this, "用户名不能为空!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(Phone.Text))
            {
                Toast.MakeText(this, "手机号不能为空!", ToastLength.Short).Show();
            }
            else if (Phone.Text.Length < 11)
            {
                Toast.MakeText(this, "手机号格式错误!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(Pwd.Text) || string.IsNullOrEmpty(OldPwd.Text))
            {
                Toast.MakeText(this, "密码不能为空!", ToastLength.Short).Show();
            }
            else if (Pwd.Text.Length < 6)
            {
                Toast.MakeText(this, "密码不能小于六位!", ToastLength.Short).Show();
            }
            else if (!Pwd.Text.Equals(OldPwd.Text))
            {
                Toast.MakeText(this, "两次密码不一致，请重新输入!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(Code.Text))
            {
                Toast.MakeText(this, "验证码不能为空!", ToastLength.Short).Show();
            }
            else
            {
                var msg = Utility.CallService<bool>("V10SYS/SysAppPCUser/Register", model,10000, null);
                if (msg.Code == 10000)
                {
                    Toast.MakeText(this, "注册成功!", ToastLength.Short).Show();
                    this.StartActivity(typeof(LoginActivity));
                }
                else
                {
                    Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                }
            }
        }
        /// <summary>
        /// 验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btn_Code_click(object sender, EventArgs e)
        {
            MyCountDownTimer myCountDownTimer = new MyCountDownTimer(60000, 1000, btn_Code);
            if (!string.IsNullOrEmpty(Phone.Text))
            {
                var model = new LoginModel();
                model.LoginName = Phone.Text;
                model.UserType = 2;
                model.Remark = "0";
                var msg = Utility.CallService<bool>("V10SYS/SysAppPCUser/SendSMS", model, 10000,null);
                if (msg.Code == 10000)
                {
                    Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                    myCountDownTimer.Start();
                }
                else
                {
                    Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                }
            }
            else
            {
                Toast.MakeText(this, "手机号不能为空!", ToastLength.Short).Show();
            }
        }
        // 定义WifiInfo对象  
        private WifiInfo mWifiInfo;
        // 定义WifiManager对象  
        private WifiManager mWifiManager;
        // 得到MAC地址  6.0以下
        public String getMacAddress()
        {
            mWifiManager = (WifiManager)this.GetSystemService(Android.Content.Context.WifiService);
            mWifiInfo = mWifiManager.ConnectionInfo;
            return (mWifiInfo == null) ? "NULL" : mWifiInfo.MacAddress;
        }
        /// <summary>
        /// 限制按键返回
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                //跳转主页面
                this.StartActivity(typeof(LoginActivity));
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}