using Android.Content;
using Android.OS;
using Android.Provider;
using Android.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using PhxGauging.Common;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using Syncfusion.XlsIO;
using Syncfusion.XlsIO.Implementation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Linq;
using ExcelDataReader;
using System.Threading.Tasks;
using System.Reflection;
using Newtonsoft.Json;
using static Android.Graphics.Bitmap;
using Microsoft.AspNet.SignalR.Client;
using System.Net.Http;
using System.Net;
using Android.Views.InputMethods;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PhxGaugingAndroid.Fragments
{
    public class WorkOrderFragment : Fragment
    {
        public delegate void GotoGroupEventHandler(Fragment f);
        public event GotoGroupEventHandler GotoGroup;
        public override void OnCreate(Bundle savedInstanceState)
        {

            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
        }
        Activity context;
        public static WorkOrderFragment NewInstance()
        {
            var frag = new WorkOrderFragment { Arguments = new Bundle() };
            return frag;
        }
        //工单数据列表
        private List<AndroidWorkOrder> workOrderList = new List<AndroidWorkOrder>();
        ListView li;
        DialogService dialogService;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.WorkOrder, container, false);
            li = v.FindViewById<ListView>(Resource.Id.liOrder);
            li.ItemClick -= Li_ItemClick;
            li.ItemClick += Li_ItemClick;
            workOrderList = GetWorkOrderList();
            li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
            Button btnImp = v.FindViewById<Button>(Resource.Id.btnImp);
            btnImp.Click -= BtnImp_Click;
            btnImp.Click += BtnImp_Click;
            Button btnDown = v.FindViewById<Button>(Resource.Id.btnDown);
            btnDown.Click -= BtnDown_Click;
            btnDown.Click += BtnDown_Click;
            RegisterForContextMenu(li);
            dialogService = new DialogService();
            return v;
        }
        #region 下载工单
        AlertDialog dialog;
        Spinner spApi;
        List<AndroidServerUrl> serverList;
        EditText etCompany;
        EditText etUser;
        EditText etPwd;
        OrderList OrderList;
        Spinner spWorkOrder;
        Spinner spCheckWorkOrder;
        Spinner spRadomCheckWorkOrder;
        LinearLayout lineLoginAPI;
        LinearLayout lineDownAPI;
        LinearLayout lineWorkOrder;
        LinearLayout lineCheckWorkOrder;
        LinearLayout lineRadomCheckWorkOrder;
        RadioGroup OrderType;
        Button btnDownLogin;
        TextView tvMsgOrder;
        private void BtnDown_Click(object sender, EventArgs e)
        {
            LayoutInflater inflater = this.Activity.LayoutInflater;
            View layout = inflater.Inflate(Resource.Layout.WorkOrderDown, (ViewGroup)this.Activity.FindViewById<ViewGroup>(Resource.Id.rlDown));
            AlertDialog.Builder builder = new AlertDialog.Builder(this.Activity);
            builder.SetView(layout);
            builder.SetCancelable(false);
            //登录
            btnDownLogin = layout.FindViewById<Button>(Resource.Id.btnDownLogin);
            btnDownLogin.Click -= BtnDownLogin_Click;
            btnDownLogin.Click += BtnDownLogin_Click;
            var close = layout.FindViewById<ImageView>(Resource.Id.close);
            close.Click -= DownClose_Click;
            close.Click += DownClose_Click;
            spApi = layout.FindViewById<Spinner>(Resource.Id.spApi);
            List<string> areas = new List<string>();
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            serverList = JsonConvert.DeserializeObject<List<AndroidServerUrl>>(UserPreferences.GetString("API" + JsonModel.LoginName));
            foreach (var item in serverList)
            {
                areas.Add(item.PlatformName);
            }
            ArrayAdapter tempArrayAdapter=new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, areas);
            //tempArrayAdapter.SetDropDownViewResource(Resource.Layout.item_drop);
            spApi.Adapter = tempArrayAdapter;
            spApi.Prompt = "请选择服务器";
            string selectApi = UserPreferences.GetString("SelectApiAdress");
            if (!string.IsNullOrEmpty(selectApi))
            {
                int index = areas.FindIndex(c => c == selectApi);
                if (index != -1)
                {
                    spApi.SetSelection(index);
                }
            }
            spApi.ItemSelected += SpApi_ItemSelected;
            etCompany = layout.FindViewById<EditText>(Resource.Id.etCompany);
          
            etUser = layout.FindViewById<EditText>(Resource.Id.etUser);
            etPwd = layout.FindViewById<EditText>(Resource.Id.etPwd);
