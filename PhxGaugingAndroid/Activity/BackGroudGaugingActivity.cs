using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Newtonsoft.Json;
using PhxGauging.Common.Devices;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;
using Plugin.TextToSpeech;
using static Android.Views.View;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class BackGroudGaugingActivity : Activity
    {
        ClearnEditText bgTextDeviceName;
        ClearnEditText bgTextDeviceCode;
        ClearnEditText bgTextTemperature;
        ClearnEditText bgTextHumidity;
        ClearnEditText bgTextAtmos;
        ClearnEditText bgTextWindDirection;
        ClearnEditText bgTextWindSpeed;
        ClearnEditText bgTextPhxName;
        ClearnEditText bgTextPhxCode;
        ClearnEditText bgTextUser;
        RadioGroup radioLocation;
        TextView bgTextPPM;
        TextView bgTextTime;
        TextView bgPPM;
        ListView ListViewBGPPM;
        Button btnStartBGGau;
        Timer timer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BackGroudGauging);
            bgTextDeviceName = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextDeviceName);
            bgTextDeviceCode = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextDeviceCode);
            bgTextTemperature = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextTemperature);
            bgTextHumidity = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextHumidity);
            bgTextAtmos = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextAtmos);
            bgTextWindDirection = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextWindDirection);
            bgTextWindSpeed = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextWindSpeed);
            bgTextPhxName = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextPhxName);
            bgTextPhxCode = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextPhxCode);
            if (App.phxDeviceService != null)
            {
                bgTextPhxCode.Text = App.phxDeviceService.Name;
            }
            bgTextUser = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.bgTextUser);
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            bgTextUser.Text = JsonModel.UserName;
            radioLocation = FindViewById<RadioGroup>(Resource.Id.radioLocation);
            bgTextPPM = FindViewById<TextView>(Resource.Id.bgTextPPM);
            bgTextTime = FindViewById<TextView>(Resource.Id.bgTextTime);
            bgPPM = FindViewById<TextView>(Resource.Id.bgPPM);
            ListViewBGPPM = FindViewById<ListView>(Resource.Id.ListViewBGPPM);
            btnStartBGGau = FindViewById<Button>(Resource.Id.btnStartBGGau);
            btnStartBGGau.Click += BtnStartBGGau_Click;
            if (timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed += Timer_Elapsed;
                IsGauging = false;
            }
            RegisterForContextMenu(ListViewBGPPM);
            Button btnBGCancel = FindViewById<Button>(Resource.Id.btnBGCancel);
            btnBGCancel.Click += BtnBGCancel_Click;
            Button btnBGSave = FindViewById<Button>(Resource.Id.btnBGSave);
            btnBGSave.Click += BtnBGSave_Click;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBGSave_Click(object sender, EventArgs e)
        {
            StringBuilder error = new StringBuilder();
            if (bgTextDeviceName.Text.Trim() == string.Empty)
            {
                error.AppendLine("装置名称不能为空");
            }
            if (bgTextDeviceCode.Text.Trim() == string.Empty)
            {
                error.AppendLine("装置编码不能为空");
            }
            if (bgTextTemperature.Text.Trim() == string.Empty)
            {
                error.AppendLine("温度不能为空");
            }
            if (bgTextHumidity.Text.Trim() == string.Empty)
            {
                error.AppendLine("湿度不能为空");
            }
            if (bgTextAtmos.Text.Trim() == string.Empty)
            {
                error.AppendLine("大气压不能为空");
            }
            if (bgTextWindDirection.Text.Trim() == string.Empty)
            {
                error.AppendLine("风向不能为空");
            }
            if (bgTextWindSpeed.Text.Trim() == string.Empty)
            {
                error.AppendLine("风速不能为空");
            }
            if (bgTextPhxName.Text.Trim() == string.Empty)
            {
                error.AppendLine("检测仪器名称不能为空");
            }
            if (bgTextPhxCode.Text.Trim() == string.Empty)
            {
                error.AppendLine("仪器序列号不能为空");
            }
            if (error.Length > 0)
            {
                Toast.MakeText(this, error.ToString(), ToastLength.Short).Show();
                return;
            }
            if (logList.Count < 5)
            {
                Toast.MakeText(this, "检测明细必须够5条才可以保存", ToastLength.Short).Show();
                return;
            }
            AndroidBackground bg = new AndroidBackground
            {
                ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),
                Name = bgTextDeviceName.Text.Trim(),
                Code = bgTextDeviceCode.Text.Trim(),
                CreateTime = DateTime.Now,
                AvgValue = logList.Average(c => c.DetectionValue),
                Temperature = bgTextTemperature.Text.Trim() != string.Empty ? double.Parse(bgTextTemperature.Text.Trim()) : double.NaN,
                Humidity = bgTextHumidity.Text.Trim() != string.Empty ? double.Parse(bgTextHumidity.Text.Trim()) : double.NaN,
                Atmos = bgTextAtmos.Text.Trim() != string.Empty ? double.Parse(bgTextAtmos.Text.Trim()) : double.NaN,
                WindDirection = bgTextWindDirection.Text.Trim(),
                WindSpeed = bgTextWindSpeed.Text.Trim() != string.Empty ? double.Parse(bgTextWindSpeed.Text.Trim()) : double.NaN,
                PhxName = bgTextPhxName.Text.Trim(),
                PhxCode = bgTextPhxCode.Text.Trim(),
                User = bgTextUser.Text.Trim()
            };
            logList.ForEach(c => c.BackgroundID = bg.ID);

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
                sqlite.Insert(bg);
                sqlite.InsertAll(logList);
                sqlite.Commit();
            }
            catch (Exception ex)
            {
                sqlite.Rollback();
                LogHelper.ErrorLog("保存环境背景值检测记录", ex);
                Toast.MakeText(this, "保存失败，" + ex.Message, ToastLength.Short).Show();
            }
            finally
            {
                sqlite.Close();
            }
            Toast.MakeText(this, "保存成功", ToastLength.Short).Show();
            //退出
            Finish();
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="pathDatabase"></param>
        private SQLite.SQLiteConnection CreateDatabase(string pathDatabase)
        {
            var connection = new SQLite.SQLiteConnection(pathDatabase);
            connection.CreateTable<AndroidBackground>();
            connection.CreateTable<AndroidBackgroundLog>();
            return connection;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBGCancel_Click(object sender, EventArgs e)
        {
            Finish();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.ListViewBGPPM)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle(logList[info.Position].EndTime.ToString() + "的检测记录");
                string[] menuItems = new string[1] { "移除" };
                for (var i = 0; i < menuItems.Length; i++)
                    menu.Add(Menu.None, i, i, menuItems[i]);
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var menuItemIndex = item.ItemId;
            //var menuItems = new string[1] { "移除" };
            //var menuItemName = menuItems[menuItemIndex];
            string select = logList[info.Position].EndTime.ToString();
            logList.RemoveAt(info.Position);
            list.RemoveAt(info.Position);
            var adapter = new SimpleAdapter(this, list, Resource.Layout.BackGroudGaugingItem, new string[]
                {
                    "itemPosition",
                    "itemPPM",
                    "itemTime"
                }, new int[]
                {
                    Resource.Id.tvLocation,
                    Resource.Id.tvBGPPM,
                    Resource.Id.tvBGTime
                });
            this.ListViewBGPPM.Adapter = adapter;
            Toast.MakeText(this, "移除" + select + "的检测记录", ToastLength.Short).Show();
            return true;
        }

        bool IsGauging;
        DateTime start;
        int second;
        /// <summary>
        /// 检测计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            second += 1;
            if (this != null)
            {
                this.RunOnUiThread(delegate { bgTextTime.Text = second.ToString(); });
            }
        }

        List<IDictionary<string, object>> list;
        /// <summary>
        /// 开始 停止检测
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStartBGGau_Click(object sender, EventArgs e)
        {
            string location = string.Empty;
            int id = radioLocation.CheckedRadioButtonId;
            if (id != -1)
            {
                location = FindViewById<RadioButton>(id).Text;
            }
            if (IsGauging == false)
            {
                if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
                {
                    Toast.MakeText(this, "请先连接检测设备", ToastLength.Short).Show();
                    return;
                }
                if (App.phxDeviceService.IsRunning == false)
                {
                    Toast.MakeText(this, "请先将检测设备点火", ToastLength.Short).Show();
                    return;
                }
                if (location == string.Empty)
                {
                    Toast.MakeText(this, "请先选定检测位置", ToastLength.Short).Show();
                    return;
                }
            }
            IsGauging = !IsGauging;
            Task.Factory.StartNew(() =>
            {
                if (IsGauging == false)
                {
                    CrossTextToSpeech.Current.Speak("停止");
                }
                else
                {
                    CrossTextToSpeech.Current.Speak("开始");
                }
            });
            if (IsGauging)
            {
                App.phxDeviceService.Phx21.DataPolled -= this.Phx21OnDataPolled;
                App.phxDeviceService.Phx21.DataPolled += this.Phx21OnDataPolled;
                start = DateTime.Now;
                ppm = 0;
                bgTextPPM.Text = string.Empty;
                bgPPM.Text = String.Empty;
                second = 0;
                bgTextTime.Text = second.ToString();
                btnStartBGGau.Text = "停止";
                timer.Start();
            }
            else
            {
                btnStartBGGau.Text = "开始";
                App.phxDeviceService.Phx21.DataPolled -= this.Phx21OnDataPolled;
                timer.Stop();
                //加入检测队列
                AndroidBackgroundLog log = new AndroidBackgroundLog
                {
                    ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),
                    StartTime = start,
                    EndTime = start.AddMilliseconds(second * 1000),
                    WasteTime = second,
                    DetectionValue = ppm,
                    Position = location
                };
                logList.Add(log);

                radioLocation.ClearCheck();
                FindViewById<RadioButton>(id).Enabled = false;
                //加入
                list = new List<IDictionary<string, object>>();
                foreach (var item in logList)
                {
                    list.Add(new JavaDictionary<string, object>
                    {
                        {
                            "itemPosition",
                            item.Position
                        },
                          {
                            "itemPPM",
                            item.DetectionValue
                        },
                        {
                            "itemTime",
                            item.EndTime
                        }
                    });
                }
                var adapter = new SimpleAdapter(this, list, Resource.Layout.BackGroudGaugingItem, new string[]
                {
                    "itemPosition",
                    "itemPPM",
                    "itemTime"
                }, new int[]
                {
                    Resource.Id.tvLocation,
                    Resource.Id.tvBGPPM,
                    Resource.Id.tvBGTime
                });
                this.ListViewBGPPM.Adapter = adapter;
            }
            bgTextUser.RequestFocus();
        }
        //背景值检测记录
        List<AndroidBackgroundLog> logList = new List<AndroidBackgroundLog>();



        double ppm;
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dataPolledEventArgs"></param>
        private void Phx21OnDataPolled(object sender, DataPolledEventArgs dataPolledEventArgs)
        {
            if (dataPolledEventArgs.Status.PpmStr == "N/A")
            {
                return;
            }
            if (double.Parse(dataPolledEventArgs.Status.PpmStr) > ppm)
            {
                ppm = dataPolledEventArgs.Status.Ppm;
            }
            if (this != null)
            {
                this.RunOnUiThread(delegate
                {
                    bgTextPPM.Text = ppm.ToString();
                    bgPPM.Text = dataPolledEventArgs.Status.PpmStr;
                });
            }
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