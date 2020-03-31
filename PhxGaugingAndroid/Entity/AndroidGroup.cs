using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 群组
    /// </summary>
    public class AndroidGroup
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 工单ID
        /// </summary>
        public string WorkOrderID { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 群组编码
        /// </summary>
        public string GroupCode { get; set; }
        /// <summary>
        /// 群组位置描述
        /// </summary>
        public string GroupDescribe { get; set; }
        /// <summary>
        /// 群组条形或二维码
        /// </summary>
        public string BarCode { get; set; }
        /// <summary>
        /// 密封点总数
        /// </summary>
        public int SealPointCount { get; set; }
        /// <summary>
        /// 图片打点数
        /// </summary>
        public int? DrawPointCount { get; set; }
        /// <summary>
        /// 不可达点数
        /// </summary>
        public int UnReachCount { get; set; }
        /// <summary>
        /// 已检密封点
        /// </summary>
        public int? CompleteCount { get; set; }
        /// <summary>
        /// 是否完成 0 未完成 1完成
        /// </summary>
        public int IsComplete { get; set; }
        /// <summary>
        /// 装置名称
        /// </summary>
        public string DeviceName { get; set; }
        /// <summary>
        /// 装置编码
        /// </summary>
        public string DeviceCode { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 图片路径
        /// </summary>
        [IgnoreAttribute]
        public string ImgPath { get; set; }
        /// <summary>
        /// Sqlite存储路径
        /// </summary>
        [IgnoreAttribute]
        public string DataPath { get; set; }
    }
}
