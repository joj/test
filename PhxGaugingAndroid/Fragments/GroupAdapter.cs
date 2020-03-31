using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Transitions;
using Android.Views;
using Android.Widget;
using Java.Util.Jar;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    /// <summary>
    /// 群组适配器
    /// </summary>
    public class GroupAdapter : BaseAdapter<AndroidGroup>
    {
        public List<AndroidGroup> items;
        Activity context;
        public Bitmap btp;
        public GroupAdapter(Activity contex, List<AndroidGroup> items)
        {
            this.context = contex;
            this.items = items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override AndroidGroup this[int position]
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
                view = context.LayoutInflater.Inflate(Resource.Layout.GroupItem, null);
            //if (item.CompleteCount == (item.SealPointCount - item.UnReachCount))
            if (item.CompleteCount == item.SealPointCount)
            {
                view.FindViewById<LinearLayout>(Resource.Id.linearGroup).Background = new ColorDrawable(Android.Graphics.Color.Rgb(193, 193, 193));
            }
            else
            {
                view.FindViewById<LinearLayout>(Resource.Id.linearGroup).Background = new ColorDrawable(Android.Graphics.Color.Rgb(250, 250, 248));
            }
            view.FindViewById<TextView>(Resource.Id.tvGroupName).Text = item.GroupName;
            view.FindViewById<TextView>(Resource.Id.tvGroupDes).Text = item.GroupCode;
            view.FindViewById<TextView>(Resource.Id.tvGroupDevice).Text = "装置:" + item.DeviceName;
            view.FindViewById<TextView>(Resource.Id.tvGroupArea).Text = "区域:" + item.AreaName;
            //view.FindViewById<TextView>(Resource.Id.tvGroupProgress).Text = "检测进度:" + item.CompleteCount + "/" + (item.SealPointCount - item.UnReachCount).ToString();
            view.FindViewById<TextView>(Resource.Id.tvGroupProgress).Text = "检测进度:" + item.CompleteCount + "/" + (item.SealPointCount).ToString();

            if (File.Exists(item.ImgPath) == false)
            {
                DirectoryInfo dir = new DirectoryInfo(item.ImgPath.Replace("/" + item.GroupCode + ".jpg", ""));
                if (dir.Exists == true)
                {
                    FileInfo[] files = dir.GetFiles(item.GroupCode + "*");
                    if (files.Length > 0)
                    {
                        item.ImgPath = files[0].FullName;
                    }
                }

            }
            ImageView img = view.FindViewById<ImageView>(Resource.Id.btnGroupImg);
            var drawble = (BitmapDrawable)img.Drawable;
            if (drawble != null)
            {
                //内存回收
                img.SetImageResource(0);
                drawble.Bitmap.Recycle();
                drawble.SetCallback(null);
            }
            img.Click -= GroupAdapter_Click;
            if (File.Exists(item.ImgPath))
            {
                img.Click += GroupAdapter_Click;
                img.SetTag(Resource.Id.btnGroupImg, position);
                string smallPath = item.ImgPath.ToLower().Replace("/img/", "/SmallImg/");
                if (File.Exists(smallPath))
                {
                    BitmapHelpers.SetImgBitMap(smallPath, img, btp);
                }
                else
                {
                    var bitmap = BitmapHelpers.LoadAndResizeBitmap(item.ImgPath, 200, 150);
                    //必须适应Bitmap绑定图片否则不能手动回收
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

            return view;
        }

        DateTime lastClick = DateTime.Now;
        private void GroupAdapter_Click(object sender, EventArgs e)
        {
            if (lastClick.AddSeconds(1) > DateTime.Now)
            {
                lastClick = DateTime.Now;
                return;
            }
            lastClick = DateTime.Now;
            int position = int.Parse((sender as ImageView).GetTag(Resource.Id.btnGroupImg).ToString());
            AndroidGroup group = items[position];
            if (File.Exists(group.ImgPath) == false)
            {
                DirectoryInfo dir = new DirectoryInfo(group.ImgPath.Replace("/" + group.GroupCode + ".jpg", ""));
                if (dir.Exists == false)
                {
                    Toast.MakeText(context, group.GroupCode + "群组没有图片", ToastLength.Short).Show();
                    return;
                }
                FileInfo[] files = dir.GetFiles(group.GroupCode + "*");
                if (files.Length == 0)
                {
                    Toast.MakeText(context, group.GroupCode + "群组没有图片", ToastLength.Short).Show();
                    return;
                }
                else
                {
                    group.ImgPath = files[0].FullName;
                }
            }
            Intent i = new Intent(context, typeof(PhotoViewActivity));
            Bundle b = new Bundle();
            b.PutString("AndroidGroup", Newtonsoft.Json.JsonConvert.SerializeObject(group));
            i.PutExtras(b);
            context.StartActivity(i);
        }
    }
}