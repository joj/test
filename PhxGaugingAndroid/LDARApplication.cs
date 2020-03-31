using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Java.IO;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;

using Android;
using PhxGauging.Common.Devices.Services;

namespace PhxGaugingAndroid
{
    [Application(Label = "LDAR检测助手")]
    public class LDARApplication : Application
    {
        public LDARApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            //注册未处理异常事件
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironment_UnhandledExceptionRaiser;
        }

        protected override void Dispose(bool disposing)
        {
            AndroidEnvironment.UnhandledExceptionRaiser -= AndroidEnvironment_UnhandledExceptionRaiser;
            App.phxDeviceService.Disconnect();
            base.Dispose(disposing);
        }

        void AndroidEnvironment_UnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            UnhandledExceptionHandler(e.Exception, e);
        }

        /// <summary>
        /// 处理未处理异常
        /// </summary>
        /// <param name="e"></param>
        private void UnhandledExceptionHandler(Exception ex, RaiseThrowableEventArgs e)
        {
            //处理程序（记录 异常、设备信息、时间等重要信息）
            //**************
            LogHelper.ErrorLog("未处理的异常", ex);
            //提示
            Task.Run(() =>
            {
                Looper.Prepare();
                //可以换成更友好的提示
                Toast.MakeText(this, "很抱歉,程序出现异常,请查看错误日志！！！", ToastLength.Short).Show();
                Looper.Loop();
            });
            //停一会，让前面的操作做完
            System.Threading.Thread.Sleep(2000);
            e.Handled = true;
        }


    }


    public static class App
    {
        public static ExepcService exepcService;
        public static PhxDeviceService phxDeviceService;
        public static TvaService tvaService;
        public static File _file;
        public static File _dir;
        public static Android.Graphics.Bitmap bitmap;
        public static bool IsFire(Activity act)
        {
            if (App.phxDeviceService == null || App.phxDeviceService.IsConnected == false)
            {
                Toast.MakeText(act, "请先连接检测设备", ToastLength.Short).Show();
                return false;
            }
            if (App.phxDeviceService.IsRunning == false)
            {
                Toast.MakeText(act, "请先将检测设备点火", ToastLength.Short).Show();
                return false;
            }
            return true;
        }
    }
}