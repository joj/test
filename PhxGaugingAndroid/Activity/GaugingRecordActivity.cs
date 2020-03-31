using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;
using Syncfusion.XlsIO;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class GaugingRecordActivity : AppCompatActivity
    {
        List<AndroidRecord> RecordList;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.GaugingList);
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                RecordList = connection.Table<AndroidRecord>().OrderByDescending(c => c.EndTime).ToList();
                FindViewById<ListView>(Resource.Id.lvGauging).Adapter = new GaugingListAdapter(this, RecordList);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("加载检测记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Gautoolbar);
            if (toolbar != null)
            {
                
                toolbar.Title = "检测记录";
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
                        dialogService.ShowYesNo(this, "提示", "是否要清空检测记录？清空后无法恢复",
                                r =>
                                {
                                    if (r == DialogResult.Yes)
                                    {
                                        ClearUp();
                                    }
                                });
                        break;
                    //导出
                    case Resource.Id.menu_2:
                        ExportExcel();
                        break;
                }
            };
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actionMenu2, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        void ExportExcel()
        {
            if (RecordList == null || RecordList.Count == 0)
                return;
            Android.App.ProgressDialog dilog = Android.App.ProgressDialog.Show(this, "提示", "数据导出中，请不要进行任何操作！！！", true, false);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    //组装数据
                    List<RecordExcel> excelList = (from item in RecordList
                                                   select new RecordExcel
                                                   {
                                                       密封点编码 = item.GroupCode + item.SealPointSeq,
                                                       群组条形或二维码 = item.BarCode,
                                                       群组编码 = item.GroupCode,
                                                       扩展编码 = item.SealPointSeq,
                                                       环境本底值 = item.BackgroundPPM.ToString(),
                                                       净检测值 = item.PPM.ToString(),
                                                       泄漏检测值 = item.LeakagePPM.ToString(),
                                                       开始检测时间 = item.StartTime.ToString(),
                                                       结束检测时间 = item.EndTime.ToString(),
                                                       停留时间 = item.WasteTime.ToString(),
                                                       温度 = item.Temperature ?? "",
                                                       风向 = item.WindDirection ?? "",
                                                       风速 = item.WindSpeed ?? "",
                                                       检测人名称 = item.UserName,
                                                       检测时间 = item.EndTime.ToString(),
                                                       检测设备号 = item.PhxCode ?? "",
                                                       检测设备名称 = item.PhxName ?? ""
                                                   }).OrderByDescending(c => c.结束检测时间).ToList();
                    //生成Excel文件
                    string path = CreateExcelFile(excelList);
                    this.RunOnUiThread(delegate
                    {
                        DialogService dialog = new DialogService();
                        dialog.ShowOk(this, "导出成功", "导出文件存储在：" + path, null);
                    });
                    LogHelper.InfoLog("检测记录导出成功");
                }
                catch (Exception ex)
                {
                    LogHelper.ErrorLog("导出检测记录", ex);
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

        public string CreateExcelFile(List<RecordExcel> excelList)
        {
            string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" + "检测记录" + "_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss") + ".xlsx";
            ExcelEngine excelEngine = new ExcelEngine();
            excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2007;
            IWorkbook workbook = excelEngine.Excel.Workbooks.Create(1);
            IWorksheet worksheet = workbook.Worksheets[0];
            worksheet.ImportData(excelList, 1, 1, true);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            workbook.SaveAs(stream);
            workbook.Close();
            excelEngine.Dispose();
            Java.IO.File file = new Java.IO.File(path);
            FileOutputStream outs = new FileOutputStream(file);
            outs.Write(stream.ToArray());
            outs.Flush();
            outs.Close();
            return path;
        }

        void ClearUp()
        {
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                connection.DeleteAll<AndroidRecord>();
                FindViewById<ListView>(Resource.Id.lvGauging).Adapter = null;
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