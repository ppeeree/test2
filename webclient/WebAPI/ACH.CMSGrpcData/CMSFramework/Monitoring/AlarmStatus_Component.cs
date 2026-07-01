using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_Component : DeviceRTAlarmStatus
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbineID
        {
            get;
            set;
        }

        /// <summary>
        /// 部件ID
        /// </summary>
        public string ComponentID
        {
            get;
            set;
        }

        public override string DevSegmentID
        {
            get
            {
                return ComponentID;
            }
            set
            {
                ComponentID = value;
            }
        }

        public override string ParentSegmentID
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
    }
}
