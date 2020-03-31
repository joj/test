using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.IO;
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;
using Syncfusion.XlsIO;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class BackGroudListActivity : AppCompatActivity
    {
        List<AndroidBackground> bgList;
        List<AndroidBackgroundLog> logList;
        string DataDirPath = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + "BackGroudSignature";
        string nameKey = "环境背景值审核人";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.BackGroudList);
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                bgList = connection.Table<AndroidBackground>().OrderByDescending(c => c.CreateTime).ToList();
                logList = connection.Table<AndroidBackgroundLog>().OrderByDescending(c => c.BackgroundID).ToList();
                foreach (var item in bgList)
                {
                    item.LogList = logList.FindAll(c => c.BackgroundID == item.ID).OrderByDescending(c => c.EndTime).ToList();
                }
                FindViewById<ListView>(Resource.Id.lvBackGroud).Adapter = new BackGroudListAdapter(this, bgList);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("加载环境背景值记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
            if (toolbar != null)
            {
                toolbar.Title = "环境背景值记录";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.MenuItemClick += (s, e) =>
            {
                switch (e.Item.ItemId)
                {
                    //清空
                    case Resource.Id.menu_1:
                        DialogService dialogService = new DialogService();
                        dialogService.ShowYesNo(this, "提示", "是否要清空环境背景值？清空后无法恢复",
                                r =>
                                {
                                    if (r == DialogResult.Yes)
                                    {
                                        ClearUp();
                                    }
                                });
                        break;
                    //签名
                    case Resource.Id.menu_4:
                    case Resource.Id.menu_5:
                        if (logList == null || logList.Count == 0)
                            return;
                        Intent i = new Intent(this, typeof(SignaturePadActivity));
                        i.PutExtra("SignatureType", e.Item.ToString());
                        if (System.IO.Directory.Exists(DataDirPath) == false)
                        {
                            System.IO.Directory.CreateDirectory(DataDirPath);
                        }
                        i.PutExtra("WorkOrderPath", DataDirPath);
                        i.PutExtra("WorkOrderName", nameKey);
                        this.StartActivity(i);
                        break;
                    //导出
                    case Resource.Id.menu_2:
                        if (IsSignaturePass(DataDirPath) == false)
                        {
                            return;
                        }
                        ExportExcel();
                        break;
                    //上传服务器
                    case Resource.Id.menu_3:
                        if (IsSignaturePass(DataDirPath) == false)
                        {
                            return;
                        }
                        UploadData();
                        break;
                }
            };
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
        }
        private bool IsSignaturePass(string dataDir)
        {
            var signafile = System.IO.Path.Combine(dataDir, "signature.Png");
            if (System.IO.File.Exists(signafile) == false)
            {
                Toast.MakeText(this, "检测人请签名", ToastLength.Short).Show();
                return false;
            }
            //审核人签名
            string CheckName = UserPreferences.GetString(nameKey);
            if (string.IsNullOrWhiteSpace(CheckName))
            {
                Toast.MakeText(this, "请审核人签名", ToastLength.Short).Show();
                return false;
            }
            //审核人手写签名
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
                    Toast.MakeText(this, "请审核人手写签名", ToastLength.Short).Show();
                    return false;
                }
            }
            return true;
        }
        Android.App.AlertDialog dialog;
        Spinner spApi;
        List<AndroidServerUrl> serverList;
        EditText etCompany;
        EditText etUser;
        EditText etPwd;
        Spinner spRound;
        LinearLayout lineLoginAPI;
        LinearLayout lineDownAPI;
        Button btnDownLogin;
        List<KVP> YearRound;
        /// <summary>
        /// 上传数据
        /// </summary>
        private void UploadData()
        {
            LayoutInflater inflater = this.LayoutInflater;
            View layout = inflater.Inflate(Resource.Layout.UploadData, (ViewGroup)this.FindViewById<ViewGroup>(Resource.Id.rlDown));
            Android.App.AlertDialog.Builder builder = new Android.App.AlertDialog.Builder(this);
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
            spApi.Adapter = new ArrayAdapter(this, Resource.Layout.ListViewItem, areas);
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
            spRound = layout.FindViewById<Spinner>(Resource.Id.spRound);
            lineLoginAPI = layout.FindViewById<LinearLayout>(Resource.Id.lineLoginAPI);
            lineDownAPI = layout.FindViewById<LinearLayout>(Resource.Id.lineDownAPI);
            //下载
            var btnDownStart = layout.FindViewById<Button>(Resource.Id.btnDownStart);
            btnDownStart.Click -= BtnDownStart_ClickAsync;
            btnDownStart.Click += BtnDownStart_ClickAsync;
            var btnDownReturn = layout.FindViewById<Button>(Resource.Id.btnDownReturn);
            btnDownReturn.Click -= BtnDownReturn_Click;
            btnDownReturn.Click += BtnDownReturn_Click;
            dialog = builder.Show();
        }

        private void SpApi_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            UserPreferences.SetString("SelectApiAdress", spApi.SelectedItem.ToString());
        }
        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownStart_ClickAsync(object sender, EventArgs e)
        {
            if (spRound.SelectedItemPosition == -1)
            {
                return;
            }
            Toast toast = Toast.MakeText(this, "", ToastLength.Long);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this, "提示", "数据生成中，请不要进行任何操作！！！", true, false);
            ProgressDialog pgdilog = new ProgressDialog(this);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //生成Excel文件
                    string fullfilename = ExportExcelForUpLoad();
                    //2.上传Excel                
                    pgdilog.SetProgressStyle(ProgressDialogStyle.Horizontal);
                    pgdilog.Indeterminate = false;
                    pgdilog.SetCancelable(false);
                    pgdilog.SetCanceledOnTouchOutside(false);
                    pgdilog.Max = 100;
                    pgdilog.SetTitle("数据上传中，请不要进行任何操作");
                    pgdilog.SetMessage("正在上传文件");
                    this.RunOnUiThread(() =>
                    {
                        dilog.Hide();
                        pgdilog.Show();
                    });
                    Uri server = new Uri(serverList[spApi.SelectedItemPosition].ApiAddress + "/V10AppPC/AppTest/Post");
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
                        this.RunOnUiThread(() =>
                        {
                            DialogService dialog = new DialogService();
                            dialog.ShowOk(this, "提示", "上传数据失败", null);
                        });
                        return;
                    }
                    this.RunOnUiThread(() =>
                    {
                        dilog.SetTitle("服务器正在处理数据");
                        dilog.SetMessage("");
                    });
                    //3.通知服务器导入Excel                        
                    this.RunOnUiThread(() =>
                    {
                        pgdilog.Hide();
                        dilog.Show();
                    });
                    BackGroudUpLoad model = new BackGroudUpLoad { Path = filename, FileName = filename, InspectionCycleId = YearRound[spRound.SelectedItemPosition].Id, CompanyId = YearRound[spRound.SelectedItemPosition].CompanyId };
                    BaseResult<bool> msg = Utility.CallService<bool>(serverList[spApi.SelectedItemPosition].ApiAddress, "V10Enterprise/LdarWorkOrderWeather/ExcelLdarWorkOrderWeather", model, 300000, null);
                    if (msg == null)
                    {
                        this.RunOnUiThread(() =>
                        {
                            toast.SetText("网络连接失败");
                            toast.Show();
                        });
                    }
                    else if (msg.Code == 10000 && msg.Data == true)
                    {
                        toast.SetText("上传成功");
                        toast.Show();
                        dialog.Dismiss();
                    }
                    else
                    {
                        this.RunOnUiThread(() =>
                        {
                            toast.SetText(msg.Message);
                            toast.Show();
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("上传环境背景值数据", ex);
                    this.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this, "上传数据发生错误", ex.Message, null);
                    });
                }
                finally
                {
                    dilog.Dismiss();
                    pgdilog.Dismiss();
                }
            });
        }
        List<string> cycleList;
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDownLogin_Click(object sender, EventArgs e)
        {
            if (spApi.SelectedItemPosition == -1 || etCompany.Text.Trim() == string.Empty || etUser.Text.Trim() == string.Empty || etPwd.Text.Trim() == string.Empty)
                return;
            AndroidServerUrl server = serverList[spApi.SelectedItemPosition];
            Toast toast = Toast.MakeText(this, "", ToastLength.Short);
            toast.SetGravity(GravityFlags.Center, 0, 0);
            BaseResult<List<KVP>> msg = null;
            LoginModel model = new LoginModel { CompanyId = etCompany.Text.Trim(), LoginName = etUser.Text.Trim(), UserPass = etPwd.Text.Trim() };
            msg = Utility.CallService<List<KVP>>(server.ApiAddress, "V10AppPC/AppTest/LoginAPIServerForUpLoadData", model, 30000, null);
            if (msg == null)
            {
                toast.SetText("网络连接失败");
                toast.Show();
            }
            else if (msg.Code == 10000)
            {
                YearRound = msg.Data;
                this.RunOnUiThread(delegate
                {
                    var btn = sender as Button;
                    InputMethodManager mInputMethodManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
                    mInputMethodManager.HideSoftInputFromWindow(btn.WindowToken, 0);
                });
                cycleList = new List<string>();
                foreach (var item in msg.Data)
                {
                    cycleList.Add(item.YearRound);
                }
                spRound.Adapter = new ArrayAdapter(this, Resource.Layout.ListViewItem, cycleList);
                spRound.Prompt = "请选择轮次";
                lineLoginAPI.Visibility = ViewStates.Gone;
                lineDownAPI.Visibility = ViewStates.Visible;
            }
            else
            {
                toast.SetText(msg.Message);
                toast.Show();
            }
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
        private void BtnDownReturn_Click(object sender, EventArgs e)
        {
            lineLoginAPI.Visibility = ViewStates.Visible;
            lineDownAPI.Visibility = ViewStates.Gone;
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        /// <summary>
        /// 生成上传文件
        /// </summary>
        /// <returns></returns>
        string ExportExcelForUpLoad()
        {
            if (logList == null || logList.Count == 0)
                return null;
            try
            {
                bool isDoubleCheck = false;
                string doubleCheck = UserPreferences.GetString("DoubleCheck");
                if (doubleCheck != null && doubleCheck != string.Empty)
                {
                    isDoubleCheck = bool.Parse(doubleCheck);
                }
                string CheckName = UserPreferences.GetString(nameKey);
                //string CheckName = string.Empty;
                //if (isDoubleCheck)
                //{
                //    CheckName = UserPreferences.GetString(nameKey);
                //}
                //组装数据
                List<BackGroudExcel> excelList = (from item in logList
                                                  join bg in bgList on item.BackgroundID equals bg.ID
                                                  select new BackGroudExcel
                                                  {
                                                      BackGroudID = bg.ID,
                                                      装置名称 = bg.Name,
                                                      装置编码 = bg.Code,
                                                      创建时间 = bg.CreateTime.ToString(),
                                                      平均值 = bg.AvgValue.ToString(),
                                                      温度 = bg.Temperature.ToString(),
                                                      湿度 = bg.Humidity.ToString(),
                                                      大气压 = bg.Atmos.ToString(),
                                                      风向 = bg.WindDirection,
                                                      风速 = bg.WindSpeed.ToString(),
                                                      检测人 = bg.User,
                                                      检测仪器名称 = bg.PhxName,
                                                      检测仪器序列号 = bg.PhxCode,
                                                      备注 = bg.Remark,
                                                      检测时间 = item.EndTime.ToString(),
                                                      检测用时 = item.WasteTime.ToString(),
                                                      检测值 = item.DetectionValue.ToString(),
                                                      检测位置 = item.Position,
                                                      审核人 = CheckName
                                                  }).OrderByDescending(c => c.创建时间).ToList();
                //生成Excel文件
                string path = CreateExcelFile(excelList);
                return path;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("导出环境背景值数据", ex);
                return null;
            }
        }
        /// <summary>
        /// 导出文件
        /// </summary>
        void ExportExcel()
        {
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this, "提示", "数据导出中，请不要进行任何操作！！！", true, false);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    string CheckName = UserPreferences.GetString(nameKey);
                    //bool isDoubleCheck = true;
                    //string doubleCheck = UserPreferences.GetString("DoubleCheck");
                    //if (doubleCheck != null && doubleCheck != string.Empty)
                    //{
                    //    isDoubleCheck = bool.Parse(doubleCheck);
                    //}
                    //string CheckName = string.Empty;
                    //if (isDoubleCheck)
                    //{
                    //    CheckName = UserPreferences.GetString(nameKey);
                    //}
                    //组装数据
                    List<BackGroudExcel> excelList = (from item in logList
                                                      join bg in bgList on item.BackgroundID equals bg.ID
                                                      select new BackGroudExcel
                                                      {
                                                          BackGroudID = bg.ID,
                                                          装置名称 = bg.Name,
                                                          装置编码 = bg.Code,
                                                          创建时间 = bg.CreateTime.ToString(),
                                                          平均值 = bg.AvgValue.ToString(),
                                                          温度 = bg.Temperature.ToString(),
                                                          湿度 = bg.Humidity.ToString(),
                                                          大气压 = bg.Atmos.ToString(),
                                                          风向 = bg.WindDirection,
                                                          风速 = bg.WindSpeed.ToString(),
                                                          检测人 = bg.User,
                                                          检测仪器名称 = bg.PhxName,
                                                          检测仪器序列号 = bg.PhxCode,
                                                          备注 = bg.Remark,
                                                          检测时间 = item.EndTime.ToString(),
                                                          检测用时 = item.WasteTime.ToString(),
                                                          检测值 = item.DetectionValue.ToString(),
                                                          检测位置 = item.Position,
                                                          审核人 = CheckName
                                                      }).OrderByDescending(c => c.创建时间).ToList();
                    //生成Excel文件
                    string path = CreateExcelFile(excelList);
                    this.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this, "导出成功", "导出文件存储在：" + path, null);
                    });
                    LogHelper.InfoLog("环境背景值数据导出成功");
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("导出环境背景值数据", ex);
                    this.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this, "导出错误", ex.Message, null);
                    });
                }
                finally
                {
                    dilog.Dismiss();
                }
            });
        }
        /// <summary>
        /// 生成Excel文件
        /// </summary>
        /// <param name="excelList"></param>
        /// <returns></returns>
        public string CreateExcelFile(List<BackGroudExcel> excelList)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + "环境背景值" + "_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx";
            ExcelEngine excelEngine = new ExcelEngine();
            excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2007;
            IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];

            IWorksheet worksheet1 = workbook.Worksheets.Create("检测人签名");
            var signafile = System.IO.Path.Combine(this.DataDirPath, "signature.Png");
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
                var signafileCheck = System.IO.Path.Combine(this.DataDirPath, "signatureDouble.Png");
                signaCheckStream = new System.IO.FileStream(signafileCheck, System.IO.FileMode.Open);
                worksheet2.Pictures.AddPicture(1, 1, signaCheckStream);
            }

            worksheet.ImportData(excelList, 1, 1, true);
            if (worksheet.Range["P1"].Text == "检测用时")
            {
                worksheet.Range["P1"].Text = "检测用时（秒）";
            }
            ////////////////////////////////合并
            foreach (var item in excelList)
            {
                int index = excelList.IndexOf(item);
                if (index != (excelList.Count - 1) && item.BackGroudID == excelList[index + 1].BackGroudID)
                {
                    worksheet.Range["A" + (index + 2).ToString() + ":A" + (index + 3).ToString()].Merge();
                    worksheet.Range["B" + (index + 2).ToString() + ":B" + (index + 3).ToString()].Merge();
                    worksheet.Range["C" + (index + 2).ToString() + ":C" + (index + 3).ToString()].Merge();
                    worksheet.Range["D" + (index + 2).ToString() + ":D" + (index + 3).ToString()].Merge();
                    worksheet.Range["E" + (index + 2).ToString() + ":E" + (index + 3).ToString()].Merge();
                    worksheet.Range["F" + (index + 2).ToString() + ":F" + (index + 3).ToString()].Merge();
                    worksheet.Range["G" + (index + 2).ToString() + ":G" + (index + 3).ToString()].Merge();
                    worksheet.Range["H" + (index + 2).ToString() + ":H" + (index + 3).ToString()].Merge();
                    worksheet.Range["I" + (index + 2).ToString() + ":I" + (index + 3).ToString()].Merge();
                    worksheet.Range["J" + (index + 2).ToString() + ":J" + (index + 3).ToString()].Merge();
                    worksheet.Range["K" + (index + 2).ToString() + ":K" + (index + 3).ToString()].Merge();
                    worksheet.Range["L" + (index + 2).ToString() + ":L" + (index + 3).ToString()].Merge();
                    worksheet.Range["M" + (index + 2).ToString() + ":M" + (index + 3).ToString()].Merge();
                    worksheet.Range["N" + (index + 2).ToString() + ":N" + (index + 3).ToString()].Merge();
                }
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
            path = FileHelper.GetHashCodeFromFile(path, EnumDataType.Background);
            return path;
        }

        void ClearUp()
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                connection.DeleteAll<AndroidBackground>();
                connection.DeleteAll<AndroidBackgroundLog>();
                FindViewById<ListView>(Resource.Id.lvBackGroud).Adapter = null;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("清空环境背景值记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
        }
    }

    public class KVP
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 显示文本
        /// </summary>
        public string YearRound { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string DepId { get; set; }
        /// <summary>
        /// 用户ID
        /// </summary>
        public string InUser { get; set; }
    }

    public class BackGroudUpLoad
    {
        public string Path { get; set; }
        public string CompanyId { get; set; }
        public string InspectionCycleId { get; set; }
        public string InUser { get; set; }
        public string FileName { get; set; }
    }
}