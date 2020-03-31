using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    /// <summary>
    /// 密封点检测记录
    /// </summary>
    public class DetectionAdapter : BaseAdapter<AndroidSealPoint>
    {
        List<AndroidSealPoint> items;
        Activity context;
        public DetectionAdapter(Activity context, List<AndroidSealPoint> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidSealPoint this[int position]
        {
            get { return items[position]; }
        }
        public override long GetItemId(int position)
        {
            return position;
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
                view = context.LayoutInflater.Inflate(Resource.Layout.DetectionItem, null);
            view.FindViewById<TextView>(Resource.Id.tvDeGroup).Text = item.SealPointCode.Replace(item.ExtCode, "");
            view.FindViewById<TextView>(Resource.Id.tvDeSealpoint).Text = item.ExtCode;
            view.FindViewById<TextView>(Resource.Id.tvDeStart).Text = item.StartTime.ToString();
            view.FindViewById<TextView>(Resource.Id.tvDeEnd).Text = item.EndTime.ToString();
            view.FindViewById<TextView>(Resource.Id.tvDeStay).Text = item.WasteTime.Value.ToString();
            return view;
        }
    }
}