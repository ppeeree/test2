using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:wangyan
    // create:2015年11月18日
    /// <summary>
    /// 报警阈值实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmDefThreshold
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 报警阈值组ID
        /// </summary>
        [DataMember]
        public string ThresholdGroup
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
        /// <summary>
        /// 阈值
        /// </summary>
        [DataMember]
        public double ThresholdValue
        {
            get;
            set;
        }
    }
}
