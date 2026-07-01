using SqlSugar;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 系统按钮
    /// </summary>
    [SugarTable("SystemMenu")]
    public class SystemMenu
    {
        /// <summary>
        /// RoleID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public long ID { get; set; }
        /// <summary>
        /// 父级ID
        /// </summary>
        public long ParentID { get; set; }

        /// <summary>
        /// 按钮名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 按钮code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// 页面路径
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string? Path { get; set; }

        /// <summary>
        /// 按钮图标
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string? Source { get; set; }

        /// <summary>
        /// 是否打开新页面
        /// </summary>
        public bool IsOpen { get; set; }

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
