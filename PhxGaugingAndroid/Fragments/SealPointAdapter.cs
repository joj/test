using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGauging.Common.Devices;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    public class SealPointAdapter : BaseAdapter<AndroidSealPoint>
    {
        public delegate void EventComplatePoint(AndroidSealPoint seapoint, int count, int? position);
        public event EventComplatePoint SetComplateCount;
        public delegate void EventReceiveData(double ppm, double max);
        public event EventReceiveData DoReceiveData;
        public delegate void EventRefreshTime(int time);
        public event EventRefreshTime RefreshTime;
        public List<AndroidSealPoint> items;
        AndroidGroup group;
        Activity context;
        Timer timer;
        AndroidBackground backGroud;
        bool IsAutoStay;
        int StayTime;
        public bool IsContinue;
        public int IntervalTime;
        private SoundPool soundPool;
        NoScrollListView lv;


        public SealPointAdapter(Activity contex, List<AndroidSealPoint> items, AndroidGroup group)
        {
            this.group = group;
            this.context = contex;
            this.items = items;
            if (timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed += Timer_Elapsed;
                IsDataPolling = false;
            }
            string value = UserPreferences.GetString("StayTime");
            if (value != null && value != string.Empty)
            {
                string[] param = value.Split(',');
                IsAutoStay = bool.Parse(param[0]);
                if (param[1].Trim() != string.Empty)
                {
                    StayTime = int.Parse(param[1]);
                }
                else
                {
                    StayTime = 5;
                }
                if (param.Length == 4)
                {
                    IsContinue = bool.Parse(param[2]);
                    if (param[3].Trim() != string.Empty)
                    {
                        IntervalTime = int.Parse(param[3]);
                    }
                    else
                    {
                        IntervalTime = 2;
                    }
                }
                else
                {
                    IsContinue = false;
                    IntervalTime = 2;
                }

            }
            soundPool = new SoundPool(1, Stream.System, 5);
            soundPool.Load(contex, Resource.Raw.FadeOut, 1);
            soundPool.Load(contex, Resource.Raw.Start, 1);
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override AndroidSealPoint this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null)
                view = context.LayoutInflater.Inflate(Resource.Layout.SealPointItem, null);
            lv = parent as NoScrollListView;
            view.FindViewById<TextView>(Resource.Id.tvPointNum).Text = item.ExtCode;
            view.FindViewById<TextView>(Resource.Id.tvPointType).Text = item.SealPointType;
            TextView tvPPM = view.FindViewById<TextView>(Resource.Id.tvPointValue);
            tvPPM.Text = item.LeakagePPM.ToString();
            TextView tvWasteTime = view.FindViewById<TextView>(Resource.Id.tvPointTime);
            if (item.WasteTime != null)
            {
                tvWasteTime.Text = item.WasteTime.ToString();
            }
            TextView tvFlashPPM = view.FindViewById<TextView>(Resource.Id.tvFlashPPM);
            Button btn = view.FindViewById<Button>(Resource.Id.btnPointTest);
            btn.Click -= Btn_Click;
            btn.Click += Btn_Click;
            btn.SetTag(Resource.Id.btnPointTest, position);
            TextView tvIstouch = view.FindViewById<TextView>(Resource.Id.tvPointIsTouch);
            if (item.IsTouch == "不可达")
            {
                btn.Visibility = ViewStates.Gone;
                tvIstouch.Visibility = ViewStates.Visible;
            }
            else
            {
                btn.Visibility = ViewStates.Visible;
                tvIstouch.Visibility = ViewStates.Gone;
            }
            if (item.StartTime != null)// || item.IsTouch == "不可达")
            {
                view.FindViewById<LinearLayout>(Resource.Id.linerSealPoint).Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
            }
            else
            {
                view.FindViewById<LinearLayout>(Resource.Id.linerSealPoint).Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
            }
            return view;
        }

        public bool IsDataPolling = false;
        int? currentPositon;
        TextView tvPPM;
        TextView tvFlashPPM;
        TextView tvWasteTime;
        Button RunningBtn;
        public void Btn_Click(object sender, EventArgs e)
        {
            if (IsDataPolling == false)
            {
                if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
                {
                    Toast.MakeText(context, "请先连接检测设备", ToastLength.Short).Show();
                    return;
                }
                if (App.phxDeviceService.IsRunning == false)
                {
                    Toast.MakeText(context, "请先将检测设备点火", ToastLength.Short).Show();
                    return;
                }
                string value = UserPreferences.GetString("SelectBackGroud");
                if (value != null && value != string.Empty)
                {
                    backGroud = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                }
                else
                {
                    backGroud = null;
                }
                if (backGroud == null || backGroud.AvgValue == null)
                {
                    Toast.MakeText(context, "环境背景值不能为空", ToastLength.Short).Show();
                    return;
                }
            }
            Button btn = sender as Button;
            if (btn.Text == "开始" && IsDataPolling == true)
            {
                return;
            }
            LinearLayout layout = btn.Parent as LinearLayout;
            tvPPM = layout.FindViewById<TextView>(Resource.Id.tvPointValue);
            tvFlashPPM = layout.FindViewById<TextView>(Resource.Id.tvFlashPPM);
            tvWasteTime = layout.FindViewById<TextView>(Resource.Id.tvPointTime);

            currentPositon = int.Parse(btn.GetTag(Resource.Id.btnPointTest).ToString());
            IsDataPolling = !IsDataPolling;
            if (IsDataPolling)
            {
                lv.IsScroll = false;
                soundPool.Play(1, 1, 1, 0, 0, 1);
                string value = UserPreferences.GetString("StayTime");
                if (value != null && value != string.Empty)
                {
                    string[] param = value.Split(',');
                    IsAutoStay = bool.Parse(param[0]);
                    if (param[1].Trim() != string.Empty)
                    {
                        StayTime = int.Parse(param[1]);
                    }
                    else
                    {
                        StayTime = 5;
                    }
                    IsContinue = bool.Parse(param[2]);
                    if (param[3].Trim() != string.Empty)
                    {
                        IntervalTime = int.Parse(param[3]);
                    }
                    else
                    {
                        IntervalTime = 3;
                    }
                }
                btn.Text = "停止";
                tvPPM.Text = "";
                tvFlashPPM.Text = "";
                tvWasteTime.Text = "0";
                tvFlashPPM.Visibility = ViewStates.Visible;
                App.phxDeviceService.Phx21.DataPolled -= this.Phx21OnDataPolled;
                App.phxDeviceService.Phx21.DataPolled += this.Phx21OnDataPolled;
                start = DateTime.Now;
                ppm = 0;
                RunningBtn = btn;
                timer.Start();
            }
            else
            {
                soundPool.Play(2, 1, 1, 0, 0, 1);
                btn.Text = "开始";
                timer.Stop();
                App.phxDeviceService.Phx21.DataPolled -= this.Phx21OnDataPolled;
                SQLite.SQLiteConnection connection = null;
                try
                {
                    string values = UserPreferences.GetString(group.WorkOrderID + "GeneratePPM");
                    if (values != null && values != string.Empty)
                    {
                        string[] param = values.Split(',');
                        DateTime dt = DateTime.Parse(param[0] + " " + param[1]);
                        if (param.Count() > 6)
                        {
                            if (param[6] == "1" && DateTime.Now < dt)
                            {
                                start = dt;
                            }
                        }
                    }
                    connection = new SQLite.SQLiteConnection(group.DataPath);
                    AndroidSealPoint point = items[(int)currentPositon];
                    point.StartTime = start;
                    point.EndTime = start.AddSeconds(second);
                    point.WasteTime = second;
                    point.DetectionTime = point.EndTime;
                    point.LeakagePPM = ppm;
                    point.BackgroundPPM = backGroud.AvgValue;
                    point.Temperature = backGroud.Temperature == null ? "" : backGroud.Temperature.ToString();
                    point.WindDirection = backGroud.WindDirection;
                    point.WindSpeed = backGroud.WindSpeed == null ? "" : backGroud.WindSpeed.ToString();
                    if (point.LeakagePPM >= 50000)
                    {
                        point.PPM = 50000;
                    }
                    else
                    {
                        point.PPM = point.LeakagePPM - point.BackgroundPPM;
                    }
                    if (point.PPM < 0)
                    {
                        point.PPM = 0;
                    }
                    if (App.phxDeviceService != null)
                    {
                        point.PhxCode = App.phxDeviceService.Name;
                    }
                    point.PhxName = "氢火焰离子化检测器";
                    string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
                    var JsonModel = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidUser>(json);
                    point.UserName = JsonModel.UserName;
                    items[(int)currentPositon] = point;
                    connection.Update(point);
                    group.CompleteCount = items.FindAll(c => c.StartTime != null).Count;
                    //if (group.CompleteCount == (group.SealPointCount - group.UnReachCount))
                    if (group.CompleteCount == group.SealPointCount)
                    {
                        group.IsComplete = 1;
                    }
                    connection.Update(group);
                    connection.Execute("UPDATE AndroidWorkOrder set CompleteCount = ( SELECT SUM(CompleteCount) FROM AndroidGroup )");
                    second = 0;
                    if (point.StartTime != null)// || point.IsTouch == "不可达")
                    {
                        layout.Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
                    }
                    else
                    {
                        layout.Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
                    }
                    tvFlashPPM.Text = "";
                    tvFlashPPM.Visibility = ViewStates.Invisible;
                    lv.IsScroll = true;
                    if (values != null && values != string.Empty)
                    {
                        string[] param = values.Split(',');
                        DateTime dt = DateTime.Parse(param[0] + " " + param[1]);
                        if (param.Count() > 6)
                        {
                            if (param[6] == "1")
                            {
                                Random rd = new Random();
                                dt = point.EndTime.Value.AddSeconds(rd.Next(2, 5));
                                param[0] = dt.ToString("yyyy-MM-dd");
                                param[1] = dt.ToString("HH:mm:ss");
                                string strParam = string.Empty;
                                for (int i = 0; i < param.Count(); i++)
                                {
                                    if (i != 0)
                                    {
                                        strParam += ",";
                                    }
                                    strParam += param[i];
                                }
                                UserPreferences.SetString(group.WorkOrderID + "GeneratePPM", strParam);
                            }
                        }
                    }
                    if (e == null)
                    {
                        SetComplateCount(point, (int)group.CompleteCount, currentPositon);
                    }
                    else
                    {
                        SetComplateCount(point, (int)group.CompleteCount, null);
                    }
                }
                catch (Exception ex)
                {
                    Toast.MakeText(context, "发生错误：" + ex.Message, ToastLength.Short).Show();
                    LogHelper.ErrorLog("更新密封点检测信息", ex);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Dispose();
                    }
                }

            }
        }

        double ppm;
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
            if (context != null)
            {
                context.RunOnUiThread(delegate
                {
                    tvPPM.Text = ppm.ToString();
                    tvFlashPPM.Text = dataPolledEventArgs.Status.PpmStr;
                    DoReceiveData(double.Parse(dataPolledEventArgs.Status.PpmStr), ppm);
                });
            }
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
                    tvWasteTime.Text = second.ToString();
                    RefreshTime(second);
                });

            }
            if (IsAutoStay == true && second == StayTime)
            {
                context.RunOnUiThread(delegate
                {
                    Btn_Click(RunningBtn, null);
                });
            }
        }
    }
}