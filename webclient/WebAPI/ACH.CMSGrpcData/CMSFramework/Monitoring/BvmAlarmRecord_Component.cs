using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public abstract class BvmAlarmRecord_Component
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
        /// 部件ID
        /// </summary>
        [DataMember]
        public string ComponentID
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


    public class BvmAlarmRecord_Surface : BvmAlarmRecord_Component
    {
 
    }


    public class BvmAlarmRecord_Structure : BvmAlarmRecord_Component
    {

    }

    public class BvmAlarmRecord_Ice : BvmAlarmRecord_Component
    {

    }
}
