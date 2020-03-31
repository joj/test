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
    /// 系统参数
    /// </summary>
    public class AndroidSysParams
    {
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 代码
        /// </summary>
        public string ParameterCode { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string ParameterName { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string ParameterType { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public int ParameterLength { get; set; }
        /// <summary>
        /// 参数值
        /// </summary>
        public string ParameterValue { get; set; }
        /// <summary>
        /// 可选值
        /// </summary>
        public string ParameterChoose { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string ParameterRemark { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int ParameterSort { get; set; }
        /// <summary>
        /// 是否系统定义
        /// </summary>
        public int IsSystemDefine { get; set; }
        /// <summary>
        /// 参数格式
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// 类别名称
        /// </summary>
        public string CategoryName { get; set; }
        /// <summary>
        /// 是否可以更改
        /// </summary>
        public int IsCanChange { get; set; }
        /// <summary>
        /// 是否有效
        /// </summary>
        public int Enabled { get; set; }
        /// <summary>
        /// 开始值
        /// </summary>
        public int BeginValue { get; set; }
        /// <summary>
        /// 结束值
        /// </summary>
        public int EndValue { get; set; }

    }
}