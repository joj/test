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
    public class GaugingListAdapter : BaseAdapter<AndroidRecord>
    {
        List<AndroidRecord> items;
        Activity context;
        public GaugingListAdapter(Activity context, List<AndroidRecord> items)
        {
            this.context = context;
            this.items = items;
        }
        public override AndroidRecord this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.GaugingListItem, null);
            view.FindViewById<TextView>(Resource.Id.tvGauGroup).Text = item.GroupCode;
            view.FindViewById<TextView>(Resource.Id.tvGauSealpoint).Text = item.SealPointSeq;
            view.FindViewById<TextView>(Resource.Id.tvGauPPM).Text = item.PPM.ToString();
            view.FindViewById<TextView>(Resource.Id.tvGauBackGroud).Text = item.BackgroundPPM.ToString();
            view.FindViewById<TextView>(Resource.Id.tvGauTime).Text = item.EndTime.ToString();
            return view;
        }
    }


}