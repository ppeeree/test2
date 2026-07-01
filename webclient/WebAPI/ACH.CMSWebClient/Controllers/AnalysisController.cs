using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerImplement.Analysis;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Text;
using System.Web;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// 诊断分析页面相关接口
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class AnalysisController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        DownloadWaveDataMethods downloadWaveDataMethods;
        DeviceTreeMethods deviceTreeMethods;
        private IConfiguration _configuration;
        MeasEventDataMethods measEventDataMethods;


        public AnalysisController(IConfiguration configuration)
        {
            _configuration = configuration;
            downloadWaveDataMethods = new DownloadWaveDataMethods(_configuration);
            deviceTreeMethods = new DeviceTreeMethods(_configuration);
            measEventDataMethods = new MeasEventDataMethods(_configuration);
        }

        /// <summary>
        /// 1、获取风场列表 
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        [HttpGet("DeviceTrees")]
        public IActionResult GetAnalyzerLists([FromQuery] string userId)
        {
            try
            {
                List<DTStationStatusDTO> res = deviceTreeMethods.ConvertToDeviceTrees(userId);

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<DTStationStatusDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 2、专项分析 
        /// </summary>
        /// <param name="groupTrendChartDto">接口传参（机组ID、测点ID列表）</param>
        /// <returns></returns>
        [HttpPost("GroupTrend")]
        public IActionResult GetSpecialAnalysis([FromBody] GroupTrendFromBodyDTO groupTrendChartDto)
        {
            try
            {
                //分组趋势数据封装
                List<GroupTrendChartDTO> res = GroupTrendMethods.GroupTrend(groupTrendChartDto);

                //返回结果
                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<GroupTrendChartDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 4、获取特征值趋势 api/Analysis/EvTrendList
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("EvTrendList")]
        public IActionResult GetEvAnalyzerData([FromBody] EvTrendFromBodyDTO param)
        {
            EvTrendDTO nullRes = new EvTrendDTO()
            {
                UnitX = "",
                UnitY = "",
                EvdataList = new List<EvTrendItemDTO>()
            };
            try
            {
                EvTrendListMethods evTrendListMethods = new EvTrendListMethods(_configuration);
                EvTrendDTO res = evTrendListMethods.ConvertToEvTrend(param);
                if (res == null || res.EvdataList.Count == 0)
                {
                    return _createReponse.CreateResponse(nullRes, 200, "暂无数据！");
                }
                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"EvTrendList-获取特征值趋势异常");
                return _createReponse.CreateResponse(nullRes, 200, "暂无数据！");
            }
        }


        /// <summary>
        /// 5.1、波形列表
        /// </summary>
        /// <param name="endTime">结束时间</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="spdRange">转速范围</param>
        /// <param name="measlocId">测点ID</param>
        /// <returns></returns>
        [HttpGet("MeasEventData")]
        public IActionResult MeasEventData([FromQuery] string endTime, string startTime, string spdRange = "", string? deviceID = null, string measlocId = "", bool detail = false)
        {
            try
            {
                List<MeasEventDTO> res = measEventDataMethods.GetMeasEventData(DateTime.Parse(startTime), DateTime.Parse(endTime), spdRange, deviceID, measlocId, detail);

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<MeasEventDTO>(), 500, ex.Message);
            }
        }

        /// <summary>
        /// 5.2、波形列表 - 获取该部件下波形列表 
        /// </summary>
        /// <param name="acqTime"></param>
        /// <param name="deviceID"></param>
        /// <param name="measlocId"></param>
        /// <returns></returns>
        [HttpGet("MeasEventDetails")]
        public IActionResult WaveDataDetails([FromQuery] string acqTime, string deviceID, string measlocId)
        {
            try
            {
                MeasEventDTO res = measEventDataMethods.GetMeasEventDetails(DateTime.Parse(acqTime), measlocId, deviceID);
                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<MeasEventDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 6、获取波形列表分析
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="measlocId">测点ID</param>
        /// <param name="waveCategory">波形类型</param>
        /// <param name="sampleRate">采样频率</param>
        /// <param name="takeDataVOS"></param>
        /// <param name="dataZoomXValue">X轴方法范围</param>
        /// <param name="takeFilterWaveData">是否稀释</param>
        /// <param name="filteringWaveRange">滤波上下限</param>
        /// <returns></returns>
        [HttpGet("WaveList")]
        public IActionResult GetWaveformData([FromQuery] string? deviceID, string acqTime, string measlocId, string waveCategory, double? sampleRate, bool takeDataVOS, string? dataZoomXValue, bool takeFilterWaveData)
        {
            try
            {
                WaveListMethods waveListMethods = new WaveListMethods(_configuration);
                List<WaveDataDTO> res = waveListMethods.ConvertToWave(deviceID, DateTime.Parse(acqTime), measlocId, waveCategory, sampleRate, takeDataVOS, dataZoomXValue, takeFilterWaveData);
                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<WaveDataDTO>(), 500, ex.Message);
            }
        }

        /// <summary>
        /// 7、获取指定月份有无数据
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="months"></param>
        /// <returns></returns>
        [HttpGet("HasDataDay")]
        public IActionResult HasDataDay([FromQuery] string deviceID, string months)
        {
            string[] deviceids = deviceID.Split(',');

            List<string> res = new List<string>();
            string[] _mouths = months.Split(",");
            foreach (string _mou in _mouths)
            {
                DateTime dateTime = DateTime.Parse(months);
                dateTime = new DateTime(dateTime.Year, dateTime.Month, 1);
                res.AddRange(downloadWaveDataMethods.GetHasDataDay(deviceids, dateTime.AddDays(-7), dateTime.AddDays(38)));
            }

            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        /// 8、下载原始波形文件
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost("DownloadWaveData")]
        public async Task<IActionResult> DownloadWaveData([FromBody] DownloadWaveDataDTO param)
        {
            try
            {
                string downloadPath = "";
                List<WaveDataDTO> res = downloadWaveDataMethods.GetWaveDataOrigin(param.originalWaveformDataRequestList);
                if (res.Count > 1)
                {
                    downloadPath = downloadWaveDataMethods.DownloadMoreWaveFilePath(res);
                    if (!string.IsNullOrEmpty(downloadPath))
                    {
                        await SendFileToResponse(downloadPath, MediaTypeNames.Application.Zip, "原始波形数据.zip");
                    }
                }
                else
                {
                    downloadPath = downloadWaveDataMethods.DownloadOneWaveFilePath(res);
                    if (!string.IsNullOrEmpty(downloadPath))
                    {
                        await SendFileToResponse(downloadPath, MediaTypeNames.Text.Plain, Path.GetFileName(downloadPath));
                    }
                }

                // 文件传输完成后删除文件
                if (!string.IsNullOrEmpty(downloadPath) && System.IO.File.Exists(downloadPath))
                {
                    System.IO.File.Delete(downloadPath);
                }

                return new EmptyResult(); // 返回空结果，因为文件已经通过响应流发送
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new string(""), 500, $"原始波形数据下载异常!{ex}");
            }
        }


        /// <summary>
        /// 8.1、文件流下载处理
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="contentType"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task SendFileToResponse(string filePath, string contentType, string fileName)
        {
            if (!string.IsNullOrEmpty(filePath) && System.IO.File.Exists(filePath))
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    Response.ContentType = contentType;
                    // 对文件名进行URL编码
                    string encodedFileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);
                    Response.Headers.Add("Content-Disposition", $"attachment; filename*={encodedFileName}");

                    await fileStream.CopyToAsync(Response.Body);
                }
            }
        }



        /// <summary>
        /// 9、获取轴承故障频率  BearFaultType:BPFI、FTF、BPFO、BSF
        /// </summary>
        /// <param name="WindturbineID"></param>
        /// <param name="MeasLoctionID"></param>
        /// <param name="AcqTime"></param>
        /// <param name="SampleRate"></param>
        /// <param name="BearFaultFrequencyType"></param>
        /// <returns></returns>
        [HttpGet("GetBearFaultFrequency")]
        public IActionResult GetBearFaultFrequency([FromQuery][Required] string WindturbineID, [FromQuery][Required] string MeasLoctionID, [FromQuery][Required] string AcqTime, [FromQuery][Required] double SampleRate, [FromQuery][Required] string BearFaultFrequencyType)
        {
            try
            {
                // 返回结果
                BearFaultFrequencyTypeVO res = AnalysisMethods.GetBearFaultFrequency(WindturbineID, MeasLoctionID, AcqTime, SampleRate, BearFaultFrequencyType);
                if (res == null)
                {
                    _createReponse.CreateResponse("", 200, "缺少轴承故障频率信息，无法进行标注！");
                }

                return _createReponse.CreateResponse(res);
            }
            catch (Exception ex)
            {

                return _createReponse.CreateResponse(new string(""), 500, $"{ex.Message}");
            }
        }


        /// <summary>
        /// 10、获取转频 RF
        /// </summary>
        /// <param name="WindturbineID"></param>
        /// <param name="MeasLoctionID"></param>
        /// <param name="AcqTime"></param>
        /// <param name="SampleRate"></param>
        /// <returns></returns>
        [HttpGet("GetRotorFrequency")]
        public IActionResult GetRotorFrequency([FromQuery][Required] string WindturbineID, [FromQuery][Required] string MeasLoctionID, [FromQuery][Required] string AcqTime, [FromQuery][Required] double SampleRate)
        {
            try
            {
                // 返回结果
                IActionResult res = AnalysisMethods.getRotorFrequency(WindturbineID, MeasLoctionID, AcqTime, SampleRate);

                return res;
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new string(""), 500, $"{ex.Message}");
            }
        }


        /// <summary>
        /// 11、获取边频 SF
        /// </summary>
        /// <param name="WindturbineID"></param>
        /// <param name="MeasLoctionID"></param>
        /// <param name="AcqTime"></param>
        /// <param name="SampleRate"></param>
        /// <param name="FaultFrequency"></param>
        /// <param name="BearFaultFrequencyType"></param>
        /// <returns></returns>
        [HttpGet("GetSideFrequency")]
        public IActionResult GetSideFrequency([FromQuery][Required] string WindturbineID, [FromQuery][Required] string MeasLoctionID, [FromQuery][Required] string AcqTime, [FromQuery][Required] double SampleRate, [FromQuery][Required] double FaultFrequency, [FromQuery][Required] string BearFaultFrequencyType)
        {
            try
            {
                // 返回结果
                IActionResult res = AnalysisMethods.GetSideFrequency(WindturbineID, MeasLoctionID, AcqTime, SampleRate, FaultFrequency, BearFaultFrequencyType);

                return res;
            }
            catch (Exception ex)
            {

                return _createReponse.CreateResponse(new string(""), 500, $"原始波形数据下载异常!{ex}");
            }
        }


        /// <summary>
        ///  12、获取轴承诊断结果(自诊断)
        /// </summary>
        /// <param name="WindturbineID"></param>
        /// <param name="MeasLoctionID"></param>
        /// <param name="AcqTime"></param>
        /// <param name="SampleRate"></param>
        /// <returns></returns>
        [HttpGet("GetBearingDiagnoseResult")]
        public IActionResult GetBearingDiagnoseResult([FromQuery][Required] string WindturbineID, [FromQuery][Required] string MeasLoctionID, [FromQuery][Required] string AcqTime, [FromQuery][Required] double SampleRate)
        {
            try
            {
                Dictionary<string, bool> BearingDiagnoseResult = AnalysisMethods.getBearingDiagnoseResult(WindturbineID, MeasLoctionID, AcqTime, SampleRate);
                return _createReponse.CreateResponse(BearingDiagnoseResult);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new Dictionary<string, bool>(), 500, ex.Message);
            }
        }
    }
}
