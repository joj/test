using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class GeneratePPMActivity : AppCompatActivity
    {
        Button etDate;
        Button etTime;
        ClearnEditText EditGPMax;
        ClearnEditText EditGPMin;
        ClearnEditText editGPStateMin;
        ClearnEditText editGPStateMax;
        AndroidGroup group;
        ClearnEditText etBackGroud;
        AndroidBackground backGroudValue;
        RadioGroup rdGenerateMode;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.GeneratePPM);
            string info = this.Intent.GetStringExtra("AndroidGroup");
            group = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidGroup>(info);
            etDate = FindViewById<Button>(Resource.Id.btnGPDate);
            etDate.Click -= EtDate_Click;
            etDate.Click += EtDate_Click;
            etDate.TextChanged -= EtDate_TextChanged;
            etDate.TextChanged += EtDate_TextChanged;
            etTime = FindViewById<Button>(Resource.Id.btnGPTime);
            etTime.Click -= EtTime_Click;
            etTime.Click += EtTime_Click;
            etTime.TextChanged -= EtTime_TextChanged;
            etTime.TextChanged += EtTime_TextChanged;
            EditGPMax = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.EditGPMax);
            EditGPMin = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.EditGPMin);
            editGPStateMin = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editGPStateMin);
            editGPStateMax = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.editGPStateMax);
            rdGenerateMode = FindViewById<RadioGroup>(Resource.Id.rdGenerateMode);
            string values = UserPreferences.GetString(group.WorkOrderID + "GeneratePPM");
            if (values != null && values != string.Empty)
            {
                string[] param = values.Split(',');
                DateTime dt = DateTime.Parse(param[0] + " " + param[1]);
                if (dt > DateTime.Now)
                {
                    etDate.Text = param[0];
                    etTime.Text = param[1].Substring(0, 5);
                }
                else
                {
                    etDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                    etTime.Text = DateTime.Now.ToString("HH:mm");
                }
                EditGPMax.Text = param[2];
                EditGPMin.Text = param[3];
                editGPStateMin.Text = param[4];
                editGPStateMax.Text = param[5];
                if (param.Count() > 6)
                {
                    if (param[6] == "0")
                    {
                        etDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                        etTime.Text = DateTime.Now.ToString("HH:mm");
                        rdGenerateMode.Check(Resource.Id.rbModeWaite);
                    }
                    else
                    {
                        rdGenerateMode.Check(Resource.Id.rbModeNoWaite);
                    }
                }
                else
                {
                    rdGenerateMode.Check(Resource.Id.rbModeWaite);
                }
            }
            else
            {
                rdGenerateMode.Check(Resource.Id.rbModeWaite);
                etDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                etTime.Text = DateTime.Now.ToString("HH:mm");
            }
            Button cancel = FindViewById<Button>(Resource.Id.btnGPCancel);
            cancel.Click -= Cancel_Click;
            cancel.Click += Cancel_Click;
            Button ok = FindViewById<Button>(Resource.Id.btnGPOk);
            ok.Click -= OK_Click;
            ok.Click += OK_Click;
            etBackGroud = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.etGPBackGroud);
            etBackGroud.RequestFocus();
            etBackGroud.TextChanged -= EtBackGroud_TextChanged;
            etBackGroud.TextChanged += EtBackGroud_TextChanged;
            string value = UserPreferences.GetString("SelectBackGroud");
            if (value != null && value != string.Empty)
            {
                backGroudValue = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                etBackGroud.Text = backGroudValue.AvgValue.ToString();
            }
            Button btn = FindViewById<Button>(Resource.Id.btnGPBackGroud);
            btn.Click -= BtnGetBackGroud_Click;
            btn.Click += BtnGetBackGroud_Click;
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.GPtoolbar);
            if (toolbar != null)
            {
                toolbar.Title = group.GroupName;
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                //SaveParam();
                Finish();
            };
        }

        private void EtTime_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (int.Parse(etTime.Text.Substring(0, 2)) < 8 || int.Parse(etTime.Text.Substring(0, 2)) > 17)
            {
                Toast.MakeText(this, "选择的时间不属于工作时间", ToastLength.Short).Show();
            }
        }

        private void EtDate_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                int count = 0;
                var list = WorkOrderFragment.GetWorkOrderList();
                foreach (var item in list)
                {
                    SQLite.SQLiteConnection connection = null;
                    try
                    {
                        connection = new SQLite.SQLiteConnection(item.DataPath);
                        var c = connection.Query<SealPointCount>("SELECT COUNT(*) AS PointCount FROM ANDROIDSEALPOINT WHERE date(STARTTIME) = date('" + etDate.Text + "')");
                        count += int.Parse(c[0].PointCount);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog("批量检测生成", ex);
                    }
                    finally
                    {
                        if (connection != null)
                        {
                            connection.Dispose();
                        }
                    }
                }
                if (count > 1000)
                {
                    RunOnUiThread(() =>
                    {
                        DisplayMetrics dm = new DisplayMetrics();
                        this.WindowManager.DefaultDisplay.GetMetrics(dm);
                        int height = dm.HeightPixels;
                        Toast toast = Toast.MakeText(this, etDate.Text + "检测密封点数已达到" + count.ToString() + "个", ToastLength.Long);
                        toast.SetGravity(GravityFlags.Top, 0, height / 4);
                        toast.Show();
                    });
                }
            });
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == 121 && resultCode == Result.Ok)
            {
                string value = data.GetStringExtra("SelectValue");
                if (value != null && value != string.Empty)
                {
                    backGroudValue = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                    UserPreferences.SetString("SelectBackGroud", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
                    etBackGroud.Text = backGroudValue.AvgValue.ToString();
                }
                else
                {
                    backGroudValue = null;
                    etBackGroud.Text = string.Empty;
                    UserPreferences.DeleteString("SelectBackGroud");
                }
            }
        }
        /// <summary>
        /// 生成检测值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OK_Click(object sender, EventArgs e)
        {
            StringBuilder msg = new StringBuilder();
            if (etDate.Text == string.Empty)
            {
                msg.Append("日期");
            }
            if (etTime.Text == string.Empty)
            {
                if (msg.Length > 0)
                {
                    msg.Append("、");
                }
                msg.Append("时间");
            }
            if (EditGPMax.Text == string.Empty)
            {
                if (msg.Length > 0)
                {
                    msg.Append("、");
                }
                msg.Append("检测值范围");
            }
            if (EditGPMin.Text == string.Empty)
            {
                if (msg.Length > 0)
                {
                    msg.Append("、");
                }
                msg.Append("检测值范围");
            }
            if (editGPStateMax.Text == string.Empty)
            {
                if (msg.Length > 0)
                {
                    msg.Append("、");
                }
                msg.Append("停留时间");
            }
            if (editGPStateMin.Text == string.Empty)
            {
                if (msg.Length > 0)
                {
                    msg.Append("、");
                }
                msg.Append("停留时间");
            }
            if (msg.Length > 0)
            {
                msg.Append("不能为空！");
            }
            if (EditGPMax.Text != string.Empty && EditGPMin.Text != string.Empty)
            {
                if (int.Parse(EditGPMax.Text) <= int.Parse(EditGPMin.Text))
                {
                    msg.Append("检测值的范围必须从小到大！");
                }
                if (int.Parse(EditGPMin.Text) < 0)
                {
                    msg.Append("检测值的范围必须大于等于0！");
                }
                if (int.Parse(EditGPMax.Text) > 499)
                {
                    msg.Append("检测值的范围必须小于500！");
                }
            }
            if (editGPStateMax.Text != string.Empty && editGPStateMin.Text != string.Empty)
            {
                if (int.Parse(editGPStateMax.Text) <= int.Parse(editGPStateMin.Text))
                {
                    msg.Append("停留时间必须从小到大！");
                }
            }
            if (msg.Length > 0)
            {
                Toast.MakeText(this, msg.ToString(), ToastLength.Short).Show();
                return;
            }
            List<AndroidSealPoint> sealPointList = GetSealPointList();
            Android.App.ProgressDialog dilog = new Android.App.ProgressDialog(this);
            dilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
            dilog.Indeterminate = false;
            dilog.SetCancelable(false);
            dilog.SetCanceledOnTouchOutside(false);
            dilog.Max = 100;
            dilog.SetTitle("提示");
            dilog.SetMessage("生成中请不要进行任何操作");
            dilog.Show();
            Task.Factory.StartNew(() =>
            {
                SQLite.SQLiteConnection connection = null;
                try
                {
                    connection = new SQLite.SQLiteConnection(group.DataPath);
                    DateTime startTime = DateTime.Parse(etDate.Text + " " + etTime.Text);
                    int PPMMax = int.Parse(EditGPMax.Text);
                    int PPMMin = int.Parse(EditGPMin.Text);
                    int SecondMax = int.Parse(editGPStateMax.Text);
                    int SecondMin = int.Parse(editGPStateMin.Text);
                    Random rd = new Random();
                    int totalSecond = 0;
                    string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
                    var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                    foreach (var point in sealPointList)
                    {
                        if (point.StartTime == null)
                        {
                            point.StartTime = startTime;
                            int second = rd.Next(SecondMin, SecondMax);
                            point.WasteTime = second;
                            point.EndTime = startTime.AddSeconds(second);
                            totalSecond += second;
                            point.DetectionTime = point.EndTime;
                            int ppm = rd.Next(PPMMin, PPMMax);
                            if (ppm < 10)
                            {
                                point.LeakagePPM = double.Parse(ppm.ToString() + "." + rd.Next(1, 9).ToString());
                            }
                            else
                            {
                                point.LeakagePPM = ppm;
                            }
                            point.BackgroundPPM = backGroudValue.AvgValue;
                            point.Temperature = backGroudValue.Temperature == null ? "" : backGroudValue.Temperature.ToString();
                            point.WindDirection = backGroudValue.WindDirection;
                            point.WindSpeed = backGroudValue.WindSpeed == null ? "" : backGroudValue.WindSpeed.ToString();
                            point.PPM = point.LeakagePPM - point.BackgroundPPM;
                            if (App.phxDeviceService != null)
                            {
                                point.PhxCode = App.phxDeviceService.Name;
                            }
                            point.PhxName = "氢火焰离子化检测器";
                            point.UserName = JsonModel.UserName;
                            int interval = rd.Next(2, 5);
                            startTime = ((DateTime)point.EndTime).AddSeconds(interval);
                            totalSecond += interval;
                        }
                    }
                    startTime = startTime.AddMinutes(rd.Next(1, 3));
                    connection.UpdateAll(sealPointList);
                    group.CompleteCount = sealPointList.FindAll(c => c.StartTime != null).Count;
                    //if (group.CompleteCount == (group.SealPointCount - group.UnReachCount))
                    if (group.CompleteCount == group.SealPointCount)
                    {
                        group.IsComplete = 1;
                    }
                    connection.Update(group);
                    connection.Execute("UPDATE AndroidWorkOrder set CompleteCount = ( SELECT SUM(CompleteCount) FROM AndroidGroup )");
                    string parem = startTime.ToString("yyyy-MM-dd") + "," + startTime.ToString("HH:mm") + ":" + rd.Next(10, 58).ToString() + "," + EditGPMax.Text + "," + EditGPMin.Text + "," + editGPStateMin.Text + "," + editGPStateMax.Text + "," + (rdGenerateMode.CheckedRadioButtonId == Resource.Id.rbModeWaite ? "0" : "1");
                    UserPreferences.SetString(group.WorkOrderID + "GeneratePPM", parem);
                    if (rdGenerateMode.CheckedRadioButtonId == Resource.Id.rbModeWaite)
                    {
                        int progress = totalSecond * 10;
                        for (int j = 1; j <= 100; j++)
                        {
                            Task.Delay(progress).Wait();
                            dilog.Progress = j;
                        }
                    }

                    Intent i = new Intent(this, typeof(GroupFragment));
                    i.PutExtra("AndroidGroup", Newtonsoft.Json.JsonConvert.SerializeObject(group));
                    SetResult(Result.Ok, i);
                    Finish();
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("生成密封点检测信息", ex);
                    Toast.MakeText(this, ex.Message, ToastLength.Short);
                }
                finally
                {
                    if (connection != null)
                    {
                        connection.Dispose();
                    }
                    dilog.Dismiss();
                }
            });

        }

        private List<AndroidSealPoint> GetSealPointList()
        {
            SQLite.SQLiteConnection connection = null;
            List<AndroidSealPoint> list = new List<AndroidSealPoint>();
            try
            {
                connection = new SQLite.SQLiteConnection(group.DataPath);
                list = connection.Table<AndroidSealPoint>().Where(c => c.GroupID == group.ID).ToList();
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("生成检测值时获取密封点数据列表", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
            return list;
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            //SaveParam();
            Finish();
        }

        private void EtTime_Click(object sender, EventArgs e)
        {
            if (etTime.Text != string.Empty)
            {
                string[] param = etTime.Text.Split(':');
                var dialog = new TimePickerDialog(this, OnTimeSet, int.Parse(param[0]), int.Parse(param[1]), true);
                dialog.Show();
            }
            else
            {
                var dialog = new TimePickerDialog(this, OnTimeSet, DateTime.Now.Hour, DateTime.Now.Minute, true);
                dialog.Show();
            }
        }

        private void OnTimeSet(object sender, TimePickerDialog.TimeSetEventArgs e)
        {
            etTime.Text = string.Format("{0:D2}", e.HourOfDay) + ":" + string.Format("{0:D2}", e.Minute);
        }

        private void EtDate_Click(object sender, EventArgs e)
        {
            if (etDate.Text != string.Empty)
            {
                string[] param = etDate.Text.Split('-');
                var dialog = new DatePickerDialog(this, OnDateSet, int.Parse(param[0]), int.Parse(param[1]) - 1, int.Parse(param[2]));
                dialog.Show();
            }
            else
            {
                var dialog = new DatePickerDialog(this, OnDateSet, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                dialog.Show();
            }
        }
        /// <summary>
        /// 日期控件回调赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDateSet(object sender, DatePickerDialog.DateSetEventArgs e)
        {
            etDate.Text = e.Date.ToString("yyyy-MM-dd");
        }
        //只在生成的时候才保存
        //void SaveParam()
        //{
        //    string parem = etDate.Text + "," + etTime.Text + "," + EditGPMax.Text + "," + EditGPMin.Text + "," + editGPStateMin.Text + "," + editGPStateMax.Text + "," + (rdGenerateMode.CheckedRadioButtonId == Resource.Id.rbModeWaite ? "0" : "1");
        //    UserPreferences.SetString(group.WorkOrderID + "GeneratePPM", parem);
        //}
        /// <summary>
        /// 背景值改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EtBackGroud_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (etBackGroud.Text != string.Empty)
            {
                if (backGroudValue == null)
                {
                    backGroudValue = new AndroidBackground();
                }
                backGroudValue.AvgValue = double.Parse(etBackGroud.Text);
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
                UserPreferences.SetString("SelectBackGroud", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
            }
        }

        /// <summary>
        /// 选择背景值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGetBackGroud_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(SelectBackGroudActivity));
            Bundle b = new Bundle();
            if (backGroudValue != null)
            {
                b.PutString("SelectValue", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
                i.PutExtras(b);
            }
            this.StartActivityForResult(i, 121);
        }
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                //SaveParam();
            }
            return base.OnKeyDown(keyCode, e);
        }
    }
}