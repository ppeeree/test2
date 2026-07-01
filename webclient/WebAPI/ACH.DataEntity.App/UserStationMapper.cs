using SqlSugar;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 用户和风场绑定类
    /// </summary>
    [SugarTable("UserStationMapper")]
    public class UserStationMapper
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string UserId { get; set; }

        /// <summary>
        /// 风场ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string StationID { get; set; }
    }
}
