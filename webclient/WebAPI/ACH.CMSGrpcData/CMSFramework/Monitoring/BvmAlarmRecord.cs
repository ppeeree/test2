using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class BvmAlarmRecord
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
        /// 表面损伤记录List
        /// </summary>
        [DataMember]
        public List<BvmAlarmRecord_Surface> AlarmRecord_SurfaceList 
        { 
            get;
            set;
        }

        /// <summary>
        /// 结构损伤记录List
        /// </summary>
        [DataMember]
        public List<BvmAlarmRecord_Structure> AlarmRecord_StructureList
        {
            get;
            set;
        }

        /// <summary>
        /// 结冰记录List
        /// </summary>
        [DataMember]
        public List<BvmAlarmRecord_Ice> AlarmRecord_IceList
        {
            get;
            set;
        }

        /// <summary>
        /// 桨距角偏差记录
        /// </summary>
        [DataMember]
        public BvmAlarmRecord_BladePb AlarmRecord_BladePb
        {
            get;
            set;
        }
    }
}
