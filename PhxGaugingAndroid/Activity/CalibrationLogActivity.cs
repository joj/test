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
using Java.IO;
using PhxGauging.Common.Devices;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class CalibrationLogActivity : Activity
    {
        AndroidCalibration cali;
        List<AndroidCalibrationLog> logList;
        private Button CalibrationButtonStart;

        private Button CalibrationBtnDone;
        private Button CalibrationBtnCancel;
        private Button CalibrationBtnBack;

        private Button CalibrationButton1;
        private Button CalibrationButton2;
        private Button CalibrationButton3;
        private Button CalibrationButton4;
        private Button CalibrationButton5;
        private Button CalibrationButton6;

        private ClearnEditText CalibrationText1;
        private ClearnEditText CalibrationText2;
        private ClearnEditText CalibrationText3;
        private ClearnEditText CalibrationText4;
        private ClearnEditText CalibrationText5;
        private ClearnEditText CalibrationText6;

        private TextView CalibrationText1New;
        private TextView CalibrationText2New;
        private TextView CalibrationText3New;
        private TextView CalibrationText4New;
        private TextView CalibrationText5New;
        private TextView CalibrationText6New;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CalibrationLog);
            string info = this.Intent.GetStringExtra("AndroidCalibration");
            cali = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidCalibration>(info);
            cali.ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N");

            logList = new List<AndroidCalibrationLog>();

            CalibrationButtonStart = FindViewById<Button>(Resource.Id.CalibrationBtnStart);
            CalibrationButtonStart.Click += CalibrationButtonStart_Click;

            CalibrationBtnDone = FindViewById<Button>(Resource.Id.CalibrationBtnDone);
            CalibrationBtnDone.Click += CalibrationBtnDone_Click;

            CalibrationBtnCancel = FindViewById<Button>(Resource.Id.CalibrationBtnCancel);
            CalibrationBtnCancel.Click += CalibrationBtnCancel_Click;

            CalibrationBtnBack = FindViewById<Button>(Resource.Id.CalibrationBtnBack);
            CalibrationBtnBack.Click += CalibrationBtnBack_Click;

            CalibrationButton1 = FindViewById<Button>(Resource.Id.CalibrationBtn1);
            CalibrationButton1.Click += CalibrationButton1_Click;
            CalibrationButton2 = FindViewById<Button>(Resource.Id.CalibrationBtn2);
            CalibrationButton2.Click += CalibrationButton2_Click;
            CalibrationButton3 = FindViewById<Button>(Resource.Id.CalibrationBtn3);
            CalibrationButton3.Click += CalibrationButton3_Click;
            CalibrationButton4 = FindViewById<Button>(Resource.Id.CalibrationBtn4);
            CalibrationButton4.Click += CalibrationButton4_Click;
            CalibrationButton5 = FindViewById<Button>(Resource.Id.CalibrationBtn5);
            CalibrationButton5.Click += CalibrationButton5_Click;
            CalibrationButton6 = FindViewById<Button>(Resource.Id.CalibrationBtn6);
            CalibrationButton6.Click += CalibrationButton6_Click;

            CalibrationText1 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText1);
            CalibrationText2 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText2);
            CalibrationText3 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText3);
            CalibrationText4 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText4);
            CalibrationText5 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText5);
            CalibrationText6 = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.CalibrationText6);

            CalibrationText1New = FindViewById<TextView>(Resource.Id.CalibrationText1New);
            CalibrationText2New = FindViewById<TextView>(Resource.Id.CalibrationText2New);
            CalibrationText3New = FindViewById<TextView>(Resource.Id.CalibrationText3New);
            CalibrationText4New = FindViewById<TextView>(Resource.Id.CalibrationText4New);
            CalibrationText5New = FindViewById<TextView>(Resource.Id.CalibrationText5New);
            CalibrationText6New = FindViewById<TextView>(Resource.Id.CalibrationText6New);
        }



        /// <summary>
        /// 设置校准值
        /// </summary>
        /// <param name="SetValue"></param>
        /// <returns></returns>
        private double GeneratePpmCalibration(int SetValue)
        {
            DateTime now = DateTime.Now;
            int num = 5000;
            TimeSpan timeSpan;
            do
            {
                Task.Delay(250).Wait();
                timeSpan = DateTime.Now - now;
                App.phxDeviceService.GetStatus();
            }
            while (timeSpan.TotalMilliseconds < (double)num);
            App.phxDeviceService.Phx21.GeneratePpmCalibration(SetValue);
            //DateTime now2 = DateTime.Now;
            //int num2 = 3000;
            //TimeSpan timeSpan2;
            //double currentPpm = 0;
            //do
            //{
            //    if (this.Ppm != "N/A")
            //    {
            //        var ppm = double.Parse(this.Ppm);
            //        if (ppm > currentPpm)
            //        {
            //            currentPpm = ppm;
            //        }
            //    }
            //    Task.Delay(250).Wait();
            //    timeSpan2 = DateTime.Now - now2;
            //}
            //while (timeSpan2.TotalMilliseconds < (double)num2);
            //return currentPpm;
            return App.phxDeviceService.GetStatus().Ppm;
        }

        /// <summary>
        /// 校准1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton1_Click(object sender, EventArgs e)
        {
            this.CalibrationButton1.Enabled = false;
            this.CalibrationButton1.Text = "校准1中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        App.phxDeviceService.Phx21.SetPpmCalibration(j, 0, 0, 0, false);
                    }
                }
                string value = this.CalibrationText1.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton2.Enabled = true;
                    this.CalibrationButton1.Text = "校准1";
                    this.CalibrationText1New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }

        private void AddCalibrationList(string value, double newValue)
        {
            AndroidCalibrationLog log = new AndroidCalibrationLog()
            {
                ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),
                CalibrationID = cali.ID,
                TheoryValue = double.Parse(value),
                RealityValue = newValue,
                Deviation = Math.Abs(double.Parse(value) - newValue) / (value == "0" ? 100D : double.Parse(value)),
                LogTime = DateTime.Now,
                ReactionTime = 5,
                IsPass = 1
            };
            logList.Add(log);
        }

        /// <summary>
        /// 校准2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton2_Click(object sender, EventArgs e)
        {
            this.CalibrationButton2.Enabled = false;
            this.CalibrationButton2.Text = "校准2中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                string value = this.CalibrationText2.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton3.Enabled = true;
                    this.CalibrationButton2.Text = "校准2";
                    this.CalibrationText2New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }

        /// <summary>
        /// 校准3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton3_Click(object sender, EventArgs e)
        {
            this.CalibrationButton3.Enabled = false;
            this.CalibrationButton3.Text = "校准3中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                string value = this.CalibrationText3.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton4.Enabled = true;
                    this.CalibrationButton3.Text = "校准3";
                    this.CalibrationText3New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }
        /// <summary>
        /// 校准4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton4_Click(object sender, EventArgs e)
        {
            this.CalibrationButton4.Enabled = false;
            this.CalibrationButton4.Text = "校准4中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                string value = this.CalibrationText4.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton5.Enabled = true;
                    this.CalibrationButton4.Text = "校准4";
                    this.CalibrationText4New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }
        /// <summary>
        /// 校准5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton5_Click(object sender, EventArgs e)
        {
            this.CalibrationButton5.Enabled = false;
            this.CalibrationButton5.Text = "校准5中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                string value = this.CalibrationText5.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton6.Enabled = true;
                    this.CalibrationButton5.Text = "校准5";
                    this.CalibrationText5New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }
        /// <summary>
        /// 校准6
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButton6_Click(object sender, EventArgs e)
        {
            this.CalibrationButton6.Enabled = false;
            this.CalibrationButton6.Text = "校准6中";
            this.CalibrationBtnCancel.Enabled = false;
            this.CalibrationBtnDone.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                string value = this.CalibrationText6.Text.Trim();
                double newValue = GeneratePpmCalibration(int.Parse(value));
                base.RunOnUiThread(delegate
                {
                    this.CalibrationButton6.Text = "校准6";
                    this.CalibrationText6New.Text = newValue.ToString();
                    this.CalibrationBtnCancel.Enabled = true;
                    this.CalibrationBtnDone.Enabled = true;
                });
                AddCalibrationList(value, newValue);
            });
        }


        /// <summary>
        /// 获取校准按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationButtonStart_Click(object sender, EventArgs e)
        {
            if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
            {
                Toast.MakeText(this, "请先连接检测设备", ToastLength.Short).Show();
                return;
            }
            App.phxDeviceService.GetStatus();
            if (App.phxDeviceService.IsRunning)
            {
                this.CalibrationButtonStart.Enabled = false;
                this.CalibrationButtonStart.Text = "获取校准中";
                Task.Factory.StartNew(() =>
                {
                    GetPhxPPm();
                });
                //App.phxDeviceService.Phx21.DataPolled -= Phx21OnDataPolled;
                //App.phxDeviceService.Phx21.DataPolled += Phx21OnDataPolled;
            }
            else
            {
                Toast.MakeText(this, "点火成功后才可以校准", ToastLength.Short).Show();
            }
        }
        string Ppm;
        private void Phx21OnDataPolled(object sender, DataPolledEventArgs dataPolledEventArgs)
        {
            lock (Ppm)
            {
                Ppm = dataPolledEventArgs.Status.PpmStr;
            }
        }

        /// <summary>
        /// 获取设备校准
        /// </summary>
        private void GetPhxPPm()
        {
            List<PpmCalibrationInfo> savedPpms = new List<PpmCalibrationInfo>();
            List<int> ppms = new List<int>();
            for (int i = 0; i < 6; i++)
            {
                ppms.Add(0);
                PpmCalibrationInfo ppmCalibrationInfo = App.phxDeviceService.Phx21.GetPpmCalibration(i);
                if (ppmCalibrationInfo.IsValid)
                    savedPpms.Add(ppmCalibrationInfo);
            }
            savedPpms = savedPpms.OrderBy(p => p.Ppm).ToList();
            for (int i = 0; i < savedPpms.Count; i++)
            {
                ppms[i] = savedPpms[i].Ppm;
            }
            base.RunOnUiThread(delegate
            {
                this.CalibrationText1.Text = ppms[0].ToString();
                this.CalibrationText2.Text = ppms[1].ToString();
                this.CalibrationText3.Text = ppms[2].ToString();
                this.CalibrationText4.Text = ppms[3].ToString();
                this.CalibrationText5.Text = ppms[4].ToString();
                this.CalibrationText6.Text = ppms[5].ToString();

                this.CalibrationButton1.Enabled = true;
                this.CalibrationButton2.Enabled = false;
                this.CalibrationButton3.Enabled = false;
                this.CalibrationButton4.Enabled = false;
                this.CalibrationButton5.Enabled = false;
                this.CalibrationButton6.Enabled = false;
                this.CalibrationButtonStart.Enabled = true;
                this.CalibrationButtonStart.Text = "获取校准";
            });
        }

        /// <summary>
        /// 保存退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationBtnDone_Click(object sender, EventArgs e)
        {
            if (logList.Count < 3)
            {
                DialogService dialog = new DialogService();
                dialog.ShowYesNo(this, "保存并退出", "校准记录少于3条是否保存？", r =>
                {
                    if (r == DialogResult.Yes)
                    {
                        SaveAndExit();
                    }
                });
            }
            else
            {
                SaveAndExit();
            }
        }

        private void SaveAndExit()
        {
            //先保存再退出
            //创建SQLITE文件
            var sdCard = Android.OS.Environment.ExternalStorageDirectory;
            var logDirectory = new File(sdCard.AbsolutePath + "/LDARAPP6");
            if (!logDirectory.Exists())
            {
                logDirectory.Mkdirs();
            }
            SQLite.SQLiteConnection sqlite = CreateDatabase(logDirectory.AbsolutePath + "/sqliteSys.db");
            try
            {
                sqlite.BeginTransaction();
                cali.InsertTime = DateTime.Now;
                sqlite.Insert(cali);
                sqlite.InsertAll(logList);
                sqlite.Commit();
            }
            catch (Exception ex)
            {
                sqlite.Rollback();
                LogHelper.ErrorLog("保存校准记录", ex);
            }
            finally
            {
                sqlite.Close();
            }
            //退出
            Intent i = new Intent(this, typeof(MainActivity));
            SetResult(Result.Ok, i);
            Finish();
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="pathDatabase"></param>
        private SQLite.SQLiteConnection CreateDatabase(string pathDatabase)
        {
            var connection = new SQLite.SQLiteConnection(pathDatabase);
            connection.CreateTable<AndroidCalibration>();
            connection.CreateTable<AndroidCalibrationLog>();
            return connection;
        }

        /// <summary>
        /// 不保存退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationBtnCancel_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(MainActivity));
            SetResult(Result.Ok, i);
            Finish();
        }
        /// <summary>
        /// 返回上一级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CalibrationBtnBack_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(MainActivity));
            SetResult(Result.Canceled, i);
            Finish();
        }
    }
}