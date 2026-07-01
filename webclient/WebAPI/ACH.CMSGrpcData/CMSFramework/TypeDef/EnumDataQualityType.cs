using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Name = "EnumDataQualityType")]
    public enum EnumDataQualityType
    {
        /// <summary>
        /// 采集失败
        /// </summary>
        [EnumMember]
        NotAcquired = 1,

        /// <summary>
        /// 正常
        /// </summary>
        [EnumMember]
        Normal = 2,

        /// <summary>
        /// 传感器故障
        /// </summary>
        [EnumMember]
        SensorError = 3,

        // added by GuoKaile time 2012-04-18
        /// <summary>
        /// 处理完成
        /// </summary>
        [EnumMember]
        Finished = 4,

        // added by ZhangMai time 2012-05-31
        /// <summary>
        /// 已经使用
        /// </summary>
        [EnumMember]
        AlarmEvent = 5,

        
        /// <summary>
        /// 大的冲击，导致传感器波形异常
        /// </summary>
        [EnumMember]
        BigShock = 6,

    }
}
