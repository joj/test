using SQLite;
using System;

namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 工单
    /// </summary>
    public class AndroidWorkOrder
    {
        /// <summary>
        /// 工单ID
        /// </summary>
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 工单名称
        /// </summary>
        public string WorkOrderName { get; set; }
        /// <summary>
        /// 密封点总数
        /// </summary>
        public int SealPointCount { get; set; }
        /// <summary>
        /// 不可达点数
        /// </summary>
        public int UnReachCount { get; set; }
        /// <summary>
        /// 已检密封点
        /// </summary>
        public int? CompleteCount { get; set; }
        /// <summary>
        /// 导入人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 导入时间
        /// </summary>
        public DateTime OperateTime { get; set; }
        /// <summary>
        /// 工单类型(0 检测 1 复检)
        /// </summary>
        public int WorkOrderType { get; set; }
        /// <summary>
        /// API地址
        /// </summary>
        public string ApiUrl { get; set; }
        /// <summary>
        /// API服务器编号
        /// </summary>
        public string ServerCode { get; set; }
        /// <summary>
        /// API服务器名称
        /// </summary>
        public string ServerName { get; set; }
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime? UploadTime { get; set; }
        /// <summary>
        /// 下载工单用户
        /// </summary>
        public string DownUserid { get; set; }
        /// <summary>
        /// Sqlite存储路径
        /// </summary>
        [IgnoreAttribute]
        public string DataPath { get; set; }
        /// <summary>
        /// 检测点数
        /// </summary>
        [IgnoreAttribute]
        public int DetectionCount { get; set; }
        /// <summary>
        /// 检测时间
        /// </summary>
        [IgnoreAttribute]
        public DateTime DetectionDate { get; set; }
    }
}
