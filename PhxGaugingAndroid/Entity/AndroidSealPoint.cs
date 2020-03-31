using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 密封点
    /// </summary>
    public class AndroidSealPoint
    {
        /// <summary>
        /// ID
        /// </summary>
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 工单ID
        /// </summary>
        public string WorkOrderID { get; set; }
        /// <summary>
        /// 群组ID
        /// </summary>
        public string GroupID { get; set; }
        /// <summary>
        /// 密封点编号
        /// </summary>
        public string SealPointCode { get; set; }
        /// <summary>
        /// 扩展号
        /// </summary>
        public string ExtCode { get; set; }
        /// <summary>
        /// 密封点类型
        /// </summary>
        public string SealPointType { get; set; }
        /// <summary>
        /// 是否可达
        /// </summary>
        public string IsTouch { get; set; }
        /// <summary>
        /// 原净检测值
        /// </summary>
        public double? LastNetPPM { get; set; }
        /// <summary>
        /// 环境本底值
        /// </summary>
        public double? BackgroundPPM { get; set; }
        /// <summary>
        /// 泄漏检测值
        /// </summary>
        public double? LeakagePPM { get; set; }
        /// <summary>
        /// 净检测值
        /// </summary>
        public double? PPM { get; set; }

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
        /// 红牌号码
        /// </summary>
        public string RedCode { get; set; }
        /// <summary>
        /// 泄漏描述
        /// </summary>
        public string LeakageDescribe { get; set; }
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
        /// 检测人名称
        /// </summary>
        public string UserName { get; set; }
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
        /// 检测时间
        /// </summary>
        public DateTime? DetectionTime { get; set; }
        /// <summary>
        /// 安防措施
        /// </summary>
        public string Security { get; set; }
        /// <summary>
        /// 检测设备号
        /// </summary>
        public string PhxCode { get; set; }
        /// <summary>
        /// 检测设备名称
        /// </summary>
        public string PhxName { get; set; }
        /// <summary>
        /// 组件检测方式
        /// </summary>
        public string DetectionMode { get; set; }
        /// <summary>
        /// 扩展字段
        /// </summary>
        public string Ext { get; set; }
        /// <summary>
        /// 公称直径
        /// </summary>
        public string Diameter { get; set; }
    }
}
