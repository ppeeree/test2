namespace ACH.Helper.Comparer
{
    /// <summary>
    /// 排序类型
    /// </summary>
    public enum EnumSortType
    {
        /// <summary>
        /// 按字母顺序排序（不使用自定义字典）
        /// </summary>
        Alphabetical,

        /// <summary>
        /// 按照风场名称枚举
        /// </summary>
        StationName,

        /// <summary>
        /// 按照聚合部件名称排序
        /// </summary>
        PageCompName,

        /// <summary>
        /// 按照实体部件名称排序
        /// </summary>
        CompName,

        /// <summary>
        /// 按照测点名称排序
        /// </summary>
        MeaslocName
    }
}
