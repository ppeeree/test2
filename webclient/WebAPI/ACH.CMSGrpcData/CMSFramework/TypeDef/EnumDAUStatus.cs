using System.Runtime.Serialization;
using System.ComponentModel;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumDAUStatus")]
    public enum EnumDAUStatus
    {
        /// <summary>
        /// 正常
        /// </summary>
        [EnumMember]
        [Description("正常")]
        Normal = 3,

        /// <summary>
        /// 未使用
        /// </summary>
        [EnumMember]
        [Description("未知")]
        Unknown = 2,

        /// <summary>
        /// 通讯异常
        /// </summary>
        [EnumMember]
        [Description("通讯异常")]
        CommunicationError = 4,

        /// <summary>
        /// 无数据到达
        /// </summary>
        [EnumMember]
        [Description("无数据到达")]
        NoDataArrive=5,

        /// <summary>
        /// 传感器故障
        /// </summary>
        [EnumMember]
        [Description("传感器故障")]
        SensorFault = 7,

        /// <summary>
        /// 转速异常
        /// </summary>
        [EnumMember]
        [Description("转速异常")]
        RotSpdFault = 8
    }
}
