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
    class BackGroudListChildAdapter : BaseAdapter<AndroidBackgroundLog>
    {
        List<AndroidBackgroundLog> items;
        Activity context;

        public BackGroudListChildAdapter(Activity context, List<AndroidBackgroundLog> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidBackgroundLog this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.BackGroudGaugingItem, null);
            view.FindViewById<TextView>(Resource.Id.tvLocation).Text = item.Position;
            view.FindViewById<TextView>(Resource.Id.tvBGPPM).Text = item.DetectionValue.ToString();
            view.FindViewById<TextView>(Resource.Id.tvBGTime).Text = item.EndTime.ToString();
            return view;
        }
    }
}