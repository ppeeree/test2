using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel.WindPark;
using ACH.DataEntity.StatusTree;
using ACH.DBRepository.SD;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;

namespace ACH.CMSWebClient.Controllers
{
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class WindParkController : ControllerBase
    {
        private readonly IAlarmStatusRepository _alarmStatusRepository;
        WindParkAPIMethods _windParkAPIMethods;
        private readonly CreateReponse _createReponse = new CreateReponse();

        public WindParkController(IConfiguration configuration)
        {
            _windParkAPIMethods = new WindParkAPIMethods(configuration);
            _alarmStatusRepository = new AlarmStatusRepository(configuration);
        }

        #region 集控接口
        /// <summary>
        /// 1、获取地图信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet("GetMapInfoByUserID")]
        public IActionResult GetMapInfoByUserID([FromQuery] string userID)
        {
            ControlMapInfoDTO res = _windParkAPIMethods.GetMapInfoByUserID(userID);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 2、获取集控风场状态信息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet("GetStationInfoList")]
        public IActionResult GetStationInfoList([FromQuery] string userID)
        {
            List<ControlStationStatusDTO> res = _windParkAPIMethods.GetStationInfoList(userID);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 3、获取集控页面风场信息
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        [HttpGet("GetStationControlInfo")]
        public IActionResult GetStationControlInfo([FromQuery] string userID)
        {
            ControlStationInfoDTO res = _windParkAPIMethods.GetStationControlInfo(userID);
            return _createReponse.CreateResponse(res);
        }
        #endregion

        #region 风场接口

        /// <summary>
        /// 1、获取机组三维经纬度
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        [HttpGet("GetDeviceCoordinatesList")]
        public IActionResult GetDeviceCoordinatesList([FromQuery] string stationId)
        {
            List<StationDeviceStatusInfoDTO> res = _windParkAPIMethods.GetDeviceCoordinatesList(stationId);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 2、机组监控信息接口
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        [HttpGet("WindParkMonitorNum")]
        public IActionResult WindParkMonitorNumAPI([FromQuery] string stationId)
        {
            StationMonitorInfoDTO res = _windParkAPIMethods.GetWindParkMonitorNum(stationId);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 3、获取风场运维概况接口
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        [HttpGet("WindParkInfo")]
        public IActionResult GetWindParkInfo([FromQuery] string stationId)
        {
            List<StationInfoDTO> res = _windParkAPIMethods.GetWindParkInfo(stationId);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 4、获取机组健康状态 - 风场页面左侧中间的机组健康状态
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        [HttpGet("WindTurbineWeekStatus")]
        public IActionResult WindTurbineWeekStatusAPI([FromQuery] string? stationId, string? userID)
        {
            StationDeviceStatusDTO res = new();
            List<DTStationInfo> stationDataList = _alarmStatusRepository.GetStationsByUserIDOrStationID(stationId, userID);
            if (stationDataList.Any())
            {
                res = _windParkAPIMethods.GetWindTurbineWeekStatusList(stationDataList);
            }

            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 5、机组健康状态发展趋势 - 统计近三个月内该风场所有机组的健康状态 
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("WindTurbineStatusTrend")]
        public IActionResult WindTurbineStatusTrendAPI([FromQuery] string? stationId, string? userID, string startTime, string endTime)
        {
            List<StationDeviceStatusTrendDTO> res = new();
            List<DTStationInfo> stationDataList = _alarmStatusRepository.GetStationsByUserIDOrStationID(stationId, userID);
            if (stationDataList.Any() && DateTime.TryParse(startTime, out DateTime start) && DateTime.TryParse(endTime, out DateTime end))
            {
                res = _windParkAPIMethods.GetWindTurbineTrendList(stationDataList, start, end);
            }

            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 6、部件健康状态统计 - 扇形图 
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("WindParkCompStatusList")]
        public IActionResult WindParkCompStatusListAPI([FromQuery] string? stationId, string? userID, string startTime, string endTime)
        {
            List<StationCompStatusDTO> res = new();
            List<DTStationInfo> stationDataList = _alarmStatusRepository.GetStationsByUserIDOrStationID(stationId, userID);
            if (stationDataList.Any() && DateTime.TryParse(startTime, out DateTime start) && DateTime.TryParse(endTime, out DateTime end))
            {
                res = _windParkAPIMethods.GetCompStatusRingList(stationDataList, start, end);
            }

            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 7、部件故障发展趋势 - 点击部件故障按钮 
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("WindParkCompTrend")]
        public IActionResult WindParkCompTrendAPI([FromQuery] string? stationId, string? userID, string startTime, string endTime)
        {
            StationCompStatusTrendDTO res = new();
            List<DTStationInfo> stationDataList = _alarmStatusRepository.GetStationsByUserIDOrStationID(stationId, userID);
            if (stationDataList.Any() && DateTime.TryParse(startTime, out DateTime start) && DateTime.TryParse(endTime, out DateTime end))
            {
                res = _windParkAPIMethods.GetCompTrendList(stationDataList, start, end);
            }
            return _createReponse.CreateResponse(res, 200);
        }

        /// <summary>
        /// 8、获取机组列表 - 风场页面中间的机组列表 
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        [HttpGet("WindTurbineStatusList")]
        public IActionResult GetWindTurbineStatusList([FromQuery] string stationId)
        {
            List<StationDeviceStatusCardDTO> res = _windParkAPIMethods.GetWindTurbineCardList(stationId);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 8、新设备树接口
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet("AllDeviceList")]
        public IActionResult DeviceList([FromQuery] string userId)
        {
            List<StationInfoTreeDTO> res = _windParkAPIMethods.GetDeviceListByUserId(userId);
            return _createReponse.CreateResponse(res);
        }

        #endregion
    }
}
