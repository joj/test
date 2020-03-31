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
    /// 选择背景值记录适配器
    /// </summary>
    public class SelectBackGroudAdapter : BaseAdapter<AndroidBackground>
    {
        List<AndroidBackground> items;
        Activity context;
        public SelectBackGroudAdapter(Activity context, List<AndroidBackground> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidBackground this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.SelectBackGroudItem, null);
            view.FindViewById<TextView>(Resource.Id.tvSeBGDeviceName).Text = item.Name;
            view.FindViewById<TextView>(Resource.Id.tvSeBGDeviceCode).Text = item.Code;
            view.FindViewById<TextView>(Resource.Id.tvSeBGCreateTime).Text = item.CreateTime.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGUser).Text = item.User;
            view.FindViewById<TextView>(Resource.Id.tvSeBGAvgValue).Text = item.AvgValue.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGTemplate).Text = item.Temperature.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGWindSpeed).Text = item.WindSpeed.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGHumidity).Text = item.Humidity.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGPress).Text = item.Atmos.ToString();
            view.FindViewById<TextView>(Resource.Id.tvSeBGWindDir).Text = item.WindDirection;
            return view;
        }
    }


}