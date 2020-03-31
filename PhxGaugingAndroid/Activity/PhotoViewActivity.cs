using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Github.Chrisbanes.Photoview;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class PhotoViewActivity : AppCompatActivity
    {
        PhotoView photoView;
        public Bitmap btp;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PhotoView);
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Phototoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "群组图片";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            photoView = this.FindViewById<PhotoView>(Resource.Id.photo_view);
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
            string info = this.Intent.GetStringExtra("AndroidGroup");
            AndroidGroup group = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidGroup>(info);
            if (System.IO.File.Exists(group.ImgPath) == false)
            {
                DirectoryInfo dir = new DirectoryInfo(group.ImgPath.Replace("/" + group.GroupCode + ".jpg", ""));
                FileInfo[] files = dir.GetFiles(group.GroupCode + "*");
                if (files.Length > 0)
                {
                    group.ImgPath = files[0].FullName;
                }
            }
            if (System.IO.File.Exists(group.ImgPath))
            {
                BitmapHelpers.SetImgBitMap(group.ImgPath, photoView, btp);
            }
            else
            {
                TextView tv = FindViewById<TextView>(Resource.Id.tvNoImg);
                tv.Text = "该群组没有图片";
                tv.Visibility = ViewStates.Visible;
                photoView.Visibility = ViewStates.Gone;
            }
        }



        protected override void OnPause()
        {
            var drawble = (BitmapDrawable)photoView.Drawable;
            if (drawble != null)
            {
                photoView.SetBackgroundResource(0);
                drawble.SetCallback(null);
                drawble.Bitmap.Recycle();
                photoView.Dispose();
            }
            if (btp != null)
            {
                btp.Recycle();
            }
            base.OnPause();
        }
    }
}