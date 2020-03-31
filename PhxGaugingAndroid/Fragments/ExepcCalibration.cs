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
using Newtonsoft.Json;
using PhxGaugingAndroid.Common;
using PhxGaugingAndroid.Entity;

namespace PhxGaugingAndroid.Fragments
{
    public class ExepcCalibration : Fragment
    {
        Button btnReadCalibration;
        Button btnSetCalibration;
        ClearnEditText CalibrationText1;
        ClearnEditText CalibrationText2;
        ClearnEditText CalibrationText3;
        ClearnEditText CalibrationText4;
        ClearnEditText CalibrationText5;
        ClearnEditText CalibrationText6;
        ClearnEditText CalibrationText7;
        ClearnEditText CalibrationText8;
        ClearnEditText CalibrationText9;
        List<ClearnEditText> textList;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            context = this.Activity;
            App.exepcService.ReadCalibrationResult -= ExepcService_ReadCalibrationResult;
            App.exepcService.ReadCalibrationResult += ExepcService_ReadCalibrationResult;
            App.exepcService.SetCalibrationResult -= ExepcService_SetCalibrationResult;
            App.exepcService.SetCalibrationResult += ExepcService_SetCalibrationResult;
        }
        Activity context;
        public static ExepcCalibration NewInstance()
        {
            var frag = new ExepcCalibration { Arguments = new Bundle() };
            return frag;
        }
        /// <summary>
        /// 设置校准返回
        /// </summary>
        /// <param name="isSuccess"></param>
        private void ExepcService_SetCalibrationResult(bool isSuccess)
        {
            this.Activity.RunOnUiThread(() =>
            {
                Toast.MakeText(this.Activity, isSuccess ? "设置成功" : "设置失败", ToastLength.Short).Show();
            });
        }
        /// <summary>
        /// 读取校准返回
        /// </summary>
        /// <param name="calibrationList"></param>
        private void ExepcService_ReadCalibrationResult(List<float> calibrationList)
        {
            this.Activity.RunOnUiThread(() =>
            {
                textList.ForEach(item => item.Text = "");
                for (int i = 0; i < calibrationList.Count; i++)
                {
                    textList[i].Text = calibrationList[i].ToString("F0");
                }
            });
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View v = inflater.Inflate(Resource.Layout.ExepcCalibration, container, false);
            btnReadCalibration = v.FindViewById<Button>(Resource.Id.btnReadCalibration);
            btnReadCalibration.Click += BtnReadCalibration_Click;
            btnSetCalibration = v.FindViewById<Button>(Resource.Id.btnSetCalibration);
            btnSetCalibration.Click += BtnSetCalibration_Click;
            textList = new List<ClearnEditText>();
            CalibrationText1 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText1);
            textList.Add(CalibrationText1);
            CalibrationText2 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText2);
            textList.Add(CalibrationText2);
            CalibrationText3 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText3);
            textList.Add(CalibrationText3);
            CalibrationText4 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText4);
            textList.Add(CalibrationText4);
            CalibrationText5 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText5);
            textList.Add(CalibrationText5);
            CalibrationText6 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText6);
            textList.Add(CalibrationText6);
            CalibrationText7 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText7);
            textList.Add(CalibrationText7);
            CalibrationText8 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText8);
            textList.Add(CalibrationText8);
            CalibrationText9 = v.FindViewById<ClearnEditText>(Resource.Id.CalibrationText9);
            textList.Add(CalibrationText9);
            return v;
        }
        /// <summary>
        /// 设置校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSetCalibration_Click(object sender, EventArgs e)
        {
            if (App.exepcService.isConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接WIFI", ToastLength.Short).Show();
                return;
            }
            List<float> list = new List<float>();
            foreach (var item in textList)
            {
                if (item.Text.Trim() != "")
                {
                    list.Add(float.Parse(item.Text.Trim()));
                }
            }
            App.exepcService.SetCalibration(list);
            textList.ForEach(item => item.Text = "");
            for (int i = 0; i < list.Count; i++)
            {
                textList[i].Text = list[i].ToString("F0");
            }
        }
        /// <summary>
        /// 读取校准
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReadCalibration_Click(object sender, EventArgs e)
        {
            if (App.exepcService.isConnected == false)
            {
                Toast.MakeText(base.Activity, "请先连接WIFI", ToastLength.Short).Show();
                return;
            }
            App.exepcService.ReadCalibration();
        }
    }
}