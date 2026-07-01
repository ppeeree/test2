using ACH.CMSWebClient.Common;
using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.ReportData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using SqlSugar;
using System.Linq.Expressions;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    public class DiagnosticReport
    {
        private DiagnosticAnalyzerRecord diagnosticAnalyzerRecord;
        private IConfiguration configuration;
        private IReportRepository _diagnosticReportRepository = new ReportRepository();
        public DiagnosticReport(IConfiguration _configuration)
        {
            configuration = _configuration;
            diagnosticAnalyzerRecord = new DiagnosticAnalyzerRecord(_configuration);
        }

        /// <summary>
        /// 获取诊断报告DTO, 包含诊断结论和诊断分析记录明细信息。
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        public DiagnosisReportDTO GetDiagnosisReport(string reportGuid)
        {
            //主表查询
            DeviceDiagnosisReport report = _diagnosticReportRepository.GetDeviceDiagnosisReportByID(reportGuid);

            var dto = AutoMapperHelper.Map<DeviceDiagnosisReport, DiagnosisReportDTO>(report);
            //分析记录明细查询
            dto.AnalyzerRecords = diagnosticAnalyzerRecord.GetReportAnalyzerRecord(reportGuid);
            //诊断结论明细查询
            dto.Conclusions = new DiagnosticConclusion(configuration).GetReportDiagnosisConclusion(reportGuid);
            //风电场信息查询
            dto.WindTurbine = new DiagnosticTurbine(configuration).GetDiagnosisTurbine(report.WindturbineId);
            dto.WindTurbine.SampleDataSpeed = Math.Round((report.SampleDataSpeed ?? 0), 3, MidpointRounding.AwayFromZero);
            return dto;
        }
        /// <summary>
        /// 保存诊断报告
        /// </summary>
        /// <param name="diagnosis"></param>
        public string SaveDiagnosisReport(SaveDiagnosisReportDTO diagnosis)
        {
            // 保存诊断报告
            if (diagnosis == null)
            {
                throw new ArgumentNullException(nameof(diagnosis), "诊断报告不能为空");
            }
            // 新增
            var reportMode = new DeviceDiagnosisReport
            {
                WindturbineId = diagnosis.WindturbineId,
                RuningAdvice = diagnosis.RuningAdvice,
                Status = diagnosis.Status ?? string.Empty,
                ReportGuid = Guid.NewGuid().ToString("N"),
                SampleDataSpeed = diagnosis.SampleDataSpeed,
                CreatedTime = diagnosis.ReportTime ?? DateTime.Now,
            };
            if (string.IsNullOrEmpty(reportMode.Status) && diagnosis.Conclusions != null && diagnosis.Conclusions.Count > 0)
            {
                reportMode.Status = new DiagnosticConclusion(configuration).GetMaxStatus(diagnosis.Conclusions.Select(x => x.Status).ToList());
            }
            int LineId = 1;
            var reportAnalyzerRecordList = new List<DeviceReportAnalyzerRecord>();
            if (diagnosis.AnalyzerRecords != null && diagnosis.AnalyzerRecords.Count > 0)
            {
                foreach (var item in diagnosis.AnalyzerRecords)
                {
                    var analyzerRecordMode = _diagnosticReportRepository.QueryableSingle<DeviceDiagnosisAnalyzerRecord>(x => x.Id == item.AnalyzerRecordId) ?? throw new Exception("分析记录不存在或已被删除！请联系管理员！");
                    var reportAnalyzerRecord = new DeviceReportAnalyzerRecord
                    {
                        AnalyzerRecordId = analyzerRecordMode.Id,
                        ReportGuid = reportMode.ReportGuid,
                        CompName = analyzerRecordMode.CompName,
                        MeaslocId = analyzerRecordMode.MeaslocId,
                        MeaslocName = analyzerRecordMode.MeaslocName,
                        EigenValueId = analyzerRecordMode.EigenValueId,
                        ImageType = analyzerRecordMode.ImageType,
                        Description = item.Description,
                        Image = analyzerRecordMode.Image,
                        ImageUrlType = analyzerRecordMode.ImageUrlType,
                        LineId = LineId
                    };
                    reportAnalyzerRecordList.Add(reportAnalyzerRecord);
                    LineId++;
                }
            }
            LineId = 1;
            var reportConclusionList = new List<DeviceReportDiagnosisConclusion>();
            if (diagnosis.Conclusions != null && diagnosis.Conclusions.Count > 0)
            {
                foreach (var item in diagnosis.Conclusions)
                {
                    if (string.IsNullOrEmpty(item.CompName))
                    {
                        throw new ArgumentNullException(nameof(item.CompName), $"机组{reportMode.WindturbineId}诊断结论部件名称不能为空");
                    }
                    var conclusionMode = AutoMapperHelper.Map<DiagnosisReportConclusionDTO, DeviceReportDiagnosisConclusion>(item);
                    conclusionMode.LineId = LineId;
                    conclusionMode.ReportGuid = reportMode.ReportGuid;
                    reportConclusionList.Add(conclusionMode);
                    LineId++;
                }
            }
            _diagnosticReportRepository.SaveDiagnosisReport(reportMode, reportAnalyzerRecordList, reportConclusionList);
            return reportMode.ReportGuid;
        }
        /// <summary>
        /// 获取诊断报告列表（简单版）
        /// </summary>
        /// <param name="windturbineId"></param>
        /// <returns></returns>
        public List<SimpleDiagnosisReportDTO> GetSimpleDiagnosisReportList(string windturbineId, DateTime startTime, DateTime endTime)
        {
            var reports = _diagnosticReportRepository.QueryableList<DeviceDiagnosisReport>(x => x.WindturbineId == windturbineId && x.CreatedTime >= startTime.Date && x.CreatedTime < endTime.Date.AddDays(1));
            // 返回数据按日期分组，只取每组的第一条记录（最新的报告）
            return AutoMapperHelper.Map<List<DeviceDiagnosisReport>, List<SimpleDiagnosisReportDTO>>(reports)
                .OrderByDescending(x => x.CreatedTime).ToList();
        }
        /// <summary>
        /// 删除诊断报告
        /// </summary>
        /// <param name="reportGuid"></param>
        public void DeleteDiagnosisReport(string reportGuid)
        {
            if (string.IsNullOrEmpty(reportGuid))
                throw new ArgumentNullException(nameof(reportGuid), "报告ID不能为空");
            _diagnosticReportRepository.DeleteDiagnosisReport(reportGuid);
        }
        /// <summary>
        /// 保存风电场诊断报告
        /// </summary>
        /// <param name="windParkReportDto"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public string SaveWindParkDiagnosisReport(SaveWindParkDiagReportDTO windParkReportDto)
        {
            if (windParkReportDto == null)
            {
                throw new ArgumentNullException(nameof(windParkReportDto), "风电场诊断报告不能为空");
            }
            if (windParkReportDto.WindParkDeviceRelations == null || windParkReportDto.WindParkDeviceRelations.Count <= 0)
            {
                throw new ArgumentException("风电场诊断报告的设备关系不能为空");
            }
            var windParkReport = new WindParkDiagnosisReport
            {
                ReportGuid = Guid.NewGuid().ToString("N"),
                WindParkId = windParkReportDto.WindParkId,
                WindParkName = windParkReportDto.WindParkName ?? string.Empty,
                //报告月份默认为当月第一天，便于报表统计
                ReportMonth = new DateTime(windParkReportDto.ReportMonth.Year, windParkReportDto.ReportMonth.Month, 1),
                MonitoringTimeStart = windParkReportDto.MonitoringTimeStart,
                MonitoringTimeEnd = windParkReportDto.MonitoringTimeEnd,
                ProjectOverview = windParkReportDto.ProjectOverview,
                CreateTime = DateTime.Now,
                Remark = windParkReportDto.Remark ?? string.Empty,
                ReportName = windParkReportDto.ReportName
            };
            if (string.IsNullOrEmpty(windParkReport.WindParkName))
            {
                windParkReport.WindParkName = new DiagnosticTurbine(configuration).GetWindParkName(windParkReportDto.WindParkId);
            }
            WindParkReportReName(windParkReport);
            var deviceReportRels = new List<WindParkReportDeviceRelation>();
            foreach (var item in windParkReportDto.WindParkDeviceRelations)
            {
                if (string.IsNullOrEmpty(item.WindTurbineReportGuid))
                {
                    throw new ArgumentException("机组报告ID不能为空");
                }
                deviceReportRels.Add(new WindParkReportDeviceRelation
                {
                    WindParkReportGuid = windParkReport.ReportGuid,
                    WindTurbineId = item.WindTurbineId,
                    WindTurbineReportGuid = item.WindTurbineReportGuid,
                    WindTurbineName = string.IsNullOrEmpty(item.WindTurbineName) ? new DiagnosticTurbine(configuration).GetWindTurbineName(item.WindTurbineId) : item.WindTurbineName,
                });
            }
            _diagnosticReportRepository.SaveWindParkDiagnosisReport(windParkReport, deviceReportRels);


            // 启动新线程生成word文档
            Task.Run(() =>
            {
                new DiagnosticReportExport(configuration).ExportStationReport(windParkReport.ReportGuid);
            });


            return windParkReport.ReportGuid;
        }

        /// <summary>
        /// 风电场报告重命名
        /// </summary>
        /// <param name="report"></param>
        public void WindParkReportReName(WindParkDiagnosisReport report)
        {
            var windParkReport = _diagnosticReportRepository.QueryableList<WindParkDiagnosisReport>(x => x.WindParkId == report.WindParkId && x.ReportMonth == report.ReportMonth)
                .GroupBy(x => x.CreateTime.Date);
            if (windParkReport != null && windParkReport.Any())
            {
                var reportCnt = windParkReport.Count();
                if (windParkReport.Any(x => x.Key == DateTime.Now.Date))
                {
                    //如果当天已有报告，则需要减去1
                    reportCnt -= 1;
                }
                var suffix = reportCnt <= 0 ? string.Empty : $"({reportCnt.ToString()})";
                //风场当月如果已有报告，则直接在名称后加序号
                report.ReportName = $"{report.WindParkName}振动分析报告{suffix}";
            }
            else if (string.IsNullOrEmpty(report.ReportName))
            {
                report.ReportName = $"{report.WindParkName}振动分析报告";
            }
        }
        /// <summary>
        /// 删除风电场诊断报告
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DeleteWindParkDiagnosisReport(string reportGuid)
        {
            if (string.IsNullOrEmpty(reportGuid))
                throw new ArgumentNullException(nameof(reportGuid), "报告ID不能为空");
            _diagnosticReportRepository.DeleteWindParkDiagnosisReport(reportGuid);
        }
        /// <summary>
        /// 获取风电场诊断报告树
        /// </summary>
        /// <param name="windParkId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<SimpleWindParkDiagReportDTO> GetWindParkDiagnosisReportTree(string windParkId, DateTime startTime, DateTime endTime)
        {
            var windParkReport = _diagnosticReportRepository.QueryableList<WindParkDiagnosisReport>(x => x.WindParkId == windParkId && x.CreateTime >= startTime.Date && x.CreateTime < endTime.Date.AddDays(1));
            if (windParkReport == null || windParkReport.Count() <= 0)
                return new List<SimpleWindParkDiagReportDTO>();
            var result = new List<SimpleWindParkDiagReportDTO>();
            foreach (var item in windParkReport.GroupBy(x => x.CreateTime.Date).OrderByDescending(x => x.Key))
            {
                var firstReport = item.OrderByDescending(x => x.CreateTime).First();
                var child = new SimpleWindParkDiagReportDTO
                {
                    Id = firstReport.ReportGuid,
                    Name = $"{firstReport.WindParkName}{firstReport.CreateTime:yyyy-MM-dd}",
                    CreateTime = firstReport.CreateTime,
                    Children = new List<WindParkDeviceReportRelDTO>()
                };
                var deviceRels = _diagnosticReportRepository.QueryableList<WindParkReportDeviceRelation>(x => x.WindParkReportGuid == firstReport.ReportGuid);
                foreach (var deviceRel in deviceRels)
                {
                    var deviceReport = _diagnosticReportRepository.QueryableSingle<DeviceDiagnosisReport>(x => x.ReportGuid == deviceRel.WindTurbineReportGuid) ?? throw new Exception($"风场{firstReport.WindParkName}，机组{deviceRel.WindTurbineId}，单机报告{deviceRel.WindTurbineReportGuid}不存在或已被删除，请联系管理员！");
                    var childDevice = new WindParkDeviceReportRelDTO
                    {
                        Id = deviceRel.WindTurbineReportGuid,
                        WindturbineId = deviceRel.WindTurbineId,
                        name = deviceRel.WindTurbineName,
                        CreateTime = deviceReport.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = deviceReport.Status
                    };
                    child.Children.Add(childDevice);
                }
                child.Children = child.Children.OrderBy(x => x.WindturbineId).ToList();
                result.Add(child);
            }
            return result;
        }
        /// <summary>
        /// 获取风电场诊断报告汇总信息
        /// </summary>
        /// <param name="windParkID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public WindParkDiagReportSummaryDTO GetWindParkReportSummaryInfo(string windParkID, DateTime startTime, DateTime endTime)
        {
            // 获取该风场的所有机组列表
            List<DevMeasLocation> deviceList = new DiagnosticTurbine(configuration).GetWindTurbineList(windParkID);
            WindParkInfoDTO windParkInfo = new DiagnosticTurbine(configuration).GetWindParkInfo(windParkID, deviceList);

            var summaryDto = AutoMapperHelper.Map<WindParkInfoDTO, WindParkDiagReportSummaryDTO>(windParkInfo);
            List<WindParkDiagReportSummaryLine> lines = new List<WindParkDiagReportSummaryLine>();
            int diagCnt = 0, noDiagCnt = 0;
            foreach (var device in deviceList)
            {
                var reportList = GetSimpleDiagnosisReportList(device.DeviceID, startTime, endTime);
                if (reportList == null || reportList.Count <= 0)
                {
                    noDiagCnt++;
                    AddNoDiagnosisWindturbineLine(device, lines);
                }
                else
                {
                    AddDiagnosisWindturbineLine(device, reportList, lines);
                    diagCnt++;
                }
            }

            // 特殊处理：给每条诊断记录新增GUID
            foreach (var item in lines)
            {
                item.DiagRecordGuid = Guid.NewGuid().ToString("N");
            }


            summaryDto.Children = lines.OrderByDescending(o => o.DiagnosisTime).OrderBy(x => x.WindturbineId).ToList();
            summaryDto.DiagnosisWindturbineCount = diagCnt;
            summaryDto.NoDiagnosisWindturbineCount = noDiagCnt;
            return summaryDto;
        }
        /// <summary>
        /// 添加未诊断的风机信息到汇总列表中
        /// </summary>
        /// <param name="device"></param>
        /// <param name="lines"></param>
        public void AddNoDiagnosisWindturbineLine(DevMeasLocation device, List<WindParkDiagReportSummaryLine> lines)
        {
            List<string> compList = new List<string>() { "主轴", "齿轮箱", "发电机" };

            foreach (var comp in compList)
            {
                var line = new WindParkDiagReportSummaryLine
                {
                    WindturbineId = device.DeviceID,
                    WindturbineName = device.DeviceName,
                    CompName = comp,
                    IsDiagnosis = false,
                };
                lines.Add(line);
            }
            lines.SortByName(ascending: true, dictType: EnumSortType.CompName);
        }
        /// <summary>
        /// 添加已诊断的风机信息到汇总列表中
        /// </summary>
        /// <param name="device"></param>
        /// <param name="lines"></param>
        public void AddDiagnosisWindturbineLine(DevMeasLocation device, List<SimpleDiagnosisReportDTO> reports, List<WindParkDiagReportSummaryLine> lines)
        {
            foreach (var report in reports)
            {
                List<DiagnosisReportConclusionTreeDTO> conclusionList = new DiagnosticConclusion(configuration).GetReportDiagnosisConclusion(report.ReportGuid);
                foreach (var item in conclusionList)
                {
                    lines.AddRange(item.Children.Select(x => new WindParkDiagReportSummaryLine
                    {
                        WindturbineId = device.DeviceID,
                        WindturbineName = device.DeviceName,
                        CompName = item.Name,
                        DiagnosisTime = report.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = report.Status,
                        MaintainAdvice = x.MaintainAdvice,
                        RuningAdvice = report.RuningAdvice,
                        CompStatus = item.CompStatus,
                        WindturbingReportGuid = report.ReportGuid,
                        IsDiagnosis = true,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        DiagnosisConclusionStatus = x.Status
                    }));
                }
                // 按照部件名称排序
                lines.SortByName(ascending: true, dictType: EnumSortType.CompName);
            }
        }
        /// <summary>
        /// 获取风电场诊断报告详情信息
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public WindParkDiagReportDetailDTO GetWindParkDiagReportDetailDTO(string reportGuid)
        {
            var report = _diagnosticReportRepository.QueryableSingle<WindParkDiagnosisReport>(x => x.ReportGuid == reportGuid) ?? throw new Exception($"风场诊断报告{reportGuid}不存在或已被删除！请联系管理员！");

            List<DevMeasLocation> deviceList = new DiagnosticTurbine(configuration).GetWindTurbineList(report.WindParkId);
            var windParkInfo = new DiagnosticTurbine(configuration).GetWindParkInfo(report.WindParkId, deviceList);

            var resultDto = AutoMapperHelper.Map<WindParkInfoDTO, WindParkDiagReportDetailDTO>(windParkInfo);
            resultDto.MonitoringTimeStart = report.MonitoringTimeStart;
            resultDto.MonitoringTimeEnd = report.MonitoringTimeEnd;
            resultDto.ProjectOvewerview = report.ProjectOverview;
            resultDto.CreateTime = report.CreateTime;
            resultDto.ReportName = report.ReportName;
            resultDto.ReportMonth = report.ReportMonth;
            var deviceRels = _diagnosticReportRepository.QueryableList<WindParkReportDeviceRelation>(x => x.WindParkReportGuid == reportGuid);
            resultDto.DiagnosisWindturbineCount = deviceRels.Count();
            resultDto.NoDiagnosisWindturbineCount = 0;

            List<WindParkDiagReportSummaryLine> lines = new List<WindParkDiagReportSummaryLine>();
            foreach (var device in deviceRels)
            {
                DiagnosisReportDTO deviceReport = GetDiagnosisReport(device.WindTurbineReportGuid);
                foreach (var item in deviceReport.Conclusions)
                {
                    lines.AddRange(item.Children.Select(x => new WindParkDiagReportSummaryLine
                    {
                        DiagRecordGuid = Guid.NewGuid().ToString("N"),
                        WindturbineId = device.WindTurbineId,
                        WindturbineName = device.WindTurbineName,
                        CompName = item.Name,
                        DiagnosisTime = deviceReport.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        Status = deviceReport.Status,
                        MaintainAdvice = x.MaintainAdvice,
                        RuningAdvice = deviceReport.RuningAdvice,
                        CompStatus = item.CompStatus,
                        WindturbingReportGuid = device.WindTurbineReportGuid,
                        IsDiagnosis = true,
                        DiagnosisConclusion = x.DiagnosisConclusion,
                        DiagnosisConclusionStatus = x.Status
                    }));
                }
            }
            resultDto.Children = lines.OrderBy(o => o.WindturbineName).ToList();//.SortByName(ascending: true, dictType: EnumSortType.CompName);
            return resultDto;
        }
        /// <summary>
        /// 获取风电场诊断报告列表
        /// </summary>
        /// <param name="windParkId"></param>
        /// <param name="reportName"></param>
        /// <param name="reportTime"></param>
        /// <param name="offset"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public WindParkDiagReportPageResultDTO GetWindParkReportList(string? windParkId, string? reportName, string? reportTime, int offset, int pageSize)
        {
            if (offset <= 0)
            {
                //页码默认从第一页开始
                offset = 1;
            }
            int totalCount = 0;
            Expression<Func<WindParkDiagnosisReport, bool>> expression = x => true;
            if (!string.IsNullOrEmpty(windParkId))
            {
                expression = expression.And(x => x.WindParkId == windParkId);
            }
            if (!string.IsNullOrEmpty(reportName))
            {
                expression = expression.And(x => x.ReportName.Contains(reportName));
            }
            if (!string.IsNullOrEmpty(reportTime) && DateTime.TryParse(reportTime, out var date))
            {
                expression = expression.And(x => x.CreateTime >= date.Date && x.CreateTime < date.Date.AddDays(1));
            }
            var reports = _diagnosticReportRepository.QueryByPageList(offset, pageSize, ref totalCount, expression, x => x.CreateTime, false);

            List<WindParkDiagReportDTO> children = new List<WindParkDiagReportDTO>();
            foreach (var item in reports)
            {
                WindParkDiagReportDTO obj = new WindParkDiagReportDTO();
                obj.ReportGuid = item.ReportGuid;
                obj.WindParkId = item.WindParkId;
                obj.WindParkName = item.WindParkName;
                obj.ReportName = item.ReportName;
                obj.CreateTime = item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
                obj.Remark = item.Remark;
                children.Add(obj);
            }

            return new WindParkDiagReportPageResultDTO
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                PageIndex = offset,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Children = children
                // Children = AutoMapperHelper.Map<List<WindParkDiagnosisReport>, List<WindParkDiagReportDTO>>(reports)
            };
        }
    }
}
