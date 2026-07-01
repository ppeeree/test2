using SqlSugar;

namespace ACH.DataEntity.TheWeave
{
    /// <summary>
    /// 测量方案
    /// </summary>
    public class Measurement
    {
        /// <summary>
        /// 场站ID
        /// </summary>
        [SugarColumn(IsNullable = true)] public string StationID { get; set; }
        /// <summary>
        /// 场站名
        /// </summary>
        [SugarColumn(IsNullable = true)] public string StationName { get; set; }
        /// <summary>
        /// 设备 ID
        /// </summary>
        [SugarColumn(IsNullable = true)] public string DeviceID { get; set; }
        /// <summary>
        /// 设备名
        /// </summary>
        [SugarColumn(IsNullable = true)] public string DeviceName { get; set; }
        /// <summary>
        /// 部件ID
        /// </summary>
        [SugarColumn(IsNullable = true)] public string ComponentID { get; set; }
        /// <summary>
        /// 部件名
        /// </summary>
        [SugarColumn(IsNullable = true)] public string ComponentName { get; set; }
        /// <summary>
        /// 测点ID
        /// </summary>
        [SugarColumn(IsNullable = true)] public string MeasLoctionID { get; set; }
        /// <summary>
        /// 测点名
        /// </summary>
        [SugarColumn(IsNullable = true)] public string MeasLoctionName { get; set; }
        /// <summary>
        /// 测点名（简称-用于页面显示）
        /// </summary>
        [SugarColumn(IsNullable = true)] public string MeasLoctionNickName { get; set; }
        /// <summary>
        /// 方向
        /// </summary>
        [SugarColumn(IsNullable = true)] public string Orientation { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(IsNullable = true)] public string Remark { get; set; }
        /// <summary>
        /// 测点排序字段 内部排序
        /// </summary>
        [SugarColumn(IsIgnore = true)] public int sort { get; set; }
    }
}
