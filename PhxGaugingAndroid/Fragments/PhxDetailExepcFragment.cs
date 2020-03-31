using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Runtime;
using Android.Util;
using PhxGauging.Common.Android.Services;
using PhxGauging.Common.Devices.Services;
using PhxGauging.Common.Devices;
using PhxGaugingAndroid.Common;
using Newtonsoft.Json;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    public class PhxDetailExepcFragment : Fragment
    {
        public delegate void GotoCalibrationEventHandler(Fragment f);
        public event GotoCalibrationEventHandler GotoCalibration;

        public bool IsIgnited;

        TextView LoginStateText;
        TextView LoginUser;
        Button btnCalibration;
        Button btnFire;
        TextView fidText;
        TextView pidText;

        public static PhxDetailExepcFragment NewInstance()
        {
            var frag1 = new PhxDetailExepcFragment { Arguments = new Bundle() };
            return frag1;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this.Activity;
            App.exepcService.DataPoll -= ExepcService_DataPoll;
            App.exepcService.DataPoll += ExepcService_DataPoll;
            App.exepcService.FireResult -= ExepcService_FireResult;
            App.exepcService.FireResult += ExepcService_FireResult;
            App.exepcService.ConnectStatus -= ExepcService_ConnectStatus;
            App.exepcService.ConnectStatus += ExepcService_ConnectStatus;
        }

        private void ExepcService_ConnectStatus(bool isConnect)
        {
            if (this.Activity == null)
            {
                return;
            }
            this.Activity.RunOnUiThread(() =>
            {
                if (isConnect == false)
                {
                    btnFire.SetBackgroundResource(Resource.Drawable.ButtonStyle);
                }
            });
        }

        private void ExepcService_FireResult(bool isSuccess)
        {
            this.Activity.RunOnUiThread(() =>
            {
                Toast.MakeText(this.Activity, isSuccess ? "点火成功" : "点火失败", ToastLength.Short).Show();
                if (isSuccess)
                {
                    btnFire.SetBackgroundResource(Resource.Drawable.PidBackgroud);
                }
            });
        }
        private void ExepcService_DataPoll(float FID, float PID)
        {
            if (this.Activity == null)
            {
                return;
            }
            this.Activity.RunOnUiThread(() =>
            {
                fidText.Text = FID.ToString("F0");
                pidText.Text = PID.ToString("F0");
            });
        }

        Activity context;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.PhxDetailExpec, container, false);
            btnFire = v.FindViewById<Button>(Resource.Id.btnFire);
            btnFire.Click -= BtnFire_Click;
            btnFire.Click += BtnFire_Click;
            btnCalibration = v.FindViewById<Button>(Resource.Id.btnCalibration);
            btnCalibration.Click -= BtnCalibration_Click;
            btnCalibration.Click += BtnCalibration_Click;
            LoginStateText = v.FindViewById<TextView>(Resource.Id.LoginStateText);
            LoginUser = v.FindViewById<TextView>(Resource.Id.LoginUser);
            fidText = v.FindViewById<TextView>(Resource.Id.tvFID);
            pidText = v.FindViewById<TextView>(Resource.Id.tvPID);
            string LoginState = UserPreferences.GetString("LoginState");
            string UserInfo = UserPreferences.GetString("UserInfo");
            string json = Utility.DecryptDES(UserInfo);
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            LoginUser.Text = LDARDevice + "，当前用户:" + JsonModel.UserName;
            if (LoginState == "2")
            {
                string Txt = "";
                DateTime Bentime = DateTime.Now;
                DateTime EndTime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime).AddDays(5);
                TimeSpan ts = EndTime.Subtract(Bentime);
                Txt += ts.Days.ToString() + "天" + ts.Hours.ToString() + "小时" + "后需在线登录!";
                this.LoginStateText.Text = Txt;
            }
            else
            {
                LoginStateText.Visibility = ViewStates.Gone;
            }
            return v;
        }
        /// <summary>
        /// 跳转校准界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCalibration_Click(object sender, EventArgs e)
        {
            GotoCalibration(new ExepcCalibration());
        }

        /// <summary>
        /// 点火
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFire_Click(object sender, EventArgs e)
        {
            if (App.exepcService.isConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接WIFI", ToastLength.Short).Show();
                return;
            }
            App.exepcService.FireAction();
        }
    }
}