using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 波形类型定义，取代原来的EnumMeasTypeDef
    /// 信号类型，波形类型，都在波形类型中有体现
    /// </summary>
    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2015-08-07
    /// <summary>
    /// 波形类型定义，取代原来的EnumMeasTypeDef
    /// </summary>
    [DataContract(Name = "WaveFormType")]
    public enum EnumWaveFormType
    {
        [EnumMember]
        [Description("Error")]
        WDF_Error = 0,

        [EnumMember]
        [Description("时域")]
        WDF_Time = 1,

        [EnumMember]
        [Description("阶次包络")]
        WDF_OrderEnvelope = 2,

        [EnumMember]
        [Description("阶次")]
        WDF_Order = 3,

        [EnumMember]
        [Description("包络")]
        WDF_Envelope = 4,

        // added by GuoKaile time 2013-05-07
        [EnumMember]
        [Description("晃度仪加速度")]
        WDF_SVM = 5,

        [EnumMember]
        [Description("晃度仪角度")]
        WDF_SVMAGL = 6,

        //[EnumMember]
        //[Description("转速波形")]
        //WDF_RotSpd = 7,

        //[EnumMember]
        //[Description("温度")]
        //WDF_TMS = 7,

        //[EnumMember]
        //[Description("电流")]
        //WDF_CUR = 8,

        //[EnumMember]
        //[Description("电压")]
        //WDF_VT = 9,
    }


    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2011-06-22
    /// <summary>
    /// 测量定义类型
    /// </summary>
    [Obsolete("Use the EnumWaveFormType.", true)]
    [DataContract(Name = "MeasDefType")]
    public enum EnumMeasDefType
    {
        [EnumMember]
        [Description("时域")]
        MDF_Time = 1,

        [EnumMember]
        [Description("阶次包络")]
        MDF_OrderEnvelope = 2,

        [EnumMember]
        [Description("阶次")]
        MDF_Order = 3,

        [EnumMember]
        [Description("包络")]
        MDF_Envelope = 4,

        // added by GuoKaile time 2013-05-07
        [EnumMember]
        [Description("晃度仪加速度")]
        MDF_SVMTime = 5


    }
}
