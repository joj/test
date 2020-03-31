using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Environment = Android.OS.Environment;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Android.Content.PM;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class CameraActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Camera);
            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
                Button sysCamera = FindViewById<Button>(Resource.Id.btnSysCamera);
                sysCamera.Click += SysCamera_Click;

                Button rangeCamera = FindViewById<Button>(Resource.Id.btnRangeCamera);
                rangeCamera.Click += RangeCamera_Click;
                rangeCamera.Visibility = ViewStates.Gone;
                SurfaceView sf = FindViewById<SurfaceView>(Resource.Id.sfCamera);
                sf.Visibility = ViewStates.Gone;
            }
        }
        /// <summary>
        /// 点击自定义相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RangeCamera_Click(object sender, EventArgs e)
        {
             
        }
        /// <summary>
        /// 系统相机
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SysCamera_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));
            intent.PutExtra(MediaStore.ExtraOutput,Android.Net.Uri.FromFile(App._file));
            StartActivityForResult(intent, 0);
        }
        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            // 结果码匹配成功
            if (requestCode == 0)
            {
                ImageView img = FindViewById<ImageView>(Resource.Id.ImageCamera);
                img.Visibility = ViewStates.Visible;
                //显示照片
                SurfaceView sfCamera = FindViewById<SurfaceView>(Resource.Id.sfCamera);
                sfCamera.Visibility = ViewStates.Gone;

                Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                Android.Net.Uri contentUri =Android.Net.Uri.FromFile(App._file);
                mediaScanIntent.SetData(contentUri);
                SendBroadcast(mediaScanIntent);

                int height = Resources.DisplayMetrics.HeightPixels-(int)(70* Resources.DisplayMetrics.Density+0.5f);
                int width = Resources.DisplayMetrics.WidthPixels;
                App.bitmap = Common.BitmapHelpers.LoadAndResizeBitmap(App._file.Path,width, height);
                if (App.bitmap != null)
                {
                    img.SetImageBitmap(App.bitmap);
                    App.bitmap = null;
                }
                // Dispose of the Java side bitmap.
                GC.Collect();                
            }
        }

        /// <summary>
        /// 创建图片文件路径
        /// </summary>
        private void CreateDirectoryForPictures()
        {
            App._dir = new File(
                Environment.GetExternalStoragePublicDirectory(
                    Environment.DirectoryPictures), "CameraAppDemo");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }
        /// <summary>
        /// 相机是否被占用
        /// </summary>
        /// <returns></returns>
        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }
    }

    
}