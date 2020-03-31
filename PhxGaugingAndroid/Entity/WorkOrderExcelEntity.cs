using System;

namespace PhxGaugingAndroid.Entity
{
    public class WorkOrderExcelEntity
    {
        /// <summary>
        /// 密封点编码
        /// </summary>
        public string SealPointCode { get; set; }
        /// <summary>
        /// 装置编码
        /// </summary>
        public string DeviceCode { get; set; }
        /// <summary>
        /// 装置名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 群组条形或二维码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 群组编码
        /// </summary>
        public string GroupCode { get; set; }
        /// <summary>
        /// 扩展编码
        /// </summary>
        public string ExtCode { get; set; }
        /// <summary>
        /// 群组位置描述
        /// </summary>
        public string GroupDescribe { get; set; }
        /// <summary>
        /// 密封点类型
        /// </summary>
        public string SealPointType { get; set; }
        /// <summary>
        /// 环境本底值 
        /// </summary>
        public double? BackgroundPPM { get; set; }
        /// <summary>
        /// 净检测值
        /// </summary>
        public double? PPM { get; set; }
        /// <summary>
        /// 泄漏检测值
        /// </summary>
        public double? LeakagePPM { get; set; }
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
        /// 红牌编号
        /// </summary>
        public string RedCode { get; set; }
        /// <summary>
        /// 开始检测时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 结束检测时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 停留时间
        /// </summary>
        public int? WasteTime { get; set; }
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
        /// 检测人名称(检测)复检人员(复检)
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 检测时间(检测)复检时间(复检) 
        /// </summary>
        public DateTime? DetectionTime { get; set; }
        /// <summary>
        /// 原净检测值(复检)
        /// </summary>
        public double? LastNetPPM { get; set; }

        /// <summary>
        /// 是否可达(检测)
        /// </summary>
        public string IsTouch { get; set; }
        /// <summary>
        /// 泄漏描述(检测)
        /// </summary>
        public string LeakageDescribe { get; set; }
    }
}