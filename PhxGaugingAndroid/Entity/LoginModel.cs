using System;
using System.Collections.Generic;

namespace PhxGaugingAndroid.Entity
{
    public class LoginModel : BaseResult
    {
        #region 公共属性
        /// <summary>
        /// 登陆名称
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string UserPass { get; set; }
        ///<summary>
        /// 用户类型   0 App   1PC
        ///</summary>
        public int? UserType { get; set; }
        ///<summary>
        /// AppPC用户Id
        ///</summary>
        public string AppPCUserId { get; set; }
        ///<summary>
        /// 用户Id
        ///</summary>

        public string UserId { get; set; }
        ///<summary>
        /// 公司Id
        ///</summary>
        public string CompanyId { get; set; }
        ///<summary>
        /// 公司名称
        ///</summary>
        public string CompanyName { get; set; }
        ///<summary>
        /// 用户名称
        ///</summary>
        public string UserName { get; set; }
        ///<summary>
        /// 优盘序号
        ///</summary>
        public string SerialNumber { get; set; }
        ///<summary>
        /// 优盘安全码
        ///</summary>
        public string SecretKey { get; set; }
        ///<summary>
        /// 优盘随机号码
        ///</summary>
        public string IkeyAccountNo { get; set; }
        ///<summary>
        /// IVRpin
        ///</summary>
        public string IVRpin { get; set; }
        ///<summary>
        /// 优盘状态
        ///</summary>
        public string IkeyStatus { get; set; }
        ///<summary>
        /// 优盘类型
        ///</summary>
        public string IkeyType { get; set; }
        ///<summary>
        /// 验证码
        ///</summary>

        public string VerificationCode { get; set; }
        ///<summary>
        /// 开始时间
        ///</summary>
        public DateTime? BeginDate { get; set; }
        ///<summary>
        /// 结束时间
        ///</summary>
        public DateTime? EndDate { get; set; }
        ///<summary>
        /// 客户端名称
        ///</summary>
        public string ClientName { get; set; }
        ///<summary>
        /// Mak网卡地址
        ///</summary>
        public string MakCardAddress { get; set; }
        ///<summary>
        /// 备注
        ///</summary>
        public string Remark { get; set; }
        ///<summary>
        /// 是否有效  0 无效  1 有效
        ///</summary>
        public int? Enabled { get; set; }
        ///<summary>
        /// 录入人员
        ///</summary>
        public string InUser { get; set; }
        ///<summary>
        /// 录入日期
        ///</summary>
        public DateTime? InDate { get; set; }
        ///<summary>
        /// 修改人员
        ///</summary>
        public string EditUser { get; set; }
        ///<summary>
        /// 修改日期
        ///</summary>
        public DateTime? EditDate { get; set; }
        ///<summary>
        /// 删除人员
        ///</summary>
        public string DelUser { get; set; }
        ///<summary>
        /// 标识该行数据是否被删除 0未删除 1已删除
        ///</summary>
        public int? DelState { get; set; }
        ///<summary>
        /// 删除时间
        ///</summary>
        public DateTime? DelDate { get; set; }
        /// <summary>
        /// 服务器时间
        /// </summary>
        public long Datetickets { get; set; }
        /// <summary>
        /// 是否启用批量检测 0不启用 1启用
        /// </summary>
        public int IsBatchTest { get; set; }
        /// <summary>
        /// 可访问的API地址列表
        /// </summary>
        public List<AndroidServerUrl> ApiList { get; set; }
        #endregion
    }
}