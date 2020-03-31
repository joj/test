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
using System.Threading.Tasks;

namespace PhxGaugingAndroid.Fragments
{
    public class PhxDetailTvaFragment : Fragment
    {
        public bool IsIgnited;

        TextView LoginStateText;
        TextView LoginUser;
        Button btnPumpOn;
        Button btnPumpOff;
        Button btnFire;
        TextView fidText;
        TextView pidText;

        public static PhxDetailTvaFragment NewInstance()
        {
            var frag1 = new PhxDetailTvaFragment { Arguments = new Bundle() };
            return frag1;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this.Activity;
            App.tvaService.DataPoll -= TvaService_DataPoll;
            App.tvaService.DataPoll += TvaService_DataPoll;
            App.tvaService.FireResult -= TvaService_FireResult;
            App.tvaService.FireResult += TvaService_FireResult;
            App.tvaService.ConnectStatus -= TvaService_ConnectStatus;
            App.tvaService.ConnectStatus += TvaService_ConnectStatus;
        }

        private void TvaService_ConnectStatus(bool isConnect)
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

        private void TvaService_FireResult(bool isSuccess)
        {
            context.RunOnUiThread(() =>
            {
                Toast.MakeText(context, isSuccess ? "点火成功" : "点火失败", ToastLength.Short).Show();
                if (isSuccess)
                {
                    btnFire.SetBackgroundResource(Resource.Drawable.PidBackgroud);
                }
            });
        }
        private void TvaService_DataPoll(float FID, string FIDStatus, float PID, string PIDStatus)
        {
            context.RunOnUiThread(() =>
            {
                fidText.Text = FID.ToString("F0");
                pidText.Text = PID.ToString("F0");
            });
        }

        Activity context;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.PhxDetailTva, container, false);
            btnFire = v.FindViewById<Button>(Resource.Id.btnFire);
            btnFire.Click -= BtnFire_Click;
            btnFire.Click += BtnFire_Click;

            btnPumpOn = v.FindViewById<Button>(Resource.Id.btnPumpOn);
            btnPumpOn.Click -= BtnPumpOn_Click;
            btnPumpOn.Click += BtnPumpOn_Click; ;

            btnPumpOff = v.FindViewById<Button>(Resource.Id.btnPumpOff);
            btnPumpOff.Click -= BtnPumpOff_Click;
            btnPumpOff.Click += BtnPumpOff_Click;

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
        /// 关闭泵
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPumpOff_Click(object sender, EventArgs e)
        {
            if (App.tvaService.IsConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接蓝牙", ToastLength.Short).Show();
                return;
            }
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "发送命令中……", true, false);
            Task.Factory.StartNew(() =>
            {
                App.tvaService.PumpOffAction();
                dilog.Dismiss();
            });
        }
        /// <summary>
        /// 打开泵
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnPumpOn_Click(object sender, EventArgs e)
        {
            if (App.tvaService.IsConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接蓝牙", ToastLength.Short).Show();
                return;
            }
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "发送命令中……", true, false);
            Task.Factory.StartNew(() =>
            {
                App.tvaService.PumpOnAction();
                dilog.Dismiss();
            });
        }

        /// <summary>
        /// 点火
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFire_Click(object sender, EventArgs e)
        {
            if (App.tvaService.IsConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接蓝牙", ToastLength.Short).Show();
                return;
            }
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "发送命令中……", true, false);
            Task.Factory.StartNew(() =>
            {
                App.tvaService.FireAction();
                dilog.Dismiss();
            });
        }
    }
}