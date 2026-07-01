using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 晃度仪参数类型
    /// </summary>
    [DataContract(Name = "EnumSVMParamType")]
    public enum EnumSVMParamType
    {
        /// <summary>
        /// 俯仰角
        /// </summary>
        [EnumMember]
        [Description("俯仰角")]
        Pitch = 0,

        /// <summary>
        /// 横滚角
        /// </summary>
        [EnumMember]
        [Description("横滚角")]
        Roll = 1,

        /// <summary>
        /// 垂直加速度
        /// </summary>
        [EnumMember]
        [Description("垂直加速度")]
        Vertical = 2,

        /// <summary>
        /// 水平加速度
        /// </summary>
        [EnumMember]
        [Description("水平加速度")]
        Horizontal = 3,

        /// <summary>
        /// 轴向加速度
        /// </summary>
        [EnumMember]
        [Description("轴向加速度")]
        Axisl = 4,

        /// <summary>
        /// 温度
        /// </summary>
        [EnumMember]
        [Description("SVM温度")]
        Temperature = 5,

        /// <summary>
        ///X轴倾角
        /// </summary>
        [EnumMember]
        [Description("X轴倾角")]
        XInclination = 6,

        /// <summary>
        ///Y轴倾角
        /// </summary>
        [EnumMember]
        [Description("Y轴倾角")]
        YInclination = 7,

        /// <summary>
        ///X轴加速度
        /// </summary>
        [EnumMember]
        [Description("X轴加速度")]
        XAcc = 8,

        /// <summary>
        ///Y轴加速度
        /// </summary>
        [EnumMember]
        [Description("Y轴加速度")]
        YAcc = 9,

        /// <summary>
        ///Z轴加速度
        /// </summary>
        [EnumMember]
        [Description("Z轴加速度")]
        ZAcc = 10,
    }
}
