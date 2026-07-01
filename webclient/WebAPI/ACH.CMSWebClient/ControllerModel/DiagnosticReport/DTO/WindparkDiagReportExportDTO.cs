using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 风场诊断报告导出DTO
    /// </summary>
    public class WindparkDiagReportExportDTO
    {
        /// <summary>
        /// 风场ID
        /// </summary>
        public string WindparkId { get; set; }
        /// <summary>
        /// 风场名称
        /// </summary>
        public string WindparkName { get; set; }
        /// <summary>
        /// 报告日期 - 报告创建时间 (yyyy-MM-dd)
        /// </summary>
        public string ReportDate { get; set; }
        /// <summary>
        /// 报告月份 - 报告创建时间 (yyyy-MM)
        /// </summary>
        public string ReportMonth { get; set; }
        /// <summary>
        /// 报告名称
        /// </summary>
        public string ReportName { get; set; }
        /// <summary>
        /// 风场地址
        /// </summary>
        public string WindparkAddress { get; set; }
        /// <summary>
        /// 风机数量
        /// </summary>
        public string WindturbineCount { get; set; }
        /// <summary>
        /// 投运时间
        /// </summary>
        public string OperationlDate { get; set; }
        /// <summary>
        /// 风机型号
        /// </summary>
        public string WindturbineType { get; set; }
        /// <summary>
        /// 传动形式
        /// </summary>
        public string TransmissionForm { get; set; }
        /// <summary>
        /// 检测时间 拼接MonitoringTimeStart-MonitoringTimeEnd
        /// </summary>
        public string DetectionTime { get; set; }
        /// <summary>
        /// 项目概述
        /// </summary>
        public string ProjectOvewerview { get; set; }
        /// <summary>
        /// 危险风机内容 - 即风场诊断报告中的“需要更换部件的机组”部分的内容
        /// 格式为：机组名称+诊断结论+维护建议，且相同状态，诊断结论相同的机组进行合并显示
        /// 例如：“HR001、HR006、HR063齿轮箱高速级齿面早期损伤，观察齿面是否存在点蚀或压痕”
        /// </summary>
        public string DangerWindturbineContent { get; set; }
        /// <summary>
        /// 警告风机内容 - 即风场诊断报告中的“需要尽快检查（建议48小时内）的机组”部分的内容
        /// 格式为：机组名称+诊断结论+维护建议，且相同状态，诊断结论相同的机组进行合并显示
        /// </summary>
        public string WarningWindturbineContent { get; set; }
        /// <summary>
        /// 注意风机内容 - 即风场诊断报告中的“可继续运行，但需要进行维护的机组”部分的内容
        /// 格式为：机组名称+诊断结论+维护建议，且相同状态，诊断结论相同的机组进行合并显示
        /// </summary>
        public string AttentionWindturbineContent { get; set; }
        /// <summary>
        /// 健康状态列表 - 风电机组健康状态DTO集合
        /// </summary>
        public List<WindturbineHealthStatusExportDTO> WindturbineHealthStatusList { get; set; }
        /// <summary>
        /// 风电机组诊断结论列表 - 风电机组诊断结论DTO集合
        /// </summary>
        public List<WindturbineDiagConclusionExportDTO> WindturbineDiagConclusionList { get; set; }
        /// <summary>
        /// 风电机组报告GUID列表 - 风场诊断报告中，涉及到的所有机组报告的GUID集合
        /// </summary>
        public List<string> WindturbineReportGuid { get; set; }


        /// <summary>
        /// 没有数据的机组列表
        /// </summary>
        public string NoDataTurbine { get; set; }
    }
    /// <summary>
    /// 风电机组健康状态DTO
    /// </summary>
    public class WindturbineHealthStatusExportDTO
    {
        /// <summary>
        /// 风电机组ID
        /// </summary>
        public string WindturbineId { get; set; }
        /// <summary>
        /// 风电机组ID
        /// </summary>
        public string WindturbineName { get; set; }
        /// <summary>
        /// 主轴承状态 - 正常、警告、危险
        /// </summary>
        public string MainBearingStatus { get; set; }
        /// <summary>
        /// 齿轮箱状态 - 正常、警告、危险
        /// </summary>
        public string GearBoxStatus { get; set; }
        /// <summary>
        /// 发电机状态 - 正常、警告、危险
        /// </summary>
        public string GeneratorStatus { get; set; }
    }
    /// <summary>
    /// 风电机组诊断结论DTO
    /// </summary>
    public class WindturbineDiagConclusionExportDTO : ISortable
    {
        /// <summary>
        /// 风电机组ID
        /// </summary>
        public string WindturbineId { get; set; }
        /// <summary>
        /// 风电机组Name
        /// </summary>
        public string WindturbineName { get; set; }
        /// <summary>
        /// 诊断结论 - 主轴承、齿轮箱、发电机各自的诊断结论，拼接为字符串
        /// </summary>
        public string DiagnosisConclusion { get; set; }
        /// <summary>
        /// 维护建议 
        /// </summary>
        public string MaintainAdvice { get; set; }
        /// <summary>
        /// 运行建议 
        /// </summary>
        public string RuningAdvice { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }


        public string GetSortableName() => DiagnosisConclusion ?? string.Empty;
    }

}
