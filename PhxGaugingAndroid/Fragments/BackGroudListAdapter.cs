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
    /// 背景值记录适配器
    /// </summary>
    public class BackGroudListAdapter : BaseAdapter<AndroidBackground>
    {
        List<AndroidBackground> items;
        Activity context;
        public BackGroudListAdapter(Activity context, List<AndroidBackground> items)
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
                view = context.LayoutInflater.Inflate(Resource.Layout.BackGroudListItem, null);
            BackGroudListChildAdapter childAda = new BackGroudListChildAdapter(context, item.LogList);
            ListView listView = view.FindViewById<ListView>(Resource.Id.lvBackGroudLog);
            listView.Adapter = childAda;
            Utility.SetListViewHeightBasedOnChildren(listView);
            view.FindViewById<TextView>(Resource.Id.tvBGDeviceName).Text = item.Name;
            view.FindViewById<TextView>(Resource.Id.tvBGDeviceCode).Text = item.Code;
            view.FindViewById<TextView>(Resource.Id.tvBGCreateTime).Text = item.CreateTime.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGUser).Text = item.User;
            view.FindViewById<TextView>(Resource.Id.tvBGAvgValue).Text = item.AvgValue.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGTemplate).Text = item.Temperature.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGWindSpeed).Text = item.WindSpeed.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGHumidity).Text = item.Humidity.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGPress).Text = item.Atmos.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGWindDir).Text = item.WindDirection;
            return view;
        }
    }

    
}