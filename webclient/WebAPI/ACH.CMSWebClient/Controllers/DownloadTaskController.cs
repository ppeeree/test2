using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel.DownloadTask;
using ACH.DataEntity.Common;
using ACH.DataEntity.DownLoad;
using ACH.DataEntity.Enum;
using ACH.Helper.ApiReponse;
using ACH.Helper.Others;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;


namespace ACH.CMSWebClient.Controllers
{
    [ApiController]
    [Route("NetApi/[controller]")]
    [EnableCors("AllowAllOrigins")]
    [ExceptionFilterAttribute]
    public class DownloadTaskController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        private IConfiguration _configuration;
        private DownloadTaskMethods downloadTaskMethods;
        private readonly string webTitle;
        private readonly string webVersion;
        private readonly LogStore _log = LogStore.Instance;

        public DownloadTaskController(IConfiguration configuration)
        {
            _configuration = configuration;
            downloadTaskMethods = new DownloadTaskMethods(configuration);
            webTitle = configuration["webTitle"] ?? "风机混凝土塔筒在线状态监测系统";
            webVersion = configuration["webVersion"] ?? "V2.0.0";
        }

        /// <summary>
        /// 数据打包根目录
        /// </summary>
        private readonly string ZIPRootPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACHDownloadData");


        /// <summary>
        /// 1、获取设备树 - 芯能平台暂不使用，在webClient的HTML页面中使用
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDevTree")]
        public IActionResult GetDevTree()
        {
            List<DownloadDTStationDTO> res = DownloadTaskMethods.GetDevTree();
            return _createReponse.CreateResponse(res);

        }


        /// <summary>
        /// 2、获取测量事件类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetMonitorType")]
        public IActionResult GetMonitorType([FromQuery] string stationID)
        {
            List<KeyValueModel> res = DownloadTaskMethods.GetMonitorType(stationID);
            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        /// 3、获取数据下载保存格式类型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDownloadWaveSaveType")]
        public IActionResult GetDownloadWaveSaveType()
        {
            List<KeyValueModel> res = DownloadTaskMethods.GetDownloadWaveSaveType();
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 3、新增自定义下载
        /// </summary>
        /// <param name="param">前端传参</param>
        /// <returns></returns>
        [HttpPost("AddTask")]
        public IActionResult AddTask([FromBody] AddTaskFromBody param)
        {
            try
            {
                // 每次重新创建自定义下载，清空日志
                _log.Clear();

                List<EnumMonitorType> types = param.measType.Select(s => (EnumMonitorType)Enum.Parse(typeof(EnumMonitorType), s)).ToList();
                EnumDownloadWaveSaveType saveType = Enum.Parse<EnumDownloadWaveSaveType>(param.WaveSaveType);

                var addItem = new DownloadParam(
                   param.stationName,
                   param.windturbineIDs.ToList(),
                   DateTime.Parse(param.startTime),
                   DateTime.Parse(param.endTime),
                   int.Parse(param.waveNum),
                   types,
                   saveType
                );

                // 后端线程实现，接口不等待
                _ = Task.Run(() =>
                {
                    try
                    {
                        // step1：删除上次打包结果 
                        downloadTaskMethods.DeleteLastZIP();

                        // Step2：打包自定义下载数据
                        downloadTaskMethods.WaveSelectDownload(addItem);
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, "后台打包失败");
                    }
                });


                return _createReponse.CreateResponse("", 200, "后台正在打包…");
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new String(""), 500, ex.Message);
            }
        }


        /// <summary>
        /// 4、获取实时打包信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDownloadLogs")]
        public IActionResult GetDownloadLogs()
        {
            return _createReponse.CreateResponse(_log.Snapshot());
        }



        /// <summary>
        /// 5、获取诊断数据的压缩包列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetZipPathList")]
        public IActionResult GetZipPathList([FromQuery] string timeString)
        {
            try
            {
                string[] timeRange = timeString.Split(",");
                DateTime startTime = DateTime.Parse(timeRange[0]);
                DateTime endTime = DateTime.Parse(timeRange[1]);

                List<ZipFileInfoDTO> list = downloadTaskMethods.GetZIPFiles(ZIPRootPath, startTime, endTime);

                return _createReponse.CreateResponse(list);
            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new List<ZipFileInfoDTO>(), 500, ex.Message);
            }
        }


        /// <summary>
        /// 6、删除诊断数据的压缩包
        /// </summary>
        /// <returns></returns>
        [HttpPost("DeleteDayZipList")]
        public IActionResult DeleteDayZipList([FromBody] List<ZipFileInfoDTO> data)
        {

            foreach (var file in data)
            {
                var filePath = file.FilePath;
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            return _createReponse.CreateResponse("");

        }

        /// <summary>
        /// 7、根据地址下载zip
        /// </summary>
        /// <param name="path">zip文件地址</param>
        /// <returns></returns>
        [HttpGet("DownloadZipByPath")]
        public async Task<IActionResult> DownloadZipByPath1([FromQuery] string path)
        {
            try
            {
                // 分块大小
                const int bufferSize = 1024 * 1024;
                await using var fileStream = new FileStream(
                        path, FileMode.Open, FileAccess.Read, FileShare.Read,
                        bufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
                Response.Headers.Add("Content-Disposition", "attachment; filename*=UTF-8''" + Uri.EscapeDataString(System.IO.Path.GetFileName(path)));
                var fileInfo = new FileInfo(path);
                Response.Headers.Add("Content-Length", fileInfo.Length.ToString());
                await fileStream.CopyToAsync(Response.Body, bufferSize);

                return new EmptyResult();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"DownloadZipByPath-根据地址获取压缩包异常");
                return new EmptyResult();
            }
        }


        /// <summary>
        /// 8、下载时间范围内全部数据采集结果
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        [HttpGet("DownloadCSVFile")]
        public IActionResult DownloadCSVFile([FromQuery] string timeString)
        {
            try
            {
                if (_configuration["DownloadCountFile"] == "true")
                {
                    string[] timeRange = timeString.Split(",");
                    DateTime startTime = DateTime.Parse(timeRange[0]);
                    DateTime endTime = DateTime.Parse(timeRange[1]).AddDays(1).AddSeconds(-1);

                    string zipPath = downloadTaskMethods.GetTimeRangeCSVFile(startTime, endTime);
                    if (zipPath != "")
                    {
                        return _createReponse.CreateResponse(zipPath);
                    }
                    else
                    {
                        return _createReponse.CreateResponse("", 200, "当前没有数据统计结果文件，暂不下载！");
                    }
                }
                else
                {
                    // 不统计金风系统的数据采集情况，故也不下载
                    return _createReponse.CreateResponse("");
                }

            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new String(""), 500, $"结果打包报错：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取运维工具版本信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetCMSWebClientInfo")]
        public IActionResult GetCMSWebClientInfo()
        {
            AppInfo res = new AppInfo();
            res.webVersion = webVersion;
            res.webTitleName = webTitle;

            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 运维工具程序信息Info
        /// </summary>
        public class AppInfo
        {
            public string webTitleName { get; set; }
            public string webVersion { get; set; }
        }
    }
}