//#if DEBUG
//            etCompany.Text = "30001";
//            etUser.Text = "30001_admin";
//            etPwd.Text = "111111";
//#endif
            #region 保存登录成功的用户
            string encryptUserStr= UserPreferences.GetString("DownLoadUserInfo");
            if(!string.IsNullOrWhiteSpace(encryptUserStr))
            {
                string encryptUserJson = Utility.DecryptDES(encryptUserStr);
                AndroidUser downUser = JsonConvert.DeserializeObject<AndroidUser>(encryptUserJson);
                if (downUser != null)
                {
                    etCompany.Text = downUser.CompanyCode ?? "";
                    etUser.Text = downUser.LoginName ?? "";
                }
            }
        
           
           
            #endregion

            spWorkOrder = layout.FindViewById<Spinner>(Resource.Id.spWorkOrder);
            spCheckWorkOrder = layout.FindViewById<Spinner>(Resource.Id.spCheckWorkOrder);
            spRadomCheckWorkOrder = layout.FindViewById<Spinner>(Resource.Id.spRadomCheckWorkOrder);
            lineLoginAPI = layout.FindViewById<LinearLayout>(Resource.Id.lineLoginAPI);
            lineDownAPI = layout.FindViewById<LinearLayout>(Resource.Id.lineDownAPI);
            lineWorkOrder = layout.FindViewById<LinearLayout>(Resource.Id.lineWorkOrder);
            lineCheckWorkOrder = layout.FindViewById<LinearLayout>(Resource.Id.lineCheckWorkOrder);
            lineRadomCheckWorkOrder = layout.FindViewById<LinearLayout>(Resource.Id.lineRadomCheckWorkOrder);
            OrderType = layout.FindViewById<RadioGroup>(Resource.Id.OrderType);
            OrderType.CheckedChange -= OrderType_CheckedChange;
            OrderType.CheckedChange += OrderType_CheckedChange;
            //下载
            var btnDownStart = layout.FindViewById<Button>(Resource.Id.btnDownStart);
            btnDownStart.Click -= BtnDownStart_ClickAsync;
            btnDownStart.Click += BtnDownStart_ClickAsync;
            var btnDownReturn = layout.FindViewById<Button>(Resource.Id.btnDownReturn);
            btnDownReturn.Click -= BtnDownReturn_Click;
            btnDownReturn.Click += BtnDownReturn_Click;
            tvMsgOrder = layout.FindViewById<TextView>(Resource.Id.tvMsgOrder);
            dialog = builder.Show();
        }

        private void SpApi_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            UserPreferences.SetString("SelectApiAdress", spApi.SelectedItem.ToString());
        }

        private void BtnDownReturn_Click(object sender, EventArgs e)
        {
            lineLoginAPI.Visibility = ViewStates.Visible;
            lineDownAPI.Visibility = ViewStates.Gone;
        }

        private void OrderType_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton1)
            {
                lineWorkOrder.Visibility = ViewStates.Visible;
                lineCheckWorkOrder.Visibility = ViewStates.Gone;
                lineRadomCheckWorkOrder.Visibility = ViewStates.Gone;
                if (spWorkOrder.SelectedItemPosition >= 0 && workOrderList.Exists(k => k.ID == OrderList.WorkOrderList[spWorkOrder.SelectedItemPosition].Id))
                {
                    tvMsgOrder.Text = "此工单已经下载过！！！";
                }
                else
                {
                    tvMsgOrder.Text = "";
                }
            } else if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton2)
            {
                lineCheckWorkOrder.Visibility = ViewStates.Visible;
                lineWorkOrder.Visibility = ViewStates.Gone;
                lineRadomCheckWorkOrder.Visibility = ViewStates.Gone;
               
                if (spCheckWorkOrder.SelectedItemPosition >= 0 && workOrderList.Exists(k => k.ID == OrderList.CheckWorkOrderList[spCheckWorkOrder.SelectedItemPosition].Id))
                {
                    tvMsgOrder.Text = "此工单已经下载过！！！";
                }
                else
                {
                    tvMsgOrder.Text = "";
                }
              
            }
            else
            {
                lineRadomCheckWorkOrder.Visibility = ViewStates.Visible;
                lineWorkOrder.Visibility = ViewStates.Gone;
                lineCheckWorkOrder.Visibility = ViewStates.Gone;
                if (spRadomCheckWorkOrder.SelectedItemPosition >= 0 && workOrderList.Exists(k => k.ID == OrderList.RadomCheckWorkOrderList[spRadomCheckWorkOrder.SelectedItemPosition].Id))
                {
                    tvMsgOrder.Text = "此工单已经下载过！！！";
                }
                else
                {
                    tvMsgOrder.Text = "";
                }
            }
        }

        private void BtnDownLogin_Click(object sender, EventArgs e)
        {
            if (spApi.SelectedItemPosition == -1 || etCompany.Text.Trim() == string.Empty || etUser.Text.Trim() == string.Empty || etPwd.Text.Trim() == string.Empty)
                return;
            AndroidServerUrl server = serverList[spApi.SelectedItemPosition];
            Toast toast = Toast.MakeText(this.Activity, "", ToastLength.Short);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            BaseResult<OrderList> msg = null;
            LoginModel model = new LoginModel { CompanyId = etCompany.Text.Trim(), LoginName = etUser.Text.Trim(), UserPass = etPwd.Text.Trim() };
            msg = Utility.CallService<OrderList>(server.ApiAddress, "V10AppPC/AppTest/LoginAPIServer", model, 30000, null);
            if (msg == null)
            {
                toast.SetText("网络连接失败");
                toast.Show();
            }
            else if (msg.Code == 10000)
            {
                #region 保存登录成功的用户
                AndroidUser downUser = new AndroidUser()
                {
                    CompanyCode = model.CompanyId,
                    LoginName = model.LoginName
                };
                string StringJson = JsonConvert.SerializeObject(downUser);
                string json = Utility.EncryptDES(StringJson);
                UserPreferences.SetString("DownLoadUserInfo", json); 
                #endregion

                this.Activity.RunOnUiThread(delegate
                {
                    var btn = sender as Button;
                    InputMethodManager mInputMethodManager = (InputMethodManager)this.Activity.GetSystemService(Context.InputMethodService);
                    mInputMethodManager.HideSoftInputFromWindow(btn.WindowToken, 0);
                });
                OrderList = msg.Data;
                //检测
                List<string> workList = new List<string>();
                foreach (var item in OrderList.WorkOrderList)
                {
                    workList.Add(item.WorkOrderName);
                }
                spWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, workList);
                spWorkOrder.Prompt = "请选择检测工单";
                spWorkOrder.ItemSelected -= SpWorkOrder_ItemSelected;
                spWorkOrder.ItemSelected += SpWorkOrder_ItemSelected;
                //抽检
                List<string> orderList = new List<string>();
                foreach (var item in OrderList.CheckWorkOrderList)
                {
                    orderList.Add(item.CheckWorkOrderName);
                }
                spCheckWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, orderList);
                spCheckWorkOrder.Prompt = "请选择复检工单";
                spCheckWorkOrder.ItemSelected -= SpCheckWorkOrder_ItemSelected;
                spCheckWorkOrder.ItemSelected += SpCheckWorkOrder_ItemSelected;
                //复检
                List<string> radomOrderList = new List<string>();
                if (radomOrderList != null)
                {
                    foreach (var item in OrderList.RadomCheckWorkOrderList)
                    {
                        radomOrderList.Add(item.CompanyId + "-" + item.LdarDeviceId);//企业编码+装置名称
                    }
                }
                spRadomCheckWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, radomOrderList);
                spRadomCheckWorkOrder.Prompt = "请选择抽检工单";
                spRadomCheckWorkOrder.ItemSelected -= SpRadomCheckWorkOrder_ItemSelected;
                spRadomCheckWorkOrder.ItemSelected += SpRadomCheckWorkOrder_ItemSelected;

                lineLoginAPI.Visibility = ViewStates.Gone;
                lineDownAPI.Visibility = ViewStates.Visible;
            }
            else
            {
                toast.SetText(msg.Message);
                toast.Show();
            }
        }

        private void SpCheckWorkOrder_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (workOrderList.Exists(k => k.ID == OrderList.CheckWorkOrderList[e.Position].Id))
            {
                var et = e.View as TextView;
                et.SetTextColor(Android.Graphics.Color.ParseColor("#FF0000"));
                tvMsgOrder.Text = "此工单已经下载过！！！";
            }
            else
            {
                tvMsgOrder.Text = "";
            }
        }

        private void SpWorkOrder_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (workOrderList.Exists(k => k.ID == OrderList.WorkOrderList[e.Position].Id))
            {
                var et = e.View as TextView;
                et.SetTextColor(Android.Graphics.Color.ParseColor("#FF0000"));
                tvMsgOrder.Text = "此工单已经下载过！！！";
            }
            else
            {
                tvMsgOrder.Text = "";
            }
        }

        #region 抽检
        private void SpRadomCheckWorkOrder_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (workOrderList.Exists(k => k.ID == OrderList.RadomCheckWorkOrderList[e.Position].Id))
            {
                var et = e.View as TextView;
                et.SetTextColor(Android.Graphics.Color.ParseColor("#FF0000"));
                tvMsgOrder.Text = "此工单已经下载过！！！";
            }
            else
            {
                tvMsgOrder.Text = "";
            }
        }

        private void SpRadomWorkOrder_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (workOrderList.Exists(k => k.ID == OrderList.RadomCheckWorkOrderList[e.Position].Id))
            {
                var et = e.View as TextView;
                et.SetTextColor(Android.Graphics.Color.ParseColor("#FF0000"));
                tvMsgOrder.Text = "此工单已经下载过！！！";
            }
            else
            {
                tvMsgOrder.Text = "";
            }
        } 
        #endregion

        public IHubProxy HubProxy { get; set; }
        public HubConnection Connection { get; set; }
        ProgressDialog pgdilog;
        /// <summary>
        /// 下载工单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownStart_ClickAsync(object sender, EventArgs e)
        {
            if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton1 && spWorkOrder.SelectedItemPosition == -1)
                return;
            if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton2 && spCheckWorkOrder.SelectedItemPosition == -1)
                return;
            if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton3 && spRadomCheckWorkOrder.SelectedItemPosition == -1)
                return;
            Toast toast = Toast.MakeText(this.Activity, "", ToastLength.Short);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "正在等待服务器处理……", true, false);
            pgdilog = new ProgressDialog(this.Activity);
            Task.Factory.StartNew(() =>
            {
                AndroidServerUrl server = serverList[spApi.SelectedItemPosition];
                BaseResult<string> msg = null;
                if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton1)
                {
                    LdarWorkOrder model = OrderList.WorkOrderList[spWorkOrder.SelectedItemPosition];
                    msg = Utility.CallService<string>(server.ApiAddress, "V10AppPC/AppTest/DownLoadWorkOrder", model, 3000000, null);
                }
                else if (OrderType.CheckedRadioButtonId == Resource.Id.radioButton2)
                {
                    LdarCheckWorkOrder model = OrderList.CheckWorkOrderList[spCheckWorkOrder.SelectedItemPosition];
                    msg = Utility.CallService<string>(server.ApiAddress, "V10AppPC/AppTest/DownLoadCheckWorkOrder", model, 3000000, null);
                }
                else
                {
                    LdarRadomCheckWorkOrder model = OrderList.RadomCheckWorkOrderList[spRadomCheckWorkOrder.SelectedItemPosition];
                    msg = Utility.CallService<string>(server.ApiAddress, "V10AppPC/AppTest/DownLoadExtractTaskOrder", model, 3000000, null);
                }
                dilog.Dismiss();
                if (msg == null)
                {
                    toast.SetText("网络连接失败");
                    toast.Show();
                }
                else if (msg.Code == 10000)
                {
                    int i = msg.Data.IndexOf("filename=");
                    int j = msg.Data.IndexOf("&id=");
                    int k = msg.Data.IndexOf("Zip\\");
                    string fileName = msg.Data.Substring(k + 4, j - k - 4);
                    string workOrderID = msg.Data.Substring(j + 4);
                    string path = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + fileName;
                    string url = msg.Data.Substring(0, j);
                    DownLoadHttpClient client = new DownLoadHttpClient(url, path);
                    client.ProgressChanged += Client_ProgressChanged;
                    pgdilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    pgdilog.Indeterminate = false;
                    pgdilog.SetCancelable(false);
                    pgdilog.SetCanceledOnTouchOutside(false);
                    pgdilog.Max = 100;
                    pgdilog.SetTitle("工单数据导入中，请不要进行任何操作");
                    pgdilog.SetMessage("正在下载文件");
                    this.Activity.RunOnUiThread(() =>
                    {
                        pgdilog.Show();
                        dialog.Hide();
                    });
                    client.StartDownload().ContinueWith(t =>
                    {
                        DateTime opreateTime = DateTime.Now;
                        try
                        {
                            string excel = UnzipDataFile(pgdilog, path, opreateTime);
                            ImpSqlite(pgdilog, excel, opreateTime, workOrderID, OrderList.UserID, server);
                            //导入完成删除文件
                            Java.IO.File f = new Java.IO.File(path);
                            f.Delete();
                            this.Activity.RunOnUiThread(delegate
                            {
                                //重新获取工单列表
                                workOrderList = GetWorkOrderList();
                                //重新加载列表
                                li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog("导入工单数据", ex);
                            this.Activity.RunOnUiThread(delegate
                            {
                                Toast.MakeText(this.Activity, "导入工单数据发生错误:" + ex.Message, ToastLength.Short).Show();
                            });
                        }
                        finally
                        {
                            pgdilog.Dismiss();
                            dialog.Dismiss();
                        }
                    });

                }
                else
                {
                    toast.SetText(msg.Message);
                    toast.Show();
                }
            });
        }

        private void Client_ProgressChanged(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage)
        {
            pgdilog.Progress = (int)progressPercentage;
        }


        /// <summary>
        /// 下载窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownClose_Click(object sender, EventArgs e)
        {
            dialog.Dismiss();
        }
        #endregion
        private void Li_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var workOrder = workOrderList[e.Position];
            GotoGroup(new GroupFragment(workOrder));
        }
        /// <summary>
        /// 导入工单数据处理
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="resultCode"></param>
        /// <param name="data"></param>
        public override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == PickZipId && resultCode == Android.App.Result.Ok && data != null)
            {
                string path = GetPath(this.Activity, data.Data);
                File file = new File(path);
                if (file.Exists() && path.ToLower().Contains(".zip"))
                {
                    Android.App.ProgressDialog dilog = new Android.App.ProgressDialog(this.Activity);
                    dilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    dilog.Indeterminate = false;
                    dilog.SetCancelable(false);
                    dilog.SetCanceledOnTouchOutside(false);
                    dilog.Max = 100;
                    dilog.SetTitle("工单数据导入中，请不要进行任何操作");
                    dilog.SetMessage("正在加载文件");
                    dilog.Show();
                    DateTime opreateTime = DateTime.Now;
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            string excel = UnzipDataFile(dilog, path, opreateTime);
                            ImpSqlite(dilog, excel, opreateTime);
                            this.Activity.RunOnUiThread(delegate
                            {
                                //重新获取工单列表
                                workOrderList = GetWorkOrderList();
                                //重新加载列表
                                li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            });
                            LogHelper.InfoLog(file.Name + "工单数据导入成功");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog("导入工单数据", ex);
                            this.Activity.RunOnUiThread(delegate
                            {
                                Toast.MakeText(this.Activity, "导入工单数据发生错误:" + ex.Message, ToastLength.Short).Show();
                            });
                        }
                        finally
                        {
                            dilog.Dismiss();
                        }
                    });
                }
                else
                {
                    Toast.MakeText(this.Context, "请选择工单压缩zip文件", ToastLength.Short).Show();
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }
        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="menu"></param>
        /// <param name="v"></param>
        /// <param name="menuInfo"></param>
        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.liOrder)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle(workOrderList[info.Position].WorkOrderName.ToString() + workOrderList[info.Position].OperateTime.ToString());
                string[] menuItems = new string[7] { "删除工单", null, null, null, "已检测密封点记录", null, null };
                string CrrentUser = UserPreferences.GetString("CrrentUser");
                string json = Utility.DecryptDES(CrrentUser);
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                if (JsonModel.IsBatchTest == 1)
                {
                    menuItems[5] = "批量检测模式";
                }
                //if (workOrderList[info.Position].CompleteCount == (workOrderList[info.Position].SealPointCount - workOrderList[info.Position].UnReachCount))
                if (workOrderList[info.Position].CompleteCount == workOrderList[info.Position].SealPointCount)
                {
                    menuItems[1] = "检测人签名";
                    menuItems[2] = "审核人签名";
                    menuItems[3] = "导出工单";
                    if (!string.IsNullOrEmpty(workOrderList[info.Position].ApiUrl))
                    {
                        menuItems[6] = "上传工单";
                    }
                }
                for (var i = 0; i < menuItems.Length; i++)
                {
                    if (menuItems[i] == null)
                        continue;
                    menu.Add(Menu.None, i, i, menuItems[i]);
                }
            }
        }
        AdapterView.AdapterContextMenuInfo info;
        /// <summary>
        /// 菜单事件
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var menuItemName = item.ToString();
            if (menuItemName == "删除工单")
            {
                this.Activity.RunOnUiThread(delegate
                {
                    dialogService.ShowYesNo(this.Activity, "提示", "是否要删除此工单？删除后无法恢复！！！",
                    r =>
                    {
                        if (r == DialogResult.Yes)
                        {
                            AndroidWorkOrder order = workOrderList[info.Position];
                            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(order.DataPath.Replace("sqlite.db", ""));
                            dir.Delete(true);
                            workOrderList.RemoveAt(info.Position);
                            li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            Toast.MakeText(this.Activity, "已删除" + order.WorkOrderName + "检测工单", ToastLength.Short).Show();
                        }
                    });
                });
                return true;
            }
            else if (menuItemName == "检测人签名" || menuItemName == "审核人签名")
            {
                Intent i = new Intent(this.Activity, typeof(SignaturePadActivity));
                i.PutExtra("SignatureType", menuItemName);
                AndroidWorkOrder order = workOrderList[info.Position];
                i.PutExtra("WorkOrderPath", order.DataPath.Replace("sqlite.db", ""));
                i.PutExtra("WorkOrderName", order.WorkOrderName);
                this.StartActivity(i);
            }
            else if (menuItemName == "导出工单")
            {
                var NetworkState = UserPreferences.GetString("NetworkState");
                if (NetworkState.Equals("离线"))
                {
                    Toast.MakeText(this.Activity, "离线登录不能导出,请退出登录后重新在线登录", ToastLength.Long).Show();
                    return true;
                }
                AndroidWorkOrder order = workOrderList[info.Position];

                if (IsSignaturePass(order.DataPath.Replace("sqlite.db", ""),order) == false)
                {
                    return true;
                }
                Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "工单数据导出中，请不要进行任何操作！！！", true, false);
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //装置数据
                        List<dynamic> excelList = BuidWorkOrderExcelData(order);
                        //生成Excel文件
                        string path = CreateExcelFile(order.WorkOrderName, excelList, workOrderList[info.Position].ApiUrl, order.DataPath.Replace("sqlite.db", ""));
                        this.Activity.RunOnUiThread(delegate
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this.Activity, order.WorkOrderName + "导出成功", "导出文件存储在：" + path, null);
                        });
                        LogHelper.InfoLog(order.WorkOrderName + "工单数据导出成功");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog("导出工单数据", ex);
                        this.Activity.RunOnUiThread(delegate
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this.Activity, "导出工单数据发生错误", ex.Message, null);
                        });
                    }
                    finally
                    {
                        dilog.Dismiss();
                    }
                });
                return true;
            }
            else if (menuItemName == "批量检测模式")
            {
                Intent i = new Intent(this.Activity, typeof(BatchModeActivity));
                Bundle b = new Bundle();
                b.PutString("AndroidWorkOrder", Newtonsoft.Json.JsonConvert.SerializeObject(workOrderList[info.Position]));
                i.PutExtras(b);
                this.StartActivity(i);
                return true;
            }
            else if (menuItemName == "已检测密封点记录")
            {
                Intent i = new Intent(this.Activity, typeof(DetectionActivity));
                Bundle b = new Bundle();
                b.PutString("AndroidWorkOrder", Newtonsoft.Json.JsonConvert.SerializeObject(workOrderList[info.Position]));
                i.PutExtras(b);
                this.StartActivity(i);
                return true;
            }
            else if (menuItemName == "上传工单")
            {
                var NetworkState = UserPreferences.GetString("NetworkState");
                if (NetworkState.Equals("离线"))
                {
                    Toast.MakeText(this.Activity, "离线登录不能上传,请退出登录后重新在线登录", ToastLength.Long).Show();
                    return true;
                }
                AndroidWorkOrder order = workOrderList[info.Position];

                if (IsSignaturePass(order.DataPath.Replace("sqlite.db", ""), order) == false)
                {
                    return true;
                }
                if (workOrderList[info.Position].UploadTime != null)
                {
                    DialogService dialogService = new DialogService();
                    dialogService.ShowYesNo(this.Activity, "提示", workOrderList[info.Position].UploadTime.ToString() + "已经上传过，是否需要再一次上传",
                            r =>
                            {
                                if (r == DialogResult.Yes)
                                {
                                    UpLoarWorkOrder(info);
                                }
                            });
                }
                else
                {
                    UpLoarWorkOrder(info);
                }

                return true;
            }
            return false;
        }
        /// <summary>
        /// 签名是否合格
        /// </summary>
        /// <param name="dataDir"></param>
        /// <returns></returns>
        private bool IsSignaturePass(string dataDir, AndroidWorkOrder order)
        {
            var signafile = System.IO.Path.Combine(dataDir, "signature.Png");
            if (System.IO.File.Exists(signafile) == false)
            {
                Toast.MakeText(this.Activity, "检测人请签名", ToastLength.Short).Show();
                return false;
            }

            //审核人签名
          
            string CheckName = UserPreferences.GetString(order.WorkOrderName);
            if (string.IsNullOrWhiteSpace(CheckName))
            {
                Toast.MakeText(this.Activity, "请审核人签名", ToastLength.Short).Show();
                return false;
            }

            bool isDoubleCheck = false;
            string doubleCheck = UserPreferences.GetString("DoubleCheck");
            if (doubleCheck != null && doubleCheck != string.Empty)
            {
                isDoubleCheck = bool.Parse(doubleCheck);
            }
            if (isDoubleCheck)
            {
                var signafileCheck = System.IO.Path.Combine(dataDir, "signatureDouble.Png");
                if (System.IO.File.Exists(signafileCheck) == false)
                {
                    Toast.MakeText(this.Activity, "请审核人手写签名", ToastLength.Short).Show();
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 上传工单
        /// </summary>
        /// <param name="info"></param>
        private void UpLoarWorkOrder(AdapterView.AdapterContextMenuInfo info)
        {
            Toast toast = Toast.MakeText(this.Activity, "", ToastLength.Short);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "提示", "工单数据生成中，请不要进行任何操作！！！", true, false);
            pgdilog = new ProgressDialog(this.Activity);
            ConnectAsync(workOrderList[info.Position], dilog, toast);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //装置数据
                    AndroidWorkOrder order = workOrderList[info.Position];
                    List<dynamic> excelList = BuidWorkOrderExcelData(order);
                    //生成Excel文件
                    string fullfilename = CreateExcelFile(order.WorkOrderName, excelList, workOrderList[info.Position].ApiUrl, order.DataPath.Replace("sqlite.db", ""));
                    //2.上传Excel                
                    pgdilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    pgdilog.Indeterminate = false;
                    pgdilog.SetCancelable(false);
                    pgdilog.SetCanceledOnTouchOutside(false);
                    pgdilog.Max = 100;
                    pgdilog.SetTitle("工单数据上传中，请不要进行任何操作");
                    pgdilog.SetMessage("正在上传文件");
                    this.Activity.RunOnUiThread(() =>
                    {
                        dilog.Hide();
                        pgdilog.Show();
                    });
                    Uri server = new Uri(workOrderList[info.Position].ApiUrl + "/V10AppPC/AppTest/Post");
                    IList<string> FilesName;
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("Authorization", "basic weixin|363F55B8-203E-4BE4-BB76-AAF7F99FF369" + ';');
                    MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                    string filename = System.IO.Path.GetFileName(fullfilename);
                    string filenameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullfilename);
                    StreamContent streamConent = new StreamContent(new System.IO.FileStream(fullfilename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read));
                    var uploadClient = new UploadHttpContent(streamConent, (sent, total) =>
                    {
                        pgdilog.Progress = (int)(sent * 100 / total);
                    });
                    multipartFormDataContent.Add(uploadClient, filenameWithoutExtension, filename);
                    HttpResponseMessage responseMessage = httpClient.PostAsync(server, multipartFormDataContent).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string content = responseMessage.Content.ReadAsStringAsync().Result;
                        FilesName = Newtonsoft.Json.JsonConvert.DeserializeObject<IList<string>>(content);
                        System.IO.File.Delete(fullfilename);
                    }
                    else
                    {
                        this.Activity.RunOnUiThread(() =>
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this.Activity, "提示", "上传工单数据失败", null);
                        });
                        return;
                    }
                    this.Activity.RunOnUiThread(() =>
                    {
                        dilog.SetTitle("服务器正在处理工单数据");
                        dilog.SetMessage("");
                    });
                    //3.通知服务器导入Excel                        
                    this.Activity.RunOnUiThread(() =>
                    {
                        pgdilog.Hide();
                        dilog.Show();
                    });
                    //获取工单信息
                    //检测
                    if (workOrderList[info.Position].WorkOrderType == 0)
                    {
                        BaseResult<LdarWorkOrder> msg = Utility.CallService<LdarWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Enterprise/LdarWorkOrder/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("网络连接失败");
                                toast.Show();
                            });
                        }
                        else if (msg.Code == 10000)
                        {
                            //调用服务器处理工单数据
                            HubProxy.Invoke("excelWorkPlanSealPointUploadComplete", filename, msg.Data.Id, workOrderList[info.Position].DownUserid, msg.Data.CompanyId, msg.Data.BeginDate.ToString(), msg.Data.EndDate.ToString(), msg.Data.InspectionCycleId, false, msg.Data.DynamicStaticType ?? 0, "", filename);
                        }
                        else
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText(msg.Message);
                                toast.Show();
                            });
                        }
                    }
                    //复检
                    else if (workOrderList[info.Position].WorkOrderType == 1)
                    {
                        BaseResult<LdarCheckWorkOrder> msg = Utility.CallService<LdarCheckWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Enterprise/LdarCheckWorkOrder/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("网络连接失败");
                                toast.Show();
                            });
                        }
                        else if (msg.Code == 10000)
                        {
                            HubProxy.Invoke("excelCheckWorkOrderUploadComplete", filename, msg.Data.Id, workOrderList[info.Position].DownUserid, msg.Data.CompanyId, false, "", filename);
                        }
                        else
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText(msg.Message);
                                toast.Show();
                            });
                        }
                    }
                    //抽检
                    else
                    {
                        BaseResult<LdarWorkOrder> msg = Utility.CallService<LdarWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Government/LdarExtractTask/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("网络连接失败");
                                toast.Show();
                            });
                        }
                        else if (msg.Code == 10000)
                        {
                            //调用服务器处理工单数据
                            HubProxy.Invoke("excelRadomCheckWorkOrderUploadComplete", filename, msg.Data.Id, workOrderList[info.Position].DownUserid, msg.Data.CompanyId,"");
                        }
                        else
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText(msg.Message);
                                toast.Show();
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("上传工单数据", ex);
                    this.Activity.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this.Activity, "上传工单数据发生错误", ex.Message, null);
                    });
                    dilog.Dismiss();
                    pgdilog.Dismiss();
                }
                finally
                {
                }
            });
        }

        /// <summary>
        /// 启动singR
        /// </summary>
        /// <param name="ServerUri"></param>
        private async void ConnectAsync(AndroidWorkOrder workOrder, ProgressDialog dialog, Toast toast)
        {
            Connection = new HubConnection(workOrder.ApiUrl);
            // 创建一个集线器代理对象
            HubProxy = Connection.CreateHubProxy("CommonHub");
            // 供服务端调用，将消息输出到消息列表框中
            HubProxy.On<AsynchronousExecutionStateModel>("apiBroadcastMessage", (message) =>
            this.Activity.RunOnUiThread(delegate
            {
                dialog.SetMessage(message.Message);
                if (message.SType == 200)
                {
                    dialog.Dismiss();
                    SQLite.SQLiteConnection connection = new SQLite.SQLiteConnection(workOrder.DataPath);
                    workOrder.UploadTime = DateTime.Now;
                    connection.Update(workOrder);
                    DialogService dig = new DialogService();
                    dig.ShowOk(this.Activity, "提示", "上传成功", null);
                }
                else if (message.SType == 500)
                {
                    dialog.Dismiss();
                    LogHelper.InfoLog("上传工单数据" + message.Message);
                    toast.SetText(message.Message);
                    toast.Show();
                }
                else
                {
                    if (dialog.IsShowing == false)
                    {
                        dialog.Show();
                    }
                }
            })
            );
            await Connection.Start();
        }
        /// <summary>
        /// 构建工单Excel数据
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private List<dynamic> BuidWorkOrderExcelData(AndroidWorkOrder order)
        {
            var connection = new SQLite.SQLiteConnection(order.DataPath);
            List<AndroidGroup> GroupList = connection.Table<AndroidGroup>().ToList();
            List<AndroidSealPoint> PointList = connection.Table<AndroidSealPoint>().ToList();
            List<dynamic> excelList = new List<dynamic>();

            string CheckName = UserPreferences.GetString(order.WorkOrderName);

            //bool isDoubleCheck = true;
            //string doubleCheck = UserPreferences.GetString("DoubleCheck");
            //if (doubleCheck != null && doubleCheck != string.Empty)
            //{
            //    isDoubleCheck = bool.Parse(doubleCheck);
            //}
            //string CheckName = string.Empty;
            //if (isDoubleCheck)
            //{
            //    CheckName = UserPreferences.GetString(order.WorkOrderName);
            //}
            foreach (var point in PointList)
            {
                AndroidGroup g = GroupList.Find(c => c.ID == point.GroupID);
                dynamic entity;
                //检测
                if (order.WorkOrderType == 0)
                {
                    entity = new WorkOrderDetectionExcelEntity();
                    entity.是否可达 = point.IsTouch;
                    entity.泄漏描述 = point.LeakageDescribe;
                    entity.检测人名称 = point.UserName;
                    entity.检测时间 = point.DetectionTime;
                }
                //复检
                else if (order.WorkOrderType == 1)
                {
                    entity = new WorkOrderRecheckExcelEntity();
                    entity.原净检测值 = point.LastNetPPM;
                    entity.复检人员 = point.UserName;
                    entity.复检时间 = point.DetectionTime;
                }
                //抽检
                else
                {
                    entity = new WorkOrderRadomCheckExcelEntity();
                    entity.是否可达 = point.IsTouch;
                    entity.泄漏描述 = point.LeakageDescribe;
                    entity.检测人名称 = point.UserName;
                    entity.检测时间 = point.DetectionTime;
                }
                entity.密封点编码 = point.SealPointCode;
                entity.装置名称 = g.DeviceName;
                entity.装置编码 = g.DeviceCode;
                entity.区域名称 = g.AreaName;
                entity.群组条形或二维码 = g.BarCode;
                entity.群组编码 = g.GroupCode;
                entity.扩展编码 = point.ExtCode;
                entity.群组位置描述 = g.GroupDescribe;
                if (point.SealPointType == "取样连接器")
                {
                    entity.密封点类型 = "取样连接系统";
                }
                else if (point.SealPointType == "开口管线")
                {
                    entity.密封点类型 = "开口阀或开口管线";

                }
                else
                {
                    entity.密封点类型 = point.SealPointType;
                }
                entity.环境本底值 = point.BackgroundPPM;
                entity.净检测值 = point.PPM;
                entity.泄漏检测值 = point.LeakagePPM;
                entity.温度 = point.Temperature;
                entity.风向 = point.WindDirection;
                entity.风速 = point.WindSpeed;
                entity.红牌编号 = point.RedCode;
                //entity.开始检测时间 = point.StartTime;
                //entity.结束检测时间 = point.EndTime;
                entity.停留时间 = point.WasteTime;
                entity.安防措施 = point.Security;
                entity.检测设备号 = point.PhxCode;
                entity.检测设备名称 = point.PhxName;
                entity.组件检测方式 = point.DetectionMode;
                entity.审核人 = CheckName;
                excelList.Add(entity);
            }

            return excelList;
        }

        /// <summary>
        /// 打开Excel文件
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static Intent OpenExcelFileIntent(string param)
        {
            Intent intent = new Intent("android.intent.action.VIEW");
            intent.AddCategory("android.intent.category.DEFAULT");
            intent.AddFlags(ActivityFlags.NewTask);
            Android.Net.Uri uri = Android.Net.Uri.FromFile(new File(param));
            intent.SetDataAndType(uri, "application/vnd.ms-excel");
            return intent;
        }


        public string CreateExcelFile(string OrderName, List<dynamic> excelList, string url, string dataDir)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + OrderName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
            ExcelEngine excelEngine = new ExcelEngine();
            excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2010;
            IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            IWorksheet worksheet1 = workbook.Worksheets.Create("检测人签名");
            var signafile = System.IO.Path.Combine(dataDir, "signature.Png");
            var signaStream = new System.IO.FileStream(signafile, System.IO.FileMode.Open);
            IPictureShape shape = worksheet1.Pictures.AddPicture(1, 1, signaStream);

            bool isDoubleCheck = false;
            string doubleCheck = UserPreferences.GetString("DoubleCheck");
            if (doubleCheck != null && doubleCheck != string.Empty)
            {
                isDoubleCheck = bool.Parse(doubleCheck);
            }
            System.IO.FileStream signaCheckStream = null;
            if (isDoubleCheck == true)
            {
                IWorksheet worksheet2 = workbook.Worksheets.Create("审核人签名");
                var signafileCheck = System.IO.Path.Combine(dataDir, "signatureDouble.Png");
                signaCheckStream = new System.IO.FileStream(signafileCheck, System.IO.FileMode.Open);
                worksheet2.Pictures.AddPicture(1, 1, signaCheckStream);
            }

  

            //worksheet1.PageSetup.BackgoundImage = new Syncfusion.Drawing.Image(pngStream);
            worksheet.ImportData(excelList, 1, 1, true);

          
          
            //worksheet.SetDefaultColumnStyle(20, DatecolumnStyle);
            if (excelList.Count > 0)
            {
                IStyle DatecolumnStyle = workbook.Styles.Add("DateColumnStyle");
                DatecolumnStyle.NumberFormat = "yyyy/mm/dd hh:mm:ss";
                string range = $"U2:U{excelList.Count+1}";
               // worksheet.Range[range].CellStyle = DatecolumnStyle;
                worksheet.SetDefaultColumnStyle(21, DatecolumnStyle);
            }
            string umax = "U" + (excelList.Count + 1).ToString();
          
            EnumDataType dataType;
            //检测
            if (excelList.First() as WorkOrderDetectionExcelEntity != null)
            {
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders.LineStyle = ExcelLineStyle.Thin;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].ShowDiagonalLine = true;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeTop].ShowDiagonalLine = true;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeRight].ShowDiagonalLine = true;
                //worksheet.Range["A1:AA" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].ShowDiagonalLine = true;
                worksheet.Range["A1:K1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["M1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["Q1:U1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                dataType = EnumDataType.Check;
            }
            //复检
            else if (excelList.First() as WorkOrderRecheckExcelEntity != null)
            {
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders.LineStyle = ExcelLineStyle.Thin;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.DiagonalDown].ShowDiagonalLine = false;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.DiagonalUp].ShowDiagonalLine = false;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeLeft].ShowDiagonalLine = true;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeTop].ShowDiagonalLine = true;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeRight].ShowDiagonalLine = true;
                //worksheet.Range["A1:Z" + excelList.Count.ToString()].CellStyle.Borders[ExcelBordersIndex.EdgeBottom].ShowDiagonalLine = true;
                worksheet.Range["A1:K1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["M1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["O1:S1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                dataType = EnumDataType.ReCheck;
            }
            else
            {
                worksheet.Range["A1:K1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["M1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["P1:S1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                worksheet.Range["U1:V1"].CellStyle.Color = Syncfusion.Drawing.Color.FromArgb(255, 255, 0);
                dataType = EnumDataType.Check;
            }
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            workbook.Close();
            excelEngine.Dispose();
            Java.IO.File file = new Java.IO.File(path);
            FileOutputStream outs = new FileOutputStream(file);
            outs.Write(stream.ToArray());
            outs.Flush();
            outs.Close();
            signaStream.Dispose();
            if (signaCheckStream != null)
            {
                signaCheckStream.Dispose();
            }
            //Aspose.Cells.Workbook wb = new Aspose.Cells.Workbook(path);
            //Aspose.Cells.Worksheet sheet = wb.Worksheets[0];
            //System.IO.FileStream fs = new System.IO.FileStream(signafile, System.IO.FileMode.Open);
            //byte[] imageData = new Byte[fs.Length];
            //fs.Read(imageData, 0, imageData.Length);
            //fs.Close();
            //sheet.SetBackground(imageData);
            //System.IO.MemoryStream asstream = new System.IO.MemoryStream();
            //workbook.SaveAs(asstream);
            //Java.IO.File asfile = new Java.IO.File(path);
            //FileOutputStream asouts = new FileOutputStream(asfile);
            //asouts.Write(asstream.ToArray());
            //asouts.Flush();
            //asouts.Close();

            path = FileHelper.GetHashCodeFromFile(path, dataType, url);
            return path;
        }

        #region 导入相关
        /// <summary>
        /// 导入SQLITE
        /// </summary>
        /// <param name="excel"></param>
        private void ImpSqlite(ProgressDialog dialog, string excel, DateTime createTime, string workOrderID = null, string userID = null, AndroidServerUrl server = null)
        {
            System.IO.FileStream fileStream = null;
            SQLite.SQLiteConnection sqlite = null;
            this.Activity.RunOnUiThread(delegate
            {
                dialog.SetMessage("正在导入密封点数据");
            });
            dialog.Progress = 0;
            try
            {
                fileStream = System.IO.File.Open(excel, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                DataTable dt = new DataTable();
                /*====================ExcelDataReader读取Excel开始============================*/
                dialog.Progress = 10;
                ExcelReaderConfiguration excelConfig = new ExcelReaderConfiguration { FallbackEncoding = System.Text.Encoding.UTF8 };
                dialog.Progress = 20;
                ExcelDataReader.IExcelDataReader reader = ExcelDataReader.ExcelReaderFactory.CreateOpenXmlReader(fileStream, excelConfig);
            
                dialog.Progress = 40;
                DataSet ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                    {
                        UseHeaderRow = true
                    }
                });
                dt = ds.Tables[0];
                int ColumnCount = dt.Columns.Count;
                dialog.Progress = 45;
                /*====================ExcelDataReader读取Excel结束============================*/
                /*====================Syncfusion读取Excel开始============================*/
                //ExcelEngine excelEngine = new ExcelEngine();
                //IApplication application = excelEngine.Excel;
                //IWorkbook workbook = application.Workbooks.Open(fileStream, ExcelOpenType.SpreadsheetML2010, ExcelParseOptions.DoNotParseCharts);
                //WorksheetImpl sheet = workbook.Worksheets[0] as WorksheetImpl;
                //int RowCount = sheet.CellRecords.LastRow;
                //int ColumnCount = sheet.CellRecords.LastColumn;

                //IRange range = sheet.Range[1, 1, RowCount, ColumnCount];
                //dt = CreateDataTable(range[1, 1, 1, ColumnCount], ColumnCount);
                //BuidData(dt, range, ColumnCount);
                /*==============================Syncfusion读取Excel结束=============================================*/
              
                dialog.Progress = 50;
                //创建SQLITE文件
                string pathDatabase = System.IO.Path.Combine(fileStream.Name.Replace(fileStream.Name.Substring(
                fileStream.Name.LastIndexOf("/") + 1), ""), "sqlite.db");
                sqlite = CreateDatabase(pathDatabase);
                //提取数据
                //工单
                AndroidWorkOrder workOrder = new AndroidWorkOrder();
                if (workOrderID != null)
                {
                    workOrder.ID = workOrderID;
                    workOrder.DownUserid = userID;
                    workOrder.ApiUrl = server.ApiAddress;
                    workOrder.ServerCode = server.PlatformCode;
                    workOrder.ServerName = server.PlatformName;
                }
                else
                {
                    workOrder.ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N");
                }
                workOrder.WorkOrderName = fileStream.Name.Substring(fileStream.Name.LastIndexOf("/") + 1).ToLower().Replace(".xlsx", "");
                workOrder.SealPointCount = dt.Rows.Count;
                workOrder.CompleteCount = 0;
                string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                workOrder.UserName = JsonModel.UserName;
                workOrder.OperateTime = createTime;
                if(ColumnCount == 26)
                {
                    if (dt.TableName == "抽样明细")
                    {
                        workOrder.WorkOrderType = 2;
                    }
                    else
                    {
                        workOrder.WorkOrderType = 0;
                    }
                }
                else
                {
                    workOrder.WorkOrderType = 1;
                }

                List<WorkOrderExcelEntity> ExcelList = ConvertDtToEntity(workOrder.WorkOrderType, dt);
                // workOrder.WorkOrderType = ColumnCount == 26 ? 0 : 1;
                //群组
                List<AndroidGroup> groups = (from item in ExcelList
                                             group item by new
                                             {
                                                 t1 = item.DeviceName,
                                                 t2 = item.AreaName,
                                                 t3 = item.GroupCode,
                                                 t4 = item.GroupDescribe,
                                                 t5 = item.BarCode,
                                                 t6 = item.DeviceCode
                                             } into m
                                             select new AndroidGroup
                                             {
                                                 ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),
                                                 WorkOrderID = workOrder.ID,
                                                 GroupName = m.Key.t4,
                                                 GroupCode = m.Key.t3,
                                                 GroupDescribe = m.Key.t4,
                                                 BarCode = m.Key.t5,
                                                 IsComplete = 0,
                                                 CompleteCount = 0,
                                                 DeviceName = m.Key.t1,
                                                 DeviceCode = m.Key.t6,
                                                 AreaName = m.Key.t2
                                             }).ToList();
                dialog.Progress = 55;
                //密封点
                List<AndroidSealPoint> selPoint = (from item in ExcelList
                                                   join g in groups
                                                   on new
                                                   {
                                                       DeviceName = item.DeviceName,
                                                       DeviceCode = item.DeviceCode,
                                                       AreaName = item.AreaName,
                                                       GroupCode = item.GroupCode,
                                                       GroupDescribe = item.GroupDescribe,
                                                       BarCode = item.BarCode
                                                   }
                                                   equals new
                                                   {
                                                       DeviceName = g.DeviceName,
                                                       DeviceCode = g.DeviceCode,
                                                       AreaName = g.AreaName,
                                                       GroupCode = g.GroupCode,
                                                       GroupDescribe = g.GroupDescribe,
                                                       BarCode = g.BarCode
                                                   }
                                                   select new AndroidSealPoint
                                                   {
                                                       ID = SequentialGuid.Create(SequentialGuidType.SequentialAsString).ToString("N"),/// ID
                                                       WorkOrderID = workOrder.ID,/// 工单ID
                                                       GroupID = g.ID,/// 群组ID
                                                       SealPointCode = item.SealPointCode,/// 密封点编号
                                                       ExtCode = item.ExtCode,/// 扩展号
                                                       SealPointType = item.SealPointType,/// 密封点类型
                                                       IsTouch = item.IsTouch,/// 是否可达
                                                       LastNetPPM = item.LastNetPPM,/// 原净检测值
                                                       BackgroundPPM = item.BackgroundPPM,/// 环境本底值
                                                       //LeakagePPM = item.LeakagePPM,/// 泄漏检测值
                                                       //PPM = item.PPM,/// 净检测值
                                                       RedCode = item.RedCode,/// 红牌号码
                                                       LeakageDescribe = item.LeakageDescribe,/// 泄漏描述
                                                       Temperature = item.Temperature,/// 温度
                                                       WindDirection = item.WindDirection,/// 风向
                                                       WindSpeed = item.WindSpeed,/// 风速
                                                       //UserName = item.UserName,/// 检测人名称
                                                       //StartTime = item.StartTime,/// 开始时间                                                       
                                                       //EndTime = item.EndTime,/// 结束时间
                                                       //DetectionTime = item.DetectionTime,///检测时间
                                                       //WasteTime = item.WasteTime, /// 停留时间(秒)
                                                       Security = item.Security,/// 安防措施
                                                       PhxCode = item.PhxCode,/// 检测设备号
                                                       PhxName = item.PhxName,/// 检测设备名称
                                                       DetectionMode = item.DetectionMode/// 组件检测方式
                                                   }).ToList();
                dialog.Progress = 60;
                workOrder.UnReachCount = selPoint.Count(m => m.IsTouch.Contains("不可达"));
                sqlite.Insert(workOrder);
                dialog.Progress = 65;
                sqlite.InsertAll(groups);
                dialog.Progress = 70;
                sqlite.InsertAll(selPoint);
                dialog.Progress = 90;
                sqlite.Execute("UPDATE AndroidGroup set SealPointCount=(SELECT COUNT(*) FROM ANDROIDSEALPOINT WHERE AndroidGroup.ID=ANDROIDSEALPOINT.GroupID)");
                sqlite.Execute("UPDATE AndroidGroup set UnReachCount=(SELECT COUNT(*) FROM ANDROIDSEALPOINT WHERE AndroidGroup.ID=ANDROIDSEALPOINT.GroupID and IsTouch='不可达')");
                dialog.Progress = 100;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("导入工单", ex);
                if (sqlite != null)
                {
                    sqlite.Dispose();
                }
                if (fileStream != null)
                {
                    string path = fileStream.Name.Replace(fileStream.Name.Substring(
                    fileStream.Name.LastIndexOf("/")), "");
                    fileStream.Close();
                    fileStream.Dispose();
                    fileStream = null;
                    System.IO.Directory.Delete(path, true);
                }
                throw ex;
            }
            finally
            {
                if (sqlite != null)
                {
                    sqlite.Dispose();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                }
            }
        }
        /// <summary>
        /// DataTable转换为实体
        /// </summary>
        /// <param name="type">检测类型</param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<WorkOrderExcelEntity> ConvertDtToEntity(int type, DataTable dt)
        {
            //DataTable转List
            List<WorkOrderExcelEntity> ExcelList = new List<WorkOrderExcelEntity>();
            foreach (DataRow row in dt.Rows)
            {
                WorkOrderExcelEntity entity = new WorkOrderExcelEntity();
                //复检
                if (type == 1)
                {
                    entity.IsTouch = "可达";
                    if (row["原净检测值"].ToString() != "")
                    {
                        entity.LastNetPPM = Convert.ToDouble(row["原净检测值"]);
                    }
                    entity.UserName = row["复检人员"].ToString();
                    if (row["复检时间"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["复检时间"].ToString());
                    }
                }
                //检测
                else if (type == 0)
                {
                    entity.IsTouch = row["是否可达"].ToString();
                    entity.LeakageDescribe = row["泄漏描述"].ToString();
                    entity.UserName = row["检测人名称"].ToString();
                    if (row["检测时间"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["检测时间"].ToString());
                    }
                }
                //抽检
                else
                {
                    entity.IsTouch = "可达";
                    entity.LeakageDescribe = row["泄漏描述"].ToString();
                    entity.UserName = row["检测人名称"].ToString();
                    if (row["检测时间"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["检测时间"].ToString());
                    }
                }
                entity.SealPointCode = row["密封点编码"].ToString();
                entity.DeviceCode = row["装置编码"].ToString();
                entity.DeviceName = row["装置名称"].ToString();
                entity.AreaName = row["区域名称"].ToString();
                entity.BarCode = row["群组条形或二维码"].ToString();
                entity.GroupCode = row["群组编码"].ToString();
                entity.GroupDescribe = row["群组位置描述"].ToString();
                entity.ExtCode = row["扩展编码"].ToString();
                entity.SealPointType = row["密封点类型"].ToString();
                if (row["环境本底值"].ToString().Trim() != "")
                {
                    entity.BackgroundPPM = Convert.ToDouble(row["环境本底值"]);
                }
                if (row["净检测值"].ToString().Trim() != "")
                {
                    entity.PPM = Convert.ToDouble(row["净检测值"]);
                }
                if (row["泄漏检测值"].ToString().Trim() != "")
                {
                    entity.LeakagePPM = Convert.ToDouble(row["泄漏检测值"]);
                }
                //if (row["开始检测时间"].ToString().Trim() != "")
                //{
                //    entity.StartTime = Convert.ToDateTime(row["开始检测时间"]);
                //}
                //if (row["结束检测时间"].ToString().Trim() != "")
                //{
                //    entity.EndTime = Convert.ToDateTime(row["结束检测时间"]);
                //}
                //if (row["停留时间"].ToString().Trim() != "")
                //{
                //    entity.WasteTime = Convert.ToInt32(row["停留时间"]);
                //}
                entity.Temperature = row["温度"].ToString();
                entity.WindDirection = row["风向"].ToString();
                entity.WindSpeed = row["风速"].ToString();
                entity.RedCode = row["红牌编号"].ToString();
                entity.Security = row["安防措施"].ToString();
                entity.DetectionMode = row["组件检测方式"].ToString();
                entity.PhxCode = row["检测设备号"].ToString();
                entity.PhxName = row["检测设备名称"].ToString();
                ExcelList.Add(entity);
            }
         
            return ExcelList;
        }

        /// <summary>
        /// 创建数据库文件
        /// </summary>
        /// <param name="pathDatabase"></param>
        private SQLite.SQLiteConnection CreateDatabase(string pathDatabase)
        {
            var connection = new SQLite.SQLiteConnection(pathDatabase);
            connection.CreateTable<AndroidWorkOrder>();
            connection.CreateTable<AndroidGroup>();
            connection.CreateTable<AndroidSealPoint>();
            return connection;
        }

        /// <summary>
        /// 组装数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="range"></param>
        /// <param name="isHead"></param>
        private static void BuidData(DataTable dt, IRange range, int colunm)
        {
            for (int i = 2; i <= range.Rows.Count(); i++)
            {
                DataRow dr = dt.NewRow();
                for (int j = 1; j <= colunm; j++)
                {
                    dr[j - 1] = range[i, j].DisplayText;
                }
                dt.Rows.Add(dr);
            }
        }

        /// <summary>
        /// Excel提取DataTable结构
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        private static DataTable CreateDataTable(IRange range, int colunm)
        {
            DataTable dt = new DataTable();
            for (int i = 1; i <= colunm; i++)
            {
                dt.Columns.Add(range[1, i].DisplayText);
            }
            return dt;
        }

        /// <summary>
        /// 解压缩
        /// </summary>
        /// <param name="path"></param>
        private string UnzipDataFile(ProgressDialog dialog, string path, DateTime createTime)
        {
            File file = new File(path);
            Java.Util.Zip.ZipFile zipFile = new Java.Util.Zip.ZipFile(file);
            using (System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite))
            {
                Java.Util.Zip.ZipInputStream zis = new Java.Util.Zip.ZipInputStream(stream);
                Java.Util.Zip.ZipEntry entry = null;
                string baseOutPath = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + file.Name.ToLower().Replace(".zip", "") + "@" + createTime.ToString("yyyy-MM-dd HH.mm.ss");
                //创建目录
                File outFileBase = new File(baseOutPath);
                if (!outFileBase.ParentFile.Exists())
                {
                    outFileBase.ParentFile.Mkdirs();
                }
                if (!outFileBase.Exists())
                {
                    outFileBase.Mkdirs();
                }
                string excelPath = string.Empty;
                this.Activity.RunOnUiThread(delegate
                {
                    dialog.SetMessage("正在解压缩文件");
                });
                int total = zipFile.Size();
                int index = 0;
                //循环解压缩
                while ((entry = zis.NextEntry) != null)
                {
                    index += 1;
                    int current = index * 100 / total;
                    dialog.Progress = current;
                    string outpath = baseOutPath + "/" + entry.Name;
                    if (entry.Name.ToLower().Contains(".xlsx"))
                    {
                        excelPath = outpath;
                    }
                    Java.IO.File outFile = new Java.IO.File(outpath);
                    if (!outFile.ParentFile.Exists())
                    {
                        outFile.ParentFile.Mkdirs();
                    }
                    if (!outFile.Exists())
                    {
                        outFile.CreateNewFile();
                    }
                    BufferedInputStream bis = new BufferedInputStream(
                            zipFile.GetInputStream(entry));

                    BufferedOutputStream bos = new BufferedOutputStream(
                           new System.IO.FileStream(outpath, System.IO.FileMode.Open));
                    byte[] b = new byte[1000];
                    while (true)
                    {
                        int len = bis.Read(b);
                        if (len == -1)
                            break;
                        bos.Write(b, 0, len);
                    }
                    bis.Close();
                    bos.Close();
                    if (entry.Name.Contains("Img/"))
                    {
                        var bitmap = BitmapHelpers.LoadAndResizeBitmap(outpath, 200, 150);
                        string small = outpath.ToLower().Replace("/img/", "/SmallImg/");
                        Java.IO.File samllFile = new Java.IO.File(small);
                        if (!samllFile.ParentFile.Exists())
                        {
                            samllFile.ParentFile.Mkdirs();
                        }
                        System.IO.FileStream fs = new System.IO.FileStream(small, System.IO.FileMode.CreateNew);
                        bitmap.Compress(CompressFormat.Jpeg, 24, fs);
                        fs.Close();
                    }
                }
                zis.Close();
                return excelPath;
            }
        }

        private string UnzipMSDataFile(string path, DateTime createTime)
        {
            File outFileBase = null;
            try
            {
                File file = new File(path);
                string baseOutPath = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + file.Name.ToLower().Replace(".zip", "") + "@" + createTime.ToString("yyyy-MM-dd HH.mm.ss");
                //创建目录
                outFileBase = new File(baseOutPath);
                if (!outFileBase.ParentFile.Exists())
                {
                    outFileBase.ParentFile.Mkdirs();
                }
                if (!outFileBase.Exists())
                {
                    outFileBase.Mkdirs();
                }
                string excelPath = string.Empty;
                System.IO.Compression.ZipFile.ExtractToDirectory(path, outFileBase.AbsolutePath);
                foreach (var item in outFileBase.ListFiles().ToList())
                {
                    if (item.Name.ToLower().Contains(".xlsx"))
                    {
                        excelPath = item.AbsolutePath;
                    }
                }
                return excelPath;
            }
            catch (Exception ex)
            {
                if (outFileBase != null && outFileBase.Exists())
                {
                    //System.IO.Directory.Delete(outFileBase.AbsolutePath, true);
                }
                throw ex;
            }
        }


        private string UnzipShareDataFile(string path, DateTime createTime)
        {
            File outFileBase = null;
            try
            {
                File zipfile = new File(path);
                string baseOutPath = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + zipfile.Name.ToLower().Replace(".zip", "") + "@" + createTime.ToString("yyyy-MM-dd HH.mm.ss");
                //创建目录
                outFileBase = new File(baseOutPath);
                if (!outFileBase.ParentFile.Exists())
                {
                    outFileBase.ParentFile.Mkdirs();
                }
                if (!outFileBase.Exists())
                {
                    outFileBase.Mkdirs();
                }
                string excelPath = string.Empty;
                System.IO.FileStream stream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite);
                ICSharpCode.SharpZipLib.Zip.ZipInputStream zipInputStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(stream);
                ICSharpCode.SharpZipLib.Zip.ZipEntry zipEntryFromZippedFile = zipInputStream.GetNextEntry();
                zipEntryFromZippedFile.IsUnicodeText = true;
                while (zipEntryFromZippedFile != null)
                {
                    string outpath = baseOutPath + "/" + zipEntryFromZippedFile.Name;
                    if (zipEntryFromZippedFile.Name.ToLower().Contains(".xlsx"))
                    {
                        excelPath = outpath;
                    }
                    File outFile = new File(outpath);
                    if (!outFile.ParentFile.Exists())
                    {
                        outFile.ParentFile.Mkdirs();
                    }
                    if (!outFile.Exists())
                    {
                        outFile.CreateNewFile();
                    }
                    if (zipEntryFromZippedFile.IsFile)
                    {
                        System.IO.FileInfo fInfo = new System.IO.FileInfo(string.Format("ZipOutPut\\{0}", zipEntryFromZippedFile.Name));
                        if (!fInfo.Directory.Exists) fInfo.Directory.Create();

                        System.IO.FileStream file = fInfo.Create();
                        byte[] bufferFromZip = new byte[zipInputStream.Length];
                        zipInputStream.Read(bufferFromZip, 0, bufferFromZip.Length);
                        file.Write(bufferFromZip, 0, bufferFromZip.Length);
                        file.Close();
                    }
                    zipEntryFromZippedFile = zipInputStream.GetNextEntry();
                }
                zipInputStream.Close();
                return excelPath;
            }
            catch (Exception ex)
            {
                if (outFileBase != null && outFileBase.Exists())
                {
                    System.IO.Directory.Delete(outFileBase.AbsolutePath, true);
                }
                throw ex;
            }


        }
        /*========================================================================*/
        #region 高于 v4.4 版本 解析真实路径

        public static String GetPath(Context context, Android.Net.Uri uri)
        {

            bool isKitKat = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;

            // DocumentProvider  
            if (isKitKat && DocumentsContract.IsDocumentUri(context, uri))
            {
                // ExternalStorageProvider  
                if (isExternalStorageDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);
                    String[] split = docId.Split(':');
                    String type = split[0];

                    if ("primary".Equals(type.ToLower()))
                    {
                        return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                    }

                    // TODO handle non-primary volumes  
                    return Android.OS.Environment.ExternalStorageDirectory + "/" + split[1];
                }
                // DownloadsProvider  
                else if (isDownloadsDocument(uri))
                {

                    String id = DocumentsContract.GetDocumentId(uri);
                    Android.Net.Uri contentUri = ContentUris.WithAppendedId(
                            Android.Net.Uri.Parse("content://downloads/public_downloads"), long.Parse(id));

                    return getDataColumn(context, contentUri, null, null);
                }
                // MediaProvider  
                else if (isMediaDocument(uri))
                {
                    String docId = DocumentsContract.GetDocumentId(uri);
                    String[] split = docId.Split(':');
                    String type = split[0];

                    Android.Net.Uri contentUri = null;
                    if ("image".Equals(type))
                    {
                        contentUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if ("video".Equals(type))
                    {
                        contentUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if ("audio".Equals(type))
                    {
                        contentUri = MediaStore.Audio.Media.ExternalContentUri;
                    }

                    String selection = "_id=?";
                    String[] selectionArgs = new String[] {
                    split[1]
            };

                    return getDataColumn(context, contentUri, selection, selectionArgs);
                }
            }
            // MediaStore (and general)  
            else if ("content".Equals(uri.Scheme.ToLower()))
            {

                // Return the remote address  
                if (isGooglePhotosUri(uri))
                    return uri.LastPathSegment;

                return getDataColumn(context, uri, null, null);
            }
            // File  
            else if ("file".Equals(uri.Scheme.ToLower()))
            {
                return uri.Path;
            }

            return null;
        }

        /** 
         * Get the value of the data column for this Uri. This is useful for 
         * MediaStore Uris, and other file-based ContentProviders. 
         * 
         * @param context The context. 
         * @param uri The Uri to query. 
         * @param selection (Optional) Filter used in the query. 
         * @param selectionArgs (Optional) Selection arguments used in the query. 
         * @return The value of the _data column, which is typically a file path. 
         */
        public static String getDataColumn(Context context, Android.Net.Uri uri, String selection,
                String[] selectionArgs)
        {

            Android.Database.ICursor cursor = null;
            String column = "_data";
            String[] projection = {
                column
            };

            try
            {
                cursor = context.ContentResolver.Query(uri, projection, selection, selectionArgs,
                        null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(column);
                    return cursor.GetString(index);
                }
            }
            finally
            {
                if (cursor != null)
                    cursor.Close();
            }
            return null;
        }


        /** 
         * @param uri The Uri to check. 
         * @return Whether the Uri authority is ExternalStorageProvider. 
         */
        public static bool isExternalStorageDocument(Android.Net.Uri uri)
        {
            return "com.android.externalstorage.documents".Equals(uri.Authority);
        }

        /** 
         * @param uri The Uri to check. 
         * @return Whether the Uri authority is DownloadsProvider. 
         */
        public static bool isDownloadsDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.downloads.documents".Equals(uri.Authority);
        }

        /** 
         * @param uri The Uri to check. 
         * @return Whether the Uri authority is MediaProvider. 
         */
        public static bool isMediaDocument(Android.Net.Uri uri)
        {
            return "com.android.providers.media.documents".Equals(uri.Authority);
        }

        /** 
         * @param uri The Uri to check. 
         * @return Whether the Uri authority is Google Photos. 
         */
        public static bool isGooglePhotosUri(Android.Net.Uri uri)
        {
            return "com.google.android.apps.photos.content".Equals(uri.Authority);
        }

        #endregion
        /*========================================================================*/
        public static readonly int PickZipId = 1000;
        /// <summary>
        /// 导入工单选择文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnImp_Click(object sender, EventArgs e)
        {
            string json = Utility.DecryptDES(UserPreferences.GetString("CrrentUser"));
            var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
            DateTime Servertime = Utility.ConvertTimeStampToDateTime(JsonModel.ServerLastLoginTime);
            if (Servertime.AddDays(5) <= DateTime.Now)
            {
                Toast.MakeText(this.Activity, "离线时间超过五天，必须在线登录!", ToastLength.Short).Show();
                return;
            }
            Intent Intent = new Intent();
            Intent.SetType("application/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "选择压缩文件"), PickZipId);
        }

        #endregion

        #region 工单列表相关
        public static List<AndroidWorkOrder> GetWorkOrderList()
        {
            List<AndroidWorkOrder> list = new List<AndroidWorkOrder>();
            var directory = new File(System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "LDARAPP6"));
            if (directory.ListFiles() != null)
            {
                foreach (var item in directory.ListFiles())
                {
                    if (item.Name.Contains("@"))
                    {
                        SQLite.SQLiteConnection connection = null;
                        try
                        {
                            string datadirectory = System.IO.Path.Combine(item.AbsolutePath, "sqlite.db");
                            connection = new SQLite.SQLiteConnection(datadirectory);
                            List<AndroidWorkOrder> itemList = connection.Table<AndroidWorkOrder>().ToList();
                            var c = connection.Query<SealPointCount>("SELECT COUNT(*) AS PointCount FROM ANDROIDSEALPOINT WHERE date(STARTTIME) = date('" + DateTime.Now.ToString("yyyy-MM-dd") + "')");
                            var count = int.Parse(c[0].PointCount);
                            itemList.ForEach(i =>
                            {
                                i.DataPath = datadirectory;
                                i.DetectionCount = count;
                                i.DetectionDate = DateTime.Now;
                            });
                            list.AddRange(itemList);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog("获取工单数据列表", ex);
                        }
                        finally
                        {
                            if (connection != null)
                            {
                                connection.Dispose();
                            }
                        }
                    }
                }
            }
            return list.OrderByDescending(c => c.OperateTime).ToList();
        }
        #endregion
    }

    public class OrderList
    {
        public string UserID;
        /// <summary>
        /// 检测工单
        /// </summary>
        public List<LdarWorkOrder> WorkOrderList;
        /// <summary>
        /// 复检工单
        /// </summary>
        public List<LdarCheckWorkOrder> CheckWorkOrderList;

        /// <summary>
        /// 抽检工单
        /// </summary>
        public List<LdarRadomCheckWorkOrder> RadomCheckWorkOrderList;
    }
    /// <summary>
    /// 检测工单
    /// </summary>
    public class LdarWorkOrder
    {
        ///<summary>
        /// 检测工单Id
        ///</summary>
        public string Id { get; set; }

        /// <summary>
        /// 检测周期
        /// </summary>
        public string InspectionCycleId { get; set; }

        ///<summary>
        /// 检测装置信息ID 多个用逗号隔开
        ///</summary>
        public string DeviceInfoId { get; set; }

        ///<summary>
        /// 工单编号
        ///</summary>
        public string WorkOrderCode { get; set; }

        ///<summary>
        /// 工单名称
        ///</summary>
        public string WorkOrderName { get; set; }

        ///<summary>
        /// 工单开始时间
        ///</summary>
        public DateTime? BeginDate { get; set; }

        ///<summary>
        /// 工单结束时间
        ///</summary>
        public DateTime? EndDate { get; set; }

        ///<summary>
        /// 计划编号
        ///</summary>
        public string PlanCode { get; set; }

        ///<summary>
        /// 计划ID
        ///</summary>
        public string WorkPlanId { get; set; }

        public string LeakageStandard { get; set; }

        ///<summary>
        /// 第三方检测机构ID
        ///</summary>
        public string DetectionMechanismId { get; set; }

        ///<summary>
        /// 检测人
        ///</summary>
        public string TestUserId { get; set; }

        ///<summary>
        /// 检测人名称
        ///</summary>
        public string TestUserName { get; set; }

        ///<summary>
        /// 备注
        ///</summary>
        public string Remark { get; set; }

        ///<summary>
        /// 公司Id
        ///</summary>
        public string CompanyId { get; set; }

        ///<summary>
        /// 业务状态 0待检测 1检测中 2完成检测
        ///</summary>
        public string BizState { get; set; }

        ///<summary>
        /// 部门ID
        ///</summary>
        public string DepId { get; set; }

        ///<summary>
        /// 密封点总数数
        ///</summary>
        public int? SealingPointCount { get; set; }

        ///<summary>
        /// 可达密封点数
        ///</summary>
        public int? AbleSealingPoint { get; set; }

        ///<summary>
        /// 不可达密封点数
        ///</summary>
        public int? UNAbleSealingPoint { get; set; }

        ///<summary>
        /// 已检测可达密封点数
        ///</summary>
        public int? TestSealingPoint { get; set; }

        ///<summary>
        /// 是否有效  0 无效  1 有效
        ///</summary>
        public int? Enabled { get; set; }

        public int? DynamicStaticType { get; set; }

    }

    /// <summary>
    /// 复检工单表
    /// </summary>
    public class LdarCheckWorkOrder
    {
        ///<summary>
        /// 复检工单Id
        ///</summary>
        public string Id { get; set; }

        ///<summary>
        /// 当前周期Id
        ///</summary>
        public string InspectionCycleId { get; set; }

        ///<summary>
        /// 维修工单Id
        ///</summary>
        public string RepairWorkOrderId { get; set; }
        ///<summary>
        /// 复检单号
        ///</summary>
        public string CheckWorkOrderCode { get; set; }
        ///<summary>
        /// 复检单名称
        ///</summary>
        public string CheckWorkOrderName { get; set; }

        ///<summary>
        /// 开始时间
        ///</summary>
        public DateTime BeginDate { get; set; }

        ///<summary>
        /// 结束时间
        ///</summary>
        public DateTime EndDate { get; set; }

        ///<summary>
        /// 第三方检测机构ID
        ///</summary>
        public string DetectionMechanismId { get; set; }

        ///<summary>
        /// 计划ID
        ///</summary>
        public string WorkPlanId { get; set; }

        ///<summary>
        /// 计划编号
        ///</summary>
        public string PlanCode { get; set; }

        ///<summary>
        /// 检测工单Id
        ///</summary>
        public string WorkOrderId { get; set; }

        ///<summary>
        /// 复检人ID
        ///</summary>
        public string CheckWorkUserId { get; set; }

        ///<summary>
        /// 复检人名称
        ///</summary>
        public string CheckWorkUserName { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string Remark { get; set; }

        ///<summary>
        /// 组织部门编号
        ///</summary>
        public string DepId { get; set; }

        ///<summary>
        /// 公司Id
        ///</summary>
        public string CompanyId { get; set; }

        ///<summary>
        /// 是否有效  0 无效  1 有效
        ///</summary>
        public int Enabled { get; set; }
    }


    /// <summary>
    /// 抽检任务
    /// </summary>
    public class LdarRadomCheckWorkOrder
    {
        #region 属性
        /// <summary>
        /// 抽检任务ID
        /// </summary>		
        public string Id { get; set; }

        /// <summary>
        /// 装置Id
        /// </summary>	
        public string LdarDeviceId { get; set; }

        /// <summary>
        /// 检测核算周期Id
        /// </summary>
        public string InspectionCycleId { get; set; }

        /// <summary>
        /// 抽检时间
        /// </summary>	
        public DateTime? TestTime { get; set; }

        /// <summary>
        /// 实际抽检时间
        /// </summary>		
        public DateTime? ActualTime { get; set; }

        /// <summary>
        /// 抽检内容,
        ///1企业建档信息
        ///2检测人员操作手法是否正确
        ///3抽样检测
        ///4企业检测质量控制档案
        ///5企业修复情况
        ///6企业延迟修复情况 
        ///7数据传输时效性
        /// </summary>	
        public string TestContent { get; set; }

        /// <summary>
        ///评估部门ID
        /// </summary>
        public string AssessmentId { get; set; }

        /// <summary>
        /// 抽检意见
        /// </summary>	
        public string OPINION { get; set; }

        /// <summary>
        /// 环保部门ID
        /// </summary>	
        public string EnvironmentalId { get; set; }

        /// <summary>
        /// 状态：0-环保部门制定抽检任务 1-评估部门抽检 2-环保部门反馈 3-结束
        /// </summary>	
        public int AudiStatus { get; set; } = 0;

        /// <summary>
        /// 1 采用定量检测方法的不泄漏点位，按照个组件类型抽检5%数量的点位 2 随机抽检比例及点位编码
        /// </summary>	
        public int? TestMethod { get; set; }

        /// <summary>
        /// 抽测比例
        /// </summary>		
        public decimal? TestProportion { get; set; }

        /// <summary>
        /// 公司Id
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        /// 是否有效  0 无效  1 有效
        /// </summary>		
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// 录入人员
        /// </summary>		
        public string InUser { get; set; }

        /// <summary>
        /// 录入日期
        /// </summary>	
        public DateTime? InDate { get; set; }

        /// <summary>
        /// 修改人员
        /// </summary>	
        public string EditUser { get; set; }

        /// <summary>
        /// 修改日期
        /// </summary>		
        public DateTime? EditDate { get; set; }

        /// <summary>
        /// 删除人员
        /// </summary>	
        public string DelUser { get; set; }

        /// <summary>
        /// 标识该行数据是否被删除 0未删除 1已删除
        /// </summary>	
        public int DelState { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>	
        public DateTime? DelDate { get; set; }
        #endregion
    }

    /// <summary>
    /// 异步执行状态,
    /// </summary>
    public class AsynchronousExecutionStateModel
    {
        /// <summary>
        /// 暂时无用，根据实际情况扩展
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 消息类型，根据实际使用情况，按顺序扩展,请写好备注
        /// <para>0：新增消息（如：顺序显示的执行状态）</para>
        /// <para>1刷新消息(如：只显示一条的进度条百分百)</para>
        /// <para>200：完成（不再继续请求消息）</para>
        /// <para>500：错误（不再继续请求消息）</para>
        /// </summary>
        public int SType { get; set; }

        /// <summary>
        /// 连接ConnectionId
        /// </summary>
        public string ConnectionId { get; set; }
    }
}