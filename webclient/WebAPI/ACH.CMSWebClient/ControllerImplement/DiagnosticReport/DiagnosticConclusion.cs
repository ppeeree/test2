using ACH.CMSWebClient.Common;
using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.ReportData;
using ACH.DBRepository.App;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    public class DiagnosticConclusion
    {
        private IReportRepository _diagnosticReportRepository = new ReportRepository();
        private IConfiguration configuration;
        private DiagnosticTurbine diagnosticTurbine;
        public DiagnosticConclusion(IConfiguration _configuration)
        {
            configuration = _configuration;
            diagnosticTurbine = new DiagnosticTurbine(configuration);
        }

        /// <summary>
        /// 获取诊断结论
        /// </summary>
        /// <param name="windturbineId"></param>
        /// <returns></returns>
        public List<SaveDiagnosisConclusionDTO> GetDiagnosisConclusion(string windturbineId)
        {
            var result = new List<SaveDiagnosisConclusionDTO>();
            if (string.IsNullOrEmpty(windturbineId))
            {
                throw new ArgumentNullException(nameof(windturbineId), "风机ID不能为空！");
            }
            var list = _diagnosticReportRepository.QueryableList<DeviceDiagnosisConclusion>(x => x.WindturbineId == windturbineId);
            if (list.Count() != 0)
            {
                //首先查询当前风机是否有自定义诊断结论，如果有则优先显示自定义的。
                result = list
                    .Select(x => new SaveDiagnosisConclusionDTO
                    {
                        CompName = x.CompName,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        MaintainAdvice = x.MaintainAdvice,
                        Status = x.WarningLevel
                    }).ToList();
            }
            else
            {
                //查询当前机组的所有组件，如果有自定义诊断结论，则显示自定义的。
                var windturbineComponent = diagnosticTurbine.GetComponentList(windturbineId)
                    .Select(x => x.ComponentName)
                    .Distinct()
                    .ToList();

                //如果没有自定义诊断结论，则查询默认的诊断结论。
                result = _diagnosticReportRepository.QueryableList<DefaultDiagnosisConclusion>(x => string.IsNullOrEmpty(x.MeaslocName) && windturbineComponent.Contains(x.CompName))
                    .Select(x => new SaveDiagnosisConclusionDTO
                    {
                        CompName = x.CompName,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        MaintainAdvice = x.MaintainAdvice,
                        Status = ConvertStatus(x.WarningLevel)
                    }).ToList();
            }
            return result;
        }
        /// <summary>
        /// 获取风机诊断结论树形结构数据。
        /// </summary>
        /// <param name="windturbineId"></param>
        /// <returns></returns>
        public object GetWindturbineDiagnosisConclusion(string windturbineId)
        {
            var diagnosisConclusion = GetDiagnosisConclusion(windturbineId);
            var dtoConclusion = new List<DiagnosisReportConclusionTreeDTO>();
            foreach (var item in diagnosisConclusion.GroupBy(x => x.CompName))
            {
                var conclusion = new DiagnosisReportConclusionTreeDTO
                {
                    Name = item.Key,
                    CompStatus = GetMaxStatus(item.Select(x => x.Status).ToList()),
                    Children = item.Select(x => new DiagnosisReportConclusionDTO
                    {
                        CompName = x.CompName,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        MaintainAdvice = x.MaintainAdvice,
                        Status = x.Status
                    }).ToList()
                };
                dtoConclusion.Add(conclusion);
            }
            //获取机组状态，取诊断结论中的最高状态。
            var deviceStatus = GetMaxStatus(dtoConclusion.Select(x => x.CompStatus).ToList());
            return new
            {
                Conclusions = dtoConclusion,
                Status = deviceStatus,
                RuningAdvice = _diagnosticReportRepository.QueryableSingle<DefaultRuningAdvice>(x => x.WarningLevel == ConvertStatusReverse(deviceStatus))?.RuningAdvice
            };
        }
        /// <summary>
        /// 获取默认诊断结论。
        /// </summary>
        /// <returns></returns>
        public List<DefaultDiagnosisConclusionDTO> GetDefaultDiagResult()
        {
            var list = _diagnosticReportRepository.QueryableAll<DefaultDiagnosisConclusion>();
            //去重，只保留诊断结论字段唯一的记录。
            return AutoMapperHelper.Map<List<DefaultDiagnosisConclusion>, List<DefaultDiagnosisConclusionDTO>>(list)
                .GroupBy(x => x.DiagnosisConclusion).Select(x => x.First()).ToList();
        }
        /// <summary>
        /// 获取默认运行建议
        /// </summary>
        /// <returns></returns>
        public List<DefaultRuningAdviceDTO> GetDefaultRuningAdvice()
        {
            var list = _diagnosticReportRepository.QueryableAll<DefaultRuningAdvice>();
            return AutoMapperHelper.Map<List<DefaultRuningAdvice>, List<DefaultRuningAdviceDTO>>(list);
        }
        /// <summary>
        /// 获取诊断报告结论树结构数据。
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        public List<DiagnosisReportConclusionTreeDTO> GetReportDiagnosisConclusion(string reportGuid)
        {
            var reportConclusion = _diagnosticReportRepository.QueryableList<DeviceReportDiagnosisConclusion>(x => x.ReportGuid == reportGuid);
            var dtoConclusion = new List<DiagnosisReportConclusionTreeDTO>();
            if (reportConclusion != null && reportConclusion.Count() > 0)
            {
                //将诊断结论按组件分组，便于前端显示。
                foreach (var item in reportConclusion.GroupBy(x => x.CompName))
                {
                    var conclusion = new DiagnosisReportConclusionTreeDTO
                    {
                        Name = item.Key,
                        CompStatus = GetMaxStatus(item.Select(x => x.WarningLevel).ToList()),
                        Children = AutoMapperHelper.Map<List<DeviceReportDiagnosisConclusion>, List<DiagnosisReportConclusionDTO>>(item.ToList())
                    };
                    dtoConclusion.Add(conclusion);
                }
            }
            return dtoConclusion;
        }
        /// <summary>
        /// 获取风机状态。取机组诊断结论中的最高状态。
        /// </summary>
        /// <param name="windTurbuineId"></param>
        /// <returns></returns>
        public string GetWindTurbineStatus(string windTurbuineId)
        {
            var list = GetDiagnosisConclusion(windTurbuineId);
            return GetMaxStatus(list.Select(x => x.Status).ToList());
        }
        /// <summary>
        /// 获取最高状态。
        /// </summary>
        /// <param name="statusList"></param>
        /// <returns></returns>
        public string GetMaxStatus(List<string> statusList)
        {
            if (statusList.Any(x => x == "danger" || x == "危险"))
                return "danger";
            else if (statusList.Any(x => x == "warning" || x == "警告"))
                return "warning";
            else if (statusList.Any(x => x == "attention" || x == "注意"))
                return "attention";
            else
                return "normal";
        }
        /// <summary>
        /// 将前端传来的状态转换为后端的状态。
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string ConvertStatus(string status)
        {
            if (status == "正常")
                return WarningLevel.Normal;
            else if (status == "警告")
                return WarningLevel.Warning;
            else if (status == "注意")
                return WarningLevel.Attention;
            else if (status == "危险")
                return WarningLevel.Danger;
            else
                return WarningLevel.Normal;
        }
        /// <summary>
        /// 将后端的状态转换为前端的状态。
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string ConvertStatusReverse(string status)
        {
            if (status == WarningLevel.Normal)
                return "正常";
            else if (status == WarningLevel.Warning)
                return "警告";
            else if (status == WarningLevel.Attention)
                return "注意";
            else if (status == WarningLevel.Danger)
                return "危险";
            else
                return "正常";
        }
        /// <summary>
        /// 警告等级枚举。
        /// </summary>
        public class WarningLevel
        {
            public const string Normal = "normal", Warning = "warning", Attention = "attention", Danger = "danger";
        }
    }
}
