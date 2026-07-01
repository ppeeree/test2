using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_Turbine : DeviceRTAlarmStatus
    {
        public AlarmStatus_Turbine()
        {
            BladeAlarmStatusList = new List<AlarmStatus_BladeCom>();
            CompontAlarmStatusList = new List<AlarmStatus_Component>();
            MeasLocAlarmStatusList = new List<AlarmStatus_MeasLocVib>();
            SVMMeasLocAlarmStatusList = new List<AlarmStatus_MeasLocSVM>();
        }

        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbineID
        {
            get;
            set;
        }


        public override string DevSegmentID
        {
            get
            {
                return WindTurbineID;
            }
            set
            {
                WindTurbineID = value;
            }
        }

        
        /// <summary>
        /// 传动链 部件实时状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_Component> CompontAlarmStatusList
        {
            get;
            set;
        }


        [DataMember]
        public List<AlarmStatus_BladeCom> BladeAlarmStatusList
        {
            get;
            set;
        }

        /// <summary>
        /// 测量位置实时状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_MeasLocVib> MeasLocAlarmStatusList
        {
            get;
            set;
        }


        /// <summary>
        /// SVM测量位置报警状态
        /// </summary>
        [DataMember]
        public List<AlarmStatus_MeasLocSVM> SVMMeasLocAlarmStatusList
        {
            get;
            set;
        }

        /// <summary>
        /// 叶片报警记录
        /// </summary>
        [DataMember]
        public BvmAlarmRecord BvmAlarmRecord
        {
            get;
            set;
        }

    }
}
