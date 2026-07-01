using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.CMSWebClient.ControllerModel.SystemCheck;
using ACH.DataEntity.Common;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// 新系统自检页面接口
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class SystemCheckController : ControllerBase
    {
        private SystemCheckMethods systemCheckMethod;
        private readonly CreateReponse _createReponse = new CreateReponse();
        public SystemCheckController(IConfiguration _configuration)
        {
            systemCheckMethod = new SystemCheckMethods(_configuration);
        }

        /// <summary>
        /// 1、获取风场下的采集器类型列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        [HttpGet("GetStationMonitorTypeList")]
        public IActionResult GetStationMonitorTypeList([FromQuery] string stationID)
        {
            try
            {
                List<StationMonitorTypeDTO> res = systemCheckMethod.GetStationMonitorTypeList(stationID);

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<StationMonitorTypeDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 2、获取采集单元状态列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        [HttpGet("GetHADUStatusList")]
        public IActionResult GetHADUStatusList([FromQuery] string stationID, string monitorType)
        {
            try
            {
                Enum.TryParse(monitorType, ignoreCase: true, out EnumMonitorType type);
                List<HADUStatusDTO> res = systemCheckMethod.GetHADUStatusList(stationID, type);

                return _createReponse.CreateResponse(res);

            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<HADUStatusDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 3、获取Modbus状态列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        [HttpGet("GetModbusStatusList")]
        public IActionResult GetModbusStatusList([FromQuery] string stationID, string monitorType)
        {
            try
            {
                Enum.TryParse(monitorType, ignoreCase: true, out EnumMonitorType type);
                List<ModbusStatusDTO> res = systemCheckMethod.GetModbusStatusList(stationID, type);

                return _createReponse.CreateResponse(res);

            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<ModbusStatusDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 4、获取采集单元通道状态列表
        /// </summary>
        /// <param name="monitorID">采集器ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        [HttpGet("GetHADUChannelStatusList")]
        public IActionResult GetHADUChannelStatusList([FromQuery] string monitorID, string monitorType)
        {
            try
            {
                Enum.TryParse(monitorType, ignoreCase: true, out EnumMonitorType type);
                List<HADUChannelStatusDTO> res = systemCheckMethod.GetHADUChannelStatusList(monitorID, type);

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<HADUChannelStatusDTO>(), 500, ex.Message);
            }
        }

        /*/// <summary>
        /// 5、获取超声采集器通道状态列表
        /// </summary>
        /// <param name="monitorID"></param>
        /// <param name="monitorType"></param>
        /// <returns></returns>
        [HttpGet("GetUltHADUChannelStatus")]
        public IActionResult GetUltHADUChannelStatus([FromQuery] string monitorID, string monitorType)
        {
            try
            {
                List<UltHADUChannelStatusItem> res = sensorCheckMethod.GetUltHADUChannelStatus(monitorID);

                return new ObjectResult(new ApiResponse<List<UltHADUChannelStatusItem>>
                {
                    Code = 200,
                    Data = res,
                    Message = "操作成功",
                    Success = true
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new ApiResponse<List<UltHADUChannelStatusItem>>
                {
                    Code = 500,
                    Data = new List<UltHADUChannelStatusItem>(),
                    Message = ex.Message,
                    Success = false
                });
            }
        }*/


        /// <summary>
        /// 6、获取趋势
        /// </summary>
        /// <param name="monitorType">采集事件类型</param>
        /// <param name="measlocIDs">测点IDs</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("GetTimeframeMonitorData")]
        public IActionResult GetTimeframeMonitorData([FromQuery] string monitorType, string measlocIDs, string startTime, string endTime)
        {
            try
            {
                Enum.TryParse(monitorType, ignoreCase: true, out EnumMonitorType type);
                List<string> measIDList = measlocIDs.Split(",").ToList();

                TimeframeMonitorDataDTO res = systemCheckMethod.GetTimeframeMonitorData(type, measIDList, DateTime.Parse(startTime), DateTime.Parse(endTime));

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<TimeframeMonitorDataDTO>(), 500, ex.Message);
            }
        }

        /// <summary>
        /// 7、获取BFM回波数据
        /// </summary>
        /// <param name="measlocID">测点ID</param>
        /// <param name="acqTimes">采集时间</param>
        /// <returns></returns>
        [HttpGet("GetBFMEchoWaveData")]
        public IActionResult GetBFMEchoWaveData([FromQuery] string measlocID, string acqTime)
        {
            try
            {
                BFMEchoWaveDTO res = systemCheckMethod.GetBFMEchoWaveData(measlocID, acqTime);

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new BFMEchoWaveDTO(), 500, ex.Message);
            }
        }
    }
}
