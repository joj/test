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
    public class CalibrationExcel
    {
        public string CalibrationID;
        public string 装置名称 { get; set; }
        public string 装置编码 { get; set; }
        public string 检测仪器名称 { get; set; }
        public string 检测仪器型号 { get; set; }
        public string 仪器序列号 { get; set; }
        public string 校正人员姓名 { get; set; }
        public string 确认人员姓名 { get; set; }
        public string 审核人 { get; set; }
        public string 标准气体名称 { get; set; }
        public string 备注 { get; set; }
        public string 标准气体浓度 { get; set; }
        public string 校准读数 { get; set; }
        public string 仪器校正时间 { get; set; }
        public string 平均反应时间 { get; set; }
        public string 仪器漂移误差 { get; set; }
        
    }
}