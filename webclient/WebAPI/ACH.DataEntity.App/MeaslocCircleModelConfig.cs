using ACH.DataEntity.Enum;
using SqlSugar;
using System;

namespace ACH.DataEntity.App
{
    /// <summary>
    /// 测点分布圆配置表
    /// </summary>
    [SugarTable("MeaslocCircleModelConfig")]
    public class MeaslocCircleModelConfig
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public string ConfigID { get; set; }

        /// <summary>
        /// 该配置的名称
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 部件类型
        /// </summary>
        public EnumCircleType CircleType { get; set; }

        /// <summary>
        /// 截面，可说明该方案中有几个卡片
        /// </summary>
        public string Section { get; set; }

        /// <summary>
        /// 测点角度
        /// </summary>
        public double AngleDegree { get; set; }

        /// <summary>
        /// 是否展示雷达图
        /// </summary>
        public Boolean IsShowRadar { get; set; }

        /// <summary>
        /// 测点名称
        /// </summary>
        public string CircleMeaslocName { get; set; }


        /// <summary>
        /// 分布圆上的特针值阈值
        /// </summary>
        public double EvThreshold { get; set; }

        /// <summary>
        /// 测点code
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public string? MeaslocCode { get; set; }
    }
}
