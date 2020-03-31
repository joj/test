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
        //���������б�
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
        #region ���ع���
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
            //��¼
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
            spApi.Prompt = "��ѡ�������";
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
            #region �����¼�ɹ����û�
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
            //����
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
                    tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
                    tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
                    tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
                toast.SetText("��������ʧ��");
                toast.Show();
            }
            else if (msg.Code == 10000)
            {
                #region �����¼�ɹ����û�
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
                //���
                List<string> workList = new List<string>();
                foreach (var item in OrderList.WorkOrderList)
                {
                    workList.Add(item.WorkOrderName);
                }
                spWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, workList);
                spWorkOrder.Prompt = "��ѡ���⹤��";
                spWorkOrder.ItemSelected -= SpWorkOrder_ItemSelected;
                spWorkOrder.ItemSelected += SpWorkOrder_ItemSelected;
                //���
                List<string> orderList = new List<string>();
                foreach (var item in OrderList.CheckWorkOrderList)
                {
                    orderList.Add(item.CheckWorkOrderName);
                }
                spCheckWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, orderList);
                spCheckWorkOrder.Prompt = "��ѡ�񸴼칤��";
                spCheckWorkOrder.ItemSelected -= SpCheckWorkOrder_ItemSelected;
                spCheckWorkOrder.ItemSelected += SpCheckWorkOrder_ItemSelected;
                //����
                List<string> radomOrderList = new List<string>();
                if (radomOrderList != null)
                {
                    foreach (var item in OrderList.RadomCheckWorkOrderList)
                    {
                        radomOrderList.Add(item.CompanyId + "-" + item.LdarDeviceId);//��ҵ����+װ������
                    }
                }
                spRadomCheckWorkOrder.Adapter = new ArrayAdapter(this.Activity, Resource.Layout.ListViewItem, radomOrderList);
                spRadomCheckWorkOrder.Prompt = "��ѡ���칤��";
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
                tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
                tvMsgOrder.Text = "�˹����Ѿ����ع�������";
            }
            else
            {
                tvMsgOrder.Text = "";
            }
        }

        #region ���
        private void SpRadomCheckWorkOrder_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (workOrderList.Exists(k => k.ID == OrderList.RadomCheckWorkOrderList[e.Position].Id))
            {
                var et = e.View as TextView;
                et.SetTextColor(Android.Graphics.Color.ParseColor("#FF0000"));
                tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
                tvMsgOrder.Text = "�˹����Ѿ����ع�������";
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
        /// ���ع���
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
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "��ʾ", "���ڵȴ�������������", true, false);
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
                    toast.SetText("��������ʧ��");
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
                    pgdilog.SetTitle("�������ݵ����У��벻Ҫ�����κβ���");
                    pgdilog.SetMessage("���������ļ�");
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
                            //�������ɾ���ļ�
                            Java.IO.File f = new Java.IO.File(path);
                            f.Delete();
                            this.Activity.RunOnUiThread(delegate
                            {
                                //���»�ȡ�����б�
                                workOrderList = GetWorkOrderList();
                                //���¼����б�
                                li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            });
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog("���빤������", ex);
                            this.Activity.RunOnUiThread(delegate
                            {
                                Toast.MakeText(this.Activity, "���빤�����ݷ�������:" + ex.Message, ToastLength.Short).Show();
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
        /// ���ش���ر�
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
        /// ���빤�����ݴ���
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
                    dilog.SetTitle("�������ݵ����У��벻Ҫ�����κβ���");
                    dilog.SetMessage("���ڼ����ļ�");
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
                                //���»�ȡ�����б�
                                workOrderList = GetWorkOrderList();
                                //���¼����б�
                                li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            });
                            LogHelper.InfoLog(file.Name + "�������ݵ���ɹ�");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.ErrorLog("���빤������", ex);
                            this.Activity.RunOnUiThread(delegate
                            {
                                Toast.MakeText(this.Activity, "���빤�����ݷ�������:" + ex.Message, ToastLength.Short).Show();
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
                    Toast.MakeText(this.Context, "��ѡ�񹤵�ѹ��zip�ļ�", ToastLength.Short).Show();
                }
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }
        /// <summary>
        /// �����˵�
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
                string[] menuItems = new string[7] { "ɾ������", null, null, null, "�Ѽ���ܷ���¼", null, null };
                string CrrentUser = UserPreferences.GetString("CrrentUser");
                string json = Utility.DecryptDES(CrrentUser);
                var JsonModel = JsonConvert.DeserializeObject<AndroidUser>(json);
                if (JsonModel.IsBatchTest == 1)
                {
                    menuItems[5] = "�������ģʽ";
                }
                //if (workOrderList[info.Position].CompleteCount == (workOrderList[info.Position].SealPointCount - workOrderList[info.Position].UnReachCount))
                if (workOrderList[info.Position].CompleteCount == workOrderList[info.Position].SealPointCount)
                {
                    menuItems[1] = "�����ǩ��";
                    menuItems[2] = "�����ǩ��";
                    menuItems[3] = "��������";
                    if (!string.IsNullOrEmpty(workOrderList[info.Position].ApiUrl))
                    {
                        menuItems[6] = "�ϴ�����";
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
        /// �˵��¼�
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool OnContextItemSelected(IMenuItem item)
        {
            info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;
            var menuItemName = item.ToString();
            if (menuItemName == "ɾ������")
            {
                this.Activity.RunOnUiThread(delegate
                {
                    dialogService.ShowYesNo(this.Activity, "��ʾ", "�Ƿ�Ҫɾ���˹�����ɾ�����޷��ָ�������",
                    r =>
                    {
                        if (r == DialogResult.Yes)
                        {
                            AndroidWorkOrder order = workOrderList[info.Position];
                            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(order.DataPath.Replace("sqlite.db", ""));
                            dir.Delete(true);
                            workOrderList.RemoveAt(info.Position);
                            li.Adapter = new WorkOrderAdapter(this.Activity, workOrderList);
                            Toast.MakeText(this.Activity, "��ɾ��" + order.WorkOrderName + "��⹤��", ToastLength.Short).Show();
                        }
                    });
                });
                return true;
            }
            else if (menuItemName == "�����ǩ��" || menuItemName == "�����ǩ��")
            {
                Intent i = new Intent(this.Activity, typeof(SignaturePadActivity));
                i.PutExtra("SignatureType", menuItemName);
                AndroidWorkOrder order = workOrderList[info.Position];
                i.PutExtra("WorkOrderPath", order.DataPath.Replace("sqlite.db", ""));
                i.PutExtra("WorkOrderName", order.WorkOrderName);
                this.StartActivity(i);
            }
            else if (menuItemName == "��������")
            {
                var NetworkState = UserPreferences.GetString("NetworkState");
                if (NetworkState.Equals("����"))
                {
                    Toast.MakeText(this.Activity, "���ߵ�¼���ܵ���,���˳���¼���������ߵ�¼", ToastLength.Long).Show();
                    return true;
                }
                AndroidWorkOrder order = workOrderList[info.Position];

                if (IsSignaturePass(order.DataPath.Replace("sqlite.db", ""),order) == false)
                {
                    return true;
                }
                Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "��ʾ", "�������ݵ����У��벻Ҫ�����κβ���������", true, false);
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        //װ������
                        List<dynamic> excelList = BuidWorkOrderExcelData(order);
                        //����Excel�ļ�
                        string path = CreateExcelFile(order.WorkOrderName, excelList, workOrderList[info.Position].ApiUrl, order.DataPath.Replace("sqlite.db", ""));
                        this.Activity.RunOnUiThread(delegate
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this.Activity, order.WorkOrderName + "�����ɹ�", "�����ļ��洢�ڣ�" + path, null);
                        });
                        LogHelper.InfoLog(order.WorkOrderName + "�������ݵ����ɹ�");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ErrorLog("������������", ex);
                        this.Activity.RunOnUiThread(delegate
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this.Activity, "�����������ݷ�������", ex.Message, null);
                        });
                    }
                    finally
                    {
                        dilog.Dismiss();
                    }
                });
                return true;
            }
            else if (menuItemName == "�������ģʽ")
            {
                Intent i = new Intent(this.Activity, typeof(BatchModeActivity));
                Bundle b = new Bundle();
                b.PutString("AndroidWorkOrder", Newtonsoft.Json.JsonConvert.SerializeObject(workOrderList[info.Position]));
                i.PutExtras(b);
                this.StartActivity(i);
                return true;
            }
            else if (menuItemName == "�Ѽ���ܷ���¼")
            {
                Intent i = new Intent(this.Activity, typeof(DetectionActivity));
                Bundle b = new Bundle();
                b.PutString("AndroidWorkOrder", Newtonsoft.Json.JsonConvert.SerializeObject(workOrderList[info.Position]));
                i.PutExtras(b);
                this.StartActivity(i);
                return true;
            }
            else if (menuItemName == "�ϴ�����")
            {
                var NetworkState = UserPreferences.GetString("NetworkState");
                if (NetworkState.Equals("����"))
                {
                    Toast.MakeText(this.Activity, "���ߵ�¼�����ϴ�,���˳���¼���������ߵ�¼", ToastLength.Long).Show();
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
                    dialogService.ShowYesNo(this.Activity, "��ʾ", workOrderList[info.Position].UploadTime.ToString() + "�Ѿ��ϴ������Ƿ���Ҫ��һ���ϴ�",
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
        /// ǩ���Ƿ�ϸ�
        /// </summary>
        /// <param name="dataDir"></param>
        /// <returns></returns>
        private bool IsSignaturePass(string dataDir, AndroidWorkOrder order)
        {
            var signafile = System.IO.Path.Combine(dataDir, "signature.Png");
            if (System.IO.File.Exists(signafile) == false)
            {
                Toast.MakeText(this.Activity, "�������ǩ��", ToastLength.Short).Show();
                return false;
            }

            //�����ǩ��
          
            string CheckName = UserPreferences.GetString(order.WorkOrderName);
            if (string.IsNullOrWhiteSpace(CheckName))
            {
                Toast.MakeText(this.Activity, "�������ǩ��", ToastLength.Short).Show();
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
                    Toast.MakeText(this.Activity, "���������дǩ��", ToastLength.Short).Show();
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// �ϴ�����
        /// </summary>
        /// <param name="info"></param>
        private void UpLoarWorkOrder(AdapterView.AdapterContextMenuInfo info)
        {
            Toast toast = Toast.MakeText(this.Activity, "", ToastLength.Short);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this.Activity, "��ʾ", "�������������У��벻Ҫ�����κβ���������", true, false);
            pgdilog = new ProgressDialog(this.Activity);
            ConnectAsync(workOrderList[info.Position], dilog, toast);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //װ������
                    AndroidWorkOrder order = workOrderList[info.Position];
                    List<dynamic> excelList = BuidWorkOrderExcelData(order);
                    //����Excel�ļ�
                    string fullfilename = CreateExcelFile(order.WorkOrderName, excelList, workOrderList[info.Position].ApiUrl, order.DataPath.Replace("sqlite.db", ""));
                    //2.�ϴ�Excel                
                    pgdilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    pgdilog.Indeterminate = false;
                    pgdilog.SetCancelable(false);
                    pgdilog.SetCanceledOnTouchOutside(false);
                    pgdilog.Max = 100;
                    pgdilog.SetTitle("���������ϴ��У��벻Ҫ�����κβ���");
                    pgdilog.SetMessage("�����ϴ��ļ�");
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
                            dialog.ShowOk(this.Activity, "��ʾ", "�ϴ���������ʧ��", null);
                        });
                        return;
                    }
                    this.Activity.RunOnUiThread(() =>
                    {
                        dilog.SetTitle("���������ڴ���������");
                        dilog.SetMessage("");
                    });
                    //3.֪ͨ����������Excel                        
                    this.Activity.RunOnUiThread(() =>
                    {
                        pgdilog.Hide();
                        dilog.Show();
                    });
                    //��ȡ������Ϣ
                    //���
                    if (workOrderList[info.Position].WorkOrderType == 0)
                    {
                        BaseResult<LdarWorkOrder> msg = Utility.CallService<LdarWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Enterprise/LdarWorkOrder/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("��������ʧ��");
                                toast.Show();
                            });
                        }
                        else if (msg.Code == 10000)
                        {
                            //���÷���������������
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
                    //����
                    else if (workOrderList[info.Position].WorkOrderType == 1)
                    {
                        BaseResult<LdarCheckWorkOrder> msg = Utility.CallService<LdarCheckWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Enterprise/LdarCheckWorkOrder/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("��������ʧ��");
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
                    //���
                    else
                    {
                        BaseResult<LdarWorkOrder> msg = Utility.CallService<LdarWorkOrder>(workOrderList[info.Position].ApiUrl, string.Format("V10Government/LdarExtractTask/GetModelById/{0}", workOrderList[info.Position].ID), null, 10000, null);
                        if (msg == null)
                        {
                            this.Activity.RunOnUiThread(() =>
                            {
                                toast.SetText("��������ʧ��");
                                toast.Show();
                            });
                        }
                        else if (msg.Code == 10000)
                        {
                            //���÷���������������
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
                    LogHelper.ErrorLog("�ϴ���������", ex);
                    this.Activity.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this.Activity, "�ϴ��������ݷ�������", ex.Message, null);
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
        /// ����singR
        /// </summary>
        /// <param name="ServerUri"></param>
        private async void ConnectAsync(AndroidWorkOrder workOrder, ProgressDialog dialog, Toast toast)
        {
            Connection = new HubConnection(workOrder.ApiUrl);
            // ����һ���������������
            HubProxy = Connection.CreateHubProxy("CommonHub");
            // ������˵��ã�����Ϣ�������Ϣ�б����
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
                    dig.ShowOk(this.Activity, "��ʾ", "�ϴ��ɹ�", null);
                }
                else if (message.SType == 500)
                {
                    dialog.Dismiss();
                    LogHelper.InfoLog("�ϴ���������" + message.Message);
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
        /// ��������Excel����
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
                //���
                if (order.WorkOrderType == 0)
                {
                    entity = new WorkOrderDetectionExcelEntity();
                    entity.�Ƿ�ɴ� = point.IsTouch;
                    entity.й©���� = point.LeakageDescribe;
                    entity.��������� = point.UserName;
                    entity.���ʱ�� = point.DetectionTime;
                }
                //����
                else if (order.WorkOrderType == 1)
                {
                    entity = new WorkOrderRecheckExcelEntity();
                    entity.ԭ�����ֵ = point.LastNetPPM;
                    entity.������Ա = point.UserName;
                    entity.����ʱ�� = point.DetectionTime;
                }
                //���
                else
                {
                    entity = new WorkOrderRadomCheckExcelEntity();
                    entity.�Ƿ�ɴ� = point.IsTouch;
                    entity.й©���� = point.LeakageDescribe;
                    entity.��������� = point.UserName;
                    entity.���ʱ�� = point.DetectionTime;
                }
                entity.�ܷ����� = point.SealPointCode;
                entity.װ������ = g.DeviceName;
                entity.װ�ñ��� = g.DeviceCode;
                entity.�������� = g.AreaName;
                entity.Ⱥ�����λ��ά�� = g.BarCode;
                entity.Ⱥ����� = g.GroupCode;
                entity.��չ���� = point.ExtCode;
                entity.Ⱥ��λ������ = g.GroupDescribe;
                if (point.SealPointType == "ȡ��������")
                {
                    entity.�ܷ������ = "ȡ������ϵͳ";
                }
                else if (point.SealPointType == "���ڹ���")
                {
                    entity.�ܷ������ = "���ڷ��򿪿ڹ���";

                }
                else
                {
                    entity.�ܷ������ = point.SealPointType;
                }
                entity.��������ֵ = point.BackgroundPPM;
                entity.�����ֵ = point.PPM;
                entity.й©���ֵ = point.LeakagePPM;
                entity.�¶� = point.Temperature;
                entity.���� = point.WindDirection;
                entity.���� = point.WindSpeed;
                entity.���Ʊ�� = point.RedCode;
                //entity.��ʼ���ʱ�� = point.StartTime;
                //entity.�������ʱ�� = point.EndTime;
                entity.ͣ��ʱ�� = point.WasteTime;
                entity.������ʩ = point.Security;
                entity.����豸�� = point.PhxCode;
                entity.����豸���� = point.PhxName;
                entity.�����ⷽʽ = point.DetectionMode;
                entity.����� = CheckName;
                excelList.Add(entity);
            }

            return excelList;
        }

        /// <summary>
        /// ��Excel�ļ�
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

            IWorksheet worksheet1 = workbook.Worksheets.Create("�����ǩ��");
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
                IWorksheet worksheet2 = workbook.Worksheets.Create("�����ǩ��");
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
            //���
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
            //����
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

        #region �������
        /// <summary>
        /// ����SQLITE
        /// </summary>
        /// <param name="excel"></param>
        private void ImpSqlite(ProgressDialog dialog, string excel, DateTime createTime, string workOrderID = null, string userID = null, AndroidServerUrl server = null)
        {
            System.IO.FileStream fileStream = null;
            SQLite.SQLiteConnection sqlite = null;
            this.Activity.RunOnUiThread(delegate
            {
                dialog.SetMessage("���ڵ����ܷ������");
            });
            dialog.Progress = 0;
            try
            {
                fileStream = System.IO.File.Open(excel, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                DataTable dt = new DataTable();
                /*====================ExcelDataReader��ȡExcel��ʼ============================*/
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
                /*====================ExcelDataReader��ȡExcel����============================*/
                /*====================Syncfusion��ȡExcel��ʼ============================*/
                //ExcelEngine excelEngine = new ExcelEngine();
                //IApplication application = excelEngine.Excel;
                //IWorkbook workbook = application.Workbooks.Open(fileStream, ExcelOpenType.SpreadsheetML2010, ExcelParseOptions.DoNotParseCharts);
                //WorksheetImpl sheet = workbook.Worksheets[0] as WorksheetImpl;
                //int RowCount = sheet.CellRecords.LastRow;
                //int ColumnCount = sheet.CellRecords.LastColumn;

                //IRange range = sheet.Range[1, 1, RowCount, ColumnCount];
                //dt = CreateDataTable(range[1, 1, 1, ColumnCount], ColumnCount);
                //BuidData(dt, range, ColumnCount);
                /*==============================Syncfusion��ȡExcel����=============================================*/
              
                dialog.Progress = 50;
                //����SQLITE�ļ�
                string pathDatabase = System.IO.Path.Combine(fileStream.Name.Replace(fileStream.Name.Substring(
                fileStream.Name.LastIndexOf("/") + 1), ""), "sqlite.db");
                sqlite = CreateDatabase(pathDatabase);
                //��ȡ����
                //����
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
                    if (dt.TableName == "������ϸ")
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
                //Ⱥ��
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
                //�ܷ��
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
                                                       WorkOrderID = workOrder.ID,/// ����ID
                                                       GroupID = g.ID,/// Ⱥ��ID
                                                       SealPointCode = item.SealPointCode,/// �ܷ����
                                                       ExtCode = item.ExtCode,/// ��չ��
                                                       SealPointType = item.SealPointType,/// �ܷ������
                                                       IsTouch = item.IsTouch,/// �Ƿ�ɴ�
                                                       LastNetPPM = item.LastNetPPM,/// ԭ�����ֵ
                                                       BackgroundPPM = item.BackgroundPPM,/// ��������ֵ
                                                       //LeakagePPM = item.LeakagePPM,/// й©���ֵ
                                                       //PPM = item.PPM,/// �����ֵ
                                                       RedCode = item.RedCode,/// ���ƺ���
                                                       LeakageDescribe = item.LeakageDescribe,/// й©����
                                                       Temperature = item.Temperature,/// �¶�
                                                       WindDirection = item.WindDirection,/// ����
                                                       WindSpeed = item.WindSpeed,/// ����
                                                       //UserName = item.UserName,/// ���������
                                                       //StartTime = item.StartTime,/// ��ʼʱ��                                                       
                                                       //EndTime = item.EndTime,/// ����ʱ��
                                                       //DetectionTime = item.DetectionTime,///���ʱ��
                                                       //WasteTime = item.WasteTime, /// ͣ��ʱ��(��)
                                                       Security = item.Security,/// ������ʩ
                                                       PhxCode = item.PhxCode,/// ����豸��
                                                       PhxName = item.PhxName,/// ����豸����
                                                       DetectionMode = item.DetectionMode/// �����ⷽʽ
                                                   }).ToList();
                dialog.Progress = 60;
                workOrder.UnReachCount = selPoint.Count(m => m.IsTouch.Contains("���ɴ�"));
                sqlite.Insert(workOrder);
                dialog.Progress = 65;
                sqlite.InsertAll(groups);
                dialog.Progress = 70;
                sqlite.InsertAll(selPoint);
                dialog.Progress = 90;
                sqlite.Execute("UPDATE AndroidGroup set SealPointCount=(SELECT COUNT(*) FROM ANDROIDSEALPOINT WHERE AndroidGroup.ID=ANDROIDSEALPOINT.GroupID)");
                sqlite.Execute("UPDATE AndroidGroup set UnReachCount=(SELECT COUNT(*) FROM ANDROIDSEALPOINT WHERE AndroidGroup.ID=ANDROIDSEALPOINT.GroupID and IsTouch='���ɴ�')");
                dialog.Progress = 100;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("���빤��", ex);
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
        /// DataTableת��Ϊʵ��
        /// </summary>
        /// <param name="type">�������</param>
        /// <param name="dt"></param>
        /// <returns></returns>
        private List<WorkOrderExcelEntity> ConvertDtToEntity(int type, DataTable dt)
        {
            //DataTableתList
            List<WorkOrderExcelEntity> ExcelList = new List<WorkOrderExcelEntity>();
            foreach (DataRow row in dt.Rows)
            {
                WorkOrderExcelEntity entity = new WorkOrderExcelEntity();
                //����
                if (type == 1)
                {
                    entity.IsTouch = "�ɴ�";
                    if (row["ԭ�����ֵ"].ToString() != "")
                    {
                        entity.LastNetPPM = Convert.ToDouble(row["ԭ�����ֵ"]);
                    }
                    entity.UserName = row["������Ա"].ToString();
                    if (row["����ʱ��"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["����ʱ��"].ToString());
                    }
                }
                //���
                else if (type == 0)
                {
                    entity.IsTouch = row["�Ƿ�ɴ�"].ToString();
                    entity.LeakageDescribe = row["й©����"].ToString();
                    entity.UserName = row["���������"].ToString();
                    if (row["���ʱ��"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["���ʱ��"].ToString());
                    }
                }
                //���
                else
                {
                    entity.IsTouch = "�ɴ�";
                    entity.LeakageDescribe = row["й©����"].ToString();
                    entity.UserName = row["���������"].ToString();
                    if (row["���ʱ��"].ToString() != "")
                    {
                        entity.DetectionTime = DateTime.Parse(row["���ʱ��"].ToString());
                    }
                }
                entity.SealPointCode = row["�ܷ�����"].ToString();
                entity.DeviceCode = row["װ�ñ���"].ToString();
                entity.DeviceName = row["װ������"].ToString();
                entity.AreaName = row["��������"].ToString();
                entity.BarCode = row["Ⱥ�����λ��ά��"].ToString();
                entity.GroupCode = row["Ⱥ�����"].ToString();
                entity.GroupDescribe = row["Ⱥ��λ������"].ToString();
                entity.ExtCode = row["��չ����"].ToString();
                entity.SealPointType = row["�ܷ������"].ToString();
                if (row["��������ֵ"].ToString().Trim() != "")
                {
                    entity.BackgroundPPM = Convert.ToDouble(row["��������ֵ"]);
                }
                if (row["�����ֵ"].ToString().Trim() != "")
                {
                    entity.PPM = Convert.ToDouble(row["�����ֵ"]);
                }
                if (row["й©���ֵ"].ToString().Trim() != "")
                {
                    entity.LeakagePPM = Convert.ToDouble(row["й©���ֵ"]);
                }
                //if (row["��ʼ���ʱ��"].ToString().Trim() != "")
                //{
                //    entity.StartTime = Convert.ToDateTime(row["��ʼ���ʱ��"]);
                //}
                //if (row["�������ʱ��"].ToString().Trim() != "")
                //{
                //    entity.EndTime = Convert.ToDateTime(row["�������ʱ��"]);
                //}
                //if (row["ͣ��ʱ��"].ToString().Trim() != "")
                //{
                //    entity.WasteTime = Convert.ToInt32(row["ͣ��ʱ��"]);
                //}
                entity.Temperature = row["�¶�"].ToString();
                entity.WindDirection = row["����"].ToString();
                entity.WindSpeed = row["����"].ToString();
                entity.RedCode = row["���Ʊ��"].ToString();
                entity.Security = row["������ʩ"].ToString();
                entity.DetectionMode = row["�����ⷽʽ"].ToString();
                entity.PhxCode = row["����豸��"].ToString();
                entity.PhxName = row["����豸����"].ToString();
                ExcelList.Add(entity);
            }
         
            return ExcelList;
        }

        /// <summary>
        /// �������ݿ��ļ�
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
        /// ��װ����
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
        /// Excel��ȡDataTable�ṹ
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
        /// ��ѹ��
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
                //����Ŀ¼
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
                    dialog.SetMessage("���ڽ�ѹ���ļ�");
                });
                int total = zipFile.Size();
                int index = 0;
                //ѭ����ѹ��
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
                //����Ŀ¼
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
                //����Ŀ¼
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
        #region ���� v4.4 �汾 ������ʵ·��

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
        /// ���빤��ѡ���ļ�
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
                Toast.MakeText(this.Activity, "����ʱ�䳬�����죬�������ߵ�¼!", ToastLength.Short).Show();
                return;
            }
            Intent Intent = new Intent();
            Intent.SetType("application/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "ѡ��ѹ���ļ�"), PickZipId);
        }

        #endregion

        #region �����б����
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
                            LogHelper.ErrorLog("��ȡ���������б�", ex);
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
        /// ��⹤��
        /// </summary>
        public List<LdarWorkOrder> WorkOrderList;
        /// <summary>
        /// ���칤��
        /// </summary>
        public List<LdarCheckWorkOrder> CheckWorkOrderList;

        /// <summary>
        /// ��칤��
        /// </summary>
        public List<LdarRadomCheckWorkOrder> RadomCheckWorkOrderList;
    }
    /// <summary>
    /// ��⹤��
    /// </summary>
    public class LdarWorkOrder
    {
        ///<summary>
        /// ��⹤��Id
        ///</summary>
        public string Id { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        public string InspectionCycleId { get; set; }

        ///<summary>
        /// ���װ����ϢID ����ö��Ÿ���
        ///</summary>
        public string DeviceInfoId { get; set; }

        ///<summary>
        /// �������
        ///</summary>
        public string WorkOrderCode { get; set; }

        ///<summary>
        /// ��������
        ///</summary>
        public string WorkOrderName { get; set; }

        ///<summary>
        /// ������ʼʱ��
        ///</summary>
        public DateTime? BeginDate { get; set; }

        ///<summary>
        /// ��������ʱ��
        ///</summary>
        public DateTime? EndDate { get; set; }

        ///<summary>
        /// �ƻ����
        ///</summary>
        public string PlanCode { get; set; }

        ///<summary>
        /// �ƻ�ID
        ///</summary>
        public string WorkPlanId { get; set; }

        public string LeakageStandard { get; set; }

        ///<summary>
        /// ������������ID
        ///</summary>
        public string DetectionMechanismId { get; set; }

        ///<summary>
        /// �����
        ///</summary>
        public string TestUserId { get; set; }

        ///<summary>
        /// ���������
        ///</summary>
        public string TestUserName { get; set; }

        ///<summary>
        /// ��ע
        ///</summary>
        public string Remark { get; set; }

        ///<summary>
        /// ��˾Id
        ///</summary>
        public string CompanyId { get; set; }

        ///<summary>
        /// ҵ��״̬ 0����� 1����� 2��ɼ��
        ///</summary>
        public string BizState { get; set; }

        ///<summary>
        /// ����ID
        ///</summary>
        public string DepId { get; set; }

        ///<summary>
        /// �ܷ��������
        ///</summary>
        public int? SealingPointCount { get; set; }

        ///<summary>
        /// �ɴ��ܷ����
        ///</summary>
        public int? AbleSealingPoint { get; set; }

        ///<summary>
        /// ���ɴ��ܷ����
        ///</summary>
        public int? UNAbleSealingPoint { get; set; }

        ///<summary>
        /// �Ѽ��ɴ��ܷ����
        ///</summary>
        public int? TestSealingPoint { get; set; }

        ///<summary>
        /// �Ƿ���Ч  0 ��Ч  1 ��Ч
        ///</summary>
        public int? Enabled { get; set; }

        public int? DynamicStaticType { get; set; }

    }

    /// <summary>
    /// ���칤����
    /// </summary>
    public class LdarCheckWorkOrder
    {
        ///<summary>
        /// ���칤��Id
        ///</summary>
        public string Id { get; set; }

        ///<summary>
        /// ��ǰ����Id
        ///</summary>
        public string InspectionCycleId { get; set; }

        ///<summary>
        /// ά�޹���Id
        ///</summary>
        public string RepairWorkOrderId { get; set; }
        ///<summary>
        /// ���쵥��
        ///</summary>
        public string CheckWorkOrderCode { get; set; }
        ///<summary>
        /// ���쵥����
        ///</summary>
        public string CheckWorkOrderName { get; set; }

        ///<summary>
        /// ��ʼʱ��
        ///</summary>
        public DateTime BeginDate { get; set; }

        ///<summary>
        /// ����ʱ��
        ///</summary>
        public DateTime EndDate { get; set; }

        ///<summary>
        /// ������������ID
        ///</summary>
        public string DetectionMechanismId { get; set; }

        ///<summary>
        /// �ƻ�ID
        ///</summary>
        public string WorkPlanId { get; set; }

        ///<summary>
        /// �ƻ����
        ///</summary>
        public string PlanCode { get; set; }

        ///<summary>
        /// ��⹤��Id
        ///</summary>
        public string WorkOrderId { get; set; }

        ///<summary>
        /// ������ID
        ///</summary>
        public string CheckWorkUserId { get; set; }

        ///<summary>
        /// ����������
        ///</summary>
        public string CheckWorkUserName { get; set; }

        ///<summary>
        /// 
        ///</summary>
        public string Remark { get; set; }

        ///<summary>
        /// ��֯���ű��
        ///</summary>
        public string DepId { get; set; }

        ///<summary>
        /// ��˾Id
        ///</summary>
        public string CompanyId { get; set; }

        ///<summary>
        /// �Ƿ���Ч  0 ��Ч  1 ��Ч
        ///</summary>
        public int Enabled { get; set; }
    }


    /// <summary>
    /// �������
    /// </summary>
    public class LdarRadomCheckWorkOrder
    {
        #region ����
        /// <summary>
        /// �������ID
        /// </summary>		
        public string Id { get; set; }

        /// <summary>
        /// װ��Id
        /// </summary>	
        public string LdarDeviceId { get; set; }

        /// <summary>
        /// ����������Id
        /// </summary>
        public string InspectionCycleId { get; set; }

        /// <summary>
        /// ���ʱ��
        /// </summary>	
        public DateTime? TestTime { get; set; }

        /// <summary>
        /// ʵ�ʳ��ʱ��
        /// </summary>		
        public DateTime? ActualTime { get; set; }

        /// <summary>
        /// �������,
        ///1��ҵ������Ϣ
        ///2�����Ա�����ַ��Ƿ���ȷ
        ///3�������
        ///4��ҵ����������Ƶ���
        ///5��ҵ�޸����
        ///6��ҵ�ӳ��޸���� 
        ///7���ݴ���ʱЧ��
        /// </summary>	
        public string TestContent { get; set; }

        /// <summary>
        ///��������ID
        /// </summary>
        public string AssessmentId { get; set; }

        /// <summary>
        /// ������
        /// </summary>	
        public string OPINION { get; set; }

        /// <summary>
        /// ��������ID
        /// </summary>	
        public string EnvironmentalId { get; set; }

        /// <summary>
        /// ״̬��0-���������ƶ�������� 1-�������ų�� 2-�������ŷ��� 3-����
        /// </summary>	
        public int AudiStatus { get; set; } = 0;

        /// <summary>
        /// 1 ���ö�����ⷽ���Ĳ�й©��λ�����ո�������ͳ��5%�����ĵ�λ 2 �������������λ����
        /// </summary>	
        public int? TestMethod { get; set; }

        /// <summary>
        /// ������
        /// </summary>		
        public decimal? TestProportion { get; set; }

        /// <summary>
        /// ��˾Id
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        /// �Ƿ���Ч  0 ��Ч  1 ��Ч
        /// </summary>		
        public int Enabled { get; set; } = 1;

        /// <summary>
        /// ¼����Ա
        /// </summary>		
        public string InUser { get; set; }

        /// <summary>
        /// ¼������
        /// </summary>	
        public DateTime? InDate { get; set; }

        /// <summary>
        /// �޸���Ա
        /// </summary>	
        public string EditUser { get; set; }

        /// <summary>
        /// �޸�����
        /// </summary>		
        public DateTime? EditDate { get; set; }

        /// <summary>
        /// ɾ����Ա
        /// </summary>	
        public string DelUser { get; set; }

        /// <summary>
        /// ��ʶ���������Ƿ�ɾ�� 0δɾ�� 1��ɾ��
        /// </summary>	
        public int DelState { get; set; }

        /// <summary>
        /// ɾ��ʱ��
        /// </summary>	
        public DateTime? DelDate { get; set; }
        #endregion
    }

    /// <summary>
    /// �첽ִ��״̬,
    /// </summary>
    public class AsynchronousExecutionStateModel
    {
        /// <summary>
        /// ��ʱ���ã�����ʵ�������չ
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// ״̬��Ϣ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// ��Ϣ���ͣ�����ʵ��ʹ���������˳����չ,��д�ñ�ע
        /// <para>0��������Ϣ���磺˳����ʾ��ִ��״̬��</para>
        /// <para>1ˢ����Ϣ(�磺ֻ��ʾһ���Ľ������ٷְ�)</para>
        /// <para>200����ɣ����ټ���������Ϣ��</para>
        /// <para>500�����󣨲��ټ���������Ϣ��</para>
        /// </summary>
        public int SType { get; set; }

        /// <summary>
        /// ����ConnectionId
        /// </summary>
        public string ConnectionId { get; set; }
    }
}