using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/Theme.Splash", NoHistory = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //6.0及以上版本需要动态申请权限
            if (int.Parse(Build.VERSION.Sdk) >= (int)Android.OS.BuildVersionCodes.M)
            {
                List<string> list = new List<string>();
                if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.ReadExternalStorage);
                    list.Add(Manifest.Permission.WriteExternalStorage);
                }
                //相机权限
                if (CheckSelfPermission(Manifest.Permission.Camera) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.Camera);
                }
                //闪光灯
                if (CheckSelfPermission(Manifest.Permission.Flashlight) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.Flashlight);
                }
                //拨打电话权限
                if (CheckSelfPermission(Manifest.Permission.CallPhone) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.CallPhone);
                }
                //蓝牙权限
                if (CheckSelfPermission(Manifest.Permission.BluetoothAdmin) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.Bluetooth);
                    list.Add(Manifest.Permission.BluetoothAdmin);
                }
                //定位权限
                if (CheckSelfPermission(Manifest.Permission.AccessCoarseLocation) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.AccessCoarseLocation);
                }
                if (CheckSelfPermission(Manifest.Permission.AccessWifiState) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.AccessWifiState);
                }
                if (CheckSelfPermission(Manifest.Permission.ChangeWifiState) != Permission.Granted)
                {
                    list.Add(Manifest.Permission.ChangeWifiState);
                }
                if (list.Count > 0)
                {
                    this.RequestPermissions(list.ToArray(), 0);
                }
                else
                {
                    StartMainActivity();
                }
            }
            else
            {
                StartMainActivity();
            }
        }
        /// <summary>
        /// 申请权限回调
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == 0)
            {
                bool isAllGranted = true;
                for (int i = 0; i < grantResults.Count(); i++)
                {
                    if (grantResults[i] != Permission.Granted)
                    {
                        isAllGranted = false;
                    }
                }
                if (isAllGranted)
                {
                    StartMainActivity();
                }
            }
        }
        void StartMainActivity()
        {
            string UserInfo = UserPreferences.GetString("LoginUser");
            var NetworkState = UserPreferences.GetString("NetworkState");
            BaseResult<LoginModel> msg = null;
            if (UserInfo == "")
            {
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                try
                {
                    string json = Utility.DecryptDES(UserInfo);
                    var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                    var model = new LoginModel();
                    model.LoginName = JsonModel.LoginName;
                    model.UserPass = JsonModel.Password;
                    model.UserType = 2;
                    if (NetworkState.Equals("在线"))
                    {
                        msg = Utility.CallService<LoginModel>("V10SYS/SysAppPCUser/Login", model, 3000, null);
                        if (msg == null)
                        {
                            msg = new BaseResult<LoginModel> { Message = "网络连接失败!", Code = 10001 };
                            Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                            StartActivity(typeof(LoginActivity));
                        }
                        else if (msg.Code == 10000)
                        {
                            if (msg.Data.IkeyStatus == "1")
                            {
                                var MAC = Utility.getLocalMacAddress();
                                if (MAC.Equals(msg.Data.MakCardAddress))
                                {
                                    AndroidUser AndroidUsermodel = new AndroidUser();
                                    AndroidUsermodel.LoginName = msg.Data.LoginName;
                                    AndroidUsermodel.UserName = msg.Data.UserName;
                                    AndroidUsermodel.Password = msg.Data.UserPass;
                                    AndroidUsermodel.ServerLastLoginTime = msg.Data.Datetickets;
                                    AndroidUsermodel.IsBatchTest = msg.Data.IsBatchTest;
                                    string StringJson = JsonConvert.SerializeObject(AndroidUsermodel);
                                    //加密json
                                    string json3 = Utility.EncryptDES(StringJson);
                                    UserPreferences.SetString(msg.Data.LoginName, json3);
                                    UserPreferences.SetString("UserInfo", json3);
                                    UserPreferences.SetString("LoginUser", json3);
                                    UserPreferences.SetString("CrrentUser", json3);
                                    UserPreferences.SetString("LoginState", "1");
                                    //用户API地址列表
                                    UserPreferences.SetString("API" + model.LoginName, JsonConvert.SerializeObject(msg.Data.ApiList));
                                    GotoMain();
                                }
                                else
                                {
                                    msg = new BaseResult<LoginModel> { Message = "该账户mac地址和本设备mac不匹配!", Code = 10001 };
                                    Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                                    StartActivity(typeof(LoginActivity));
                                }
                            }
                            else
                            {
                                AndroidUser AndroidUsermodel = new AndroidUser();
                                AndroidUsermodel.LoginName = msg.Data.LoginName;
                                AndroidUsermodel.UserName = msg.Data.UserName;
                                AndroidUsermodel.Password = msg.Data.UserPass;
                                AndroidUsermodel.ServerLastLoginTime = msg.Data.Datetickets;
                                AndroidUsermodel.IsBatchTest = msg.Data.IsBatchTest;
                                string StringJson = JsonConvert.SerializeObject(AndroidUsermodel);
                                //加密json
                                string json4 = Utility.EncryptDES(StringJson);
                                UserPreferences.SetString(msg.Data.LoginName, json4);
                                UserPreferences.SetString("UserInfo", json4);
                                UserPreferences.SetString("LoginUser", json4);
                                UserPreferences.SetString("CrrentUser", json4);
                                UserPreferences.SetString("LoginState", "1");
                                //用户API地址列表
                                UserPreferences.SetString("API" + model.LoginName, JsonConvert.SerializeObject(msg.Data.ApiList));
                                GotoMain();
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                            StartActivity(typeof(LoginActivity));
                        }
                    }
                    else
                    {
                        UserPreferences.DeleteString("CrrentUser");
                        DateTime time = DateTime.Now.ToUniversalTime();
                        //服务器在线登陆时间
                        DateTime Stime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime);
                        if (time > Utility.ConvertTimeStampToDateTime(JsonModel.LoginTime) || JsonModel.LoginTime == 0)
                        {
                            if (time < Stime.AddDays(5))
                            {
                                UserPreferences.SetString("LoginState", "2");
                                UserPreferences.SetString("CrrentUser", UserPreferences.GetString(JsonModel.LoginName));
                                GotoMain();
                            }
                            else
                            {
                                msg = new BaseResult<LoginModel> { Message = "离线时间超过五天，必须在线登录!", Code = 10001 };
                                Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                                StartActivity(typeof(LoginActivity));
                            }
                        }
                        else
                        {
                            msg = new BaseResult<LoginModel> { Message = "使用时间已过期，必须在线登录!", Code = 10001 };
                            Toast.MakeText(this, msg.Message, ToastLength.Short).Show();
                            StartActivity(typeof(LoginActivity));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("启动界面", ex);
                    Toast.MakeText(this, ex.Message, ToastLength.Short).Show();
                    msg = null;
                    StartActivity(typeof(LoginActivity));
                }
            }
            DisplayMetrics dm = new DisplayMetrics();
            this.WindowManager.DefaultDisplay.GetMetrics(dm);
            int _width = dm.WidthPixels;//分辨率宽度
            int _height = dm.HeightPixels;//分辨率高度
            int _dpi = (int)dm.DensityDpi;
            Finish();

        }

        private void GotoMain()
        {
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
            if (DateTime.Now.AddDays(-7) > firstTime || (lastTime != null && DateTime.Now < lastTime))
            {
                Toast.MakeText(this, "授权已过期!", ToastLength.Short).Show();
                StartActivity(typeof(LoginActivity));
            }
            else
            {
                UserPreferences.SetString("Last", DateTime.Now.ToString());
                //跳转主页面
                string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
                if (string.IsNullOrEmpty(LDARDevice))
                {
                    LDARDevice = "Phx21";
                    UserPreferences.SetString("SelectLDARDevice", LDARDevice);
                }
                if (LDARDevice == "TVA2020")
                {
                    this.StartActivity(typeof(MainTvaActivity));
                }
                else if (LDARDevice == "EXEPC3100")
                {
                    this.StartActivity(typeof(MainExepcActivity));
                }
                else
                {
                    this.StartActivity(typeof(MainActivity));
                }
            }
        }
    }
}