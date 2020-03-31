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
    public class PhxDetailFragment : Fragment
    {
        public delegate void GotoCalibrationEventHandler(Fragment f);
        public event GotoCalibrationEventHandler GotoCalibration;
        public delegate void GotoDiagnosticEventHandler();
        public event GotoDiagnosticEventHandler GotoDiagnostic;
        public PhxDeviceService phxDeviceService;

        #region 显示值
        public string PhxName;

        public string Ppm;

        public float BatteryVoltage;

        public float BatteryVoltagePrecent;

        public float ChamberOuterTemp;

        public float SamplePressure;

        public float AirPressure;

        public float TankPressure;

        public float TankPressurePrecent;

        public float ThermoCouple;

        public double PicoAmps;

        public float SystemCurrent;

        public float PumpPower;

        public bool IsPumpAOn;

        public bool IsSolenoidAOn;

        public bool IsSolenoidBOn;

        public bool IsIgnited;
        #endregion

        TextView BatteryVoltageTextView;
        TextView BatteryVoltagePrecentTextView;
        TextView PpmStrTextView;
        TextView SamplePressureTextView;
        TextView AirPressureTextView;
        TextView TankPressureTextView;
        TextView TankPressurePrecentTextView;
        TextView PicoAmpsTextView;
        TextView PumpPowerTextView;
        TextView chamberouterTmpTextView;
        TextView thermoCoupleTextView;
        TextView systemCurrentTextView;
        TextView statusTextView;
        TextView LoginStateText;
        TextView LoginUser;
        Button btnFire;

        public static PhxDetailFragment NewInstance()
        {
            var frag1 = new PhxDetailFragment { Arguments = new Bundle() };
            return frag1;
        }
        /// <summary>
        /// 获取检测设备实时数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dataPolledEventArgs"></param>
        private void Phx21OnDataPolled(object sender, DataPolledEventArgs dataPolledEventArgs)
        {
            try
            {
                this.BatteryVoltage = dataPolledEventArgs.Status.BatteryVoltage;
                this.BatteryVoltagePrecent = (this.BatteryVoltage - 5.5f) / 3.4f * 100f;
                this.Ppm = dataPolledEventArgs.Status.PpmStr;
                //if (this.Ppm == "N/A")
                //{
                //    this.Ppm = "0";
                //}
                this.ChamberOuterTemp = dataPolledEventArgs.Status.ChamberOuterTemp;
                this.SamplePressure = dataPolledEventArgs.Status.SamplePressure;
                this.AirPressure = dataPolledEventArgs.Status.AirPressure;
                this.TankPressure = dataPolledEventArgs.Status.TankPressure;
                this.TankPressurePrecent = (this.TankPressure - 200f) / 1600f * 100f;
                this.ThermoCouple = dataPolledEventArgs.Status.ThermoCouple;
                this.PicoAmps = dataPolledEventArgs.Status.PicoAmps;
                this.SystemCurrent = dataPolledEventArgs.Status.SystemCurrent;
                this.PumpPower = dataPolledEventArgs.Status.PumpPower;
                this.IsSolenoidAOn = dataPolledEventArgs.Status.IsSolenoidAOn;
                this.IsSolenoidBOn = dataPolledEventArgs.Status.IsSolenoidBOn;
                base.Activity.RunOnUiThread(delegate
                {
                    this.BatteryVoltageTextView.Text = this.BatteryVoltage.ToString();
                    this.BatteryVoltagePrecentTextView.Text = this.BatteryVoltagePrecent.ToString("F2") + "%";
                    this.PpmStrTextView.Text = this.Ppm.ToString();
                    this.SamplePressureTextView.Text = this.SamplePressure.ToString();
                    this.AirPressureTextView.Text = this.AirPressure.ToString();
                    this.TankPressureTextView.Text = this.TankPressure.ToString();
                    this.TankPressurePrecentTextView.Text = this.TankPressurePrecent.ToString("F2") + "%";
                    this.PicoAmpsTextView.Text = this.PicoAmps.ToString();
                    this.PumpPowerTextView.Text = this.PumpPower.ToString();
                    this.chamberouterTmpTextView.Text = this.ChamberOuterTemp.ToString();
                    this.thermoCoupleTextView.Text = this.ThermoCouple.ToString();
                    this.systemCurrentTextView.Text = this.SystemCurrent.ToString();
                    if (this.phxDeviceService.IsRunning && this.PumpPower > 85f)
                    {
                        this.statusTextView.Text = "泵已达到最大输出限制！请查看探头是否阻塞，或者需要更新探头或过滤器";
                        return;
                    }
                    if (this.PumpPower > 85f)
                    {
                        this.statusTextView.Text = "取样空气压力较低！请查看探头是否阻塞，或者需要更新探头或过滤器";
                        return;
                    }
                    if ((double)this.BatteryVoltage < 5.5)
                    {
                        this.statusTextView.Text = "电池电压低，请充电";
                        return;
                    }
                    if (this.phxDeviceService.IsRunning && this.ChamberOuterTemp > 500f)
                    {
                        this.statusTextView.Text = "燃烧室内部温度过高";
                        return;
                    }
                    if (this.TankPressure < 400f)
                    {
                        this.statusTextView.Text = "氢气量即将用完，请及时充气";
                        return;
                    }
                    this.statusTextView.Text = "";
                });
            }
            catch
            {
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.PhxDetail, container, false);
            Button btnDiagn = v.FindViewById<Button>(Resource.Id.btn1);
            btnDiagn.Click -= Diagnostic_Click;
            btnDiagn.Click += Diagnostic_Click;
            Button button = v.FindViewById<Button>(Resource.Id.btn2);
            button.Click -= this.CalibrationButtonClick;
            button.Click += this.CalibrationButtonClick;
            btnFire = v.FindViewById<Button>(Resource.Id.btn3);
            btnFire.Click -= BtnFire_Click;
            btnFire.Click += BtnFire_Click;
            if (switchButtonStates == 1)
            {
                btnFire.Text = "再次点火/关闭";
            }
            else
            {
                btnFire.Text = "点火";
            }
            BatteryVoltageTextView = v.FindViewById<TextView>(Resource.Id.BatteryVoltageText);
            BatteryVoltagePrecentTextView = v.FindViewById<TextView>(Resource.Id.BatteryVoltagePrecentText);
            PpmStrTextView = v.FindViewById<TextView>(Resource.Id.PpmStrText);
            SamplePressureTextView = v.FindViewById<TextView>(Resource.Id.SamplePressureText);
            AirPressureTextView = v.FindViewById<TextView>(Resource.Id.AirPressureText);
            TankPressureTextView = v.FindViewById<TextView>(Resource.Id.TankPressureText);
            TankPressurePrecentTextView = v.FindViewById<TextView>(Resource.Id.TankPressurePrecentText);
            PicoAmpsTextView = v.FindViewById<TextView>(Resource.Id.PicoAmpsText);
            PumpPowerTextView = v.FindViewById<TextView>(Resource.Id.PumpPowerText);
            chamberouterTmpTextView = v.FindViewById<TextView>(Resource.Id.chamberouterTmpText);
            thermoCoupleTextView = v.FindViewById<TextView>(Resource.Id.thermoCoupleText);
            systemCurrentTextView = v.FindViewById<TextView>(Resource.Id.systemCurrentText);
            statusTextView = v.FindViewById<TextView>(Resource.Id.statusText);
            LoginStateText = v.FindViewById<TextView>(Resource.Id.LoginStateText);
            LoginUser = v.FindViewById<TextView>(Resource.Id.LoginUser);
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
        /// 连接蓝牙
        /// </summary>
        /// <param name="service"></param>
        public void ConnectBluetools(PhxDeviceService service)
        {
            phxDeviceService = service;
            phxDeviceService.Phx21.DataPolled -= this.Phx21OnDataPolled;
            phxDeviceService.Phx21.DataPolled += this.Phx21OnDataPolled;
            phxDeviceService.ConfigureLogging();
            //是否需要记录状态日志
            bool isLog = false;
            string value = UserPreferences.GetString("PhxLogEnable");
            if (value != null && value != string.Empty)
            {
                isLog = bool.Parse(value);
            }
            phxDeviceService.Phx21.IsLoging = isLog;
            int logTime = 1000;
            string param = UserPreferences.GetString("PhxLogTime");
            if (param != null && param != string.Empty)
            {
                logTime = int.Parse(param);
            }
            phxDeviceService.Phx21.LogTimeInterval = logTime;
            phxDeviceService.StartPollingData();
        }
        /// <summary>
        /// 诊断
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Diagnostic_Click(object sender, EventArgs e)
        {
            GotoDiagnostic();
        }
        /// <summary>
        /// 校准按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButtonClick(object sender, EventArgs e)
        {
            GotoCalibration(new CalibrationInfoFragment());
        }

        int switchButtonStates;
        /// <summary>
        /// 点火
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFire_Click(object sender, EventArgs e)
        {
            if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接检测设备", ToastLength.Short).Show();
                return;
            }
            if (this.switchButtonStates == 1)
            {
                if (this.phxDeviceService.IsRunning)
                {
                    this.phxDeviceService.Stop();
                    this.switchButtonStates = 0;
                    this.btnFire.Text = "点火";
                }
                else
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this.Activity);
                    builder.SetTitle("选择点火装置与泵控制");
                    builder.SetMessage("主泵正在运行，请选择以下选项：");
                    builder.SetPositiveButton("第二点火器", new EventHandler<DialogClickEventArgs>(this.handllerThreeButton));
                    builder.SetNegativeButton("第一点火器", new EventHandler<DialogClickEventArgs>(this.handllerThreeButton));
                    builder.SetNeutralButton("关闭泵", new EventHandler<DialogClickEventArgs>(this.handllerThreeButton));
                    AlertDialog alertDialog = builder.Create();
                    alertDialog.Show();
                }
            }
            else
            {
                this.btnFire.Text = "再次点火/关闭";
                this.phxDeviceService.Start();
                this.switchButtonStates = 1;
            }
        }

        private void handllerThreeButton(object sender, DialogClickEventArgs e)
        {
            AlertDialog alertDialog = sender as AlertDialog;
            Button button = alertDialog.GetButton(e.Which);
            if (button.Text == "第一点火器")
            {
                this.phxDeviceService.Phx21.Ignite(true, 0);
            }
            else if (button.Text == "关闭泵")
            {
                this.phxDeviceService.Stop();
                this.switchButtonStates = 0;
                this.btnFire.Text = "点火";
            }
            else if (button.Text == "第二点火器")
            {
                this.phxDeviceService.Phx21.Ignite(true, 1);
            }
        }
    }
}