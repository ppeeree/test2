using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.Component;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;
using static ACH.CMSWebClient.ControllerModel.Component.DeviceEvCardStatusDTO;

namespace ACH.CMSWebClient.Controllers
{
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class ComponentController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        private readonly ComponentAPIMethods componentAPIMethods;

        public ComponentController(IConfiguration configuration)
        {
            componentAPIMethods = new ComponentAPIMethods(configuration);
        }

        /// <summary>
        /// 1、部件页面特征值列表 
        /// </summary>
        /// <param name="id">机组ID</param>
        /// <param name="type">部件类型</param>
        /// <returns></returns>
        /*[HttpGet("CompEvList")]
        public IActionResult CompEvCardListAPI([FromQuery] string id, string type)
        {
            if (type == "windturbine")
            {
                DeviceEvCardStatusDTO res = componentAPIMethods.GetTurbineEvCardList(id, type);

                return _createReponse.CreateResponse(res);
            }
            else
            {
                LatestEigenValue res = componentAPIMethods.GetCompEvCardList(id, type);

                return _createReponse.CreateResponse(res);
            }
        }*/


        /// <summary>
        /// NEW 1、部件页面特征值列表 
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="pageCompType">聚合部件Code</param>
        /// <returns></returns>
        [HttpGet("CompEvStatusList")]
        public IActionResult CompEvStatusList([FromQuery] string deviceID, string pageCompType)
        {

            if (pageCompType == "windturbine")
            {
                DeviceEvCardStatusDTO res = componentAPIMethods.GetTurbineEvCardList(deviceID, pageCompType);
                return _createReponse.CreateResponse(res);
            }
            else
            {
                List<EvCardStatusDTO> res = componentAPIMethods.CompEvStatusList(deviceID, pageCompType);

                return _createReponse.CreateResponse(res);
            }
        }

        /// <summary>
        ///  2、获取该部件的状态数据  
        /// </summary>
        /// <param name="entityId">机组ID</param>
        /// <param name="entityCode">部件code</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="startTime"></param>
        /// <returns></returns>开始时间
        [HttpGet("CompStatusTrend")]
        public IActionResult ComponentStatusTrendAPI([FromQuery] string entityId, string entityCode, string endTime, string startTime)
        {
            List<CompStatusTrendDTO> res = componentAPIMethods.GetPageCompStatusList(entityId, entityCode, DateTime.Parse(startTime), DateTime.Parse(endTime));

            return _createReponse.CreateResponse(res);
        }
    }
}
