using ACH.CMSWebClient.ControllerImplement.Config;
using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.Config;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// 处理测点通道配置表
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class HADUChannelConfigController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        /// <summary>
        /// 1、根据风场ID获取测点配置（一个风场是一套测量方案）
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        [HttpGet("GetMeasIDByStationID")]
        public IActionResult GetMeasIDByStationID([FromQuery] string stationId)
        {
            List<KeyValueModel> res = HADUChannelConfigMethods.GetMeasCodeByStationID(stationId);
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 2、查
        /// </summary>
        /// <param name="current">页数</param>
        /// <param name="size">每页大小</param>
        /// <returns></returns>
        [HttpGet("GetHADUConfigList")]
        public IActionResult GetHADUConfigList([FromQuery] string current, string size)
        {
            // 获取列表中全部信息
            List<HADUConfigList> data = HADUChannelConfigMethods.GetHADUConfigList();

            int pageNumber = int.Parse(current);
            int pageSize = int.Parse(size);
            List<HADUConfigList> res = data.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            int totalCount = data.Count;
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var obj = new TableResult<HADUConfigList>
            {
                Data = res,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageSize = pageSize,
                PageNumber = pageNumber
            };
            return Ok(obj);
        }


        /// <summary>
        /// 3、增
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("AddHADUConfig")]
        public IActionResult AddHADUConfig([FromBody] AddHADUConfigParam param)
        {
            HADUChannelConfigMethods.AddHADUConfig(param);

            return _createReponse.CreateResponse("");
        }

        /// <summary>
        /// 4、更新
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("UpdateHADUConfig")]
        public IActionResult UpdateHADUConfig([FromBody] HADUConfigList param)
        {
            HADUChannelConfigMethods.UpdateHADUConfig(param);
            return _createReponse.CreateResponse("");
        }


        /// <summary>
        /// 5、删除
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpGet("DeleteHADUConfig")]
        public IActionResult DeleteHADUConfig([FromQuery] string measIDs)
        {
            HADUChannelConfigMethods.DeleteHADUConfig(measIDs);
            return _createReponse.CreateResponse("");
        }
    }
}
