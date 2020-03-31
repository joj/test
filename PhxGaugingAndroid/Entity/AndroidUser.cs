namespace PhxGaugingAndroid.Entity
{
    /// <summary>
    /// 用户
    /// </summary>
    public class AndroidUser
    {
        public string ID { get; set; }
        /// <summary>
        /// 企业编码
        /// </summary>
        public string CompanyCode { get; set; }
        /// <summary>
        /// 登陆名称
        /// </summary>
        public string LoginName { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 最后一次在线登陆时间
        /// </summary>
        public long ServerLastLoginTime { get; set; }
        /// <summary>
        /// 登陆时间
        /// </summary>
        public long LoginTime { get; set; }

        public string Ext { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否启用批量检测 0不启用 1启用
        /// </summary>
        public int IsBatchTest { get; set; }
    }
}