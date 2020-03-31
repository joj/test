using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGauging.Common.Devices;
using PhxGauging.Common.Devices.Services;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class DeviceDiagnosticActivity : Activity
    {
        ListView lvActivities;
        ProgressBar progressBar;
        TextView tvInfo;
        Button startBtn;
        Button doneBtn;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DeviceDiagnostic);
            lvActivities = FindViewById<ListView>(Resource.Id.lvActivities);
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            tvInfo = FindViewById<TextView>(Resource.Id.tvInfo);
            startBtn = FindViewById<Button>(Resource.Id.startBtn);
            startBtn.Click += ExecuteStart;
            doneBtn = FindViewById<Button>(Resource.Id.doneBtn);
            doneBtn.Click += ExecuteDone;
            StartEnabled = true;
            dialogService = new DialogService();
            ResetDiagnostic();
            progressBar.Visibility = ViewStates.Invisible;
            Info = "确保phx21没有连接到充电器！！！";
        }

        private bool plugCovered;
        private string info;
        /// <summary>
        /// 消息
        /// </summary>
        public string Info
        {
            get { return info; }
            set
            {
                info = value;
                RunOnUiThread(delegate
                {
                    tvInfo.Text = info;
                });
            }
        }

        private int progress;
        /// <summary>
        /// 进度条
        /// </summary>
        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                RunOnUiThread(delegate
                {
                    progressBar.Progress = progress;
                });
            }
        }
        private bool chargerConnected = false;
        private IList<string> Activities;
        private DialogService dialogService;
        private bool doneEnabled;
        /// <summary>
        /// 完成按钮
        /// </summary>
        public bool DoneEnabled
        {
            get { return doneEnabled; }
            set
            {
                doneEnabled = value;
                RunOnUiThread(delegate
                {
                    doneBtn.Enabled = doneEnabled;
                });
            }
        }

        private bool startEnabled;

        public bool StartEnabled
        {
            get { return startEnabled; }
            set
            {
                startEnabled = value;
                RunOnUiThread(delegate
                {
                    startBtn.Enabled = startEnabled;
                });
            }
        }
        /// <summary>
        /// 刷新检查项
        /// </summary>
        private void RefreshActive()
        {
            RunOnUiThread(delegate
            {
                if (this.lvActivities.Adapter == null || this.lvActivities.Adapter.Count != Activities.Count)
                {
                    this.lvActivities.Adapter = new ArrayAdapter<string>(this.BaseContext, Resource.Layout.ListViewItem, Activities);
                }
            });
        }

        private void ExecuteDone(object sender, EventArgs e)
        {
            //诊断完成返回
            this.Finish();
        }

        private void ResetDiagnostic()
        {
            DoneEnabled = true;
            Activities = new List<String>();
            RefreshActive();
            plugCovered = false;
            chargerConnected = false;
            progressBar.Visibility = ViewStates.Invisible;
        }
        private void ExecuteStart(object sender, EventArgs e)
        {
            if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
            {
                Toast.MakeText(this, "请先连接检测设备", ToastLength.Short).Show();
                return;
            }
            StartEnabled = false;            
            ResetDiagnostic();
            DoneEnabled = false;
            progressBar.Visibility = ViewStates.Visible;
            Task diagnosticTask = new Task(() =>
            {
                
                if (!CheckBattery())
                {
                    StartEnabled = true;
                    DoneActive();
                    return;
                }
                if (!CheckPump())
                {
                    StartEnabled = true;
                    DoneActive();
                    return;
                }
                if (!CheckProbe())
                {
                    StartEnabled = true;
                    DoneActive();
                    return;
                }
                if (!CheckGlowPlug1())
                {
                    StartEnabled = true;
                    DoneActive();
                    return;
                }
                if (!CheckGlowPlug2())
                {
                    StartEnabled = true;
                    DoneActive();
                    return;
                }
                Activities.Add("诊断完成");
                RefreshActive();
                Info = "phx21诊断通过";
                App.phxDeviceService.Phx21.TurnOffPump();
                DoneActive();
            });
            diagnosticTask.Start();
        }
        /// <summary>
        /// 完成动作 启用完成按钮 隐藏进度条 
        /// </summary>
        private void DoneActive()
        {
            DoneEnabled = true;
            RunOnUiThread(delegate
            {
                progressBar.Visibility = ViewStates.Invisible;
            });
        }

        private bool CheckBattery()
        {
            Activities.Add("充电器检查");
            Progress = 0;
            RefreshActive();
            Info = "关闭泵";
            App.phxDeviceService.Phx21.IgniteOff();
            Progress = 33;
            Task.Delay(500).Wait();

            Phx21Status data = GetValidPhxData();

            if (data.BatteryVoltage < 8)
            {
                Info = "错误：电池充电过低，phx21电压至少8v";
                return false;
            }

            Task.Delay(500).Wait();

            Info = "检查电池… ";

            bool cancel = false;
            RunOnUiThread(delegate
            {
                dialogService.ShowOkCancel(this, "提示", "连接phx21的充电器然后按确认",
                r =>
                {
                    if (r == DialogResult.Ok)
                        chargerConnected = true;

                    if (r == DialogResult.Cancel)
                        cancel = true;
                });
            });
            while (!chargerConnected)
            {
                if (cancel)
                    return false;
            }

            Task.Delay(500).Wait();

            Phx21Status dataConnected = GetValidPhxData();

            Task.Delay(1000).Wait();

            if (data.BatteryVoltage + 0.03 >= dataConnected.BatteryVoltage)
            {
                Info = "错误: 电池电压没有显著变化。充电器可能需要更换。";
                return false;
            }

            Progress = 66;

            Info = "断开phx21的充电器然后按确认。";

            cancel = false;
            RunOnUiThread(delegate
            {
                dialogService.ShowOkCancel(this, "提示", "断开phx21充电器然后按确认。",
                r =>
                {
                    if (r == DialogResult.Ok)
                        chargerConnected = false;

                    if (r == DialogResult.Cancel)
                        cancel = true;
                });
            });
            while (chargerConnected)
            {
                if (cancel)
                    return false;
            }

            Phx21Status dataDisconnected = GetValidPhxData();

            if (dataDisconnected.BatteryVoltage + 0.03 >= dataConnected.BatteryVoltage)
            {
                Info = "错误: 电池电压没有显著变化。充电器可能需要更换。 ";
                return false;
            }

            Task.Delay(500).Wait();

            Info = "充电器检查通过";
            Progress = 100;

            Task.Delay(500).Wait();

            return true;
        }

        private bool CheckPump()
        {
            Activities.Add("检查泵");
            Progress = 0;
            RefreshActive();

            Info = "打开泵";
            App.phxDeviceService.Phx21.TurnOnPumpToTargetPressure(1.75);
            Progress = 33;
            Task.Delay(1000).Wait();

            Info = "读取泵水平 ";
            Task.Delay(1000).Wait();
            Phx21Status data = GetValidPhxData();
            Progress = 66;

            if (data.PumpPower >= 85)
            {
                Info = "错误: 泵功率太高。泵很快就要更换了。";
                return false;
            }
            Task.Delay(500).Wait();

            Info = "泵检查通过";
            Progress = 100;
            Task.Delay(500).Wait();

            return true;
        }

        private bool CheckProbe()
        {
            Activities.Add("探头检测");
            Progress = 0;
            RefreshActive();
            Info = "检查排气压力";
            bool cancel = false;
            RunOnUiThread(delegate
            {
                dialogService.ShowOkCancel(this, "提示", "插入（完全覆盖）探头并按确认。 ",
                r =>
                {
                    if (r == DialogResult.Ok)
                        plugCovered = true;

                    if (r == DialogResult.Cancel)
                        cancel = true;
                });
            });
            while (!plugCovered)
            {
                if (cancel)
                    return false;
            }
            Progress = 33;

            DateTime now = DateTime.Now;
            TimeSpan span = TimeSpan.Zero;
            Phx21Status data = GetValidPhxData();

            while (span.TotalMilliseconds < 2000 && data.SamplePressure > 0.2)
            {
                data = GetValidPhxData();
                span = DateTime.Now - now;
            }

            App.phxDeviceService.Phx21.TurnOffPump();

            Progress = 66;
            if (data.SamplePressure > 0.2)
            {
                Info = "错误: 排气压力过高。拔下探头并检查快速连接O形环和孔的探测管。";
                return false;
            }

            Info = "探头检查通过";
            Progress = 100;

            cancel = false;

            RunOnUiThread(delegate
            {
                dialogService.ShowOkCancel(this, "提示", "拔下（完全揭开）探头并按确认。",
                r =>
                {
                    if (r == DialogResult.Ok)
                        plugCovered = false;

                    if (r == DialogResult.Cancel)
                        cancel = true;
                });
            });
            while (plugCovered)
            {
                if (cancel)
                    return false;
            }

            Task.Delay(500).Wait();

            return true;
        }

        private bool CheckGlowPlug1()
        {
            Activities.Add("第一点火器检查");
            Progress = 0;
            RefreshActive();
            Info = "第一点火器点火";
            Task.Delay(500).Wait();
            App.phxDeviceService.Phx21.IgniteGlowPlug1(2);

            Task.Delay(500).Wait();
            Phx21Status data = GetValidPhxData();

            Progress = 33;
            Info = "检查电池";
            Task.Delay(500).Wait();

            if (data.BatteryVoltage < 6.0)
            {
                Info = "错误: 第一点火器点火时电池电压过低。电池可能需要充电或更换。";
                return false;
            }
            Progress = 66;
            Info = "检查点火器";
            Task.Delay(250).Wait();

            //Do this again??
            //data = GetValidPhxData();

            if (data.BatteryVoltage > 8.0)
            {
                Info = "错误: 第一点火器点火时电池电压过高。点火器可能需要更换。";
                return false;
            }
            Task.Delay(500).Wait();

            Info = "第一点火器检查通过";
            Progress = 100;
            Task.Delay(500).Wait();
            return true;
        }

        private bool CheckGlowPlug2()
        {
            Activities.Add("第二点火器检查");
            Progress = 0;
            RefreshActive();
            Info = "第二点火器点火";
            Task.Delay(500).Wait();
            App.phxDeviceService.Phx21.IgniteGlowPlug2(2);
            Task.Delay(500).Wait();
            Phx21Status data = GetValidPhxData();
            Progress = 33;
            Info = "检查电池";
            Task.Delay(500).Wait();

            if (data.BatteryVoltage < 6.0)
            {
                Info = "错误:第二点火器点火时电池电压过低。电池可能需要充电或更换。";
                return false;
            }

            Progress = 66;
            Info = "检查点火器";
            Task.Delay(250).Wait();

            //Do this again?
            //data = GetValidPhxData();

            if (data.BatteryVoltage > 8.0)
            {
                Info = "错误: 第二点火器点火时电池电压过高。点火器可能需要更换。";
                return false;
            }
            Task.Delay(250).Wait();
            Info = "第二点火器检查通过";
            Progress = 100;

            return true;
        }

        private Phx21Status GetValidPhxData()
        {
            Phx21Status data = App.phxDeviceService.Phx21.ReadDataExtended();
            bool valid = data.BatteryVoltage < 9;

            for (int i = 0; i < 5 || !valid; i++)
            {
                data = App.phxDeviceService.Phx21.ReadDataExtended();
                valid = data.BatteryVoltage < 9;
            }
            return data;
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
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}