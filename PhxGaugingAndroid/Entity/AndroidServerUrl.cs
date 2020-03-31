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
    public class AndroidServerUrl
    {
        ///<summary>
        /// id
        ///</summary>
        public string Id { get; set; }

        ///<summary>
        /// AppPC用户Id
        ///</summary>
        public string AppPCUserId { get; set; }

        ///<summary>
        /// 平台编号
        ///</summary>
        public string PlatformCode { get; set; }

        ///<summary>
        /// 平台名称
        ///</summary>
        public string PlatformName { get; set; }

        ///<summary>
        /// API地址
        ///</summary>
        public string ApiAddress { get; set; }

        ///<summary>
        /// API版本号
        ///</summary>
        public string ApiVersion { get; set; }
    }
}