using Android.App;
using Android.OS;
using PhxGaugingAndroid.Fragments;
using Android.Support.Design.Widget;
using Android.Widget;
using PhxGauging.Common.Devices.Services;
using System.Collections.Generic;
using Android.Bluetooth;
using System;
using Android.Runtime;
using Android.Views;
using PhxGaugingAndroid.Common;
using Android.Content.PM;
using Android;
using NLog.Config;
using NLog;
using NLog.Layouts;
using NLog.Targets;
using Android.Content;
using PhxGaugingAndroid.Entity;
using Newtonsoft.Json;
using PhxGauging.Common.Android.Services;
using PhxGauging.Common.Android.IO;
using PhxGaugingAndroid;
using Resource = PhxGaugingAndroid.Resource;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTask, Icon = "@drawable/icon", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class MainExepcActivity : Activity
    {
        ImageView backIco;
        private List<ImageView> ivlist;
        private List<TextView> tvlist;
        TextView wifiText;

        protected override void OnCreate(Bundle bundle)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("@31372e332e30fHJlegti4VlULjm8xeWmE5hsP10VIbTkRLC32bjH2hg=");
            App.exepcService = new ExepcService();
            App.exepcService.Messager -= ExepcService_Messager;
            App.exepcService.Messager += ExepcService_Messager;
            App.exepcService.ConnectStatus -= ExepcService_ConnectStatus;
            App.exepcService.ConnectStatus += ExepcService_ConnectStatus;
            base.OnCreate(bundle);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.mainExepc);
            this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;//竖屏，禁止横屏 
            FindViewById<LinearLayout>(Resource.Id.tb1).Click += Tab_Click;
            FindViewById<LinearLayout>(Resource.Id.tb2).Click += Tab_Click;
            FindViewById<LinearLayout>(Resource.Id.tb3).Click += Tab_Click;
            FindViewById<LinearLayout>(Resource.Id.tb4).Click += Tab_Click;
            ivlist = new List<ImageView>();
            tvlist = new List<TextView>();
            ivlist.Add(FindViewById<ImageView>(Resource.Id.iv1));
            ivlist.Add(FindViewById<ImageView>(Resource.Id.iv2));
            ivlist.Add(FindViewById<ImageView>(Resource.Id.iv3));
            ivlist.Add(FindViewById<ImageView>(Resource.Id.iv4));
            tvlist.Add(FindViewById<TextView>(Resource.Id.tv1));
            tvlist.Add(FindViewById<TextView>(Resource.Id.tv2));
            tvlist.Add(FindViewById<TextView>(Resource.Id.tv3));
            tvlist.Add(FindViewById<TextView>(Resource.Id.tv4));
            ChangePageSelect(0);
            LoadFragment(Resource.Id.tb1);
            backIco = FindViewById<ImageView>(Resource.Id.backIco);
            backIco.Click += IcoBack_Click;
            backIco.Visibility = ViewStates.Gone;
            LinearLayout imgDiscover = FindViewById<LinearLayout>(Resource.Id.btnDiscover);
            imgDiscover.Click += ImgDiscover_Click;
            wifiText = FindViewById<TextView>(Resource.Id.wifiText);
            if (App.exepcService.client != null && App.exepcService.client.Connected)
            {
                wifiText.Text = "WIFI已连接";
            }
            else
            {
                wifiText.Text = "WIFI未连接";
            }
            string model = UserPreferences.GetString("ExepcModel");
            if (string.IsNullOrEmpty(model))
            {
                UserPreferences.SetString("ExepcModel", "FID");
            }
        }
        /// <summary>
        /// 导航选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tab_Click(object sender, EventArgs e)
        {
            var linear = sender as LinearLayout;
            ChangePageSelect(int.Parse(linear.Tag.ToString()));
            LoadFragment(linear.Id);
            VisibilityBackIco();
        }
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IcoBack_Click(object sender, EventArgs e)
        {
            Android.App.Fragment fragment = null;
            switch (currntPage)
            {
                case Resource.Id.tb1:
                    if (F1Level > 0)
                    {
                        F1Level -= 1;
                        fragment = F1FragmentList[F1Level];
                    }
                    break;
                case Resource.Id.tb2:
                    if (F2Level > 0)
                    {
                        F2Level -= 1;
                        fragment = F2FragmentList[F2Level];
                    }
                    break;
                case Resource.Id.tb3:
                    if (F3Level > 0)
                    {
                        F3Level -= 1;
                        fragment = F3FragmentList[F3Level];
                    }
                    break;
                case Resource.Id.tb4:
                    if (F4Level > 0)
                    {
                        F4Level -= 1;
                        fragment = F4FragmentList[F4Level];
                    }
                    break;
            }
            if (fragment != null)
            {
                ReplaceFragment(fragment);
            }
            VisibilityBackIco();
        }

        private void ImgDiscover_Click(object sender, EventArgs e)
        {
            App.exepcService.Connect();
        }
        private void VisibilityBackIco()
        {
            switch (currntPage)
            {
                case Resource.Id.tb1:
                    if (F1Level > 0)
                    {
                        backIco.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        backIco.Visibility = ViewStates.Gone;
                    }
                    break;
                case Resource.Id.tb2:
                    if (F2Level > 0)
                    {
                        backIco.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        backIco.Visibility = ViewStates.Gone;
                    }
                    break;
                case Resource.Id.tb3:
                    if (F3Level > 0)
                    {
                        backIco.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        backIco.Visibility = ViewStates.Gone;
                    }
                    break;
                case Resource.Id.tb4:
                    if (F4Level > 0)
                    {
                        backIco.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        backIco.Visibility = ViewStates.Gone;
                    }
                    break;
            }
        }

        private int F1Level, F2Level, F3Level, F4Level;
        private Android.App.Fragment[] F1FragmentList = new Android.App.Fragment[2];
        private Android.App.Fragment[] F2FragmentList = new Android.App.Fragment[3];
        private Android.App.Fragment[] F3FragmentList = new Android.App.Fragment[1];
        private Android.App.Fragment[] F4FragmentList = new Android.App.Fragment[1];
        int currntPage;
        void LoadFragment(int id)
        {
            Android.App.Fragment fragment = null;
            currntPage = id;
            switch (id)
            {
                case Resource.Id.tb1:
                    if (F1Level > 0)
                    {
                        fragment = F1FragmentList[F1Level];
                    }
                    else
                    {
                        PhxDetailExepcFragment f1 = F1FragmentList[F1Level] as PhxDetailExepcFragment;
                        if (f1 == null)
                        {
                            f1 = PhxDetailExepcFragment.NewInstance();
                            f1.GotoCalibration += Detail_GotoCalibration;
                        }
                        fragment = f1;
                        F1Level = 0;
                    }
                    F1FragmentList[F1Level] = fragment;
                    break;
                case Resource.Id.tb2:
                    if (F2Level > 0)
                    {
                        fragment = F2FragmentList[F2Level];
                    }
                    else
                    {
                        WorkOrderFragment f2 = F2FragmentList[F2Level] as WorkOrderFragment;
                        if (f2 == null)
                        {
                            f2 = WorkOrderFragment.NewInstance();
                            f2.GotoGroup += WorkOrder_GotoGroup;
                        }
                        fragment = f2;
                        F2Level = 0;
                    }
                    F2FragmentList[F2Level] = fragment;
                    break;
                case Resource.Id.tb3:
                    if (F3Level > 0)
                    {
                        fragment = F3FragmentList[F3Level];
                    }
                    else
                    {
                        GaugingFragmentExepc f3 = F3FragmentList[F3Level] as GaugingFragmentExepc;
                        if (f3 == null)
                        {
                            f3 = GaugingFragmentExepc.NewInstance();
                        }
                        fragment = f3;
                        F3Level = 0;
                    }
                    F3FragmentList[F3Level] = fragment;
                    break;
                case Resource.Id.tb4:
                    if (F4Level > 0)
                    {
                        fragment = F4FragmentList[F4Level];
                    }
                    else
                    {
                        MoreMenuFragment f4 = F4FragmentList[F4Level] as MoreMenuFragment;
                        if (f4 == null)
                        {
                            f4 = MoreMenuFragment.NewInstance();
                        }
                        fragment = f4;
                        F4Level = 0;
                    }
                    F4FragmentList[F4Level] = fragment;
                    break;
            }
            if (fragment == null)
                return;
            ReplaceFragment(fragment);
        }
        public void ChangePageSelect(int index)
        {
            for (int i = 0; i < ivlist.Count; i++)
            {
                if (index == i)
                {
                    ivlist[i].Enabled = false;
                    tvlist[i].SetTextColor(Android.Support.V4.Content.ContextCompat.GetColorStateList(this, Resource.Color.colorLightRed));
                }
                else
                {
                    ivlist[i].Enabled = true;
                    tvlist[i].SetTextColor(Android.Support.V4.Content.ContextCompat.GetColorStateList(this, Resource.Color.colorTextGrey));
                }
            }
        }

        private void ReplaceFragment(Fragment fragment)
        {
            var frag = FragmentManager.FindFragmentById(Resource.Id.content_frame);
            var stopReceive = frag as StopReceiveData;
            if (stopReceive != null)
            {
                stopReceive.StopReceiveData();
            }
            FragmentTransaction trans = this.FragmentManager.BeginTransaction();
            trans.Replace(Resource.Id.content_frame, fragment);
            trans.Commit();
        }

        /// <summary>
        /// 工单跳转群组
        /// </summary>
        /// <param name="f"></param>
        private void WorkOrder_GotoGroup(Android.App.Fragment f)
        {
            var fragment = f as GroupFragment;
            fragment.GotoSealPoint += Group_GotoSealPoint;
            F2Level = 1;
            F2FragmentList[F2Level] = fragment;
            ReplaceFragment(f);
            VisibilityBackIco();
        }
        /// <summary>
        /// 群组跳转密封点列表
        /// </summary>
        /// <param name="f"></param>
        private void Group_GotoSealPoint(Android.App.Fragment f)
        {
            F2Level = 2;
            F2FragmentList[F2Level] = f;
            ReplaceFragment(f);
            VisibilityBackIco();
        }


        /// <summary>
        /// 详细信息页面跳转校准页面
        /// </summary>
        /// <param name="f"></param>
        private void Detail_GotoCalibration(Android.App.Fragment f)
        {
            var fragment = f as ExepcCalibration;
            F1Level = 1;
            F1FragmentList[F1Level] = fragment;
            ReplaceFragment(fragment);
            VisibilityBackIco();
        }

        DateTime? lastBackKeyDownTime;
        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                Android.App.Fragment fragment = null;
                switch (currntPage)
                {
                    case Resource.Id.tb1:
                        if (F1Level > 0)
                        {
                            F1Level -= 1;
                            fragment = F1FragmentList[F1Level];
                        }
                        break;
                    case Resource.Id.tb2:
                        if (F2Level > 0)
                        {
                            F2Level -= 1;
                            fragment = F2FragmentList[F2Level];
                        }
                        break;
                    case Resource.Id.tb3:
                        if (F3Level > 0)
                        {
                            F3Level -= 1;
                            fragment = F3FragmentList[F3Level];
                        }
                        break;
                    case Resource.Id.tb4:
                        if (F4Level > 0)
                        {
                            F4Level -= 1;
                            fragment = F4FragmentList[F4Level];
                        }
                        break;
                }
                if (fragment != null)
                {
                    ReplaceFragment(fragment);
                    VisibilityBackIco();
                    return true;
                }
                if (!lastBackKeyDownTime.HasValue || DateTime.Now - lastBackKeyDownTime.Value > new TimeSpan(0, 0, 2))
                {
                    Toast.MakeText(this.ApplicationContext, "再按一次退出程序", ToastLength.Short).Show();
                    lastBackKeyDownTime = DateTime.Now;
                }
                else
                {
                    if (App.phxDeviceService != null)
                    {
                        App.phxDeviceService.Stop();
                        App.phxDeviceService.Disconnect();
                    }
                    UserPreferences.DeleteString("LoginState");
                    UserPreferences.SetString("LoginState", "Back");
                    MoveTaskToBack(true);
                }
                return true;
            }
            return base.OnKeyDown(keyCode, e);
        }

        private void ExepcService_Messager(string msg)
        {
            if (this == null)
            {
                return;
            }
            this.RunOnUiThread(() =>
            {
                Toast.MakeText(this, msg, ToastLength.Short).Show();
            });
        }

        private void ExepcService_ConnectStatus(bool isConnect)
        {
            if (this == null)
            {
                return;
            }
            this.RunOnUiThread(() =>
            {
                if (isConnect)
                {
                    wifiText.Text = "WIFI已连接";
                }
                else
                {
                    wifiText.Text = "WIFI未连接";
                }
            });
        }
    }
}

