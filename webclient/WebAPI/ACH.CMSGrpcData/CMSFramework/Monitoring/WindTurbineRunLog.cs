using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class WindTurbineRunLog
    {
        //机组ID
        public string WindTurbineID { get; set; }
        public DateTime EventTime { get; set; }
        public EnumAlarmDegree AlarmDegree { get; set; }
        public string LogTitle { get; set; }
    }
}
