using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.DownloadTask;
using ACH.DataEntity.DownLoad;
using ACH.DataEntity.Enum;
using ACH.DataRepository.DevTree;
using ACH.DevTree.Entity;
using ACH.Download.DataDownload;
using ACH.Helper.ApiReponse;
using ACH.Helper.FileSystem;
using ACH.Helper.Others;
using Dm.util;
using Ionic.Zip;
using System.Globalization;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class DownloadTaskMethods
    {
        /// <summary>
        /// 数据采集结果统计目录路径
        /// </summary>
        private static string saveCSVFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csvFolder");
        /// <summary>
        /// 自定义下载结果根目录路径
        /// </summary>
        private static string saveZIPFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ACHDownloadData");
        /// <summary>
        /// 自定义打包日志
        /// </summary>
        private readonly LogStore _log = LogStore.Instance;

        private IConfiguration _configuration;
        private DownloadCSVFile downloadCSVFile;
        private DownloadWaveData downloadWaveData;
        public DownloadTaskMethods(IConfiguration configuration)
        {
            _configuration = configuration;
            downloadCSVFile = new DownloadCSVFile(_configuration);
            downloadWaveData = new DownloadWaveData(_configuration);
        }

        /// <summary>
        /// 1、GetDevTree接口实现：获取全部设备树
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static List<DownloadDTStationDTO> GetDevTree()
        {
            List<DownloadDTStationDTO> res = new List<DownloadDTStationDTO>();

            List<DevMeasLocation> data = DevTreeRepsitory.Instance.GetAllMeasLocation();// DevMeasLocationRepository.GetMeasLocList();
            var stationGroup = data.GroupBy(o => o.StationID);
            foreach (var station in stationGroup)
            {
                DownloadDTStationDTO stationObj = new DownloadDTStationDTO();
                stationObj.StationID = station.First().StationID;
                stationObj.StationName = station.First().StationName;
                stationObj.DeviceList = new List<DownloadDTDeviceDTO>();
                var deviceGroup = station.GroupBy(o => o.DeviceID);
                foreach (var device in deviceGroup)
                {
                    DownloadDTDeviceDTO deviceObj = new DownloadDTDeviceDTO();
                    deviceObj.DeviceID = device.Key;
                    deviceObj.DeviceName = device.First().DeviceName;
                    deviceObj.MeasLocList = new List<DownloadDTMeaslocDTO>();
                    foreach (var measloc in device)
                    {
                        DownloadDTMeaslocDTO meas = new DownloadDTMeaslocDTO();
                        meas.CompName = measloc.ComponentName;
                        meas.CompID = measloc.ComponentID;
                        meas.MeasLocID = measloc.MeasLoctionID;
                        meas.MeasLocName = measloc.MeasLoctionName;
                        deviceObj.MeasLocList.Add(meas);
                    }
                    stationObj.DeviceList.Add(deviceObj);
                }
                res.Add(stationObj);
            }
            return res;
        }


        /// <summary>
        /// 2、接口实现：根据设备树获取数据下载类型
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static List<KeyValueModel> GetMonitorType(string stationID)
        {
            List<KeyValueModel> res = new List<KeyValueModel>();
            List<DevMeasLocation> data = DevTreeRepsitory.Instance.GetAllMeasLocation(stationID);

            var groupData = data.GroupBy(o => o.MeasDataType);
            foreach (var item in groupData)
            {
                KeyValueModel obj = new KeyValueModel
                {
                    Key = item.Key.ToString(),
                    Value = Helper.Others.EnumHelper.GetDescription(item.Key)
                };

                res.Add(obj);
            }
            return res;
        }


        /// <summary>
        /// 3、接口实现：获取波形保存类型
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static List<KeyValueModel> GetDownloadWaveSaveType()
        {
            List<KeyValueModel> res = new List<KeyValueModel>();
            var data = EnumHelper.GetEnumDescriptions<EnumDownloadWaveSaveType>();

            foreach (var item in data)
            {
                KeyValueModel obj = new KeyValueModel
                {
                    Key = ((EnumDownloadWaveSaveType)item.Key).toString(),
                    Value = item.Value
                };

                res.Add(obj);
            }
            return res;
        }


        #region 自定义下载实现
        /// <summary>
        /// 3、接口实现：自定义下载
        /// </summary>
        /// <param name="param"></param>
        internal void WaveSelectDownload(DownloadParam param)
        {
            string taskPath = string.Empty;
            string zipPath = string.Empty;

            try
            {
                ALog.Information($"自定义下载开始");
                _log.AddLog($"自定义下载开始");

                // 使用通用方法创建临时目录
                taskPath = FileSystemHelper.CreateTempDirectory(saveZIPFolder);

                int turbineNum = 0;
                turbineNum = downloadWaveData.GetMeasDataFiles(param, taskPath);
                ALog.Information($"波形文件已全部筛选并COPY到临时目录下");

                // 使用通用方法检查临时目录是否为空
                if (FileSystemHelper.IsDirectoryEmpty(taskPath))
                {
                    _log.AddLog($"未筛选到数据，打包失败");
                    return;
                }

                zipPath = Path.Combine(saveZIPFolder, $"{param.StationName}_{turbineNum}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}_temp.zip");

                // 将文件夹打包
                _log.AddLog($"数据打包压缩开始");
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(60)); // 设置60分钟超时
                ShowPackagingStatus(cts.Token);

                // 添加进度回调
                Action<ZipProgressEventArgs> progressCallback = (e) =>
                {
                    if (e.EventType == ZipProgressEventType.Saving_EntryBytesRead)
                    {
                        // 可以在这里添加更详细的进度信息
                    }
                };

                ZipCryptoHelper.CreateEncryptedZip(taskPath, zipPath, true, progressCallback);
                cts.Cancel();

                ALog.Information($"数据打包压缩完成");
                _log.AddLog($"数据打包压缩完成");

                ALog.Information($"自定义下载完成");
                _log.AddLog($"自定义下载完成");

                // 实时日志返回压缩包路径
                _log.SetPath(zipPath);
            }
            catch (OperationCanceledException ex)
            {
                ALog.Error(ex, $"WaveSelectDownload-打包操作超时");
                _log.AddLog($"打包超时，请重试");
                _log.AddLog($"错误原因: 打包操作超过60分钟，已自动取消");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"WaveSelectDownload-自定义下载异常");
                _log.AddLog($"打包失败，请重试");
                _log.AddLog($"错误原因: {ex.Message}");
            }
            finally
            {
                // 确保临时目录在任何情况下都能被清理
                FileSystemHelper.CleanupTempDirectory(taskPath);
            }
        }


        /// <summary>
        /// 3.1、删除上次自定义打包结果
        /// </summary>
        public void DeleteLastZIP()
        {
            ALog.Debug($"开始删除上次自定义打包结果: {saveZIPFolder}");

            if (!Directory.Exists(saveZIPFolder))
            {
                ALog.Debug($"目标目录不存在，跳过删除: {saveZIPFolder}");
                return;
            }

            // 删除临时zip文件
            int deletedFiles = FileSystemHelper.DeleteFilesByPattern(saveZIPFolder, "*_temp.zip", SearchOption.TopDirectoryOnly);

            // 删除临时目录（命名格式：yyyyMMddHHmmssfff）
            int deletedDirs = FileSystemHelper.DeleteDirectoriesByPredicate(saveZIPFolder, (dir) =>
            {
                string dirName = Path.GetFileName(dir);
                return dirName.Length == 17 && long.TryParse(dirName, out _);
            }, SearchOption.TopDirectoryOnly);

            ALog.Debug($"删除完成: 成功删除{deletedFiles}个临时文件，{deletedDirs}个临时目录");
        }



        /// <summary>
        /// 3.2、压缩包打包时，给页面上展示真实的打包状态信息
        /// </summary>
        /// <param name="token"></param>
        public void ShowPackagingStatus(CancellationToken token)
        {
            _ = Task.Run(async () =>
            {
                DateTime startTime = DateTime.Now;
                int statusIndex = 0;
                string[] statusMessages = {
                    "正在收集文件信息…",
                    "正在准备压缩数据…",
                    "正在进行文件压缩…",
                    "正在加密压缩包…",
                    "正在验证压缩包完整性…"
                };

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        // 显示当前状态和已用时间
                        string elapsedTime = (DateTime.Now - startTime).ToString("mm\\:ss");
                        string currentStatus = statusMessages[statusIndex];
                        _log.AddLog($"{currentStatus} 已用时: {elapsedTime}");

                        // 循环显示不同的状态信息
                        statusIndex = (statusIndex + 1) % statusMessages.Length;

                        await Task.Delay(5000, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // 正常取消，不记录异常
                }
                catch (Exception ex)
                {
                    ALog.Debug($"打包状态显示异常: {ex.Message}");
                }
            }, token);
        }
        #endregion

        /// <summary>
        /// 5、接口实现：对根目录下的所有dat.zip文件进行统计
        /// </summary>
        /// <param name="zipRootPath">压缩包根目录</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<ZipFileInfoDTO> GetZIPFiles(string zipRootPath, DateTime startTime, DateTime endTime)
        {
            List<ZipFileInfoDTO> res = new List<ZipFileInfoDTO>();
            // 获取文件夹内所有文件
            string[] files = Directory.GetFiles(zipRootPath, "*.*", SearchOption.AllDirectories).Where(o => o.EndsWith("day.zip")).ToArray();

            foreach (string file in files)
            {
                ZipFileInfoDTO obj = new ZipFileInfoDTO();
                FileInfo fileInfo = new FileInfo(file);

                string[] fileNames = fileInfo.Name.Split('_');

                string fileTime = fileNames[2].Replace("_day.zip", "");
                DateTime time = DateTime.ParseExact(fileTime, "yyyyMMdd", CultureInfo.InvariantCulture);
                if (time >= startTime && time <= endTime)
                {
                    // 解析zip的文件名，获取时间
                    obj.FileTime = time.ToString("yyyy-MM-dd");
                    obj.FileName = fileInfo.Name;
                    obj.FileMemory = FormatSize(fileInfo.Length);
                    obj.FilePath = file;
                    obj.TurbineNum = fileNames[1];
                    res.Add(obj);
                }
            }

            return res.OrderByDescending(o => o.FileTime).ToList();
        }


        /// <summary>
        /// 5.1、文件大小单位转化
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        private static string FormatSize(long bytes)
        {
            string[] suf = { "B", "KB", "MB", "GB" };
            if (bytes == 0)
                return "0 B";
            var place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 2);
            return $"{num} {suf[place]}";

            /*const double gb = 1024 * 1024 * 1024;
            return $"{Math.Round(bytes / gb, 2):0.00}";*/
        }

        /// <summary>
        /// 6.1、DownloadCSVFile接口实现：获取时间范围内的全部CSV文件，并打包
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal string GetTimeRangeCSVFile(DateTime startTime, DateTime endTime)
        {
            if (!Directory.Exists(saveCSVFolder)) return "";

            // 该次下载的临时文件夹
            string folderPath = Path.Combine(saveCSVFolder, $"{startTime.ToString("yyyyMMdd")}-{endTime.ToString("yyyyMMdd")}数据采集结果");

            // 每次下载前，删除其余的csv压缩包
            DeleteOlderFile(saveCSVFolder, "数据采集结果");

            downloadCSVFile.GetCSVFiles(startTime, endTime, saveCSVFolder, folderPath);

            if (Directory.Exists(folderPath))
            {
                string zipPath = $"{folderPath}.zip";

                // 将folderPath压缩为zipPath
                System.IO.Compression.ZipFile.CreateFromDirectory(folderPath, zipPath);
                Directory.Delete(folderPath, true);
                return zipPath;
            }
            else
            {
                return "";
            }
        }


        /// <summary>
        /// 6.2、删除旧的文件
        /// </summary>
        /// <param name="folderPath">快捷方式生成的zip包存储地址</param>
        /// <param name="fileName">文件名称</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void DeleteOlderFile(string folderPath, string fileName)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    return;
                }
                var dirs = Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories).Where(d => d.Contains(fileName)).ToArray();
                foreach (string dir in dirs)
                {
                    Directory.Delete(dir, true);
                }

                var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Where(f => f.Contains(fileName)).ToArray();
                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"DeleteOlderFile-删除旧文件异常");
            }
        }


    }
}
