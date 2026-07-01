using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 测量事件
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [KnownType(typeof(MeasEvent_Alarm))]
    [KnownType(typeof(MeasEvent_Wave))]
    [KnownType(typeof(MeasEvent_EigenValue))]
    public abstract class MeasEvent
    {

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasDefinitionID 测量定义ID
        /// </summary>
        [DataMember]
        public string MeasDefinitionID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember]
        public DateTime AcquisitionTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WkConDataNum 工况数据个数
        /// </summary>
        [IgnoreDataMember]
        public int WkConDataNum
        {
            get;
            set;
        }

        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// OutPowerBandCode 工况级别
        /// </summary>
        [DataMember]
        public string OutPowerBandCode
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmDegree 报警等级
        /// </summary>
        [DataMember]
        public EnumAlarmDegree AlarmDegree
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // modified by GuoKaile time 2012-05-04
        /// <summary>
        /// WorkingConData 工况数据
        /// </summary>
        [DataMember]
        public List<WorkingConditionData> WorkingConDataList
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Data_Qual_Type 数据质量
        /// </summary>
        [DataMember]
        public EnumDataQualityType Data_Qual_Type
        {
            get;
            set;
        }
    }
}
