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
    /// <summary>
    /// 检测记录导出Excel模板
    /// </summary>
    public class RecordExcel
    {
        public string 密封点编码 { get; set; }
        public string 群组条形或二维码 { get; set; }
        public string 群组编码 { get; set; }
        public string 扩展编码 { get; set; }
        public string 环境本底值 { get; set; }
        public string 净检测值 { get; set; }
        public string 泄漏检测值 { get; set; }
        public string 开始检测时间 { get; set; }
        public string 结束检测时间 { get; set; }
        public string 停留时间 { get; set; }
        public string 温度 { get; set; }
        public string 风向 { get; set; }
        public string 风速 { get; set; }
        public string 检测人名称 { get; set; }
        public string 检测时间 { get; set; }
        public string 检测设备号 { get; set; }
        public string 检测设备名称 { get; set; }
    }
}