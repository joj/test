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
    /// 校准明细
    /// </summary>
    public class AndroidCalibrationLog
    {
        [PrimaryKey]
        public string ID { get; set; }
        /// <summary>
        /// 校准记录ID
        /// </summary>
        public string CalibrationID { get; set; }
        /// <summary>
        /// 理论浓度
        /// </summary>
        public double TheoryValue { get; set; }
        /// <summary>
        /// 实际校准读数
        /// </summary>
        public double RealityValue { get; set; }
        /// <summary>
        /// 相对偏差
        /// </summary>
        public double Deviation { get; set; }
        /// <summary>
        /// 仪器校准时间
        /// </summary>
        public DateTime LogTime { get; set; }
        /// <summary>
        /// 平均反应时间
        /// </summary>
        public double ReactionTime { get; set; }
        /// <summary>
        /// 是否通过(0不通过1通过)
        /// </summary>
        public int IsPass { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}