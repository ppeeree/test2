using CMSFramework.BusinessEntity.Diagnosis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{

    [DataContract]
    public class WTDiagnosisReport
    {
        public WTDiagnosisReport()
        {
            ConclusionList = new List<Conclusion>();
            AdviceList = new List<Advice>();
        }
        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID { get; set; }

        /// <summary>
        /// 诊断时间
        /// </summary>
        [DataMember]
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// 风场ID
        /// </summary>
        [DataMember]
        public string WindParkID { get; set; }

        [DataMember]
        public string DiagnosisUser { get; set; }

        /// <summary>
        /// 机组故障等级
        /// </summary>
        [DataMember]
        public EnumFaultLevelType WTFaultLevel { get; set; }

        /// <summary>
        /// 诊断报告状态
        /// </summary>
        public EnumDiagnoseReportState DiagnosisState { get; set; }

        /// <summary>
        /// 数据采集时间
        /// </summary>
        [DataMember]
        public DateTime AcquisitionTime { get; set; }

        /// <summary>
        /// 异常诊断结论
        /// </summary>
        [DataMember]
        public List<Conclusion> ConclusionList { get; set; }

        /// <summary>
        /// 异常处理意见
        /// </summary>
        [DataMember]
        public List<Advice> AdviceList { get; set; }

        /// <summary>
        /// 文档路径
        /// </summary>
        [DataMember]
        public string DocumentPath { get; set; }

        /// <summary>
        /// 报告上传时间
        /// </summary>
        public DateTime UpLoadDate { get; set; }

    }
}
