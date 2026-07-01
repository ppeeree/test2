using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_MeasLocVib : DeviceRTAlarmStatus
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

        /// <summary>
        /// 振动测量位置ID
        /// </summary>
        public string MeasLocationID
        {
            get;
            set;
        }


        public override string DevSegmentID
        {
            get
            {
                return MeasLocationID;
            }
            set
            {
                MeasLocationID = value;
            }
        }

        public override string ParentSegmentID
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
    }
}
