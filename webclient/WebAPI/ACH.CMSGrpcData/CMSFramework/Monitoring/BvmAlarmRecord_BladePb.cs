using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class BvmAlarmRecord_BladePb
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }
        /// <summary>
        /// 报警时间
        /// </summary>
        [DataMember]
        public DateTime AlarmTime
        {
            get;
            set;
        }
        /// <summary>
        /// 报警等级
        /// </summary>
        [DataMember]
        public EnumAlarmDegree AlarmDegree
        {
            get;
            set;
        }
    }
}
