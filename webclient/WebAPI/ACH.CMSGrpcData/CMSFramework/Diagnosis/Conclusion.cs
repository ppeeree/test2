using CMSFramework.BusinessEntity.Diagnosis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 诊断结论
    /// </summary>
    [DataContract]
    public class Conclusion
    {
        [DataMember]
        public string GUID { get; set; }
        [DataMember]
        public string WindTurbineID { get; set; }
        [DataMember]
        public DateTime ReportDate { get; set; }
        
        [DataMember]
        public string ComponentType { get; set; }

        [DataMember]
        public EnumFaultLevelType FaultLevelType { get; set; }

        [DataMember]
        public string ConclusionText { get; set; }

        [DataMember]
        public int? OrderSeq { get; set; }
    }
}
