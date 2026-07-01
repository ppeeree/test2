using ACH.DataEntity.Common;
using ACH.DataEntity.Enum;
using SqlSugar;
using System;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 风场/机组的部件绑定测点分布圆配置表
    /// </summary>
    [SugarTable("MeaslocCircleConfigMapper")]
    public class MeaslocCircleConfigMapper
    {
        /// <summary>
        /// 风场ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string StationID { get; set; }

        /// <summary>
        /// 机组ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string DeviceID { get; set; }

        /// <summary>
        /// 部件类型
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public EnumCircleType CircleType { get; set; }

        /// <summary>
        /// 配置ID
        /// </summary>
        public string ConfigID { get; set; }
    }
}
