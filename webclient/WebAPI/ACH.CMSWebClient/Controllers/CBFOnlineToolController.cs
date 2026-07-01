using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;


namespace WTPHM.TheWeave.WebClient.Controllers
{
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class CBFOnlineToolController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        private IConfiguration _configuration;
        public CBFOnlineToolController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///  CBF配置工具，配置支持(已全部配置机组级，或者配置了部分机组级，部分测点级 或者 全部测点),*测点定义表该风场必须是完成(重要)
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <returns></returns>
        [HttpGet("ExecuteCBFTool")]
        public IActionResult ExecuteCBFTool([FromQuery][Required] string StationID, [FromQuery][Required] double CBF, [FromQuery] string StartTime, [FromQuery] string EndTime)
        {
            string returns = "";
            try
            {   // 执行CBF测点配置工具
                CBFOnlineToolMethods tools = new CBFOnlineToolMethods(_configuration);
                string info = tools.ExecuteCBFTool(StationID, CBF, StartTime, EndTime);

                return _createReponse.CreateResponse(info);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new String(""), 500, ex.Message);
            }
        }
    }
}
