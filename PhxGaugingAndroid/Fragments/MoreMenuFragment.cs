using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;

namespace PhxGaugingAndroid.Fragments
{
    public class MoreMenuFragment : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        public static MoreMenuFragment NewInstance()
        {
            var frag4 = new MoreMenuFragment { Arguments = new Bundle() };
            return frag4;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.MoreMenu, container, false);
            TextView tvGauging = v.FindViewById<TextView>(Resource.Id.tvGauging);
            tvGauging.Click -= TvGauging_Click;
            tvGauging.Click += TvGauging_Click;
            TextView tvBackGroud = v.FindViewById<TextView>(Resource.Id.tvBackGroud);
            tvBackGroud.Click -= TvBackGroud_Click;
            tvBackGroud.Click += TvBackGroud_Click;
            TextView tvBackGroudGauging = v.FindViewById<TextView>(Resource.Id.tvBackGroudGauging);
            tvBackGroudGauging.Click -= TvBackGroudGauging_Click;
            tvBackGroudGauging.Click += TvBackGroudGauging_Click;
            TextView tvCalibration = v.FindViewById<TextView>(Resource.Id.tvCalibration);
            tvCalibration.Click -= TvCalibration_Click;
            tvCalibration.Click += TvCalibration_Click;
            TextView tvStayTime = v.FindViewById<TextView>(Resource.Id.tvStayTime);
            TextView tvTestingTime = v.FindViewById<TextView>(Resource.Id.tvTestingTime);
            TextView LoginOut = v.FindViewById<TextView>(Resource.Id.LoginOut);
            TextView changeModel = v.FindViewById<TextView>(Resource.Id.changeModel);
            TextView About = v.FindViewById<TextView>(Resource.Id.About);
            LoginOut.Click -= LoginOut_click;
            LoginOut.Click += LoginOut_click;
            changeModel.Click -= ChangeModel_Click;
            changeModel.Click += ChangeModel_Click;
            tvStayTime.Click -= TvStayTime_Click;
            tvStayTime.Click += TvStayTime_Click;
            tvTestingTime.Click -= tvTestingTime_Click;
            tvTestingTime.Click += tvTestingTime_Click;
            About.Click -= About_Click;
            About.Click += About_Click;
            TextView tvPhxLog = v.FindViewById<TextView>(Resource.Id.tvPhxLog);
            tvPhxLog.Click -= TvPhxLog_Click;
            tvPhxLog.Click += TvPhxLog_Click;
            TextView tvHelp = v.FindViewById<TextView>(Resource.Id.tvHelp);
            tvHelp.Click -= TvHelp_Click;
            tvHelp.Click += TvHelp_Click;
            TextView tvExepcModel = v.FindViewById<TextView>(Resource.Id.tvExepcModel);
            tvExepcModel.Click -= TvExepcModel_Click;
            tvExepcModel.Click += TvExepcModel_Click;
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            if (LDARDevice != "EXEPC3100" && LDARDevice != "TVA2020")
            {
                var vExepcModel = v.FindViewById<View>(Resource.Id.vExepcModel);
                vExepcModel.Visibility = ViewStates.Gone;
                var rlExepcModel = v.FindViewById<RelativeLayout>(Resource.Id.rlExepcModel);
                rlExepcModel.Visibility = ViewStates.Gone;
            }
            return v;
        }
        AlertDialog dialog;
        Spinner spDevice;
        Button buttonConfirm;
        private void ChangeModel_Click(object sender, EventArgs e)
        {
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            AlertDialog.Builder builder = new AlertDialog.Builder(this.Activity);
            LayoutInflater inflater = this.Activity.LayoutInflater;
            View layout = inflater.Inflate(Resource.Layout.ChangeModel, (ViewGroup)this.Activity.FindViewById<ViewGroup>(Resource.Id.rlModel));
            spDevice = layout.FindViewById<Spinner>(Resource.Id.spModel);
            List<string> DeviceList = new List<string>() { "Phx21", "EXEPC3100", "TVA2020" };
            DeviceList.Remove(LDARDevice);
            ArrayAdapter tempArrayAdapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem2, DeviceList);
            tempArrayAdapter.SetDropDownViewResource(Resource.Layout.ListViewItem);
            spDevice.Adapter = tempArrayAdapter;
            buttonConfirm = layout.FindViewById<Button>(Resource.Id.btnConfirm);
            buttonConfirm.Click += ButtonConfirm_Click;
            builder.SetView(layout);
            builder.SetTitle("请选择要切换的设备类型");
            builder.SetCancelable(true);
            dialog = builder.Show();
        }

        private void ButtonConfirm_Click(object sender, EventArgs e)
        {
            string select = spDevice.SelectedItem.ToString();
            UserPreferences.SetString("SelectLDARDevice", select);
            this.Activity.Finish();
            if (select == "EXEPC3100")
            {
                this.Activity.StartActivity(typeof(MainExepcActivity));
            }
            else if (select == "TVA2020")
            {
                this.Activity.StartActivity(typeof(MainTvaActivity));
            }
            else
            {
                this.Activity.StartActivity(typeof(MainActivity));
            }
        }

        private void SpDevice_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            UserPreferences.SetString("SelectLDARDevice", spDevice.SelectedItem.ToString());
        }

        private void TvExepcModel_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(ExepcModelActivity));
        }

        /// <summary>
        /// 帮助
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvHelp_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(HelperActivity));
        }

        /// <summary>
        /// 检测设备日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvPhxLog_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(PhxLogActivity));
        }

        /// <summary>
        /// 停留时间设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvStayTime_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(StayTimeActivity));
        }
        /// <summary>
        /// 检测设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvTestingTime_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(TestingTimeActivity));
        }
        /// <summary>
        /// 新增环境背景值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvBackGroudGauging_Click(object sender, EventArgs e)
        {
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            if (LDARDevice == "EXEPC3100")
            {
                this.Activity.StartActivity(typeof(BackGroudGaugingActivityExepc));
            }
            else if (LDARDevice == "TVA2020")
            {
                this.Activity.StartActivity(typeof(BackGroudGaugingActivityTva));
            }
            else
            {
                this.Activity.StartActivity(typeof(BackGroudGaugingActivity));
            }
        }

        /// <summary>
        /// 背景值记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvBackGroud_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(BackGroudListActivity));
        }
        /// <summary>
        /// 检测值记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvGauging_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(GaugingRecordActivity));
        }
        /// <summary>
        /// 校准记录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TvCalibration_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(CalibrationListActivity));
        }
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoginOut_click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this.Activity);
            builder.SetTitle("提示");
            builder.SetMessage("确定要退出当前用户吗?");
            builder.SetPositiveButton("确认", (o, v) =>
            {
                this.Activity.Finish();
                this.Activity.StartActivity(typeof(LoginActivity));
                UserPreferences.DeleteString("LoginState");
                UserPreferences.DeleteString("LoginUser");
                UserPreferences.SetString("LoginState", "Out");
            });
            builder.SetNegativeButton("取消", (o, v) =>
            {

            });
            builder.Show();
        }
        /// <summary>
        /// 关于
        /// </summary>
        private void About_Click(object sender, EventArgs e)
        {
            this.Activity.StartActivity(typeof(AboutActivity));
        }
    }
}