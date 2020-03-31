using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using System;
using PhxGaugingAndroid.Entity;
using System.IO;
using Android.Content;
using System.Collections.Generic;
using System.Linq;
using PhxGaugingAndroid.Common;
using System.Threading.Tasks;
using Android.Runtime;
using Android.Graphics.Drawables;
using Plugin.TextToSpeech.Abstractions;
using Plugin.TextToSpeech;
using Android.Util;
using Android.Graphics;
using Newtonsoft.Json;

namespace PhxGaugingAndroid.Fragments
{
    public class ExepcSealPointFragment : Fragment
    {
        private AndroidGroup group;
        private List<AndroidGroup> groupList;
        private int position;
        public Bitmap btp;
        public ExepcSealPointFragment(List<AndroidGroup> groupList, int position)
        {
            this.groupList = groupList;
            this.position = position;
            this.group = groupList[position];
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        NoScrollListView lvSealPoints;
        TextView tvComplate;
        ClearnEditText etBackGroud;
        AndroidBackground backGroudValue;
        TextView tvSealPointPPM;
        TextView edtSealPointPPM;
        TextView edtSealPointTime;
        TextView tvSealPointPPMPID;
        TextView edtSealPointPPMPID;
        TextView edtSealPointTimePID;
        ImageView img;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.ExepcSealPoint, container, false);
            //缓存的rootView需要判断是否已经被加过parent， 如果有parent则从parent删除，防止发生这个rootview已经有parent的错误。
            ViewGroup mViewGroup = (ViewGroup)v.Parent;
            if (mViewGroup != null)
            {
                mViewGroup.RemoveView(v);
            }
            tvSealPointPPM = v.FindViewById<TextView>(Resource.Id.tvSealPointPPM);
            edtSealPointPPM = v.FindViewById<TextView>(Resource.Id.edtSealPointPPM);
            edtSealPointTime = v.FindViewById<TextView>(Resource.Id.edtSealPointTime);
            tvSealPointPPMPID = v.FindViewById<TextView>(Resource.Id.tvSealPointPPMPID);
            edtSealPointPPMPID = v.FindViewById<TextView>(Resource.Id.edtSealPointPPMPID);
            edtSealPointTimePID = v.FindViewById<TextView>(Resource.Id.edtSealPointTimePID);
            etBackGroud = v.FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.etSPBackGroud);
            etBackGroud.TextChanged -= EtBackGroud_TextChanged;
            etBackGroud.TextChanged += EtBackGroud_TextChanged;
            string value = UserPreferences.GetString("SelectBackGroud");
            if (value != null && value != string.Empty)
            {
                backGroudValue = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                etBackGroud.Text = backGroudValue.AvgValue.ToString();
            }
            v.FindViewById<TextView>(Resource.Id.tvSPDevice).Text = "装置:" + group.DeviceName;
            v.FindViewById<TextView>(Resource.Id.tvSPArea).Text = "区域:" + group.AreaName;
            v.FindViewById<TextView>(Resource.Id.tvSPGroupName).Text = "群组:" + group.GroupName;
            v.FindViewById<TextView>(Resource.Id.tvSPGroupCode).Text = "编码:" + group.GroupCode;
            v.FindViewById<TextView>(Resource.Id.tvPointInExcel).Text = "台账点数:" + group.SealPointCount.ToString();
            tvComplate = v.FindViewById<TextView>(Resource.Id.tvPointComplete);
            tvComplate.Text = "已检:" + group.CompleteCount.ToString();
            if (File.Exists(group.ImgPath) == false)
            {
                DirectoryInfo dir = new DirectoryInfo(group.ImgPath.Replace("/" + group.GroupCode + ".jpg", ""));
                if (dir.Exists)
                {
                    FileInfo[] files = dir.GetFiles(group.GroupCode + "*");
                    if (files.Length > 0)
                    {
                        group.ImgPath = files[0].FullName;
                    }
                }
            }
            img = v.FindViewById<ImageView>(Resource.Id.imgSP);
            if (File.Exists(group.ImgPath) == true)
            {
                img.Click -= Img_Click;
                img.Click += Img_Click;
                var exif = new Android.Media.ExifInterface(group.ImgPath);
                string comment = exif.GetAttribute(Android.Media.ExifInterface.TagUserComment);
                if (string.IsNullOrEmpty(comment) == false)
                {
                    try
                    {
                        var jsonStr = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(comment));
                        GroupStyle style = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupStyle>(jsonStr);
                        if (style != null)
                        {
                            v.FindViewById<TextView>(Resource.Id.tvPointInImg).Text = "图片点数" + style.markList.Count.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog("读取图片备注", ex);
                    }
                }
                string smallPath = group.ImgPath.ToLower().Replace("/img/", "/SmallImg/");
                if (File.Exists(smallPath))
                {
                    BitmapHelpers.SetImgBitMap(smallPath, img, btp);
                }
                else
                {
                    var bitmap = BitmapHelpers.LoadAndResizeBitmap(group.ImgPath, 200, 150);
                    img.SetImageBitmap(bitmap);
                }
            }
            else
            {
                var stream = context.Resources.OpenRawResource(Resource.Raw.NoPic);
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = false;
                options.InPreferredConfig = Bitmap.Config.Rgb565;
                options.InPurgeable = true;
                options.InInputShareable = true;
                options.InSampleSize = 1;
                btp = BitmapFactory.DecodeStream(stream, null, options);
                img.SetImageBitmap(btp);
                stream.Close();
            }
            lvSealPoints = v.FindViewById<NoScrollListView>(Resource.Id.lvSealPoints);
            SetSealPointListAdapter();
            RegisterForContextMenu(lvSealPoints);
            Button btn = v.FindViewById<Button>(Resource.Id.btnGetBackGroud);
            btn.Click -= BtnGetBackGroud_Click;
            btn.Click += BtnGetBackGroud_Click;
            var next = v.FindViewById<Button>(Resource.Id.btnNextGroup);
            next.Click -= Next_Click;
            next.Click += Next_Click;
            return v;
        }

        private void Next_Click(object sender, EventArgs e)
        {
            var adapter = lvSealPoints.Adapter as SealPointAdapter;
            if (adapter.IsDataPolling == true)
            {
                Toast.MakeText(this.Activity, "请先停止检测", ToastLength.Short).Show();
                return;
            }
            position += 1;
            if (position >= groupList.Count)
            {
                return;
            }
            group = groupList[position];

            this.View.FindViewById<TextView>(Resource.Id.tvSPDevice).Text = "装置:" + group.DeviceName;
            this.View.FindViewById<TextView>(Resource.Id.tvSPArea).Text = "区域:" + group.AreaName;
            this.View.FindViewById<TextView>(Resource.Id.tvSPGroupName).Text = "群组:" + group.GroupName;
            this.View.FindViewById<TextView>(Resource.Id.tvSPGroupCode).Text = "编码:" + group.GroupCode;
            this.View.FindViewById<TextView>(Resource.Id.tvPointInExcel).Text = "台账点数" + group.SealPointCount.ToString();
            tvComplate = this.View.FindViewById<TextView>(Resource.Id.tvPointComplete);
            tvComplate.Text = "已检:" + group.CompleteCount.ToString();
            var drawble = (BitmapDrawable)img.Drawable;
            if (drawble != null)
            {
                img.SetBackgroundResource(0);
                drawble.SetCallback(null);
                drawble.Bitmap.Recycle();
            }
            if (btp != null)
            {
                btp.Recycle();
            }
            if (File.Exists(group.ImgPath) == false)
            {
                DirectoryInfo dir = new DirectoryInfo(group.ImgPath.Replace("/" + group.GroupCode + ".jpg", ""));
                if (dir.Exists)
                {
                    FileInfo[] files = dir.GetFiles(group.GroupCode + "*");
                    if (files.Length > 0)
                    {
                        group.ImgPath = files[0].FullName;
                    }
                }
            }
            if (File.Exists(group.ImgPath) == true)
            {
                img.Click -= Img_Click;
                img.Click += Img_Click;
                var exif = new Android.Media.ExifInterface(group.ImgPath);
                string comment = exif.GetAttribute(Android.Media.ExifInterface.TagUserComment);
                if (string.IsNullOrEmpty(comment) == false)
                {
                    try
                    {
                        var jsonStr = System.Text.Encoding.GetEncoding("utf-8").GetString(Convert.FromBase64String(comment));
                        GroupStyle style = Newtonsoft.Json.JsonConvert.DeserializeObject<GroupStyle>(jsonStr);
                        if (style != null)
                        {
                            this.View.FindViewById<TextView>(Resource.Id.tvPointInImg).Text = "图片点数" + style.markList.Count.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog("读取图片备注", ex);
                    }
                }
                string smallPath = group.ImgPath.ToLower().Replace("/img/", "/SmallImg/");
                if (File.Exists(smallPath))
                {
                    BitmapHelpers.SetImgBitMap(smallPath, img, btp);
                }
                else
                {
                    var bitmap = BitmapHelpers.LoadAndResizeBitmap(group.ImgPath, 200, 150);
                    img.SetImageBitmap(bitmap);
                }
            }
            else
            {
                var stream = context.Resources.OpenRawResource(Resource.Raw.NoPic);
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InJustDecodeBounds = false;
                options.InPreferredConfig = Bitmap.Config.Rgb565;
                options.InPurgeable = true;
                options.InInputShareable = true;
                options.InSampleSize = 1;
                btp = BitmapFactory.DecodeStream(stream, null, options);
                img.SetImageBitmap(btp);
                stream.Close();
            }
            SetSealPointListAdapter();
            RegisterForContextMenu(lvSealPoints);
        }

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
            Intent i = new Intent(this.Activity, typeof(SelectBackGroudActivity));
            Bundle b = new Bundle();
            if (backGroudValue != null)
            {
                b.PutString("SelectValue", Newtonsoft.Json.JsonConvert.SerializeObject(backGroudValue));
                i.PutExtras(b);
            }
            this.StartActivityForResult(i, 121);
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
        /// 创建菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="v"></param>
        /// <param name="menuInfo"></param>
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.lvSealPoints)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle("密封点" + sealPointList[info.Position].ExtCode.ToString());
                if (sealPointList[info.Position].IsTouch.ToString() == "可达")
                {
                    menu.Add(Menu.None, 0, 0, "转换为不可达密封点");
                }
                else
                {
                    menu.Add(Menu.None, 0, 0, "转换为可达密封点");
                    menu.Add(Menu.None, 1, 0, "设置检测时间");
                }
            }
        }
        /// <summary>
        /// 菜单事件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            int firstVisiblePosition = lvSealPoints.FirstVisiblePosition; //屏幕内当前可以看见的第一条数据
            var view = lvSealPoints.GetChildAt(info.Position - firstVisiblePosition);
            Button btn = view.FindViewById<Button>(Resource.Id.btnPointTest);
            TextView tvIstouch = view.FindViewById<TextView>(Resource.Id.tvPointIsTouch);
            TextView tvPPM = view.FindViewById<TextView>(Resource.Id.tvPointValue);
            TextView tvWasteTime = view.FindViewById<TextView>(Resource.Id.tvPointTime);

            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);

            sealPointList = GetSealPointList();
            AndroidSealPoint point = sealPointList[info.Position];
            if (item.ToString() == "转换为不可达密封点")
            {
                point.IsTouch = "不可达";
                point.LeakagePPM = null;
                point.PPM = null;
                point.WasteTime = null;
                point.StartTime = null;
                point.EndTime = null;
                point.DetectionTime = null;
                point.UserName = null;
                this.Activity.RunOnUiThread(delegate
                {
                    tvPPM.Text = "";
                    tvWasteTime.Text = "";
                    btn.Visibility = ViewStates.Gone;
                    tvIstouch.Visibility = ViewStates.Visible;
                    // tvComplate.Text = "已检:" + sealPointList.FindAll(c => c.StartTime != null).Count.ToString();
                });
            }
            else if (item.ToString() == "转换为可达密封点")
            {
                point.IsTouch = "可达";
                this.Activity.RunOnUiThread(delegate
                {
                    btn.Visibility = ViewStates.Visible;
                    tvIstouch.Visibility = ViewStates.Gone;
                });
            }
            else if (item.ToString() == "设置检测时间")
            {
                point.StartTime = DateTime.Now;
                point.DetectionTime = DateTime.Now;
                point.UserName = JsonModel.UserName;
                if (backGroudValue != null)
                {
                    point.BackgroundPPM = backGroudValue.AvgValue;
                    point.Temperature = backGroudValue.Temperature == null ? "" : backGroudValue.Temperature.ToString();
                    point.WindDirection = backGroudValue.WindDirection ?? "";
                    point.WindSpeed = backGroudValue.WindSpeed == null ? "" : backGroudValue.WindSpeed.ToString();
                }
            }
            if (point.StartTime != null)// || point.IsTouch == "不可达")
            {
                view.Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
            }
            else
            {
                view.Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
            }
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(group.DataPath);
                connection.Update(point);
                group.UnReachCount = sealPointList.FindAll(c => c.IsTouch == "不可达").Count;
                group.CompleteCount = sealPointList.FindAll(c => c.StartTime != null).Count;
                //if (group.CompleteCount == (group.SealPointCount - group.UnReachCount))
                if (group.CompleteCount == group.SealPointCount)
                {
                    group.IsComplete = 1;
                }
                connection.Update(group);

                connection.Execute("UPDATE AndroidWorkOrder set UnReachCount = ( SELECT SUM(UnReachCount) FROM AndroidGroup )");
                connection.Execute("UPDATE AndroidWorkOrder set CompleteCount = ( SELECT SUM(CompleteCount) FROM AndroidGroup )");
                var pointAdap = adapter as ExepcSealPointAdapter;
                sealPointList[info.Position] = point;
                pointAdap.items[info.Position] = point;
                this.Activity.RunOnUiThread(delegate
                {
                    tvComplate.Text = "已检:" + group.CompleteCount.ToString();
                });
                return true;
            }
            catch (Exception ex)
            {
                Toast.MakeText(context, "发生错误：" + ex.Message, ToastLength.Short).Show();
                LogHelper.ErrorLog("转换密封点是否可达", ex);
                return false;
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        ExepcSealPointAdapter adapter;
        List<AndroidSealPoint> sealPointList = null;

        private List<AndroidSealPoint> GetSealPointList()
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(group.DataPath);
                sealPointList = connection.Table<AndroidSealPoint>().Where(c => c.GroupID == group.ID).ToList();
                return sealPointList;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("获取密封点数据列表", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
            return new List<AndroidSealPoint>();
        }
        private void SetSealPointListAdapter()
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(group.DataPath);
                sealPointList = connection.Table<AndroidSealPoint>().Where(c => c.GroupID == group.ID).ToList().OrderBy(c => Convert.ToInt32(c.ExtCode.Substring(0, (c.ExtCode.Length - 1)))).ToList();
                adapter = new ExepcSealPointAdapter(this.Activity, sealPointList, group);
                lvSealPoints.Adapter = adapter;
                adapter.SetComplateCount += Adapter_SetComplateCount;
                adapter.DoReceiveData += Adapter_DoReceiveData1;
                adapter.RefreshTime += Adapter_RefreshTime;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this.Activity, "发生错误：" + ex.Message, ToastLength.Short).Show();
                LogHelper.ErrorLog("初始化密封点数据列表", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }

        private void Adapter_DoReceiveData1(float fidppm, float fidmax, float pidppm, float pidmax)
        {
            tvSealPointPPM.Text = fidppm.ToString("F0");
            edtSealPointPPM.Text = fidmax.ToString("F0");
            tvSealPointPPMPID.Text = pidppm.ToString("F0");
            edtSealPointPPMPID.Text = pidmax.ToString("F0");
        }

        private void Adapter_RefreshTime(int time)
        {
            edtSealPointTime.Text = time.ToString("F0");
            edtSealPointTimePID.Text = time.ToString("F0");
        }

        private void Adapter_SetComplateCount(AndroidSealPoint sealPoint, int count, int? position)
        {
            context.RunOnUiThread(delegate
            {
                tvComplate.Text = "已检:" + count.ToString();
            });
            if (position == null)
            {
                return;
            }
            lvSealPoints.IsScroll = false;
            sealPointList[(int)position] = sealPoint;
            do
            {
                position += 1;
                if (position < sealPointList.Count)
                {
                    context.RunOnUiThread(delegate
                    {
                        lvSealPoints.SmoothScrollToPosition((int)position);
                    });
                    //if (sealPointList[(int)position].IsTouch == "不可达")
                    //{
                    //    var code = sealPointList[(int)position].ExtCode;
                    //    int num = int.Parse(code.Substring(0, code.Length - 1));
                    //    CrossTextToSpeech.Current.Speak(num.ToString() + sealPointList[(int)position].SealPointType + "不可达密封点");
                    //}
                    //else if (sealPointList[(int)position].StartTime != null)
                    //{
                    //    var code = sealPointList[(int)position].ExtCode;
                    //    int num = int.Parse(code.Substring(0, code.Length - 1));
                    //    CrossTextToSpeech.Current.Speak(num.ToString() + sealPointList[(int)position].SealPointType + "已检测");
                    //}
                }
                else
                {
                    lvSealPoints.IsScroll = true;
                    return;
                }
            } while (sealPointList[(int)position].IsTouch == "不可达" || sealPointList[(int)position].StartTime != null);

            var adapter = lvSealPoints.Adapter as SealPointAdapter;
            if (adapter.IsContinue == true)
            {
                Task.Factory.StartNew(() =>
                {
                    var code = sealPointList[(int)position].ExtCode;
                    int num = int.Parse(code.Substring(0, code.Length - 1));
                    CrossTextToSpeech.Current.Speak(num.ToString() + sealPointList[(int)position].SealPointType + "开始检测");
                });
                Task.Factory.StartNew(() =>
                {
                    System.Threading.Thread.Sleep(adapter.IntervalTime * 1000);
                    int firstVisiblePosition = lvSealPoints.FirstVisiblePosition; //屏幕内当前可以看见的第一条数据
                    var view = lvSealPoints.GetChildAt((int)position - firstVisiblePosition);
                    Button btn = view.FindViewById<Button>(Resource.Id.btnPointTest);
                    context.RunOnUiThread(delegate
                    {
                        adapter.Btn_Click(btn, null);
                    });
                });
            }
            else
            {
                lvSealPoints.IsScroll = true;
            }
        }

        private void Img_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this.Activity, typeof(PhotoViewActivity));
            Bundle b = new Bundle();
            b.PutString("AndroidGroup", Newtonsoft.Json.JsonConvert.SerializeObject(group));
            i.PutExtras(b);
            this.Activity.StartActivity(i);
        }
    }
}