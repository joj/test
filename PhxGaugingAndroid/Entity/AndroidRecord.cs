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
    /// 检测记录
    /// </summary>
    public class AndroidRecord
    {
        /// <summary>
        /// 检测记录ID
        /// </summary>
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 群组编号
        /// </summary>
        public string GroupCode { get; set; }
        /// <summary>
        /// 群组条形码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 密封点编号
        /// </summary>
        public string SealPointSeq { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 停留时间
        /// </summary>
        public int? WasteTime { get; set; }
        /// <summary>
        /// 背景值
        /// </summary>
        public double? BackgroundPPM { get; set; }
        /// <summary>
        /// 泄漏检测值
        /// </summary>
        public double? LeakagePPM { get; set; }
        /// <summary>
        /// 净检测值
        /// </summary>
        public double PPM { get; set; }
        /// <summary>
        /// PID环境本底值
        /// </summary>
        public double? PIDBackgroundPPM { get; set; }
        /// <summary>
        /// PID泄漏检测值
        /// </summary>
        public double? PIDLeakagePPM { get; set; }
        /// <summary>
        /// PID净检测值
        /// </summary>
        public double? PIDPPM { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public string Temperature { get; set; }
        /// <summary>
        /// 风向
        /// </summary>
        public string WindDirection { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public string WindSpeed { get; set; }
        /// <summary>
        /// 检测人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 检测设备号
        /// </summary>
        public string PhxCode { get; set; }
        /// <summary>
        /// 检测设备名称
        /// </summary>
        public string PhxName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
        /// <summary>
        /// 组件检测方式
        /// </summary>
        public string DetectionMode { get; set; }
        /// <summary>
        /// 公称直径
        /// </summary>
        public string Diameter { get; set; }
    }
}