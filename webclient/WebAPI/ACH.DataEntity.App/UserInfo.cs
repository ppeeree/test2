using SqlSugar;
namespace ACH.DataEntity.App
{
    /// <summary>
    /// 用户信息表
    /// </summary>
    [SugarTable("UserInfo")]
    public class UserInfo
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 用户登录名
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string Id { get; set; }

        /// <summary>
        /// 用户登录名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户登录密码
        /// </summary>
        public string Password { get; set; }
    }
}
