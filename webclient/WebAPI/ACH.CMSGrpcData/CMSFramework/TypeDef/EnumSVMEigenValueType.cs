using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity.SVM
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:Maxy
    // create:2013-05-22
    /// <summary>
    /// 晃度仪特征值类型
    /// </summary>
    [DataContract(Name = "EnumSVMEigenvalueType")]
    public enum EnumSVMEigenValueType
    {
        /// <summary>
        ///最大值
        /// </summary>
        [EnumMember]
        [Description("最大值")]
        MaxValue = 0,

        /// <summary>
        /// 最小值
        /// </summary>
        [EnumMember]
        [Description("最小值")]
        MinValue = 1,

        /// <summary>
        /// 平均值 == > 倾斜角度（只有角度特征值用到，算术平均值)
        /// </summary>
        [EnumMember]
        [Description("平均值")]
        AvgValue = 2,

        // start added by GuoKaile time 2013-07-03
        /// <summary>
        /// 固有频率
        /// </summary>
        [EnumMember]
        [Description("固有频率")]
        NaturalFrequency = 3,

        /// <summary>
        /// 幅值
        /// </summary>
        [EnumMember]
        [Description("固有振动")]
        NFAmplitude = 4,
        // end added by GuoKaile time 2013-07-03


        // add added by steel@2013-07-16
        /// <summary>
        /// 倾角特征值
        /// </summary>
        [EnumMember]
        [Description("摆动角")]
        SwingAngle = 5,

        // start added by steel@2013-07-16
        /// <summary>
        /// 加速度特征值
        /// </summary>
        [EnumMember]
        [Description("有效值")]
        ACCRMS = 6,

        /// <summary>
        /// 弯曲角
        /// </summary>
        [EnumMember]
        [Description("弯曲角")]
        BendingAngle = 7,

        /// <summary>
        /// 横向加速度
        /// </summary>
        [EnumMember]
        [Description("横向加速度 RMS")]
        LateralAcceleration = 8,

        /// <summary>
        /// 横向加速度
        /// </summary>
        [EnumMember]
        [Description("轴向加速度 RMS")]
        AxialAcceleration = 9,

        /// <summary>
        /// 横向加速度
        /// </summary>
        [EnumMember]
        [Description("垂直加速度 RMS")]
        VerticalAcceleration = 10,


        [EnumMember]
        [Description("塔筒位移偏移平均值")]//top displacement bias Avg
        TDB_Avg = 11,

        [EnumMember]
        [Description("塔筒位移偏移最大值")]//top displacement bias Max
        TDB_Max = 12,

        [EnumMember]
        [Description("塔顶倾角偏移")]//top lean bias
        TLB = 13,

        /* added by liyc@2022-2-11 增加14到18枚举项 */
        [EnumMember]
        [Description("峰值")]
        PK = 14,

        [EnumMember]
        [Description("峰峰值")]
        PPK = 15,

        [EnumMember]
        [Description("平均幅值")]
        AbsAvgValue = 16,

        [EnumMember]
        [Description("频谱峰值")]
        FreSpectrumPeak = 17,

        [EnumMember]
        [Description("频谱峰值频率")]
        FreSpectrumPeakFrequency = 18,

    }
}
