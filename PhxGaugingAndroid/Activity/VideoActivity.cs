using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.Hardware.Camera;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class VideoActivity : Activity, TextureView.ISurfaceTextureListener
    {
        Camera _camera;
        TextureView _textureView;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _textureView = new TextureView(this);
            _textureView.SurfaceTextureListener = this;
            SetContentView(_textureView);
        }
        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            _camera = Camera.Open();
            _camera.SetPreviewTexture(surface);
            int cameraId = GetCameraId(CameraFacing.Back);//获取相机ID
            int result = setCameraDisplayOrientation(this, cameraId, _camera);//获取相机方向
            _camera.SetDisplayOrientation(result);//设置相机方向
            Camera.Parameters parameters = _camera.GetParameters();//获取camera的parameter实例
            List<Size> listSize = parameters.SupportedPreviewSizes.ToList();
            Camera.Size optionSize;
            if (listSize.First().Width> listSize.First().Height)
            {
                optionSize = getOptimalPreviewSize(listSize, Resources.DisplayMetrics.HeightPixels, Resources.DisplayMetrics.WidthPixels);//获取一个最为适配的camera.size 
                _textureView.LayoutParameters = new FrameLayout.LayoutParams(optionSize.Height, optionSize.Width);
            }
            else
            {
                optionSize = getOptimalPreviewSize(listSize, Resources.DisplayMetrics.WidthPixels, Resources.DisplayMetrics.HeightPixels);//获取一个最为适配的camera.size 
                _textureView.LayoutParameters = new FrameLayout.LayoutParams(optionSize.Width, optionSize.Height);
            }
            parameters.SetPreviewSize(optionSize.Width, optionSize.Height);//把camera.size赋值到parameters
            ;
            _camera.StartPreview();
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            _camera.StopPreview();
            _camera.Release();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            //throw new NotImplementedException();
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {
            //throw new NotImplementedException();
        }
        /// <summary>
        /// 设置相机选择
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="cameraId"></param>
        /// <param name="camera"></param>
        public static int setCameraDisplayOrientation(Activity activity,
        int cameraId, Camera camera)
        {
            Camera.CameraInfo info = new Camera.CameraInfo();
            Camera.GetCameraInfo(cameraId, info);
            SurfaceOrientation rotation = activity.WindowManager.DefaultDisplay.Rotation;
            int degrees = 0;
            switch (rotation)
            {
                case SurfaceOrientation.Rotation0: degrees = 0; break;
                case SurfaceOrientation.Rotation90: degrees = 90; break;
                case SurfaceOrientation.Rotation180: degrees = 180; break;
                case SurfaceOrientation.Rotation270: degrees = 270; break;
            }

            int result;
            if (info.Facing == Camera.CameraInfo.CameraFacingFront)
            {
                result = (info.Orientation + degrees) % 360;
                result = (360 - result) % 360;  // compensate the mirror
            }
            else
            {  // back-facing
                result = (info.Orientation - degrees + 360) % 360;
            }
            return result;

        }
        /// <summary>
        /// 获取相机ID
        /// </summary>
        /// <param name="tagInfo"></param>
        /// <returns></returns>
        public int GetCameraId(CameraFacing tagInfo)
        {
            Camera.CameraInfo cameraInfo = new Camera.CameraInfo();
            // 开始遍历摄像头，得到camera info
            int cameraId, cameraCount;
            for (cameraId = 0, cameraCount = Camera.NumberOfCameras; cameraId < cameraCount; cameraId++)
            {
                Camera.GetCameraInfo(cameraId, cameraInfo);
                if (cameraInfo.Facing == tagInfo)
                {
                    break;
                }
            }
            return cameraId;
        }

        private Size getOptimalPreviewSize(List<Size> sizes, int w, int h)
        {
            double ASPECT_TOLERANCE = 0.1;
            double targetRatio = (double)w / h;
            if (sizes == null) return null;

            Size optimalSize = null;
            double minDiff = Double.MaxValue;

            int targetHeight = h;

            // Try to find an size match aspect ratio and size  
            foreach (Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;
                if (Math.Abs(ratio - targetRatio) > ASPECT_TOLERANCE) continue;
                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            // Cannot find the one match the aspect ratio, ignore the requirement  
            if (optimalSize == null)
            {
                minDiff = Double.MaxValue;
                foreach (Size size in sizes)
                {
                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }
            return optimalSize;
        }

    }
}