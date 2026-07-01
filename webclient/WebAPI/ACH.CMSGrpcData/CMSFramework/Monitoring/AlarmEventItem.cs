using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-08
    /// <summary>
    /// 报警
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmEventItem
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string WindTurbineID
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
        /// 开始时间
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public DateTime BeginTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmDegree 报警等级
        /// </summary>
        [DataMember]
        public int AlarmDegree
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 结束时间
        /// </summary>
        [DataMember]
        public DateTime EndTime
        {
            get;
            set;
        }

    }
}
