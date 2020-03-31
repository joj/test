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
using PhxGaugingAndroid.Common;

namespace PhxGaugingAndroid.Entity
{
    public class HashCode
    {
        /// <summary>
        /// SHA1码
        /// </summary>
        public string SHA1Code;
        /// <summary>
        /// MD5码
        /// </summary>
        public string MD5Code;
        /// <summary>
        /// 业务类型
        /// </summary>
        public EnumDataType BizId;
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName;
        /// <summary>
        /// 检测人
        /// </summary>
        public string InUserName;
    }
}