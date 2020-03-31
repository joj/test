using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using PhxGaugingAndroid.Fragments;
using ZXing;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, HardwareAccelerated = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class ScanBarCodeActivity : Activity, ISurfaceHolderCallback, Camera.IPreviewCallback
    {
        private string TAG = "ChrisAcvitity";
        private Camera mCamera;
        private ISurfaceHolder mHolder;
        private SurfaceView mView;

        // 创建Activity时执行的动作
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(WindowFeatures.NoTitle);
            Window.AddFlags(WindowManagerFlags.Fullscreen);
            SetContentView(Resource.Layout.ZxingOverlay);
            mView = (SurfaceView)FindViewById(Resource.Id.surfaceView);
            mHolder = mView.Holder;
            mHolder.AddCallback(this);
            var flash = FindViewById<Button>(Resource.Id.buttonZxingFlash);
            flash.Click -= Flash_Click;
            flash.Click += Flash_Click;
            var cancel = FindViewById<Button>(Resource.Id.buttonZxingCancel);
            cancel.Click -= Cancel_Click;
            cancel.Click += Cancel_Click;
            DisplayMetrics dm = this.Resources.DisplayMetrics;
            int h_screen = dm.HeightPixels / 4;
            var scanLine = FindViewById<View>(Resource.Id.ScanLine);
            scanLine.SetLayerType(LayerType.Hardware, null);
            //Android.Views.Animations.Animation verticalAnimation = new Android.Views.Animations.TranslateAnimation(0, 0, 0, h_screen);
            //verticalAnimation.Duration = 6000;
            //verticalAnimation.RepeatCount = Android.Views.Animations.Animation.Infinite;
            //verticalAnimation.SetInterpolator(this, Android.Resource.Animation.LinearInterpolator);
            //verticalAnimation.FillAfter = false;
            //scanLine.StartAnimation(verticalAnimation);

            Android.Animation.ObjectAnimator animator = Android.Animation.ObjectAnimator.OfFloat(scanLine, "Y", 0, h_screen);
            animator.RepeatCount = Android.Animation.ValueAnimator.Infinite;
            animator.SetDuration(3000);
            animator.Start();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.ToFinish();
        }
        /// <summary>
        /// 闪光灯
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Flash_Click(object sender, EventArgs e)
        {
            if (mCamera != null)
            {
                var mParameters = mCamera.GetParameters();
                if (mParameters.FlashMode != Camera.Parameters.FlashModeTorch)
                {
                    mParameters.FlashMode = Camera.Parameters.FlashModeTorch;
                }
                else
                {
                    mParameters.FlashMode = Camera.Parameters.FlashModeOff;
                }
                mCamera.SetParameters(mParameters);
            }
        }

        // apk暂停时执行的动作：把相机关闭，避免占用导致其他应用无法使用相机
        protected override void OnPause()
        {
            base.OnPause();
            if (mCamera != null)
            {
                mCamera.SetPreviewDisplay(null);
                mCamera.SetPreviewCallback(null);
                mCamera.StopPreview();
                mCamera.Release();
            }
        }

        // 恢复apk时执行的动作
        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                if (mCamera != null)
                {
                    mCamera = GetCameraInstance();
                    refreshCamera();
                    int rotation = getDisplayOrientation(); //获取当前窗口方向
                    mCamera.SetDisplayOrientation(rotation); //设定相机显示方向                    
                }
            }
            catch (Java.IO.IOException e)
            {

            }
        }
        //关闭界面销毁对象释放相机
        void ToFinish()
        {
            if (mView != null)
            {
                mView.Dispose();
                mView = null;
            }
            if (mHolder != null)
            {
                mHolder.RemoveCallback(this);
                mHolder.Dispose();
                mHolder = null;
            }
            if (mCamera != null)
            {
                mCamera.StopPreview();
                mCamera.SetPreviewCallback(null);
                mCamera.SetPreviewDisplay(null);
                mCamera.Release();
                mCamera.Dispose();
                mCamera = null;
            }
            System.GC.Collect();
            Finish();
        }

        private Camera.Parameters SettingParameters()
        {
            // 获取相机参数
            Camera.Parameters param = mCamera.GetParameters();
            List<string> focusModes = param.SupportedFocusModes.ToList();
            //设置持续的对焦模式
            if (focusModes.Contains(
            Camera.Parameters.FocusModeContinuousPicture))
            {
                param.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
            }
            List<string> flashModes = param.SupportedFlashModes.ToList();
            //设置闪光灯自动开启
            if (flashModes.Contains(Camera.Parameters.FlashModeAuto))
            {
                param.FlashMode = Camera.Parameters.FlashModeAuto;
            }
            param.Zoom = 4;
            int rota = getDisplayOrientation();
            Android.Graphics.Point screen = GetScreenMetrics(this);
            if (int.Parse(Build.VERSION.Sdk) >= (int)Android.OS.BuildVersionCodes.M)
            {
                Camera.Size bestSize = GetBestCameraResolution(param, screen, rota);
                param.SetPreviewSize(bestSize.Width, bestSize.Height);
                Camera.Size cameraSize;
                if (rota == 90 || rota == 270)
                {
                    cameraSize = new Camera.Size(mCamera, param.PreviewSize.Height, param.PreviewSize.Width);
                }
                else
                {
                    cameraSize = param.PreviewSize;
                }

                int width, height;
                if (cameraSize.Width / (float)cameraSize.Height > screen.X / (float)screen.Y)
                {
                    float zoom = cameraSize.Width / (float)screen.X;
                    width = screen.X;
                    height = (int)(cameraSize.Height / zoom);
                }
                else
                {
                    float zoom = cameraSize.Height / (float)screen.Y;
                    height = screen.Y;
                    width = (int)(cameraSize.Width / zoom);
                }
                ViewGroup.LayoutParams svParams = mView.LayoutParameters;
                svParams.Height = height;
                svParams.Width = width;
                mView.LayoutParameters = svParams;
            }

            return param;
        }
        /// <summary>
        /// 获取最佳预览大小
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="screenResolution"></param>
        /// <returns></returns>
        private Camera.Size GetBestCameraResolution(Camera.Parameters parameters, Android.Graphics.Point screenResolution, int cameraRota)
        {
            float tmp = 0f;
            float mindiff = 100f;
            float x_d_y = (float)screenResolution.X / (float)screenResolution.Y;
            Android.Hardware.Camera.Size best = null;
            List<Android.Hardware.Camera.Size> supportedPreviewSizes = parameters.SupportedPreviewSizes.OrderByDescending(c => c.Width).ToList();
            foreach (var s in supportedPreviewSizes)
            {
                if (cameraRota == 90 || cameraRota == 270)
                {
                    tmp = Math.Abs(((float)s.Height / (float)s.Width) - x_d_y);
                }
                else
                {
                    tmp = Math.Abs(((float)s.Width / (float)s.Height) - x_d_y);
                }
                if (tmp < mindiff)
                {
                    mindiff = tmp;
                    best = s;
                }
            }
            foreach (var item in supportedPreviewSizes)
            {
                if (cameraRota == 90 || cameraRota == 270)
                {
                    if (item.Width < 800)
                    {
                        return item;
                    }
                }
                else
                {
                    if (item.Height < 800)
                    {
                        return item;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// 获取屏幕宽度和高度，单位为px 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static Android.Graphics.Point GetScreenMetrics(Context context)
        {
            DisplayMetrics dm = context.Resources.DisplayMetrics;
            int w_screen = dm.WidthPixels;
            int h_screen = dm.HeightPixels;
            return new Android.Graphics.Point(w_screen, h_screen);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            try
            {
                if (mCamera == null)
                {
                    mCamera = GetCameraInstance();
                }
            }
            catch (Java.IO.IOException e)
            {
            }
        }
        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Android.Graphics.Format format, int width, int height)
        {
            refreshCamera();
            int rotation = getDisplayOrientation(); //获取当前窗口方向
            mCamera.SetDisplayOrientation(rotation); //设定相机显示方向         
        }
        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
        }
        string LastScan = string.Empty;
        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            mCamera.SetPreviewCallback(null);
            Task.Factory.StartNew(() =>
            {
                Camera.Size size = camera.GetParameters().PreviewSize;
                ZXing.BarcodeReader BarReader = new BarcodeReader();
                BarReader.AutoRotate = true;
                BarReader.Options.PossibleFormats = new List<ZXing.BarcodeFormat> { ZXing.BarcodeFormat.CODE_128 };
                using (Android.Graphics.YuvImage image = new Android.Graphics.YuvImage(data, Android.Graphics.ImageFormat.Nv21, size.Width, size.Height, null))
                {
                    if (image != null)
                    {
                        using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                        {
                            int rota = getDisplayOrientation();
                            if (rota == 90 || rota == 270)
                            {
                                image.CompressToJpeg(new Android.Graphics.Rect(size.Width * 3 / 8, size.Height / 5, size.Width * 5 / 8, size.Height * 4 / 5), 80, stream);
                            }
                            else
                            {
                                image.CompressToJpeg(new Android.Graphics.Rect(size.Width / 5, size.Height * 3 / 8, size.Width * 4 / 5, size.Height * 5 / 8), 80, stream);
                            }
                            ZXing.Result res = null;
                            using (Android.Graphics.Bitmap bmp = Android.Graphics.BitmapFactory.DecodeByteArray(stream.ToArray(), 0, (int)stream.Length))
                            {
                                using (Android.Graphics.Matrix matrix = new Android.Graphics.Matrix())
                                {
                                    for (int i = 0; i < 9; i++)
                                    {
                                        res = ScanBarCodeBitmap(rota, (0 - 10 * i), bmp, matrix, BarReader);
                                        if (res != null)
                                        {
                                            break;
                                        }
                                    }
                                    bmp.Recycle();
                                }
                            }
                            //以10度旋转扫描                        
                            if (res != null)
                            {
                                if (LastScan != res.Text)
                                {
                                    LastScan = res.Text;
                                }
                                else
                                {
                                    RunOnUiThread(() =>
                                    {
                                        Vibrator vibrator = (Vibrator)this.GetSystemService(Service.VibratorService);
                                        vibrator.Vibrate(500);
                                        Intent i = new Intent(this, typeof(MainActivity));
                                        i.PutExtra("ScanResult", LastScan);
                                        SetResult(Android.App.Result.Ok, i);
                                        ToFinish();
                                    });
                                    return;
                                }
                            }
                        }
                    }
                }
                mCamera.SetPreviewCallback(this);
            });
        }

        private ZXing.Result ScanBarCodeBitmap(int rota, int angle, Android.Graphics.Bitmap bmp, Android.Graphics.Matrix matrix, BarcodeReader BarReader)
        {
            matrix.PostRotate(rota - angle);
            ZXing.Result res = null;
            Android.Graphics.Bitmap nbmp = null; ;
            try
            {
                nbmp = Android.Graphics.Bitmap.CreateBitmap(bmp, 0, 0, bmp.Width, bmp.Height, matrix, true);
                res = BarReader.Decode(nbmp);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (nbmp != null)
                {
                    nbmp.Recycle();
                    nbmp = null;
                }
            }
            return res;
        }

        // === 以下是各种辅助函数 ===

        // 获取camera实例
        public Camera GetCameraInstance()
        {
            Camera c = null;
            try
            {
                c = Camera.Open();
            }
            catch (Exception e)
            {

            }
            return c;
        }

        // 获取当前窗口管理器显示方向
        private int getDisplayOrientation()
        {
            Display display = this.WindowManager.DefaultDisplay;
            SurfaceOrientation rotation = display.Rotation;
            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0:
                    degrees = 0;
                    break;
                case SurfaceOrientation.Rotation90:
                    degrees = 90;
                    break;
                case SurfaceOrientation.Rotation180:
                    degrees = 180;
                    break;
                case SurfaceOrientation.Rotation270:
                    degrees = 270;
                    break;
            }

            Camera.CameraInfo camInfo = new Camera.CameraInfo();
            Camera.GetCameraInfo((int)CameraFacing.Back, camInfo);
            // 相机标定
            int result = (camInfo.Orientation - degrees + 360) % 360;

            return result;
        }

        // 刷新相机
        private void refreshCamera()
        {
            if (mHolder.Surface == null)
            {
                return;
            }
            try
            {
                mCamera.SetPreviewDisplay(null);
                mCamera.SetPreviewCallback(null);
                mCamera.StopPreview();
            }
            catch (Exception e)
            {
            }
            try
            {
                mCamera.SetPreviewDisplay(mHolder);
                mCamera.SetPreviewCallback(this);
                mCamera.StartPreview();
                // 设置相机参数
                mCamera.SetParameters(SettingParameters());
            }
            catch (Exception e)
            {

            }
        }
    }
}