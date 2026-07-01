using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 用户角色信息
    /// </summary>
    [SugarTable("RoleInfo")]
    public class RoleInfo
    {
        /// <summary>
        /// 主键：角色ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string RoleID { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 角色编码
        /// </summary>
        public string RoleCode { get; set; }

    }
}
