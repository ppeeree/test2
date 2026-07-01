using SqlSugar;

namespace ACH.DataEntity.DevTree
{
    /// <summary>
    /// 实体部件和聚合部件关系
    /// </summary>
    public class ComponentMapping
    {
        /// <summary>
        /// 聚合部件名称
        /// </summary>
        public string PageCompName { get; set; }
        /// <summary>
        /// 聚合部件类型
        /// </summary>
        public string PageCompType { get; set; }
        /// <summary>
        /// 实体部件名称
        /// </summary>
        public string CompName { get; set; }
        /// <summary>
        /// 实体部件类型
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string CompType { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ComponentMapping(string pageCompName, string pageCompType, string compType, string compName, int sort)
        {
            PageCompName = pageCompName;
            PageCompType = pageCompType;
            CompName = compName;
            CompType = compType;
            Sort = sort;
        }
    }
}
