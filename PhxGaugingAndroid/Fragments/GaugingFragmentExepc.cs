using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System;
using PhxGauging.Common.Devices;
using System.Timers;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Common;
using Java.IO;
using System.Collections.Generic;
using Android.Runtime;
using Android.Content;
using Newtonsoft.Json;
using Plugin.TextToSpeech;

namespace PhxGaugingAndroid.Fragments
{
    public class GaugingFragmentExepc : Fragment, StopReceiveData
    {
        private ClearnEditText editBackGroud;
        private ClearnEditText editBarCode;
        private ClearnEditText editGroupCode;
        private ClearnEditText editSealpointCode;
        private TextView editPPM;
        private TextView editTime;
        private TextView tvPPM;
        private TextView editPPMPID;
        private TextView editTimePID;
        private TextView tvPPMPID;
        private Button btnReStartGau;
        //0 重置模式 1 开始停止模式
        int OprateType;
        private CheckBox cbIsSave;
        bool IsSave;
        bool IsAuto;
        int StayTime;
        Timer timer;
        private ListView listViewPPM;
        AndroidBackground backGroudValue;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this.Activity;
        }
        Activity context;
        public static GaugingFragmentExepc NewInstance()
        {
            var frag3 = new GaugingFragmentExepc { Arguments = new Bundle() };
            return frag3;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.ExepcGauging, container, false);
            //缓存的rootView需要判断是否已经被加过parent， 如果有parent则从parent删除，防止发生这个rootview已经有parent的错误。
            ViewGroup mViewGroup = (ViewGroup)v.Parent;
            if (mViewGroup != null)
            {
                mViewGroup.RemoveView(v);
            }
            Button btnBarCode = v.FindViewById<Button>(Resource.Id.btnBarCode);
            btnBarCode.Click -= BtnBarCode_Click;
            btnBarCode.Click += BtnBarCode_Click;
            editBackGroud = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editBackGroud);
            editBackGroud.RequestFocus();
            editBackGroud.TextChanged -= EditBackGroud_TextChanged;
            editBackGroud.TextChanged += EditBackGroud_TextChanged;
            string value = UserPreferences.GetString("SelectBackGroud2");
            if (value != null && value != string.Empty)
            {
                backGroudValue = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                editBackGroud.Text = backGroudValue.AvgValue.ToString();
            }
            editBarCode = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editBarCode);
            editGroupCode = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editGroupCode);
            editGroupCode.RequestFocus();
            editSealpointCode = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editSealpointCode);
            btnReStartGau = v.FindViewById<Button>(Resource.Id.btnReStartGau);
            btnReStartGau.Click -= BtnReStartGau_Click;
            btnReStartGau.Click += BtnReStartGau_Click;
            editPPM = v.FindViewById<TextView>(Resource.Id.edtPPM);
            editTime = v.FindViewById<TextView>(Resource.Id.edtTime);
            tvPPM = v.FindViewById<TextView>(Resource.Id.tvGauPPM);
            editPPMPID = v.FindViewById<TextView>(Resource.Id.edtPPMPID);
            editTimePID = v.FindViewById<TextView>(Resource.Id.edtTimePID);
            tvPPMPID = v.FindViewById<TextView>(Resource.Id.tvGauPPMPID);
            listViewPPM = v.FindViewById<ListView>(Resource.Id.ListViewPPM);
            Button btnSelect = v.FindViewById<Button>(Resource.Id.btnGauSelect);
            btnSelect.Click -= BtnSelect_Click;
            btnSelect.Click += BtnSelect_Click;
            cbIsSave = v.FindViewById<CheckBox>(Resource.Id.cbIsSave);
            string isSave = UserPreferences.GetString("DetectionIsSave");
            if (isSave != null && isSave != string.Empty)
            {
                IsSave = bool.Parse(isSave);
            }
            else
            {
                IsSave = true;
            }
            cbIsSave.Checked = IsSave;
            cbIsSave.CheckedChange += CbIsSave_CheckedChange;
            string seting = UserPreferences.GetString("DetectionSetting");
            if (seting != null && seting != string.Empty)
            {
                string[] param = seting.Split(',');
                IsAuto = bool.Parse(param[0]);
                StayTime = int.Parse(param[1]);
            }
            else
            {
                IsAuto = false;
                StayTime = 0;
            }
            if (timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed -= Timer_Elapsed;
                timer.Elapsed += Timer_Elapsed;
            }
            IsGauging = false;
            if (IsSave == false)
            {
                OprateType = 0;
                btnReStartGau.Text = "重置";
                if (App.exepcService.isConnected == true)
                {
                    App.exepcService.DataPoll -= ExepcService_DataPoll;
                    App.exepcService.DataPoll += ExepcService_DataPoll;
                    start = DateTime.Now;
                    ppm = 0;
                    editPPM.Text = string.Empty;
                    second = 0;
                    editTime.Text = second.ToString();
                    timer.Start();
                    IsGauging = true;
                }
            }
            else
            {
                OprateType = 1;
                btnReStartGau.Text = "开始";
            }
            BindListView();
            return v;
        }


        private void CbIsSave_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            IsSave = cbIsSave.Checked;
            UserPreferences.SetString("DetectionIsSave", IsSave.ToString());
            if (IsSave == true)
            {
                OprateType = 1;
            }
            else
            {
                OprateType = 0;
            }
            if (OprateType == 0)
            {
                btnReStartGau.Text = "重置";
                if (App.exepcService.isConnected == true)
                {
                    App.exepcService.DataPoll -= ExepcService_DataPoll;
                    App.exepcService.DataPoll += ExepcService_DataPoll;
                    start = DateTime.Now;
                    ppm = 0;
                    editPPM.Text = string.Empty;
                    second = 0;
                    editTime.Text = second.ToString();
                    timer.Start();
                    IsGauging = true;
                }
            }
            else
            {
                btnReStartGau.Text = "开始";
                App.exepcService.DataPoll -= ExepcService_DataPoll;
                timer.Stop();
                IsGauging = false;
            }
        }

        public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 121 && resultCode == Result.Ok)
            {
                string value = data.GetStringExtra("SelectValue");
                if (value != null && value != string.Empty)
                {
                    backGroudValue = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                    UserPreferences.SetString("SelectBackGroud2", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
                    editBackGroud.Text = backGroudValue.AvgValue.ToString();
                }
                else
                {
                    backGroudValue = null;
                    editBackGroud.Text = string.Empty;
                    UserPreferences.DeleteString("SelectBackGroud2");
                }
            }
            else if (requestCode == 786 && resultCode == Result.Ok)
            {
                string value = data.GetStringExtra("ScanResult");
                if (value != null && value != string.Empty)
                {
                    editBarCode.Text = value;
                }
            }
        }

        private void EditBackGroud_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (editBackGroud.Text != string.Empty)
            {
                if (backGroudValue == null)
                {
                    backGroudValue = new AndroidBackground();
                }
                backGroudValue.AvgValue = double.Parse(editBackGroud.Text);
            }
            else
            {
                if (backGroudValue != null)
                {
                    backGroudValue.AvgValue = null;
                }
            }
            if (backGroudValue != null)
            {
                UserPreferences.SetString("SelectBackGroud2", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
            }
        }

        private void BtnSelect_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this.Activity, typeof(SelectBackGroudActivity));
            Bundle b = new Bundle();
            if (backGroudValue != null)
            {
                b.PutString("SelectValue", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
                i.PutExtras(b);
            }
            this.StartActivityForResult(i, 121);
        }

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
            if (context != null)
            {
                context.RunOnUiThread(delegate
                {
                    editTime.Text = second.ToString();
                    editTimePID.Text = second.ToString();
                });
            }
            if (OprateType == 1)
            {
                if (IsAuto == true && second == StayTime)
                {
                    context.RunOnUiThread(delegate
                    {
                        BtnReStartGau_Click(null, null);
                    });
                }
            }
        }
        /// <summary>
        /// 绑定检测结果数据
        /// </summary>
        private void BindListView()
        {
            var sdCard = Android.OS.Environment.ExternalStorageDirectory;
            var logDirectory = new File(sdCard.AbsolutePath + "/LDARAPP6");
            if (!logDirectory.Exists())
            {
                logDirectory.Mkdirs();
            }
            SQLite.SQLiteConnection sqlite = CreateDatabase(logDirectory.AbsolutePath + "/sqliteSys.db");
            List<AndroidRecord> recordList = sqlite.Query<AndroidRecord>("select * from AndroidRecord order by EndTime DESC");
            sqlite.Close();

            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            foreach (var item in recordList)
            {
                list.Add(new JavaDictionary<string, object>
                    {
                        {
                            "itemGroup",
                            item.GroupCode
                        },
                         {
                            "itemSealpoint",
                            item.SealPointSeq
                        },
                          {
                            "itemPPM",
                            item.PPM
                        },
                        {
                            "itemTime",
                            item.EndTime
                        }
                    });
            }
            var adapter = new SimpleAdapter(this.Activity, list, Resource.Layout.GaugingItem, new string[]
            {
                    "itemGroup",
                    "itemSealpoint",
                    "itemPPM",
                    "itemTime"
            }, new int[]
            {
                    Resource.Id.tvGroup,
                    Resource.Id.tvSealpoint,
                    Resource.Id.tvPPM,
                    Resource.Id.tvTime
            });
            this.listViewPPM.Adapter = adapter;
        }

        private bool IsGauging;
        /// <summary>
        /// 停止并保存按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReStartGau_Click(object sender, System.EventArgs e)
        {
            if (IsGauging == false)
            {
                if (App.exepcService.isConnected == false)
                {
                    Toast.MakeText(base.Activity, "请先连接WIFI", ToastLength.Short).Show();
                    return;
                }
                if (editBackGroud.Text.Trim() == string.Empty)
                {
                    Toast.MakeText(base.Activity, "环境背景值不能为空", ToastLength.Short).Show();
                    return;
                }
            }
            else
            {
                if (IsSave == true)
                {
                    if (App.exepcService.isConnected == false)
                    {
                        Toast.MakeText(base.Activity, "请先连接WIFI", ToastLength.Short).Show();
                        return;
                    }
                    if (editBackGroud.Text.Trim() == string.Empty)
                    {
                        Toast.MakeText(base.Activity, "环境背景值不能为空", ToastLength.Short).Show();
                        return;
                    }
                }
            }
            if (OprateType == 1)
            {
                IsGauging = !IsGauging;
            }
            Task.Factory.StartNew(() =>
            {
                if (OprateType == 1)
                {
                    if (IsGauging == false)
                    {
                        CrossTextToSpeech.Current.Speak("停止");
                    }
                    else
                    {
                        CrossTextToSpeech.Current.Speak("开始");
                    }
                }
                else
                {
                    CrossTextToSpeech.Current.Speak("重置");
                }
            });
            if (OprateType == 1)
            {
                if (IsGauging == true)
                {
                    btnReStartGau.Text = "停止";
                }
                else
                {
                    btnReStartGau.Text = "开始";
                }
            }
            if (((OprateType == 1 && IsGauging == false) || OprateType == 0) && IsSave == true)
            {
                App.exepcService.DataPoll -= ExepcService_DataPoll;
                timer.Stop();
                //保存数据
                string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                string model = UserPreferences.GetString("ExepcModel");
                AndroidRecord record = new AndroidRecord()
                {
                    ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),
                    GroupCode = editGroupCode.Text.Trim(),
                    SealPointSeq = editSealpointCode.Text.Trim(),
                    BarCode = editBarCode.Text.Trim(),
                    StartTime = start,
                    EndTime = start.AddSeconds(second),
                    WasteTime = second,
                    BackgroundPPM = editBackGroud.Text.Trim() == string.Empty ? double.NaN : double.Parse(editBackGroud.Text.Trim()),
                    Temperature = backGroudValue.Temperature.ToString(),
                    WindSpeed = backGroudValue.WindSpeed.ToString(),
                    WindDirection = backGroudValue.WindDirection,
                    PhxCode = App.phxDeviceService != null ? App.phxDeviceService.Name : null,
                    PhxName = "氢火焰离子化检测器",
                    LeakagePPM = ppm,
                    PIDLeakagePPM = ppmpid,
                    UserName = JsonModel.UserName,
                    DetectionMode = model
                };
                if (record.LeakagePPM >= 50000)
                {
                    record.PPM = 50000;
                }
                else
                {
                    record.PPM = (double)(record.LeakagePPM - record.BackgroundPPM);
                }
                if (record.PPM < 0)
                {
                    record.PPM = 0;
                }
                var sdCard = Android.OS.Environment.ExternalStorageDirectory;
                var logDirectory = new File(sdCard.AbsolutePath + "/LDARAPP6");
                if (!logDirectory.Exists())
                {
                    logDirectory.Mkdirs();
                }
                SQLite.SQLiteConnection sqlite = CreateDatabase(logDirectory.AbsolutePath + "/sqliteSys.db");
                sqlite.Insert(record);
                sqlite.Close();
                BindListView();
            }
            if (OprateType == 0 || IsGauging == true)
            {
                App.exepcService.DataPoll -= ExepcService_DataPoll;
                App.exepcService.DataPoll += ExepcService_DataPoll;
                start = DateTime.Now;
                ppm = 0;
                ppmpid = 0;
                editPPM.Text = string.Empty;
                editPPMPID.Text = string.Empty;
                second = 0;
                editTime.Text = second.ToString();
                editTimePID.Text = second.ToString();
                timer.Start();
            }
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="pathDatabase"></param>
        private SQLite.SQLiteConnection CreateDatabase(string pathDatabase)
        {
            var connection = new SQLite.SQLiteConnection(pathDatabase);
            connection.CreateTable<AndroidRecord>();
            return connection;
        }


        /// <summary>
        /// 扫描条形码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBarCode_Click(object sender, System.EventArgs e)
        {
            try
            {
                Intent i = new Intent(base.Activity, typeof(ScanBarCodeActivity));
                StartActivityForResult(i, 786);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("检测调用群组条形码扫描", ex);
            }
        }

        float ppm;
        float ppmpid;
        private void ExepcService_DataPoll(float FID, float PID)
        {
            if (FID > ppm)
            {
                ppm = FID;
            }
            if (PID > ppmpid)
            {
                ppmpid = PID;
            }
            if (this.Activity != null)
            {
                this.Activity.RunOnUiThread(delegate
                {
                    editPPM.Text = ppm.ToString("F0");
                    tvPPM.Text = FID.ToString("F0");
                    editPPMPID.Text = ppmpid.ToString("F0");
                    tvPPMPID.Text = PID.ToString("F0");
                });
            }
        }

        public void StopReceiveData()
        {
            if (App.phxDeviceService != null && App.phxDeviceService.Phx21 != null)
            {
                App.exepcService.DataPoll -= ExepcService_DataPoll;
            }
            if (timer != null)
            {
                timer.Stop();
            }
            IsGauging = false;
        }
    }
}