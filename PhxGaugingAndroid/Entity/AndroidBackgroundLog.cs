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
    /// 环境本底值明细
    /// </summary>
    [Serializable]
    public class AndroidBackgroundLog
    {
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 本底值ID
        /// </summary>
        public string BackgroundID { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 检测时间
        /// </summary>
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 检测用时(秒)
        /// </summary>
        public int WasteTime { get; set; }
        /// <summary>
        /// 检测值
        /// </summary>
        public double DetectionValue { get; set; }
        /// <summary>
        /// PID检测值
        /// </summary>
        public double PIDDetectionValue { get; set; }
        /// <summary>
        /// 检测位置
        /// </summary>
        public string Position { get; set; }        
    }
}