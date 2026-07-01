
namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    [Serializable]
    public class DefaultRuningAdviceDTO
    {
        /// <summary>
        /// 预警等级
        /// </summary>
        public string WarningLevel { get; set; }

        /// <summary>
        /// 运行建议
        /// </summary>
        public string RuningAdvice { get; set; }
    }

}
