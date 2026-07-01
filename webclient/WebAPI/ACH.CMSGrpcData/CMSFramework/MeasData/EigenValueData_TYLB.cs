using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class EigenValueData_TYLB
    {
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }

        [DataMember]
        public string Month//存储格式为 year_month  eg: 2019_12
        {
            get;
            set;
        }

        /// <summary>
        /// 偏航角度  整型 范围 -720~720
        /// </summary>
        [DataMember]
        public int YawAngle
        {
            get;
            set;
        }

        [DataMember]
        public double EigenValue
        {
            get;
            set;
        }

        [DataMember]
        public DateTime AcquisitionTime
        {
            get;
            set;
        }
    }
}
