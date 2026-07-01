using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.Common;
using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.ReportData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using ACH.Helper.Image;
using ACH.Helper.ImageSizeReader;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    /// <summary>
    /// 分析记录相关方法
    /// </summary>
    public class DiagnosticAnalyzerRecord
    {
        private IDevTreeRepsitory _devTreeRepository;
        DBSelect dbSelect;
        private IConfiguration configuration;
        private IReportRepository _diagnosticReportRepository = new ReportRepository();

        public DiagnosticAnalyzerRecord(IConfiguration _configuration)
        {
            configuration = _configuration;
            _devTreeRepository = DevTreeRepsitory.Instance;
            dbSelect = new DBSelect(configuration);
        }

        /// <summary>
        /// 获取分析记录树
        /// </summary>
        /// <returns></returns>
        public AnalyzerRecordTreeDTO GetAnalyzerRecordTree(string windID, DateTime startTime, DateTime endTime)
        {
            return BindAnalyzerRecordTree(GetAnalyzerRecords(windID, startTime, endTime));
        }

        /// <summary>
        /// 绑定分析记录树
        /// </summary>
        /// <param name="analyzerRecords"></param>
        /// <returns></returns>
        public AnalyzerRecordTreeDTO BindAnalyzerRecordTree(List<DeviceDiagnosisAnalyzerRecord> analyzerRecords)
        {
            if (analyzerRecords == null || analyzerRecords.Count == 0)
                return new AnalyzerRecordTreeDTO();
            var analyzerRecordTree = new AnalyzerRecordTreeDTO
            {
                Name = new DiagnosticTurbine(configuration).GetWindTurbineName(analyzerRecords[0].WindturbineId),
                Id = analyzerRecords[0].WindturbineId,
                Children = new List<CompTreeDTO>()
            };
            foreach (var recordGroup in analyzerRecords.GroupBy(x => x.CompName))
            {
                var compTree = new CompTreeDTO
                {
                    Name = recordGroup.Key,
                    Id = $"{analyzerRecordTree.Id}_{recordGroup.Key}",
                    Children = new List<MeasurementTreeDTO>()
                };
                foreach (var measurementGroup in recordGroup.GroupBy(x => x.MeaslocId))
                {
                    var measurementTree = new MeasurementTreeDTO
                    {
                        Id = measurementGroup.Key,
                        Name = measurementGroup.First().MeaslocName,
                        Children = new List<AnalyzerImageTreeDTO>()
                    };
                    foreach (var imageGroup in measurementGroup.GroupBy(x => x.ImageType))
                    {
                        var analyzerImageTree = new AnalyzerImageTreeDTO
                        {
                            Children = new List<RecordTreeDTO>(),
                            Name = imageGroup.Key,
                            Id = $"{measurementTree.Id}_{imageGroup.Key}"
                        };
                        foreach (var image in imageGroup.OrderByDescending(x => x.RecordTime))
                        {
                            string stringImage = ImageTypeReader.GetImageStringByImageByte(image.Image, image.ImageUrlType);

                            var analyzerRecordDto = new RecordTreeDTO
                            {
                                EigenValueId = image.EigenValueId,
                                Id = image.Id.ToString(),
                                Name = image.RecordTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                Description = image.Description,
                                SampleDataSpeed = Math.Round((image.Avg ?? 0), 3, MidpointRounding.AwayFromZero),
                                Image = $"{stringImage}"
                            };
                            analyzerImageTree.Children.Add(analyzerRecordDto);
                        }
                        measurementTree.Children.Add(analyzerImageTree);
                    }
                    compTree.Children.Add(measurementTree);
                }
                // 对测点名称排序
                compTree.Children.SortByName(ascending: true, dictType: EnumSortType.MeaslocName);
                analyzerRecordTree.Children.Add(compTree);
            }
            // 对实体部件名称排序
            analyzerRecordTree.Children.SortByName(ascending: true, dictType: EnumSortType.CompName);
            return analyzerRecordTree;
        }
        /// <summary>
        /// 保存分析记录
        /// </summary>
        /// <param name="record"></param>
        public void SaveAnalyzerRecord(SaveAnalyzerRecordDTO record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record), "分析记录不能为空");
            }
            if (string.IsNullOrEmpty(record.Image))
            {
                throw new ArgumentNullException(nameof(record.Image), "分析图表不能为空");
            }
            var wtphmRecord = new DeviceDiagnosisAnalyzerRecord();
            AutoMapperHelper.Map(record, wtphmRecord);
            wtphmRecord.CompName = record.CompName;
            wtphmRecord.RecordTime = DateTime.Now;

            // 根据前端的图片传参解析，获取传参
            ImageInfo image = ImageTypeReader.GetByteTypeByImageUri(record.Image);
            wtphmRecord.Image = image.ImageByte;
            wtphmRecord.ImageUrlType = image.ImageType;

            if (record.AcqTime == null || DateTime.TryParse(record.AcqTime, out DateTime acqTime) == false)
                wtphmRecord.AcqTime = null;
            else
                wtphmRecord.AcqTime = acqTime;

            SetWaveDataAvg(wtphmRecord);
            var wtphmDiagnosisConclusionList = new List<DeviceDiagnosisConclusion>();
            foreach (var conclusion in record.DiagnosisConclusions)
            {
                if (string.IsNullOrEmpty(conclusion.CompName))
                {
                    throw new ArgumentNullException(nameof(conclusion), "诊断结论部件名称不能为空");
                }
                wtphmDiagnosisConclusionList.Add(new DeviceDiagnosisConclusion
                {
                    WindturbineId = wtphmRecord.WindturbineId,
                    CompName = conclusion.CompName,
                    DiagnosisConclusion = conclusion.DiagnosisConclusion,
                    MaintainAdvice = conclusion.MaintainAdvice,
                    WarningLevel = conclusion.Status
                });
            }
            _diagnosticReportRepository.SaveAnalyzerRecordAndDiagCollection(wtphmRecord, wtphmDiagnosisConclusionList);
        }


        /// <summary>
        /// 设置分析记录的平均值数据信息
        /// </summary>
        /// <param name="record"></param>
        /// <param name="wtphmRecord"></param>
        public void SetWaveDataAvg(DeviceDiagnosisAnalyzerRecord wtphmRecord)
        {
            try
            {
                wtphmRecord.Avg = 0;
                if (wtphmRecord.AcqTime == null)
                    return;
                DevMeasLocation measLoc = _devTreeRepository.GetMeasLocationByMeaslocID(wtphmRecord.MeaslocId);
                if (measLoc == null)
                    return;

                // 在转速表中获取同时间的转速数据
                List<RotSpdWaveData> rotSpdWaveDatas = dbSelect.GetSPDatas(measLoc.StationID, measLoc.DeviceID, wtphmRecord.AcqTime.Value);
                if (rotSpdWaveDatas != null && rotSpdWaveDatas.Count != 0)
                {
                    var first = rotSpdWaveDatas.FirstOrDefault();
                    if (first == null)
                    {
                        ALog.Debug($"RotSpdWaveDatas 首条记录为 null");
                        return;
                    }
                    var avg = Math.Round(first.AVG, 3, MidpointRounding.AwayFromZero);
                    wtphmRecord.Avg = avg;
                    ALog.Debug($"RotSpdWaveData表中获取{measLoc.DeviceID}，{wtphmRecord.AcqTime.Value}数据为{avg}");
                    return;
                }
                else
                {
                    ALog.Debug($"RotSpdWaveData表中获取{measLoc.DeviceID}，{wtphmRecord.AcqTime.Value}数据为0");
                }
            }
            catch (Exception ex)
            {
                wtphmRecord.Avg = 0;
                ALog.Error(ex, $"SetWaveDataAvg-保存诊断记录时，获取样本数据发电机转速异常");
            }
        }

        /// <summary>
        /// 获取分析描述信息
        /// </summary>
        /// <returns></returns>
        public AnalyzeDescAndDiagConclusion GetLastAnalyzerDescription(string windturbineId, string imageType, string? eigenValueId, string? measlocId)
        {
            string defaultDescription = "未见异常";
            var result = new AnalyzeDescAndDiagConclusion();
            if (imageType.Contains("趋势"))
            {
                var recordList = _diagnosticReportRepository.QueryableList<DeviceDiagnosisAnalyzerRecord>(x => x.WindturbineId == windturbineId && x.ImageType == imageType && x.EigenValueId == eigenValueId);
                if (recordList.Count() != 0)
                {
                    result.AnalyzeDesc = recordList.OrderByDescending(x => x.RecordTime).First().Description;
                }
                else
                {
                    result.AnalyzeDesc = defaultDescription;
                }
            }
            else
            {
                var recordList = _diagnosticReportRepository.QueryableList<DeviceDiagnosisAnalyzerRecord>(x => x.WindturbineId == windturbineId && x.MeaslocId == measlocId);
                if (recordList.Count() != 0)
                {
                    result.AnalyzeDesc = recordList.OrderByDescending(x => x.RecordTime).First().Description;
                }
                else
                {
                    result.AnalyzeDesc = defaultDescription;
                }
            }
            result.DiagnosisConclusions = new DiagnosticConclusion(configuration).GetDiagnosisConclusion(windturbineId);
            return result;
        }
        /// <summary>
        /// 获取分析记录列表
        /// </summary>
        /// <param name="windID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<DeviceDiagnosisAnalyzerRecord> GetAnalyzerRecords(string windID, DateTime startTime, DateTime endTime)
        {
            var list = _diagnosticReportRepository.QueryableList<DeviceDiagnosisAnalyzerRecord>(x => x.WindturbineId == windID && x.RecordTime >= startTime.Date && x.RecordTime < endTime.Date.AddDays(1));
            return list;
        }
        /// <summary>
        /// 获取分析记录图表信息
        /// </summary>
        /// <param name="recordID"></param>
        /// <returns></returns>
        public string GetAnalyzerRecordImage(int recordID)
        {
            DeviceDiagnosisAnalyzerRecord chart = _diagnosticReportRepository.QueryableSingle<DeviceDiagnosisAnalyzerRecord>(x => x.Id == recordID);
            if (chart == null) return string.Empty;

            return ImageTypeReader.GetImageStringByImageByte(chart.Image, chart.ImageUrlType);
            /*string imageString = System.Convert.ToBase64String(chart.Image);
            string imgType = GetImageString(imageString);
            return $"{imgType}{imageString}";*/
        }
        /// <summary>
        /// 获取报告分析记录
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        public List<DiagnosisReportAnalyzerRecordDTO> GetReportAnalyzerRecord(string reportGuid)
        {
            var reportAnalysis = _diagnosticReportRepository.QueryableList<DeviceReportAnalyzerRecord>(x => x.ReportGuid == reportGuid);
            var list = new List<DiagnosisReportAnalyzerRecordDTO>();
            foreach (var item in reportAnalysis)
            {
                var dtoItem = AutoMapperHelper.Map<DeviceReportAnalyzerRecord, DiagnosisReportAnalyzerRecordDTO>(item);

                var turbineAnalyzerRecord = _diagnosticReportRepository.QueryableSingle<DeviceDiagnosisAnalyzerRecord>(x => x.Id == item.AnalyzerRecordId);
                if (turbineAnalyzerRecord != null)
                {
                    //增加返回分析记录图谱记录时间
                    dtoItem.RecordTime = turbineAnalyzerRecord.RecordTime.ToString("yyyy-MM-dd HH:mm:ss");
                }

                dtoItem.Image = ImageTypeReader.GetImageStringByImageByte(item.Image, item.ImageUrlType);

                list.Add(dtoItem);
            }
            return list;
        }

    }
}
