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
    public class CalibrationListActivity : AppCompatActivity
    {
        List<AndroidCalibration> CaList;
        List<AndroidCalibrationLog> logList;
        string DataDirPath = Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/" + "CalibrationSignature";
        string nameKey = "校准记录审核人";
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.CalibrationList);
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                CaList = connection.Table<AndroidCalibration>().OrderByDescending(c => c.InsertTime).ToList();
                logList = connection.Table<AndroidCalibrationLog>().OrderByDescending(c => c.CalibrationID).ToList();
                foreach (var item in CaList)
                {
                    item.LogList = logList.FindAll(c => c.CalibrationID == item.ID).OrderBy(c => c.TheoryValue).ToList();
                }
                FindViewById<ListView>(Resource.Id.lvCalibration).Adapter = new CalibrationListAdapter(this, CaList);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("加载校准记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Catoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "校准记录";
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
                        dialogService.ShowYesNo(this, "提示", "是否要清空校准记录？清空后无法恢复",
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
        /// <summary>
        /// 签名是否通过
        /// </summary>
        /// <param name="dataDir"></param>
        /// <returns></returns>
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
                    BackGroudUpLoad model = new BackGroudUpLoad { Path = filename, FileName = filename, InspectionCycleId = YearRound[spRound.SelectedItemPosition].Id, CompanyId = YearRound[spRound.SelectedItemPosition].CompanyId, InUser = YearRound[spRound.SelectedItemPosition].InUser };
                    //服务器处理
                    BaseResult<bool> msg = Utility.CallService<bool>(serverList[spApi.SelectedItemPosition].ApiAddress, "V10Enterprise/LdarInstrumenRecord/ImportExcel", model, 300000, null);
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
                List<CalibrationExcel> excelList = (from item in logList
                                                    join ca in CaList on item.CalibrationID equals ca.ID
                                                    select new CalibrationExcel
                                                    {
                                                        CalibrationID = ca.ID,
                                                        装置名称 = ca.DeviceName,
                                                        装置编码 = ca.DeviceCode,
                                                        检测仪器名称 = ca.PhxName,
                                                        检测仪器型号 = ca.PhxType,
                                                        仪器序列号 = ca.PhxCode,
                                                        校正人员姓名 = ca.User,
                                                        确认人员姓名 = ca.Confirm,
                                                        标准气体名称 = ca.GasName,
                                                        备注 = ca.Remark,
                                                        标准气体浓度 = item.TheoryValue.ToString(),
                                                        校准读数 = item.RealityValue.ToString(),
                                                        仪器校正时间 = item.LogTime.ToString(),
                                                        平均反应时间 = item.ReactionTime.ToString(),
                                                        仪器漂移误差 = item.Deviation.ToString(),
                                                        审核人 = CheckName
                                                    }).OrderByDescending(c => c.CalibrationID).ToList();
                //生成Excel文件
                string path = CreateExcelFile(excelList);
                return path;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("导出校准数据", ex);
                return null;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        void ExportExcel()
        {
            if (logList == null || logList.Count == 0)
                return;
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
                    List<CalibrationExcel> excelList = (from item in logList
                                                        join ca in CaList on item.CalibrationID equals ca.ID
                                                        select new CalibrationExcel
                                                        {
                                                            CalibrationID = ca.ID,
                                                            装置名称 = ca.DeviceName,
                                                            装置编码 = ca.DeviceCode,
                                                            检测仪器名称 = ca.PhxName,
                                                            检测仪器型号 = ca.PhxType,
                                                            仪器序列号 = ca.PhxCode,
                                                            校正人员姓名 = ca.User,
                                                            确认人员姓名 = ca.Confirm,
                                                            标准气体名称 = ca.GasName,
                                                            备注 = ca.Remark,
                                                            标准气体浓度 = item.TheoryValue.ToString(),
                                                            校准读数 = item.RealityValue.ToString(),
                                                            仪器校正时间 = item.LogTime.ToString(),
                                                            平均反应时间 = item.ReactionTime.ToString(),
                                                            仪器漂移误差 = item.Deviation.ToString(),
                                                            审核人 = CheckName
                                                        }).OrderByDescending(c => c.CalibrationID).ToList();
                    //生成Excel文件
                    string path = CreateExcelFile(excelList);
                    this.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this, "导出成功", "导出文件存储在：" + path, null);
                    });
                    LogHelper.InfoLog("校准数据导出成功");
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("导出校准数据", ex);
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

        public string CreateExcelFile(List<CalibrationExcel> excelList)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + "校准记录" + "_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx";
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
            if (worksheet.Range["K1"].Text == "标准气体浓度")
            {
                worksheet.Range["K1"].Text = "标准气体浓度μmol/mol";
            }
            if (worksheet.Range["L1"].Text == "校准读数")
            {
                worksheet.Range["L1"].Text = "校准读数μmol/mol";
            }
            if (worksheet.Range["N1"].Text == "平均反应时间")
            {
                worksheet.Range["N1"].Text = "平均反应时间(s)";
            }
            ////////////////////////////////合并//////////////////////////////////////
            foreach (var item in excelList)
            {
                int index = excelList.IndexOf(item);
                if (index != (excelList.Count - 1) && item.CalibrationID == excelList[index + 1].CalibrationID)
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
            path = FileHelper.GetHashCodeFromFile(path, EnumDataType.Calibration);
            return path;
        }

        void ClearUp()
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                connection.DeleteAll<AndroidCalibration>();
                connection.DeleteAll<AndroidCalibrationLog>();
                FindViewById<ListView>(Resource.Id.lvCalibration).Adapter = null;
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("清空校准记录", ex);
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
}