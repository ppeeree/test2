using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:lidan
    // create:2011-10-07
    /// <summary>
    /// 采集通道实时状态实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class RTAlarmStatus_Channel
    {
        public RTAlarmStatus_Channel()
        {
            DauID = "1";//默认为1
        }

        /// <summary>
        /// DAU编号
        /// </summary>
        [DataMember]
        public string DauID
        {
            get;
            set;
        }


        /// <summary>
        /// 机组编号
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }


        [DataMember]
        public string MeasLocationID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ChannelNumber 采集通道编号
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public int ChannelNumber
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmState 报警状态
        /// </summary>
        [DataMember]
        public EnumICPSensorStatus AlarmState
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// StateUpdateTime 状态更新时间
        /// </summary>
        [DataMember]
        public DateTime StatusUpdateTime
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// DCDataValue 直流分量实时数据值
        /// </summary>
        [DataMember]
        public double DCDataValue
        {
            get;
            set;
        }

    }
}
