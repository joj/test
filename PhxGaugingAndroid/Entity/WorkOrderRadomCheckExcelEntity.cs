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
    /// 抽检工单
    /// </summary>
    public class WorkOrderRadomCheckExcelEntity
    {
        /// <summary>
        /// 密封点编码
        /// </summary>
        public string 密封点编码 { get; set; }
        /// <summary>
        /// 装置编码
        /// </summary>
        public string 装置编码 { get; set; }
        /// <summary>
        /// 装置名称
        /// </summary>
        public string 装置名称 { get; set; }
        /// <summary>
        /// 区域名称
        /// </summary>
        public string 区域名称 { get; set; }
        /// <summary>
        /// 群组条形或二维码
        /// </summary>
        public string 群组条形或二维码 { get; set; }
        /// <summary>
        /// 群组编码
        /// </summary>
        public string 群组编码 { get; set; }
        /// <summary>
        /// 群组位置描述
        /// </summary>
        public string 群组位置描述 { get; set; }
        /// <summary>
        /// 扩展编码
        /// </summary>
        public string 扩展编码 { get; set; }
        
        /// <summary>
        /// 密封点类型
        /// </summary>
        public string 密封点类型 { get; set; }
        /// <summary>
        /// 是否可达(检测)
        /// </summary>
        public string 是否可达 { get; set; }
        /// <summary>
        /// 环境本底值 
        /// </summary>
        public double? 环境本底值 { get; set; }

        /// <summary>
        /// 净检测值
        /// </summary>
        public double? 净检测值 { get; set; }
        /// <summary>
        /// 泄漏检测值
        /// </summary>
        public double? 泄漏检测值 { get; set; }
        /// <summary>
        /// 红牌编号
        /// </summary>
        public string 红牌编号 { get; set; }
        /// <summary>
        /// 泄漏描述(检测)
        /// </summary>
        public string 泄漏描述 { get; set; }
        /// <summary>
        /// 开始检测时间
        /// </summary>
        //public DateTime? 开始检测时间 { get; set; }
        /// <summary>
        /// 结束检测时间
        /// </summary>
        //public DateTime? 结束检测时间 { get; set; }

        /// <summary>
        /// 温度
        /// </summary>
        public string 温度 { get; set; }
        /// <summary>
        /// 风向
        /// </summary>
        public string 风向 { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public string 风速 { get; set; }
        /// <summary>
        /// 停留时间
        /// </summary>
        public int? 停留时间 { get; set; }
   
        /// <summary>
        /// 检测人名称(检测)
        /// </summary>
        public string 检测人名称 { get; set; }
        /// <summary>
        /// 审核人
        /// </summary>
        public string 审核人 { get; set; }
        /// <summary>
        /// 检测时间(检测)
        /// </summary>
        public DateTime? 检测时间 { get; set; }
        /// <summary>
        /// 安防措施
        /// </summary>
        public string 安防措施 { get; set; }
        /// <summary>
        /// 组件检测方式
        /// </summary>
        public string 组件检测方式 { get; set; }
        /// <summary>
        /// 检测设备号
        /// </summary>
        public string 检测设备号 { get; set; }
        /// <summary>
        /// 检测设备名称
        /// </summary>
        public string 检测设备名称 { get; set; }
  
    }
}