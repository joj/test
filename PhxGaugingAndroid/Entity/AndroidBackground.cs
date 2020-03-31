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
using SQLite;

namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 环境本底值
    /// </summary>
    [Serializable]
    public class AndroidBackground
    {
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 装置名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 装置编码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 检测时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 平均值
        /// </summary>
        public double? AvgValue { get; set; }
        /// <summary>
        /// PID平均值
        /// </summary>
        public double? PIDAvgValue { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public double? Temperature { get; set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public double? Humidity { get; set; }
        /// <summary>
        /// 大气压
        /// </summary>
        public double? Atmos { get; set; }
        /// <summary>
        /// 风向
        /// </summary>
        public string WindDirection { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public double? WindSpeed { get; set; }
        /// <summary>
        /// 检测人
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 检测仪器名称
        /// </summary>
        public string PhxName { get; set; }
        /// <summary>
        /// 检测仪器序列号
        /// </summary>
        public string PhxCode { get; set; }
        /// <summary>
        /// 组件检测方式
        /// </summary>
        public string DetectionMode { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 背景值明细
        /// </summary>
        [IgnoreAttribute]
        public List<AndroidBackgroundLog> LogList { get; set; }
    }
}