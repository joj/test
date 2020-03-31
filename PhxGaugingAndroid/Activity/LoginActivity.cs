using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class LoginActivity : Activity
    {
        EditText user_name;
        EditText password;
        AlertDialog dialog;
        Spinner spDevice;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);
            user_name = FindViewById<EditText>(Resource.Id.user_name);
            password = FindViewById<EditText>(Resource.Id.password);
            Button sign_in_button = FindViewById<Button>(Resource.Id.sign_in_button);
            sign_in_button.Click += Login_Click;
            TextView tv_register = FindViewById<TextView>(Resource.Id.tv_register);
            tv_register.Click += tv_register_Click;
            TextView tv_forgetPsw = FindViewById<TextView>(Resource.Id.tv_forgetPsw);
            tv_forgetPsw.Click += tv_forgetPsw_Click;
            //解密json
            string Getuser_name = UserPreferences.GetString("UserInfo");
            if (!string.IsNullOrEmpty(Getuser_name))
            {
                string json = Utility.DecryptDES(Getuser_name);
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                user_name.Text = JsonModel.LoginName;
                password.Text = JsonModel.Password;
            }
            var LoginState = UserPreferences.GetString("LoginState");
            var NetworkState = UserPreferences.GetString("NetworkState");
            if (LoginState.Equals("Back"))
            {
                //在线
                RadioButton radioButton1 = FindViewById<RadioButton>(Resource.Id.radioButton1);
                //离线
                RadioButton radioButton2 = FindViewById<RadioButton>(Resource.Id.radioButton2);
                if (NetworkState.Equals("在线"))
                {
                    radioButton1.Checked = true;
                }
                else
                {
                    radioButton2.Checked = true;
                }
            }

            spDevice = FindViewById<Spinner>(Resource.Id.spDevice);
            List<string> DeviceList = new List<string>() { "Phx21", "EXEPC3100", "TVA2020" };
            ArrayAdapter tempArrayAdapter = new ArrayAdapter(this, Resource.Layout.ListViewItem2, DeviceList);
            tempArrayAdapter.SetDropDownViewResource(Resource.Layout.ListViewItem);
            spDevice.Adapter = tempArrayAdapter;
            spDevice.Prompt = "请选择检测设备类型";
            string selectDevice = UserPreferences.GetString("SelectLDARDevice");
            if (!string.IsNullOrEmpty(selectDevice))
            {
                int index = DeviceList.FindIndex(c => c == selectDevice);
                if (index != -1)
                {
                    spDevice.SetSelection(index);
                }
            }
            spDevice.ItemSelected += SpDevice_ItemSelected;
        }

        private void SpDevice_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            UserPreferences.SetString("SelectLDARDevice", spDevice.SelectedItem.ToString());
        }

        //登陆
        public void Login_Click(object sender, EventArgs e)
        {
            Login();
        }
        public void Login()
        {
            if (spDevice.SelectedItemPosition == -1)
            {
                Toast.MakeText(this, "必须选择设备类型!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(user_name.Text))
            {
                Toast.MakeText(this, "用户名不能为空!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(password.Text))
            {
                Toast.MakeText(this, "密码不能为空!", ToastLength.Short).Show();
            }
            else
            {
                //在线
                RadioButton radioButton1 = FindViewById<RadioButton>(Resource.Id.radioButton1);
                //离线
                RadioButton radioButton2 = FindViewById<RadioButton>(Resource.Id.radioButton2);
                var model = new LoginModel();
                model.LoginName = user_name.Text;
                model.UserPass = password.Text;
                model.UserType = 2;
                Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this, "提示", "登录中……", true, false);
                Task.Factory.StartNew(() =>
                {
                    BaseResult<LoginModel> msg = null;
                    if (radioButton1.Checked)
                    {
                        UserPreferences.DeleteString("NetworkState");
                        UserPreferences.SetString("NetworkState", "在线");
                        string Getuser_name = UserPreferences.GetString(user_name.Text);
                        msg = Utility.CallService<LoginModel>("V10SYS/SysAppPCUser/Login", model, 10000, null);
                        if (msg == null)
                        {
                            msg = new BaseResult<LoginModel> { Message = "网络连接失败!", Code = 10001 };
                        }
                        else if (msg.Code == 10000)
                        {
                            if (msg.Data.IkeyStatus == "1")
                            {
                                var MAC = Utility.getLocalMacAddress();
                                if (MAC.Equals(msg.Data.MakCardAddress))
                                {
                                    UserPreferences.DeleteString(user_name.Text);
                                    UserPreferences.DeleteString("CrrentUser");
                                    AndroidUser AndroidUsermodel = new AndroidUser();
                                    AndroidUsermodel.LoginName = user_name.Text;
                                    AndroidUsermodel.UserName = msg.Data.UserName;
                                    AndroidUsermodel.Password = password.Text;
                                    AndroidUsermodel.ServerLastLoginTime = msg.Data.Datetickets;
                                    AndroidUsermodel.IsBatchTest = msg.Data.IsBatchTest;
                                    string StringJson = JsonConvert.SerializeObject(AndroidUsermodel);
                                    //加密json
                                    string json = Utility.EncryptDES(StringJson);
                                    UserPreferences.SetString(user_name.Text, json);
                                    UserPreferences.SetString("UserInfo", json);
                                    UserPreferences.SetString("CrrentUser", json);
                                    UserPreferences.SetString("LoginUser", json);
                                    UserPreferences.SetString("LoginState", "1");
                                    //用户API地址列表
                                    UserPreferences.SetString("API" + user_name.Text, JsonConvert.SerializeObject(msg.Data.ApiList));
                                    GotoMain();
                                }
                                else
                                {
                                    msg = new BaseResult<LoginModel> { Message = "该账户mac地址和本设备mac不匹配!", Code = 10001 };
                                }
                            }
                            else
                            {
                                UserPreferences.DeleteString(user_name.Text);
                                UserPreferences.DeleteString("CrrentUser");
                                AndroidUser AndroidUsermodel = new AndroidUser();
                                AndroidUsermodel.LoginName = user_name.Text;
                                AndroidUsermodel.UserName = msg.Data.UserName;
                                AndroidUsermodel.Password = password.Text;
                                AndroidUsermodel.ServerLastLoginTime = msg.Data.Datetickets;
                                AndroidUsermodel.IsBatchTest = msg.Data.IsBatchTest;
                                string StringJson = JsonConvert.SerializeObject(AndroidUsermodel);
                                //加密json
                                string json = Utility.EncryptDES(StringJson);
                                UserPreferences.SetString(user_name.Text, json);
                                UserPreferences.SetString("UserInfo", json);
                                UserPreferences.SetString("LoginUser", json);
                                UserPreferences.SetString("CrrentUser", json);
                                UserPreferences.SetString("LoginState", "1");
                                //用户API地址列表
                                UserPreferences.SetString("API" + user_name.Text, JsonConvert.SerializeObject(msg.Data.ApiList));
                                GotoMain();
                            }
                        }
                    }
                    else
                    {
                        UserPreferences.DeleteString("NetworkState");
                        UserPreferences.DeleteString("CrrentUser");
                        UserPreferences.SetString("NetworkState", "离线");
                        string Getuser_name = UserPreferences.GetString(user_name.Text);
                        if (Getuser_name == "")
                        {
                            msg = new BaseResult<LoginModel> { Message = "请先在线登录!", Code = 10001 };
                        }
                        else
                        {
                            //解密json
                            string json = Utility.DecryptDES(Getuser_name);
                            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                            DateTime time = DateTime.Now.ToUniversalTime();
                            //服务器在线登陆时间
                            DateTime Stime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime);
                            //登陆时间大于当前系统时间
                            if (time > Utility.ConvertTimeStampToDateTime(JsonModel.LoginTime) || JsonModel.LoginTime == 0)
                            {
                                if (time < Stime.AddDays(5))
                                {
                                    if (JsonModel.LoginName == user_name.Text && JsonModel.Password == password.Text)
                                    {
                                        AndroidUser AndroidUsermodel = new AndroidUser();
                                        AndroidUsermodel.LoginName = user_name.Text;
                                        AndroidUsermodel.UserName = JsonModel.UserName;
                                        AndroidUsermodel.Password = password.Text;
                                        AndroidUsermodel.ServerLastLoginTime = JsonModel.ServerLastLoginTime;
                                        AndroidUsermodel.IsBatchTest = JsonModel.IsBatchTest;
                                        AndroidUsermodel.LoginTime = Utility.ConvertToTimestamp(DateTime.Now.ToUniversalTime());
                                        string StringJson = JsonConvert.SerializeObject(AndroidUsermodel);
                                        //加密json
                                        string json2 = Utility.EncryptDES(StringJson);
                                        UserPreferences.SetString(user_name.Text, json2);
                                        UserPreferences.SetString("LoginState", "2");
                                        UserPreferences.SetString("LoginUser", json2);
                                        UserPreferences.SetString("CrrentUser", UserPreferences.GetString(user_name.Text));
                                        msg = new BaseResult<LoginModel> { Message = "登录成功!", Code = 10000 };
                                        GotoMain();
                                    }
                                    else
                                    {
                                        msg = new BaseResult<LoginModel> { Message = "用户名或密码错误!", Code = 10001 };
                                    }
                                }
                                else
                                {
                                    msg = new BaseResult<LoginModel> { Message = "离线时间超过五天，必须在线登录!", Code = 10001 };
                                }
                            }
                            else
                            {
                                msg = new BaseResult<LoginModel> { Message = "使用时间已过期，必须在线登录!", Code = 10001 };
                            }
                        }
                    }
                    if (msg != null)
                    {
                        this.RunOnUiThread(delegate
                        {
                            Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                        });
                    }
                    else
                    {
                        this.RunOnUiThread(delegate
                        {
                            Toast.MakeText(this, "登录异常，请检查！", ToastLength.Short).Show();
                        });
                    }
                    dilog.Dismiss();
                });
            }
        }

        private void GotoMain()
        {
            //跳转主页面
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            string first = UserPreferences.GetString("First");
            if (string.IsNullOrEmpty(first))
            {
                first = DateTime.Now.ToString();
                UserPreferences.SetString("First", first);
            }
            DateTime? lastTime = null;
            string last = UserPreferences.GetString("Last");
            if (string.IsNullOrEmpty(last) == false)
            {
                lastTime = DateTime.Parse(last);
            }
            DateTime firstTime = DateTime.Parse(first);
            //if (DateTime.Now.AddDays(-7) > firstTime || (lastTime != null && DateTime.Now < lastTime))
            //{
            //    this.RunOnUiThread(delegate
            //    {
            //        Toast.MakeText(this, "授权已过期!", ToastLength.Short).Show();
            //    });
            //}
            //else
            //{
                UserPreferences.SetString("Last", DateTime.Now.ToString());
                if (LDARDevice == "EXEPC3100")
                {
                    this.StartActivityForResult(typeof(MainExepcActivity), 3100);
                }
                else if (LDARDevice == "TVA2020")
                {
                    this.StartActivityForResult(typeof(MainTvaActivity), 2020);
                }
                else
                {
                    this.StartActivityForResult(typeof(MainActivity), 21);
                }
             //}
        }

        /// <summary>
        /// 注册跳转
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tv_register_Click(object sender, EventArgs e)
        {
            this.StartActivity(typeof(RegisterActivity));
        }
        ClearnEditText PwdCode;
        ClearnEditText PwdPhone;
        ClearnEditText ChangePwd;
        ClearnEditText ChangeOldPwd;
        Button btn_Pwd_Code;
        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void tv_forgetPsw_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = this.LayoutInflater;
            View layout = inflater.Inflate(Resource.Layout.ForgetPwd, (ViewGroup)FindViewById<ViewGroup>(Resource.Id.dialog));
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetView(layout);
            builder.SetCancelable(false);
            Button btn_Pwd_Determine = layout.FindViewById<Button>(Resource.Id.btn_Pwd_Determine);
            btn_Pwd_Determine.Click += btn_Pwd_Determine_Click;
            Button btn_Pwd_Cancel = layout.FindViewById<Button>(Resource.Id.btn_Pwd_Cancel);
            PwdCode = layout.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.PwdCode);
            PwdPhone = layout.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.PwdPhone);
            ChangePwd = layout.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.ChangePwd);
            ChangeOldPwd = layout.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.ChangeOldPwd);
            btn_Pwd_Cancel.Click += btn_Pwd_Cancel_Click;
            btn_Pwd_Code = layout.FindViewById<Button>(Resource.Id.btn_Pwd_Code);
            btn_Pwd_Code.Click += btn_Pwd_Code_Click;
            dialog = builder.Show();
        }
        /// <summary>
        /// 确定事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btn_Pwd_Determine_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(PwdPhone.Text))
            {
                Toast.MakeText(this, "手机号不能为空!", ToastLength.Short).Show();
            }
            else if (PwdPhone.Text.Length < 11)
            {
                Toast.MakeText(this, "手机号格式错误!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(ChangePwd.Text))
            {
                Toast.MakeText(this, "密码不能为空!", ToastLength.Short).Show();
            }
            else if (ChangePwd.Text.Length < 6)
            {
                Toast.MakeText(this, "密码不能小于六位!", ToastLength.Short).Show();
            }
            else if (!ChangePwd.Text.Equals(ChangeOldPwd.Text))
            {
                Toast.MakeText(this, "两次密码不一致，请重新输入!", ToastLength.Short).Show();
            }
            else if (string.IsNullOrEmpty(PwdCode.Text))
            {
                Toast.MakeText(this, "验证码不能为空!", ToastLength.Short).Show();
            }
            else
            {
                var model = new LoginModel();
                model.LoginName = PwdPhone.Text;
                model.VerificationCode = PwdCode.Text;
                var msg = Utility.CallService<bool>("V10SYS/SysAppPCUser/CheckVerificationCode", model, 10000, null);
                if (msg.Code == 10000)
                {
                    var Pwdmodel = new LoginModel();
                    Pwdmodel.LoginName = PwdPhone.Text;
                    Pwdmodel.UserPass = ChangePwd.Text;
                    Pwdmodel.UserType = 2;
                    var Pwdmsg = Utility.CallService<bool>("V10SYS/SysAppPCUser/ModifyUserPass", Pwdmodel, 10000, null);
                    if (Pwdmsg.Code == 10000)
                    {
                        Toast.MakeText(this, "修改成功!", ToastLength.Short).Show();
                        UserPreferences.DeleteString(PwdPhone.Text);
                        password.Text = "";
                        dialog.Dismiss();
                    }
                    else
                    {
                        Toast.MakeText(this, "修改失败!", ToastLength.Short).Show();
                    }
                }
                else
                {
                    Toast.MakeText(this, "验证码错误!", ToastLength.Short).Show();
                }
            }
        }
        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void btn_Pwd_Code_Click(object sender, EventArgs e)
        {
            MyCountDownTimer myCountDownTimer = new MyCountDownTimer(60000, 1000, btn_Pwd_Code);
            if (!string.IsNullOrEmpty(PwdPhone.Text))
            {
                var model = new LoginModel();
                model.LoginName = PwdPhone.Text;
                model.UserType = 2;
                model.Remark = "1";
                var msg = Utility.CallService<bool>("V10SYS/SysAppPCUser/SendSMS", model, 10000, null);
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
        /// <summary>
        /// 取消事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public void btn_Pwd_Cancel_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
        }


    }
}