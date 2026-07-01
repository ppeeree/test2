namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 保存分析器记录的DTO。
    /// </summary>
    [Serializable]
    public class SaveAnalyzerRecordDTO
    {
        public SaveAnalyzerRecordDTO()
        {
            DiagnosisConclusions = new List<SaveDiagnosisConclusionDTO>();
        }
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindturbineId { get; set; }
        /// <summary>
        /// 部件名称
        /// </summary>
        public string CompName { get; set; }

        /// <summary>
        /// 测量位置名称
        /// </summary>
        public string MeaslocId { get; set; }
        /// <summary>
        /// 特征值名称
        /// </summary>
        public string MeaslocName { get; set; }

        /// <summary>
        /// 特征值ID
        /// </summary>
        public string? EigenValueId { get; set; }

        /// <summary>
        /// 图谱类型
        /// </summary>
        public string ImageType { get; set; }

        /// <summary>
        /// 分析图谱,Base64编码字符串
        /// </summary>
        public string Image { get; set; }
        /// <summary>
        /// 波形时间
        /// </summary>
        public string? AcqTime { get; set; }

        /// <summary>
        /// 分析描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 诊断结论列表
        /// </summary>
        public List<SaveDiagnosisConclusionDTO> DiagnosisConclusions { get; set; }
    }
    /// <summary>
    /// 保存诊断结论的DTO。
    /// </summary>
    [Serializable]
    public class SaveDiagnosisConclusionDTO
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


}
