using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 简单诊断报告DTO类，用于列表展示
    /// </summary>
    [Serializable]
    public class SimpleDiagnosisReportDTO
    {
        public string ReportGuid { get; set; }

        /// <summary>
        /// 机组ID - 机组信息由机组ID从其他表中带出
        /// </summary>
        public string WindturbineId { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        public string RuningAdvice { get; set; }

        /// <summary>
        /// 机组状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
    /// <summary>
    /// 详细诊断报告DTO类，用于详情展示
    /// </summary>
    [Serializable]
    public class DiagnosisReportDTO : SimpleDiagnosisReportDTO
    {
        /// <summary>
        /// 机组分析记录详情DTO列表
        /// </summary>
        public List<DiagnosisReportAnalyzerRecordDTO> AnalyzerRecords { get; set; }
        /// <summary>
        /// 此处返回结果按照部件名称分组
        /// </summary>
        public List<DiagnosisReportConclusionTreeDTO> Conclusions { get; set; }
        /// <summary>
        /// 机组信息DTO
        /// </summary>
        public DiagnosisWindTurbineDTO WindTurbine { get; set; }
    }
    /// <summary>
    /// 诊断报告分析记录详情DTO类
    /// </summary>
    [Serializable]
    public class DiagnosisReportAnalyzerRecordDTO : ISortable
    {
        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }

        /// <summary>
        /// 测量位置名称
        /// </summary>
        public string MeaslocId { get; set; }
        /// <summary>
        /// 测量位置名称
        /// </summary>
        public string MeaslocName { get; set; }

        /// <summary>
        /// 特征值类型
        /// </summary>
        public string EigenValueId { get; set; }

        /// <summary>
        /// 图谱类型
        /// </summary>
        public string ImageType { get; set; }
        /// <summary>
        /// 分析描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 机组分析记录主键ID
        /// </summary>
        public int AnalyzerRecordId { get; set; }
        /// <summary>
        /// 记录时间
        /// </summary>
        public string RecordTime { get; set; }
        /// <summary>
        /// 分析图谱
        /// </summary>
        public string Image { get; set; }

        public string GetSortableName() => MeaslocName ?? string.Empty;
    }
    /// <summary>
    /// 诊断报告结论详情DTO类
    /// </summary>
    [Serializable]
    public class DiagnosisReportConclusionDTO
    {
        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }

        /// <summary>
        /// 诊断结论
        /// </summary>
        public string DiagnosisConclusion { get; set; }

        /// <summary>
        /// 预警等级
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 维护建议
        /// </summary>
        public string MaintainAdvice { get; set; }
    }
    [Serializable]
    public class DiagnosisReportConclusionTreeDTO
    {

        public string Name { get; set; }

        public string Type
        {
            get
            {
                return "MeaslocName";
            }
        }
        /// <summary>
        /// 部件状态
        /// </summary>
        public string CompStatus { get; set; }

        public List<DiagnosisReportConclusionDTO> Children { get; set; }
    }


}
