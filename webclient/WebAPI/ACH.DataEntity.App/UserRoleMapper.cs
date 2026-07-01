using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 用户角色和用户映射关系表
    /// </summary>
    [SugarTable("UserRoleMapper")]
    public class UserRoleMapper
    {
        /// <summary>
        /// 主键：角色ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string RoleID { get; set; }


        /// <summary>
        /// 主键：用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string UserID { get; set; }
    }
}
