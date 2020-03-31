using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    /// <summary>
    /// 工单适配器
    /// </summary>
    public class WorkOrderAdapter : BaseAdapter<AndroidWorkOrder>
    {
        List<AndroidWorkOrder> items;
        Activity context;
        public WorkOrderAdapter(Activity contex, List<AndroidWorkOrder> items)
        {
            this.context = contex;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override AndroidWorkOrder this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.WorkOrderItem, null);
            //if (item.CompleteCount == (item.SealPointCount - item.UnReachCount))
            if (item.CompleteCount == item.SealPointCount)
            {
                view.FindViewById<LinearLayout>(Resource.Id.linearWorkOrder).Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
            }
            else
            {
                view.FindViewById<LinearLayout>(Resource.Id.linearWorkOrder).Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
            }
            view.FindViewById<TextView>(Resource.Id.tvOrderName).Text = item.WorkOrderName;
            string tName = "";
            if(item.WorkOrderType == 0)
            {
                tName = "检测工单";
            }
            else if (item.WorkOrderType == 1)
            {
                tName = "复检工单";
            }
            else
            {
                tName = "抽检工单";
            }
            view.FindViewById<TextView>(Resource.Id.tvOrderType).Text = tName;
            ////view.FindViewById<TextView>(Resource.Id.tvOrderType).Text = item.WorkOrderType == 0 ? "检测工单" : "复检工单";
            //view.FindViewById<TextView>(Resource.Id.tvOrderProgress).Text = "检测进度:" + item.CompleteCount + "/" + (item.SealPointCount - item.UnReachCount).ToString();
            view.FindViewById<TextView>(Resource.Id.tvOrderProgress).Text = "检测进度:" + item.CompleteCount + "/" + item.SealPointCount.ToString();

            view.FindViewById<TextView>(Resource.Id.tvOrderTime).Text = item.OperateTime.ToString();
            view.FindViewById<TextView>(Resource.Id.tvCreateType).Text = string.IsNullOrEmpty(item.ApiUrl) ? "导入" : "下载";
            view.FindViewById<TextView>(Resource.Id.tvUploadTime).Text = item.UploadTime == null ? "" : "(已上传)";
            var before = view.FindViewById<TextView>(Resource.Id.tvDateBefore);
            before.Click -= DateBefore_Click;
            before.Click += DateBefore_Click;
            before.SetTag(Resource.Id.tvDateBefore, position);
            var after = view.FindViewById<TextView>(Resource.Id.tvDateAfter);
            after.Click -= DateAfter_Click;
            after.Click += DateAfter_Click;
            after.SetTag(Resource.Id.tvDateAfter, position);
            view.FindViewById<TextView>(Resource.Id.tvGaugingCount).Text = item.DetectionDate.ToString("yyyy-MM-dd") + "当天已检密封点" + item.DetectionCount.ToString() + "个";
            if (item.DetectionDate.Date == DateTime.Now.Date)
            {
                after.Enabled = false;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#B6B6B6"));
            }
            else
            {
                after.Enabled = true;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#536FDC"));
            }
            return view;
        }

        private void DateAfter_Click(object sender, EventArgs e)
        {
            TextView btn = sender as TextView;
            LinearLayout layout = btn.Parent.Parent as LinearLayout;
            var currentPositon = int.Parse(btn.GetTag(Resource.Id.tvDateAfter).ToString());
            var DeTime = items[currentPositon].DetectionDate.AddDays(1);
            int count = 0;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(items[currentPositon].DataPath);
                var c = connection.Query<SealPointCount>("SELECT COUNT(*) AS PointCount FROM ANDROIDSEALPOINT WHERE date(STARTTIME) = date('" + DeTime.ToString("yyyy-MM-dd") + "')");
                count = int.Parse(c[0].PointCount);
                items[currentPositon].DetectionDate = DeTime;
                items[currentPositon].DetectionCount = count;
                layout.FindViewById<TextView>(Resource.Id.tvGaugingCount).Text = items[currentPositon].DetectionDate.ToString("yyyy-MM-dd") + "当天已检密封点" + items[currentPositon].DetectionCount.ToString() + "个";
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("工单后一天检测点数", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
            var after = layout.FindViewById<TextView>(Resource.Id.tvDateAfter);
            if (items[currentPositon].DetectionDate.Date == DateTime.Now.Date)
            {
                after.Enabled = false;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#B6B6B6"));
            }
            else
            {
                after.Enabled = true;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#536FDC"));
            }
        }

        private void DateBefore_Click(object sender, EventArgs e)
        {
            TextView btn = sender as TextView;
            LinearLayout layout = btn.Parent.Parent as LinearLayout;
            var currentPositon = int.Parse(btn.GetTag(Resource.Id.tvDateBefore).ToString());
            var DeTime = items[currentPositon].DetectionDate.AddDays(-1);
            int count = 0;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(items[currentPositon].DataPath);
                var c = connection.Query<SealPointCount>("SELECT COUNT(*) AS PointCount FROM ANDROIDSEALPOINT WHERE date(STARTTIME) = date('" + DeTime.ToString("yyyy-MM-dd") + "')");
                count = int.Parse(c[0].PointCount);
                items[currentPositon].DetectionDate = DeTime;
                items[currentPositon].DetectionCount = count;
                layout.FindViewById<TextView>(Resource.Id.tvGaugingCount).Text = items[currentPositon].DetectionDate.ToString("yyyy-MM-dd") + "当天已检密封点" + items[currentPositon].DetectionCount.ToString() + "个";
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("工单前一天检测点数", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
            var after = layout.FindViewById<TextView>(Resource.Id.tvDateAfter);
            if (items[currentPositon].DetectionDate.Date == DateTime.Now.Date)
            {
                after.Enabled = false;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#B6B6B6"));
            }
            else
            {
                after.Enabled = true;
                after.SetTextColor(Android.Graphics.Color.ParseColor("#536FDC"));
            }
        }
    }
}