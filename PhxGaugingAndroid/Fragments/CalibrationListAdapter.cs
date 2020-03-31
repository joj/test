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
using Java.Lang;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    /// <summary>
    /// 校准记录适配器
    /// </summary>
    public class CalibrationListAdapter : BaseAdapter<AndroidCalibration>
    {
        List<AndroidCalibration> items;
        Activity context;
        public CalibrationListAdapter(Activity context, List<AndroidCalibration> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidCalibration this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.CalibrationListItem, null);
            CalibrationListChildAdapter childAda = new CalibrationListChildAdapter(context, item.LogList);
            ListView listView = view.FindViewById<ListView>(Resource.Id.lvCalibrationLog);
            listView.Adapter = childAda;
            Utility.SetListViewHeightBasedOnChildren(listView);
            view.FindViewById<TextView>(Resource.Id.tvCaDeviceName).Text = item.DeviceName;
            view.FindViewById<TextView>(Resource.Id.tvCaDeviceCode).Text = item.DeviceCode;
            view.FindViewById<TextView>(Resource.Id.tvCaUser).Text = item.User;
            view.FindViewById<TextView>(Resource.Id.tvCaConfirm).Text = item.Confirm;
            view.FindViewById<TextView>(Resource.Id.tvCaGasName).Text = item.GasName;
            return view;
        }
    }


}