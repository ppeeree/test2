using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using System.IO.Compression;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    public class DownloadWaveDataMethods
    {
        IDevTreeRepsitory _devTreeRepository;
        WaveListMethods waveListMethods;
        DBSelect dbSelect;
        private readonly DeviceWaveDBFactory _waveFactory;
        private readonly DeviceWaveDBFactory _waveReadFactory;
        public DownloadWaveDataMethods(IConfiguration _configuration)
        {
            _devTreeRepository = DevTreeRepsitory.Instance;
            waveListMethods = new WaveListMethods(_configuration);
            _waveFactory = new DeviceWaveDBFactory(_configuration);
            dbSelect = new DBSelect(_configuration);
            _waveReadFactory = new DeviceWaveDBFactory(_configuration);
        }

        /// <summary>
        /// 7.1、downloadWaveData接口处理：根据传参获取全部波形数据
        /// </summary>
        /// <param name="downloadList"></param>
        /// <returns></returns>
        public List<WaveDataDTO> GetWaveDataOrigin(List<OriginalWaveformDataRequestDTO> downloadList)
        {
            List<WaveDataDTO> waveDatas = new List<WaveDataDTO>();
            foreach (OriginalWaveformDataRequestDTO item in downloadList)
            {
                if ((string.IsNullOrEmpty(item.acqTime) && string.IsNullOrEmpty(item.measlocId)))
                {
                    continue;
                }
                DateTime acqTime = DateTime.Parse(item.acqTime);

                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(item.measlocId) ?? new();
                List<TWDataBase> data = dbSelect.GetMeasWaveData(measObj, acqTime, null);

                if (data.Count > 0)
                {
                    foreach (var ele in data)
                    {
                        WaveDataDTO obj = waveListMethods.ConvertToWaveDataModel(ele, measObj, item.waveCategory);
                        waveDatas.Add(obj);
                    }
                }
            }

            return waveDatas;
        }

        /// <summary>
        /// 7.2、downloadWaveData接口处理：下载单个文件路径
        /// </summary>
        /// <param name="downloadList"></param>
        /// <returns></returns>
        public string DownloadOneWaveFilePath(List<WaveDataDTO> downloadList)
        {
            try
            {
                var first = downloadList.First();
                string fileName = $"{first.WindParkName}_{first.WindturbineName}_{first.MeaslocName}_{DateTime.Parse(first.Time).ToString("yyyyMMddHHmmss")}_{first.SampleRate}_{HandlerWaveName(first.WaveCategory)}.txt";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                List<double> waveData = GetYElement(first.DataVOS);
                string json = string.Join("\n", waveData);
                System.IO.File.WriteAllText(filePath, json);

                return filePath;

            }
            catch (Exception ex)
            {
                ALog.Error($"原始波形文件下载异常:{ex}");
                return "";
            }
        }


        /// <summary>
        /// 7.3、downloadWaveData接口处理：下载多个文件路径
        /// </summary>
        /// <param name="downloadList"></param>
        /// <returns></returns>
        public string DownloadMoreWaveFilePath(List<WaveDataDTO> downloadList)
        {
            try
            {
                string zipFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"原始波形数据_{DateTime.Now.ToString("yyyyMMddHHmmss")}");
                if (!Directory.Exists(zipFolderPath))
                {
                    Directory.CreateDirectory(zipFolderPath);
                }

                foreach (WaveDataDTO item in downloadList)
                {
                    string fileName = $"{item.WindParkName}_{item.WindturbineName}_{item.MeaslocName}_{DateTime.Parse(item.Time).ToString("yyyyMMddHHmmss")}_{item.SampleRate}_{HandlerWaveName(item.WaveCategory)}.txt";
                    string filePath = Path.Combine(zipFolderPath, fileName);

                    List<double> waveData = GetYElement(item.DataVOS);
                    string json = string.Join("\n", waveData);
                    System.IO.File.WriteAllText(filePath, json);
                    /*string json = JsonConvert.SerializeObject(GetYElement(item.DataVOS), Formatting.Indented);
                    System.IO.FileSystem.WriteAllText(filePath, json);*/
                }

                // 将存储了波形文件的文件夹
                ZipFile.CreateFromDirectory(zipFolderPath, $"{zipFolderPath}.zip", CompressionLevel.Fastest, true);

                Directory.Delete(zipFolderPath, true);

                return $"{zipFolderPath}.zip";
            }
            catch (Exception ex)
            {
                ALog.Error($"原始波形文件下载异常:{ex}");
                return "";
            }
        }



        /// <summary>
        /// 7.5、将字符串转化为double，如果不能转化返回null
        /// </summary>
        /// <param name="input">字符串</param>
        /// <returns></returns>
        public static double? TryParseDouble(string input)
        {
            if (double.TryParse(input, out double result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// 读取Y轴内容
        /// </summary>
        /// <param name="DataVOS"></param>
        /// <returns></returns>
        private static List<double> GetYElement(List<List<double>> DataVOS)
        {
            List<double> res = new List<double>();
            // 遍历DataVOS并取出每个子列表的第二个数字
            foreach (var sublist in DataVOS)
            {
                if (sublist.Count >= 2)
                {
                    double secondValue = sublist[1];
                    res.Add(secondValue);
                }
            }

            return res;
        }


        /// <summary>
        /// 根据波形code获取名称
        /// </summary>
        /// <param name="waveTypeCode"></param>
        /// <returns></returns>
        private string HandlerWaveName(string waveTypeCode)
        {
            Dictionary<string, string> directory = new Dictionary<string, string> {
                {"FreqDomain", "频域" },
                { "TimeDomain","时域"}
            };

            // 查找键
            if (directory.TryGetValue(waveTypeCode, out string value))
            {
                return value;
            }
            else
            {
                return waveTypeCode;
            }
        }


        internal List<string> GetHasDataDay(string[] deviceids, DateTime bgTime, DateTime endTime)
        {
            List<string> dateTimes = new List<string>();
            foreach (var deviceid in deviceids)
            {
                var measLoc = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(deviceid);
                List<MeasEventBase> events = _waveReadFactory.GetMeasEventByDeviceID(measLoc.First().StationID, deviceid, bgTime, endTime);
                foreach (var ev in events)
                {
                    string time = ev.AcqTime.ToString("yyyy-MM-dd");
                    if (dateTimes.Contains(time))
                        continue;
                    dateTimes.Add(time);
                }
            }
            return dateTimes;
        }
    }
}
