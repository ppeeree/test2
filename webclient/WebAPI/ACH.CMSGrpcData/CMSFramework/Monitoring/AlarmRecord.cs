using CMSFramework.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    // add by steel @ 2015-10-15
    /// <summary>
    /// 报警记录
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmRecord
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
        /// 测量位置ID
        /// </summary>
        [DataMember]
        public string MeasLocID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 特征值ID
        /// </summary>
        [DataMember]
        public string EigenValueID
        {
            get;
            set;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmTime 报警时间
        /// </summary>
        [DataMember]
        public DateTime AlarmTime
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
        /// 报警描述
        /// </summary>
        [DataMember]
        public string AlarmDescription
        {
            get;
            set;
        }


        /// <summary>
        /// 报警参考标准
        /// VDI3834
        /// ISO10816
        /// 自定义
        /// </summary>
        [DataMember]
        public string RefStandard
        {
            get;
            set;
        }
    }
}
