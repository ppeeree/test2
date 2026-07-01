using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DataEntity.DownLoad;
using ACH.DataEntity.Enum;
using ACH.DataQuality.FileCount;
using ACH.DataRepository.DevTree;
using ACH.DevTree.Entity;
using ACH.Download.DataDownload;
using ACH.Helper.FileSystem;
using Dm.util;

namespace ACH.CMSWebClient.Service
{
    public class DiagDataDownLoad : BackgroundService
    {
        private IConfiguration _configuration;
        /// <summary>
        /// 金风文件存储路径
        /// </summary>
        private string _achDataDir;
        /// <summary>
        /// 定时任务执行的时间间隔-24小时
        /// </summary>
        private static readonly TimeSpan period = TimeSpan.FromHours(24);

        /// <summary>
        /// 定时任务删除时间
        /// </summary>
        private readonly int dayZIPDeleteTime;

        /// <summary>
        /// 数据采集结果统计目录路径
        /// </summary>
        private static string saveCSVFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csvFolder");
        /// <summary>
        /// 自定义下载结果根目录路径
        /// </summary>
        private static string saveZIPFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACHDownloadData");

        /// <summary>
        /// 下载采集服务日志
        /// </summary>
        private DownloadMonitorServerLog downloadMonitorServerLog;
        private DownloadWaveData downloadWaveData;
        private IFileCount gwFileCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public DiagDataDownLoad(IConfiguration configuration)
        {
            _configuration = configuration;
            _achDataDir = configuration["ACHDataDir"];
            downloadWaveData = new DownloadWaveData(_configuration);
            downloadMonitorServerLog = new DownloadMonitorServerLog(_configuration);

            string time = configuration["dayZIPDeleteTime"] ?? "60";
            dayZIPDeleteTime = int.Parse(time);

            gwFileCount = new GWFileCount();
        }


        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ALog.Information("webClient-诊断数据下载服务启动");

            // 等待2min后再执行
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                // 立即执行一次
                DoWork(stoppingToken);

                // 每 24h 循环
                await Task.Delay(period, stoppingToken);
            }
        }


        /// <summary>
        /// 运行执行内容
        /// </summary>
        /// <param name="ct"></param>
        private void DoWork(CancellationToken ct)
        {
            try
            {
                ALog.Debug($"诊断数据打包开始");

                DateTime deleteTime = DateTime.Today.AddDays(-dayZIPDeleteTime);
                // 判定是否生成金风数据采集结果
                if (_configuration["DownloadCountFile"] == "true" && Directory.Exists(_achDataDir))
                {
                    ALog.Debug($"统计金风文件采集结果开始");
                    // step1：删除超出范围的csv文件
                    FileSystemHelper.DeleteOvertimeFileSystemEntries(saveCSVFolder, deleteTime);

                    // step2：统计数据采集结果
                    CountFileStart(saveCSVFolder);
                }

                // step1：删除超出范围的压缩包
                FileSystemHelper.DeleteOvertimeFileSystemEntries(saveZIPFolder, deleteTime);

                // step2：诊断数据打包，每个部件每天下载4组数据
                DownloadParam downloadParam = BuildDiagnosticDownloadParam(4);
                HandlerDiagnosticDownload(downloadParam, saveZIPFolder);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, "DoWork-定时任务异常");
            }
        }


        /// <summary>
        /// 统计数据采集结果
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CountFileStart(string saveFolder)
        {
            ALog.Information($"定时统计采集结果开始");
            try
            {
                // 生成近10天内的数据采集结果
                if (Directory.Exists(_achDataDir))
                {
                    for (int i = 1; i < 11; i++)
                    {
                        DateTime dateTime = DateTime.Now.AddDays(-i);
                        int year = dateTime.Year;
                        string mouth = dateTime.Month.ToString("D2");
                        string day = dateTime.Day.ToString("D2");

                        List<CMSDataStatistics> data = gwFileCount.GetFileCount(_achDataDir, dateTime);
                        if (data.Count == 0)
                        {
                            ALog.Debug($"{dateTime}时间段内的波形文件统计结果为空，不生成CSV文件");
                        }

                        gwFileCount.WriteCSVFile(saveFolder, $"{year}{mouth}{day}.csv", data);
                    }
                }
                else
                {
                    ALog.Information($"数据统计目录不存在: {_achDataDir}");
                }
                ALog.Information($"定时统计采集结果完成");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CountFileStart-数据采集结果统计异常");
            }
        }


        #region 诊断数据下载

        /// <summary>
        /// 1、处理诊断下载传参
        /// </summary>
        /// <param name="downLoadNum">下载个数</param>
        private DownloadParam BuildDiagnosticDownloadParam(int downLoadNum)
        {
            List<DevMeasLocation> measlocList = DevTreeRepsitory.Instance.GetAllMeasLocation();

            List<string> stationNames = measlocList.Select(o => o.StationName).Distinct().ToList();
            List<string> deviceIDs = measlocList.Select(o => o.DeviceID).Distinct().ToList();

            List<EnumMonitorType> measTypes = new List<EnumMonitorType>();
            var groupData = measlocList.GroupBy(o => o.MeasDataType);
            foreach (var item in groupData)
            {
                measTypes.Add(item.Key);
            }

            DateTime createTime = DateTime.Now;
            DateTime endTime = DateTime.Today;
            DateTime bgTime = DateTime.Today.AddDays(-1);

            var stringStationName = string.Join("&", stationNames);
            var stationName = stringStationName.length() > 20 ? $"{stringStationName.Substring(0, 20)}..." : stringStationName;

            return new DownloadParam(
                 stationName,
                 deviceIDs,
                 bgTime,
                 endTime,
                 downLoadNum,
                 measTypes,
                 EnumDownloadWaveSaveType.ACH
              );
        }

        /// <summary>
        /// 2、处理诊断下载
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private void HandlerDiagnosticDownload(DownloadParam param, string saveZIPFolder)
        {
            ALog.Debug("诊断数据打包开始");

            // 数据筛选临时路径
            string temporaryPath = Path.Combine(saveZIPFolder, $"{param.StartTime.ToString("yyyy-MM-dd")}_day");
            try
            {
                // 确保临时目录存在
                if (!Directory.Exists(temporaryPath))
                {
                    Directory.CreateDirectory(temporaryPath);
                    ALog.Debug($"创建临时目录: {temporaryPath}");
                }

                int turbineNum = 0;
                turbineNum = downloadWaveData.GetMeasDataFiles(param, temporaryPath);
                ALog.Debug($"波形数据处理完成");

                ALog.Information($"采集程序日志下载开始");
                downloadMonitorServerLog.DownloadLogs(temporaryPath, param.StartTime, param.EndTime);
                ALog.Information($"采集程序日志下载结束");

                // 使用通用方法检查临时目录是否为空
                if (FileSystemHelper.IsDirectoryEmpty(temporaryPath))
                {
                    ALog.Information($"临时目录为空，跳过打包: {temporaryPath}");
                    return;
                }

                // 压缩包名称
                string zipPath = Path.Combine(saveZIPFolder, $"{param.StationName}_{turbineNum}_{param.StartTime.ToString("yyyyMMdd")}_day.zip");

                // 将文件夹打包
                ZipCryptoHelper.CreateEncryptedZip(temporaryPath, zipPath, true);
                ALog.Debug($"数据压缩完成");

                ALog.Debug($"诊断数据打包完成");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"HandlerDiagnosticDownload-诊断数据打包异常");
            }
            finally
            {
                // 直接清理临时目录
                FileSystemHelper.CleanupTempDirectory(temporaryPath);
            }
        }


        #endregion
    }
}
