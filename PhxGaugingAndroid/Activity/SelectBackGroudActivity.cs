using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content.Res;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;
using PhxGaugingAndroid.Fragments;

namespace PhxGaugingAndroid
{
    [Activity(MainLauncher = false, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class SelectBackGroudActivity : AppCompatActivity
    {
        List<AndroidBackground> bgList;
        ClearnEditText eTAvgValue;
        ClearnEditText eTTemplate;
        ClearnEditText eTWindSpeed;
        ClearnEditText eTHumidity;
        ClearnEditText eTPress;
        ClearnEditText eTWindDir;
        AndroidBackground selectValue;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SelectBackGroud);
            ListView listview = FindViewById<ListView>(Resource.Id.lvSelectBackGroud);
            listview.ItemClick -= Listview_ItemClick;
            listview.ItemClick += Listview_ItemClick;
            listview.RequestFocus();
            eTAvgValue = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTAvgValue);
            eTTemplate = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTTemplate);
            eTWindSpeed = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTWindSpeed);
            eTHumidity = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTHumidity);
            eTPress = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTPress);
            eTWindDir = FindViewById<PhxGaugingAndroid.Fragments.ClearnEditText>(Resource.Id.eTWindDir);
            string value = this.Intent.GetStringExtra("SelectValue");
            if (value != null && value != string.Empty)
            {
                AndroidBackground backGroud = Newtonsoft.Json.JsonConvert.DeserializeObject<AndroidBackground>(value);
                eTAvgValue.Text = backGroud.AvgValue.ToString();
                eTTemplate.Text = backGroud.Temperature.ToString();
                eTWindSpeed.Text = backGroud.WindSpeed.ToString();
                eTHumidity.Text = backGroud.Humidity.ToString();
                eTPress.Text = backGroud.Atmos.ToString();
                eTWindDir.Text = backGroud.WindDirection;
                selectValue = backGroud;
            }
            FindViewById<Button>(Resource.Id.btnSbClearn).Click += BtnClearn_Click;
            FindViewById<Button>(Resource.Id.btnSbOk).Click += BtnOK_Click;
            SQLite.SQLiteConnection connection = null;
            try
            {
                connection = new SQLite.SQLiteConnection(Android.OS.Environment.ExternalStorageDirectory + "/LDARAPP6/sqliteSys.db");
                bgList = connection.Table<AndroidBackground>().OrderByDescending(c => c.CreateTime).ToList();
                listview.Adapter = new SelectBackGroudAdapter(this, bgList);
            }
            catch (Exception ex)
            {
                LogHelper.ErrorLog("加载选择环境背景值记录", ex);
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                    connection.Dispose();
                }
            }
            var toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.Selecttoolbar);
            if (toolbar != null)
            {
                toolbar.Title = "选择环境背景值";
                toolbar.NavigationIcon = ResourcesCompat.GetDrawable(Resources, Resource.Drawable.ic_return1, null);
                SetSupportActionBar(toolbar);
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(false);
            }
            toolbar.NavigationClick += (s, e) =>
            {
                Finish();
            };
        }
        /// <summary>
        /// 确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (eTAvgValue.Text != string.Empty || eTHumidity.Text != string.Empty || eTPress.Text != string.Empty || eTTemplate.Text != string.Empty || eTWindDir.Text != string.Empty || eTWindSpeed.Text != string.Empty)
            {
                if (selectValue == null)
                {
                    selectValue = new AndroidBackground();
                }
                if (eTAvgValue.Text != string.Empty)
                {
                    selectValue.AvgValue = double.Parse(eTAvgValue.Text);
                }
                else
                {
                    selectValue.AvgValue = null;
                }
                if (eTHumidity.Text != string.Empty)
                {
                    selectValue.Humidity = double.Parse(eTHumidity.Text);
                }
                else
                {
                    selectValue.Humidity = null;
                }
                if (eTPress.Text != string.Empty)
                {
                    selectValue.Atmos = double.Parse(eTPress.Text);
                }
                else
                {
                    selectValue.Atmos = null;
                }
                if (eTTemplate.Text != string.Empty)
                {
                    selectValue.Temperature = double.Parse(eTTemplate.Text);
                }
                else
                {
                    selectValue.Temperature = null;
                }
                if (eTWindDir.Text != string.Empty)
                {
                    selectValue.WindDirection = eTWindDir.Text;
                }
                else
                {
                    selectValue.WindDirection = null;
                }
                if (eTWindSpeed.Text != string.Empty)
                {
                    selectValue.WindSpeed = double.Parse(eTWindSpeed.Text);
                }
                else
                {
                    selectValue.WindSpeed = null;
                }
            }
            else
            {
                selectValue = null;
            }
            Intent i;
            string LDARDevice = UserPreferences.GetString("SelectLDARDevice");
            if (LDARDevice == "EXEPC3100")
            {
                i = new Intent(this, typeof(ExepcSealPointFragment));
            }
            else if(LDARDevice == "TVA2020")
            {
                i = new Intent(this, typeof(TvaSealPointFragment));
            }
            else
            {
                i = new Intent(this, typeof(SealPointFragment));
            }
            if (selectValue != null)
            {
                i.PutExtra("SelectValue", Newtonsoft.Json.JsonConvert.SerializeObject(selectValue));
            }
            SetResult(Result.Ok, i);
            Finish();
        }
        /// <summary>
        /// 清除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClearn_Click(object sender, EventArgs e)
        {
            eTAvgValue.Text = "";
            eTTemplate.Text = "";
            eTWindSpeed.Text = "";
            eTHumidity.Text = "";
            eTPress.Text = "";
            eTWindDir.Text = "";
            selectValue = null;
        }

        private void Listview_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            selectValue = bgList[e.Position];
            eTAvgValue.Text = selectValue.AvgValue.ToString();
            eTTemplate.Text = selectValue.Temperature.ToString();
            eTWindSpeed.Text = selectValue.WindSpeed.ToString();
            eTHumidity.Text = selectValue.Humidity.ToString();
            eTPress.Text = selectValue.Atmos.ToString();
            eTWindDir.Text = selectValue.WindDirection;
        }
    }
}