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
using Java.IO;
using Android.Content;
using PhxGaugingAndroid.Entity;
using Newtonsoft.Json;
using PhxGauging.Common.Android.Services;
using PhxGauging.Common.Android.IO;
using Android.Locations;

namespace PhxGaugingAndroid
{
    [Activity(Label = "@string/app_name", MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTask, Icon = "@drawable/icon", WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden)]
    public class MainTvaActivity : Activity, ILocationListener
    {
        ImageView backIco;
        private List<ImageView> ivlist;
        private List<TextView> tvlist;
        protected override void OnCreate(Bundle bundle)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("@31372e332e30fHJlegti4VlULjm8xeWmE5hsP10VIbTkRLC32bjH2hg=");
            App.tvaService = new TvaService();
            App.tvaService.Messager -= TvaService_Messager;
            App.tvaService.Messager += TvaService_Messager;
            base.OnCreate(bundle);
            Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);
            SetContentView(Resource.Layout.main);
            //日志初始化
            InitializeNLog();
            this.androidBluetoothService = new TvaAndroidBluetoothService(this);
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
            LinearLayout btnDisconnect = FindViewById<LinearLayout>(Resource.Id.btnDisconnect);
            btnDisconnect.Click += BtnDisconnect_Click;
            //string LoginState = UserPreferences.GetString("LoginState");
            //string UserInfo = UserPreferences.GetString("UserInfo");
            //if (LoginState == "2")
            //{
            //    TextView LoginStateText = FindViewById<TextView>(Resource.Id.LoginStateText);
            //    DateTime Bentime = DateTime.Now;
            //    string Txt = "";
            //    if (!string.IsNullOrEmpty(UserInfo))
            //    {
            //        string json = Utility.DecryptDES(UserInfo);
            //        var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            //        DateTime EndTime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime).AddDays(5);
            //        TimeSpan ts = EndTime.Subtract(Bentime);
            //        //Txt = ts.Days.ToString() + "天" + ts.Hours.ToString() + "小时" + "后需在线登陆!";
            //    }
            //    LoginStateText.Text = Txt;
            //}
            //GPS
            //LocationManager lm = (LocationManager)GetSystemService(LocationService);
            //lm.RequestLocationUpdates(LocationManager.GpsProvider, 5000, 0, this);
        }

        private void TvaService_DeviceDiscoveryComplete(object sender, EventArgs e)
        {
            this.bluetoothsDevices = this.androidBluetoothService.receiver.getBluetooths();
            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            foreach (BluetoothDevice current in this.bluetoothsDevices)
            {
                list.Add(new JavaDictionary<string, object>
                    {
                        {
                            "itemTitle",
                            current.Name
                        },
                        {
                            "itemText",
                            current.Address
                        }
                    });
            }
            adapter = new SimpleAdapter(this, list, Resource.Layout.Phx21Item, new string[]
            {
                    "itemTitle",
                    "itemText"
            }, new int[]
            {
                    Resource.Id.itemTitle,
                    Resource.Id.itemText
            });
            this.listView.Adapter = adapter;
        }

        private void TvaService_Messager(string msg)
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
        /// 日志初始化
        /// </summary>
        private void InitializeNLog()
        {
            var sdCard = Android.OS.Environment.ExternalStorageDirectory;
            var logDirectory = new File(sdCard.AbsolutePath + "/LDARAPP6/log");
            if (!logDirectory.Exists())
            {
                logDirectory.Mkdirs();
            }
            var config = new LoggingConfiguration();
            var FileTarget = new FileTarget
            {
                FileName = logDirectory.AbsolutePath + "/${shortdate}.txt"
            };
            config.AddTarget("File", FileTarget);
            var rule1 = new LoggingRule("*", LogLevel.Debug, FileTarget);
            config.LoggingRules.Add(rule1);
            LogManager.Configuration = config;
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

        /// <summary>
        /// 断开蓝牙连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            DialogService dialogService = new DialogService();
            dialogService.ShowYesNo(this, "提示", "是否要断开与检测设备的连接？",
                    r =>
                    {
                        if (r == DialogResult.Yes)
                        {
                            App.tvaService.Disconnect();
                            F1Level = 0;
                            F1FragmentList.SetValue(null, F1Level);
                            var frag = F3FragmentList[0] as StopReceiveData;
                            if (frag != null)
                            {
                                frag.StopReceiveData();
                            }
                            var nav = FindViewById<LinearLayout>(Resource.Id.bottom_navigation);
                            if (currntPage == Resource.Id.tb1)
                            {
                                LoadFragment(Resource.Id.tb1);
                                VisibilityBackIco();
                            }
                            LinearLayout btnDisConnect = sender as LinearLayout;
                            btnDisConnect.Visibility = ViewStates.Gone;
                            FindViewById<LinearLayout>(Resource.Id.btnDiscover).Visibility = ViewStates.Visible;
                            TextView tvPhxName = FindViewById<TextView>(Resource.Id.tvPhxName);
                            tvPhxName.Text = "请先搜索检测设备";
                            Toast.MakeText(this, "已经断开", ToastLength.Short).Show();
                        }
                    });
        }

        AlertDialog dialog;
        public TvaAndroidBluetoothService androidBluetoothService;
        private List<BluetoothDevice> bluetoothsDevices;
        public ListView listView;
        public Button btnRefresh;
        SimpleAdapter adapter;
        private void ImgDiscover_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = this.LayoutInflater;
            View layout = inflater.Inflate(Resource.Layout.DiscoverBluetooth, (ViewGroup)FindViewById<ViewGroup>(Resource.Id.rlDiscover));
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetView(layout);
            builder.SetCancelable(true);
            btnRefresh = layout.FindViewById<Button>(Resource.Id.btnRefresh);
            btnRefresh.Click -= BtnRefresh_Click;
            btnRefresh.Click += BtnRefresh_Click;
            listView = layout.FindViewById<ListView>(Resource.Id.PhxlistView);
            listView.ItemClick -= ListView_ItemClick;
            listView.ItemClick += ListView_ItemClick;
            var close = layout.FindViewById<ImageView>(Resource.Id.close);
            close.Click -= DiscoverClose_Click;
            close.Click += DiscoverClose_Click;
            dialog = builder.Show();
        }

        private void DiscoverClose_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            this.androidBluetoothService.bluetoothDevice = this.bluetoothsDevices[e.Position];
            if (this.androidBluetoothService.bluetoothDevice != null)
            {
                try
                {
                    this.androidBluetoothService.StopDiscovery();
                    Device device = new Device(this.androidBluetoothService.bluetoothDevice);
                    App.tvaService.Connect(this.androidBluetoothService, device);
                    if (App.tvaService.IsConnected)
                    {
                        //蓝牙
                        dialog.Dismiss();
                        //标题显示连接信息
                        TextView tvPhxName = FindViewById<TextView>(Resource.Id.tvPhxName);
                        tvPhxName.Text = App.tvaService.Name + "连接成功";
                        LinearLayout imgDiscover = FindViewById<LinearLayout>(Resource.Id.btnDiscover);
                        imgDiscover.Visibility = ViewStates.Gone;
                        LinearLayout btnDisconnect = FindViewById<LinearLayout>(Resource.Id.btnDisconnect);
                        btnDisconnect.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        Toast.MakeText(this, "连接失败", ToastLength.Short).Show();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("蓝牙连接检测设备", ex);
                    Toast.MakeText(this, "连接失败", ToastLength.Short).Show();
                }
            }
        }
        Toast buleToast;
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            DateTime Servertime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime);
            if (Servertime.AddDays(5) <= DateTime.Now)
            {
                Toast.MakeText(this, "离线时间超过五天，必须在线登录!", ToastLength.Short).Show();
                return;
            }
            if (buleToast != null)
            {
                buleToast.Cancel();
            }
            buleToast = Toast.MakeText(this, "蓝牙搜索中.....", ToastLength.Short);
            buleToast.Show();
            btnRefresh.Text = "刷新";
            androidBluetoothService.StartDiscovery();
            this.bluetoothsDevices = this.androidBluetoothService.receiver.getBluetooths();
            List<IDictionary<string, object>> list = new List<IDictionary<string, object>>();
            foreach (BluetoothDevice current in this.bluetoothsDevices)
            {
                list.Add(new JavaDictionary<string, object>
                    {
                        {
                            "itemTitle",
                            current.Name
                        },
                        {
                            "itemText",
                            current.Address
                        }
                    });
            }
            adapter = new SimpleAdapter(this, list, Resource.Layout.Phx21Item, new string[]
            {
                    "itemTitle",
                    "itemText"
            }, new int[]
            {
                    Resource.Id.itemTitle,
                    Resource.Id.itemText
            });
            this.listView.Adapter = adapter;
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
                        PhxDetailTvaFragment f1 = F1FragmentList[F1Level] as PhxDetailTvaFragment;
                        if (f1 == null)
                        {
                            f1 = PhxDetailTvaFragment.NewInstance();
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
                        GaugingFragmentTva f3 = F3FragmentList[F3Level] as GaugingFragmentTva;
                        if (f3 == null)
                        {
                            f3 = GaugingFragmentTva.NewInstance();
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
        /// 详细信息页面跳转诊断页面
        /// </summary>
        private void Detail_GotoDiagnostic()
        {
            StartActivity(typeof(DeviceDiagnosticActivity));
        }

        /// <summary>
        /// 详细信息页面跳转校准页面
        /// </summary>
        /// <param name="f"></param>
        private void Detail_GotoCalibration(Android.App.Fragment f)
        {
            var fragment = f as CalibrationInfoFragment;
            fragment.goBack += CalibrationInfoFragment_goBack;
            F1Level = 1;
            F1FragmentList[F1Level] = fragment;
            ReplaceFragment(fragment);
            VisibilityBackIco();
        }

        private void CalibrationInfoFragment_goBack()
        {
            IcoBack_Click(null, null);
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

        #region GPS监听
        public void OnLocationChanged(Location location)
        {
            string s = string.Format("{0}   {1}", location.Longitude, location.Latitude);
            Toast.MakeText(ApplicationContext, s, ToastLength.Short).Show();
        }

        public void OnProviderDisabled(string provider)
        {

        }

        public void OnProviderEnabled(string provider)
        {

        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }
        #endregion
    }
}

