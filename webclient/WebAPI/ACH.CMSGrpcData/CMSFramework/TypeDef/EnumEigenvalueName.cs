using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 特征值名称(DAU计算使用)
    /// </summary>
    /// author zhanggw
    /// create 2016-10-26
    [DataContract(Name = "EnumEigenvalueName")]
    public enum EnumEigenvalueName
    {
        [EnumMember]
        [Description("直流分量")]
        Enum_DC = 1,
        [EnumMember]
        [Description("有效值")]
        Enum_RMS = 2,
        [EnumMember]
        [Description("峰值")]
        Enum_PK = 3,
        [EnumMember]
        [Description("峰峰值")]
        Enum_PPK = 4,
        [EnumMember]
        [Description("峭度指标")]
        Enum_KTS = 5,
        [EnumMember]
        [Description("峰值指标")]
        Enum_CF = 6,
        [EnumMember]
        [Description("偏独指标")]
        Enum_SK = 7,
        [EnumMember]
        [Description("脉冲指标")]
        Enum_EIF = 8,
        [EnumMember]
        [Description("裕度指标")]
        Enum_LF = 9,
        [EnumMember]
        [Description("波形指标")]
        Enum_ESF = 10,
        [EnumMember]
        [Description("包络最大值")]
        Enum_gPKm = 11,
        [EnumMember]
        [Description("包络最小值")]
        Enum_gPKc = 12,
        [EnumMember]
        [Description("包络平均值")]
        Enum_gPKmean = 13,
        [EnumMember]
        [Description("频带特征值(未实现)")]
        Enum_FBE = 14,
        [EnumMember]
        [Description("窄带特征值(未实现)")]
        Enum_NBE = 15,
        [EnumMember]
        [Description("转速平均值(未实现)")]
        Enum_Spdmean = 16,
    }
}
