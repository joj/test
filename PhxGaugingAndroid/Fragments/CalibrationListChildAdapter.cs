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
    /// 校准记录明细适配器
    /// </summary>
    class CalibrationListChildAdapter : BaseAdapter<AndroidCalibrationLog>
    {
        List<AndroidCalibrationLog> items;
        Activity context;
        public CalibrationListChildAdapter(Activity context, List<AndroidCalibrationLog> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidCalibrationLog this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.CalibrationListItemChild, null);
            view.FindViewById<TextView>(Resource.Id.tvCaTheoryValue).Text = item.TheoryValue.ToString();
            view.FindViewById<TextView>(Resource.Id.tvCaRealityValue).Text = item.RealityValue.ToString();
            view.FindViewById<TextView>(Resource.Id.tvCaDeviation).Text = item.Deviation.ToString();
            view.FindViewById<TextView>(Resource.Id.tvCaLogTime).Text = item.LogTime.ToString();
            return view;
        }
    }
}