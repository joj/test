using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PhxGaugingAndroid.Entity
{
    public class BackGroudExcel
    {
        public string BackGroudID;
        public string 装置名称 { get; set; }
        public string 装置编码 { get; set; }
        public string 创建时间 { get; set; }
        public string 平均值 { get; set; }
        public string 温度 { get; set; }
        public string 湿度 { get; set; }
        public string 大气压 { get; set; }
        public string 风向 { get; set; }
        public string 风速 { get; set; }
        public string 检测人 { get; set; }
        public string 审核人 { get; set; }
        public string 检测仪器名称 { get; set; }
        public string 检测仪器序列号 { get; set; }
        public string 备注 { get; set; }
        public string 检测时间 { get; set; }
        public string 检测用时 { get; set; }
        public string 检测值 { get; set; }
        public string 检测位置 { get; set; }        
    }
}