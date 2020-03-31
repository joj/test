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
using Java.IO;
using SQLite;

namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 校准记录
    /// </summary>
    [Serializable]
    public class AndroidCalibration
    {
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 装置名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 装置编码
        /// </summary>
        public string DeviceCode { get; set; }
        /// <summary>
        /// 录入时间
        /// </summary>
        public DateTime InsertTime { get; set; }
        /// <summary>
        /// 检测仪器名称
        /// </summary>
        public string PhxName { get; set; }
        /// <summary>
        /// 检测仪器型号
        /// </summary>
        public string PhxType { get; set; }
        /// <summary>
        /// 检测器类型
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 仪器序列号
        /// </summary>
        public string PhxCode { get; set; }
        /// <summary>
        /// 标准气体名称
        /// </summary>
        public string GasName { get; set; }
        /// <summary>
        /// 标准气类别
        /// </summary>
        public string GasType { get; set; }
        /// <summary>
        /// 校准人员姓名
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// 确认人员姓名
        /// </summary>
        public string Confirm { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 校准记录明细
        /// </summary>
        [IgnoreAttribute]
        public List<AndroidCalibrationLog> LogList { get; set; }
    }
}