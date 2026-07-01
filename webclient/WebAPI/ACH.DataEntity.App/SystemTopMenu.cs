using SqlSugar;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 顶部菜单
    /// </summary>
    [SugarTable("SystemTopMenu")]
    public class SystemTopMenu
    {
        /// <summary>
        /// RoleID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int ID { get; set; }

        /// <summary>
        /// 按钮名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 按钮code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 按钮图标
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

    }
}
