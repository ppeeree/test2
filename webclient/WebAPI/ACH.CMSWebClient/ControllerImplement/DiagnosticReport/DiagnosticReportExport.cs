using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.Common;
using ACH.DataEntity.ReportData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using ACH.Helper.Comparer;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using System.Text;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    /// <summary>
    /// 诊断报告导出
    /// </summary>
    public class DiagnosticReportExport
    {
        /// <summary>
        /// 获取风场报告存储的文件夹路径
        /// </summary>
        private readonly string reportRootFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StationReport");

        /// <summary>
        /// 诊断报告模板路径（相对路径）,用于导出诊断报告时替换模板中的内容。
        /// </summary>
        private readonly string reportTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StationReport", "cms_temp.docx");

        /// <summary>
        /// 当前生成报告类型：传动链/叶片/塔筒
        /// </summary>
        private EnumMonitorType monitorType = EnumMonitorType.CVM;

        private IReportRepository _diagnosticReportRepository = new ReportRepository();
        private IConfiguration configuration;
        private DiagnosticReport diagnosticReport;
        private IDeviceWaveRead _deviceWaveRead;
        private IDevTreeRepsitory devTreeRepsitory = DevTreeRepsitory.Instance;
        public DiagnosticReportExport(IConfiguration _configuration)
        {
            configuration = _configuration;
            diagnosticReport = new DiagnosticReport(configuration);
            _deviceWaveRead = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(monitorType);
        }

        /// <summary>
        /// 下载诊断报告
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Stream DownloadWindParkDiagReport(string reportGuid, out string fileName)
        {
            var exportDto = GetWindparkDiagReportExportDTO(reportGuid);
            fileName = exportDto.ReportName + ".docx";
            // 生成报告的目录
            string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StationReport", fileName);
            // 生成报告
            ReportExportTool.Export(reportTemplatePath, outputPath, exportDto, configuration);

            var stream = File.OpenRead(fileName);
            return stream;
        }


        /// <summary>
        /// 根据guid获取报告的地址，返回文档流
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        internal Stream GetStationReportStream(string reportGuid, out string fileName)
        {
            var report = _diagnosticReportRepository.QueryableSingle<WindParkDiagnosisReport>(x => x.ReportGuid == reportGuid) ?? throw new Exception($"风场诊断报告{reportGuid}不存在或已被删除！请联系管理员！");

            fileName = $"{report.ReportName}.docx";
            string reportPath = Path.Combine(reportRootFolderPath, fileName);

            if (File.Exists(reportPath))
            {
                var stream = File.OpenRead(reportPath);
                return stream;
            }
            else
            {
                throw new Exception($"风场诊断报告{reportGuid}未生成完成，请稍后");
            }
        }


        /// <summary>
        /// 在添加诊断记录时生成诊断报告.docx
        /// </summary>
        /// <param name="reportGuid"></param>
        public void ExportStationReport(string reportGuid)
        {
            try
            {
                ALog.Information($"[后台任务] 开始生成风电场报告 Word 文档，ReportGuid: {reportGuid}");

                // 获取导出数据
                var exportDto = GetWindparkDiagReportExportDTO(reportGuid);

                // 诊断报告.docx的临时目录：在服务的运行目录下
                string tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{exportDto.ReportName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx");

                // 生成报告
                ReportExportTool.Export(reportTemplatePath, tempPath, exportDto, configuration);

                // 生成报告的正式路径
                Directory.CreateDirectory(reportRootFolderPath);
                // 删除100天前的报告
                string[] reports = Directory.GetFiles(reportRootFolderPath);
                foreach (string report in reports)
                {
                    if (!Path.GetFileName(report).Contains("cms_temp") && File.GetLastWriteTime(report) < DateTime.Now.AddDays(-100))
                    {
                        File.Delete(report);
                    }
                }

                string outputPath = Path.Combine(reportRootFolderPath, $"{exportDto.ReportName}.docx");
                if (File.Exists(outputPath))
                {
                    // 如果输出路径存在，则删除原有文件
                    File.Delete(outputPath);
                }
                File.Move(tempPath, outputPath);

                ALog.Information($"[后台任务] Word 文档生成成功: {outputPath}");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"[后台任务] 生成报告失败，ReportGuid: {reportGuid}");
            }
        }

        /// <summary>
        /// 获取风电场诊断报告详情
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        public WindparkDiagReportExportDTO GetWindparkDiagReportExportDTO(string reportGuid)
        {
            var report = diagnosticReport.GetWindParkDiagReportDetailDTO(reportGuid);
            report.Children.SortByName(ascending: true, dictType: EnumSortType.CompName);

            var result = new WindparkDiagReportExportDTO
            {
                WindparkId = report.Id,
                WindparkName = report.Name,
                ReportDate = report.CreateTime.ToString("yyyy.MM.dd"),
                WindparkAddress = report.WindParkAddress,
                WindturbineCount = report.WindTurbineCount.ToString(),
                OperationlDate = DateTime.Parse(report.OperationlDate).ToString("yyyy年MM月dd日"),
                WindturbineType = report.WindTurbineType,
                TransmissionForm = report.TransmissionForm,
                DetectionTime = $"{report.MonitoringTimeStart.ToString("yyyy.MM.dd")}-{report.MonitoringTimeEnd.ToString("yyyy.MM.dd")}",
                ProjectOvewerview = report.ProjectOvewerview,
                ReportMonth = report.ReportMonth.ToString("yyyy年MM月"),
                ReportName = report.ReportName,
            };
            // result.NoDataTurbine = GetNoDataTurbineByReport(report.Id, report.Children);
            result.ProjectOvewerview = GetOvwerviewContent(result.WindparkName, report.WindTurbineCount, result.DetectionTime);
            result.DangerWindturbineContent = GetOvwerviewContent(report.Children.Where(x => x.Status == DiagnosticConclusion.WarningLevel.Danger).ToList());
            result.WarningWindturbineContent = GetOvwerviewContent(report.Children.Where(x => x.Status == DiagnosticConclusion.WarningLevel.Warning).ToList());
            result.AttentionWindturbineContent = GetOvwerviewContent(report.Children.Where(x => x.Status == DiagnosticConclusion.WarningLevel.Attention).ToList());
            result.WindturbineHealthStatusList = GetWindturbineHealthStatus(report.Children);
            result.WindturbineDiagConclusionList = GetWindturbineDiagConclusion(report.Children);
            result.WindturbineReportGuid = report.Children.Select(x => x.WindturbingReportGuid).Distinct().ToList();
            return result;
        }



        /// <summary>
        /// 获取风电机组健康状态
        /// </summary>
        /// <param name="summaryLines"></param>
        /// <returns></returns>
        public List<WindturbineHealthStatusExportDTO> GetWindturbineHealthStatus(List<WindParkDiagReportSummaryLine> summaryLines)
        {
            var result = new List<WindturbineHealthStatusExportDTO>();
            foreach (var summaryLine in summaryLines.GroupBy(x => x.WindturbineId))
            {
                result.Add(new WindturbineHealthStatusExportDTO
                {
                    WindturbineId = summaryLine.Key,
                    WindturbineName = summaryLine.FirstOrDefault().WindturbineName,
                    MainBearingStatus = DiagnosticConclusion.ConvertStatusReverse(summaryLine.FirstOrDefault(x => x.CompName == "主轴")?.CompStatus ?? "normal"),
                    GearBoxStatus = DiagnosticConclusion.ConvertStatusReverse(summaryLine.FirstOrDefault(x => x.CompName == "齿轮箱")?.CompStatus ?? "normal"),
                    GeneratorStatus = DiagnosticConclusion.ConvertStatusReverse(summaryLine.FirstOrDefault(x => x.CompName == "发电机")?.CompStatus ?? "normal")
                });
            }
            return result;
        }

        /// <summary>
        /// 获取风电机组诊断结论
        /// </summary>
        /// <param name="summaryLines"></param>
        /// <returns></returns>
        public List<WindturbineDiagConclusionExportDTO> GetWindturbineDiagConclusion(List<WindParkDiagReportSummaryLine> summaryLines)
        {
            var result = new List<WindturbineDiagConclusionExportDTO>();
            foreach (var summaryLine in summaryLines.GroupBy(x => x.WindturbineId))
            {
                foreach (var diagGroup in summaryLine.GroupBy(x => x.CompName))
                {
                    result.AddRange(diagGroup.Select(x => new WindturbineDiagConclusionExportDTO
                    {
                        WindturbineId = diagGroup.Key,
                        WindturbineName = diagGroup.FirstOrDefault().WindturbineName,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        MaintainAdvice = x.MaintainAdvice,
                        RuningAdvice = x.RuningAdvice,
                        Status = DiagnosticConclusion.ConvertStatusReverse(x.DiagnosisConclusionStatus)
                    }));
                }
            }
            // 对实体部件名称排序
            return result;
        }




        #region 新：修改报告汇总

        /// <summary>
        /// 生成概述内容
        /// </summary>
        /// <param name="windparkName">风场名称</param>
        /// <param name="windTurbineCount">风机个数</param>
        /// <param name="detectionTime">时间范围</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetOvwerviewContent(string windparkName, int windTurbineCount, string detectionTime)
        {
            return $"通过对{windparkName}风电场{windTurbineCount}台风机{detectionTime}期间在线振动监测数据分析，机组健康状态为：";
        }

        /// <summary>
        /// 获取没有数据的机组名称 - 从波形索引中获取
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        private string GetNoDataTurbineByDB(string stationID, DateTime bgTime, DateTime endTime)
        {
            List<string> ids = new List<string>();

            // 获取该风场下全部机组
            var deviceList = devTreeRepsitory.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);

            foreach (var item in deviceList)
            {
                string deviceID = item.Key;
                // 获取时间段内该机组的所有振动数据
                List<MeasEventBase> meas = _deviceWaveRead.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime);
                if (meas.Count == 0)
                {
                    ids.Add(item.First().DeviceName);
                }
            }

            if (ids.Count == 0)
            {
                return "本次报告周期内该风场振动数据全部采集。";
            }
            else
            {
                return $"{string.Join("、", ids)}风机本次报告周期内未采集到振动数据，未进行健康状态评估。";
            }
        }


        /// <summary>
        /// 获取没有数据的机组名称 - 从已汇总的报告中获取
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        private string GetNoDataTurbineByReport(string stationID, List<WindParkDiagReportSummaryLine> children)
        {
            List<string> ids = new List<string>();

            // 获取该风场下全部机组
            var deviceList = devTreeRepsitory.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);
            var groupChildren = children.GroupBy(o => o.WindturbineId);

            foreach (var item in deviceList)
            {
                var obj = groupChildren.Where(o => o.Key == item.Key).ToList();
                if (obj == null || obj.Count == 0)
                {
                    ids.Add(item.First().DeviceName);
                }
            }

            if (ids.Count == 0)
            {
                return "本次报告周期内该风场振动数据全部采集。";
            }
            else
            {
                return $"{string.Join("、", ids)}风机本次报告周期内未采集到振动数据，未进行健康状态评估。";
            }
        }

        /// <summary>
        /// 新概述汇总方法
        /// 策略：按照相同的故障类型+维护建议进行聚合，对聚合后的列表按照最小的机组名称排序
        /// </summary>
        /// <param name="summaryLines"></param>
        /// <returns></returns>
        public string GetOvwerviewContent(List<WindParkDiagReportSummaryLine> summaryLines)
        {
            if (summaryLines.Count == 0)
                return "无";

            // 1、按照机组聚合故障类型+维护建议
            var groupDevice = summaryLines.GroupBy(o => o.WindturbineName);
            Dictionary<string, string> deviceDic = new Dictionary<string, string>();

            // 先聚合一个机组内的各个部件的概述，其中状态为正常的部件和状态异常的部件聚合方式不同。
            foreach (var group in groupDevice)
            {
                string deviceMessage = "";
                List<string> deviceMessageList = new List<string>();
                // 处理部件状态为正常的部件
                var normalCompName = string.Join("、", group.Where(o => o.CompStatus == "normal").Select(o => o.CompName));
                string normalCompMessage = $"{normalCompName}运行正常";

                // 处理部件状态为异常的部件
                var unnormalComp = group.Where(o => o.CompStatus != "normal").ToList();
                if (unnormalComp.Count == 0)
                {
                    deviceMessage = normalCompMessage + "。";
                }
                else
                {
                    foreach (var unnormalItem in unnormalComp)
                    {
                        // 将部件名称提取出来
                        string newDiagnosisConclusion = unnormalItem.DiagnosisConclusion.Replace(unnormalItem.CompName, "");
                        string newMaintainAdvice = unnormalItem.MaintainAdvice.Replace(unnormalItem.CompName, "");
                        string unnormalMessage = $"{unnormalItem.CompName}{newDiagnosisConclusion}，{newMaintainAdvice}";

                        deviceMessageList.Add(unnormalMessage);
                    }
                    deviceMessageList.Add(normalCompMessage);

                    deviceMessage = string.Join("；", deviceMessageList) + "。";
                }

                deviceDic.Add(group.Key, deviceMessage);
            }

            // 对字典中信息一致的机组进行聚合，并按照最小的机组名排序
            var aggregatedGroups = deviceDic
                .GroupBy(kvp => kvp.Value)
                .Select(g => new
                {
                    RepresentativeTurbine = g.Select(kvp => kvp.Key).OrderBy(name => name).First(), // 取排序后第一个机组名
                    Message = g.Key,  // 消息内容
                    AllTurbines = g.Select(kvp => kvp.Key).OrderBy(name => name).ToList() // 所有机组名（排序后）
                })
                .OrderBy(x => x.RepresentativeTurbine) // 按代表机组名排序
                .ToList();


            var result = new StringBuilder();
            foreach (var group in aggregatedGroups)
            {
                // 1. 用顿号连接所有机组名（已排序）
                var turbineNames = string.Join("、", group.AllTurbines);

                // 2. 拼接状态消息（已包含具体描述和标点）
                var lineStr = $"{turbineNames}{group.Message}";

                result.AppendLine(lineStr);
            }

            return result.ToString();
        }

        /// <summary>
        /// 获取概览内容
        /// </summary>
        /// <param name="summaryLines"></param>
        /// <returns></returns>
        public string GetOvwerviewContentOld(List<WindParkDiagReportSummaryLine> summaryLines)
        {
            if (summaryLines.Count == 0)
                return "无";
            var result = new StringBuilder();
            foreach (var diagGroup in summaryLines.GroupBy(x => x.DiagnosisConclusion))
            {
                foreach (var adviceGroup in diagGroup.GroupBy(x => x.MaintainAdvice))
                {
                    var lineStr = $"{string.Join("、", adviceGroup.Select(x => x.WindturbineName).Distinct())}{diagGroup.Key}，{adviceGroup.Key}。";
                    result.AppendLine(lineStr);
                }
            }
            return result.ToString();
        }


        #endregion
    }
}
