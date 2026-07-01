using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.ReportData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;

namespace ACH.CMSWebClient.ControllerImplement.DiagnosticReport
{
    public class DiagnosticTurbine
    {
        IDevTreeRepsitory devTreeRepository = DevTreeRepsitory.Instance;
        private IReportRepository _diagnosticReportRepository = new ReportRepository();
        /*private DiagnosticConclusion diagnosticConclusion;
        private DiagnosticReport diagnosticReport;*/
        private IConfiguration configuration;
        public DiagnosticTurbine(IConfiguration _configuration)
        {
            configuration = _configuration;
            /*diagnosticReport = new DiagnosticReport(configuration);
            diagnosticConclusion = new DiagnosticConclusion(configuration);*/
        }

        /// <summary>
        /// 获取诊断设备详情数据
        /// </summary>
        /// <param name="windTurbuineId"></param>
        /// <returns></returns>
        public DiagnosisWindTurbineDTO GetDiagnosisTurbine(string windTurbuineId)
        {
            var measurement = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(windTurbuineId).FirstOrDefault();
            if (measurement == null)
                return new DiagnosisWindTurbineDTO();

            var device = DevTreeRepsitory.Instance.GetDeviceInfoByID(windTurbuineId);
            if (device == null)
                return new DiagnosisWindTurbineDTO();

            var result = new DiagnosisWindTurbineDTO
            {
                WindParkName = measurement.StationName,
                WindTurbineID = measurement.DeviceID,
                WindTurbineName = measurement.DeviceName,
                WindTurbineType = device.DeviceModel,
                Manufactory = device.DeviceMaker,
                TransmissionFormAndRatio = $"{device.TransmissionForm}_1:{device.TransmissionRatio}" // 传动形式及传动比
            };

            if (device.RatedGeneratorSpeed != null)
                result.RatedGeneratorSpeed = device.RatedGeneratorSpeed.Value;
            else
                result.RatedGeneratorSpeed = 0;

            return result;
        }
        /// <summary>
        /// 获取机组名称
        /// </summary>
        /// <param name="windTurbuineId"></param>
        /// <returns></returns>
        public string GetWindTurbineName(string windTurbuineId)
        {
            var measurement = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(windTurbuineId).FirstOrDefault();
            if (measurement != null)
                return measurement.DeviceName;
            else return "";
        }
        public string GetWindParkName(string windParkID)
        {
            var stationItem = DevTreeRepsitory.Instance.GetAllMeasLocation(windParkID).FirstOrDefault();
            return stationItem == null ? "" : stationItem.StationName;
        }
        /// <summary>
        /// 获取风电场机组树结构数据
        /// </summary>
        /// <param name="windParkID"></param>
        public DiagnosisTurbineTreeDTO GetWindParkTurbineTree(string windParkID, DateTime startTime, DateTime endTime)
        {
            var deviceList = GetWindTurbineList(windParkID);
            if (deviceList == null || deviceList.Count == 0)
                throw new Exception($"风电场{windParkID}不存在");
            var result = new DiagnosisTurbineTreeDTO
            {
                Name = deviceList.First().StationName,
                Id = deviceList.First().StationID
            };
            foreach (var device in deviceList)
            {
                var diagTurbine = GetDiagnosisTurbine(device.DeviceID);
                var trubine = new TurbineDTO
                {
                    WindParkName = device.StationName,
                    //增加状态位，用于前端显示机组状态
                    Status = new DiagnosticConclusion(configuration).GetWindTurbineStatus(device.DeviceID),
                    Id = device.DeviceID,
                    Name = device.DeviceName,
                    Manufactory = diagTurbine.Manufactory,
                    WindTurbineType = diagTurbine.WindTurbineType,
                    TransmissionFormAndRatio = diagTurbine.TransmissionFormAndRatio,
                    SampleDataSpeed = 0,
                    RatedGeneratorSpeed = diagTurbine.RatedGeneratorSpeed,
                    Children = new DiagnosticReport(configuration).GetSimpleDiagnosisReportList(device.DeviceID, startTime, endTime)
                    .Select(x => new WindturbineReportDTO
                    {
                        Id = x.ReportGuid,
                        WindturbineId = x.WindturbineId,
                        CreatedTime = x.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        RuningAdvice = x.RuningAdvice,
                        Status = x.Status,
                        IsCorrelationWindpark = _diagnosticReportRepository.QueryableList<WindParkReportDeviceRelation>(y => y.WindTurbineReportGuid == x.ReportGuid).Count != 0
                    }).ToList()
                };
                result.Children.Add(trubine);
            }
            return result;
        }
        /// <summary>
        /// 获取风电场信息
        /// </summary>
        /// <param name="windParkID"></param>
        public WindParkInfoDTO GetWindParkInfo(string windParkID, List<DevMeasLocation> deviceList)
        {
            StationInfo stationInfo = devTreeRepository.GetStationInfoByID(windParkID);

            if (stationInfo == null || deviceList.Count == 0)
            {
                throw new Exception($"风电场{windParkID}不存在");
            }
            WindParkInfoDTO windParkInfo = new WindParkInfoDTO
            {
                Id = stationInfo.StationId,
                Name = stationInfo.StationName,
                WindParkAddress = stationInfo.StationAddress,
                WindTurbineCount = deviceList.Count,
                WindTurbineType = stationInfo.DeviceModel,
                OperationlDate = stationInfo.OperationlDate.ToString("yyyy-MM-dd HH:mm:ss"),
                TransmissionForm = stationInfo.TransmissionForm,
                DetectionObject = stationInfo.DetectionObject,
                DetectionDevice = "WTPHM-DT",
                DetectionMethod = "在线振动监测",
            };
            return windParkInfo;
        }
        /// <summary>
        /// 获取风电场机组列表
        /// </summary>
        /// <param name="windParkID"></param>
        /// <returns></returns>
        public List<DevMeasLocation> GetWindTurbineList(string windParkID)
        {
            var data = DevTreeRepsitory.Instance.GetAllMeasLocation(windParkID);
            return data.GroupBy(x => x.DeviceID).Select(x => x.First()).ToList();
        }
        /// <summary>
        /// 获取机组部件列表
        /// </summary>
        /// <param name="windturbineId"></param>
        /// <returns></returns>
        public List<DevMeasLocation> GetComponentList(string windturbineId)
        {
            var data = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(windturbineId);
            return data.GroupBy(x => x.ComponentID).Select(x => x.First()).ToList();
        }
    }
}
