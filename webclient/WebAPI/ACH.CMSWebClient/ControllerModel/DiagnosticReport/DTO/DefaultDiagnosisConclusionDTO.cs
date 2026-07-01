namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 默认诊断结论DTO
    /// </summary>
    [Serializable]
    public class DefaultDiagnosisConclusionDTO
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
        public string WarningLevel { get; set; }

        /// <summary>
        /// 维护建议
        /// </summary>
        public string MaintainAdvice { get; set; }
    }

}
