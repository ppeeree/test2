using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CMSFramework.BusinessEntity
{
    [DataContract]
    public class Advice
    {
        /// <summary>
        /// 诊断建议的GUID
        /// </summary>
        [DataMember]
        public string Guid { get; set; }
        [DataMember]
        public string WindTurbineID { get; set; }
        [DataMember]
        public DateTime ReportDate { get; set; }
       
        [DataMember]
        public string ComponentType { get; set; }
        [DataMember]
        public string HandleAdvice { get; set; }
        [DataMember]
        public int? OrderSeq { get; set; }

    }
}
