using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DataEntity.DownLoad;
using ACH.DataEntity.Enum;
using ACH.DataRepository.DevTree;
using ACH.DevTree.Entity;
using ACH.Download.WaveDataSelect;
using ACH.Download.WaveDataSelect.SelectWavePath;
using ACH.Helper.FileSystem;
using ACH.Helper.Others;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ACH.Download.DataDownload
{
    public class DownloadWaveData
    {
        private DownloadMYFormatWaveData downloadMYFormatWaveData;
        IConfiguration _configuration;
        IWaveFileSelect selectCMSWavePath;
        IWaveFileSelect selectBMSWavePath;
        IWaveFileSelect selectTMSWavePath;
        IWaveFileSelect selectFMSWavePath;
        IWaveFileSelect selectGMSWavePath;
        IWaveFileSelect selectBFMWavePath;
        // private readonly int concurrencyNum;
        private readonly LogStore _log = LogStore.Instance;
        public DownloadWaveData(IConfiguration configuration)
        {
            _configuration = configuration;

            selectCMSWavePath = new SelectCMSWavePath(_configuration);
            selectBMSWavePath = new SelectBMSWavePath(_configuration);
            selectTMSWavePath = new SelectTMSWavePath(_configuration);
            selectFMSWavePath = new SelectFMSWavePath(_configuration);
            selectGMSWavePath = new SelectGMSWavePath(_configuration);
            selectBFMWavePath = new SelectTVMBFMWavePath(_configuration);

            downloadMYFormatWaveData = new DownloadMYFormatWaveData();
            // concurrencyNum = int.Parse(configuration["TaskMaxThread"] == null ? "3" : configuration["TaskMaxThread"]);
        }


        /// <summary>
        /// 处理测量事件
        /// </summary>
        /// <param name="param">数据打包传参</param>
        /// <param name="temporaryPath">存储波形的临时目录</param>
        /// <returns></returns>
        public int GetMeasDataFiles(DownloadParam param, string temporaryPath)
        {
            try
            {
                if (!temporaryPath.Contains("_day"))
                {
                    _log.AddLog($"开始处理波形数据");
                }

                int selectDeviceNum = 0; // 筛选到数据的机组个数


                foreach (var deviceID in param.DeviceIDs)
                {
                    ALog.Debug($"{deviceID} 波形筛选开始");
                    if (!temporaryPath.Contains("_day"))
                    {
                        _log.AddLog($"{deviceID}机组波形筛选开始");
                    }

                    // 根据机组ID获取所有测点列表
                    List<DevMeasLocation> measList = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(deviceID);

                    // 查询路径
                    var wavePath = new List<string>();
                    foreach (var type in param.MeasTypes)
                    {
                        switch (type)
                        {
                            case EnumMonitorType.CVM:
                                wavePath.AddRange(selectCMSWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            case EnumMonitorType.BVM:
                                wavePath.AddRange(selectBMSWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            case EnumMonitorType.TVM_STE:
                                wavePath.AddRange(selectTMSWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            case EnumMonitorType.TVM_CBF:
                                wavePath.AddRange(selectFMSWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            case EnumMonitorType.TVM_FLG_GAP:
                                wavePath.AddRange(selectGMSWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            case EnumMonitorType.TVM_BFM:
                                wavePath.AddRange(selectBFMWavePath.SelectWavePath(deviceID, measList, param.StartTime, param.EndTime, param.WaveNum));
                                break;
                            default:
                                ALog.Information($"当前不支持{type}类型筛选");
                                break;
                        }
                    }

                    wavePath = wavePath.Distinct().ToList();

                    ALog.Debug($"{deviceID} 波形筛选完成，共 {wavePath.Count} 条");
                    if (wavePath.Count > 0)
                    {
                        selectDeviceNum++;

                        if (param.SaveType == EnumDownloadWaveSaveType.MY)
                        {
                            // 如果需要下载明阳格式的波形文件则需要读取转化
                            downloadMYFormatWaveData.SaveMYFormatWaveData(measList, wavePath, temporaryPath);
                        }
                        else
                        {
                            // 默认下载众芯格式数据，直接COPY
                            CopyWaveDataToSavePath(deviceID, wavePath, temporaryPath);
                        }

                        if (!temporaryPath.Contains("_day"))
                        {
                            _log.AddLog($"{deviceID}机组数据筛选、COPY完成，共{wavePath.Count}条");
                        }
                    }
                }

                return selectDeviceNum;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasDataFiles-筛选测量事件异常");
                return 0;
            }
        }

        #region 下载众芯格式
        /// <summary>
        /// 标准对象获取并存储
        /// </summary>
        /// <param name="waveList"></param>
        /// <param name="savePath"></param>
        /// <returns></returns>
        private void CopyWaveDataToSavePath(string deviceID, List<string> waveList, string savePath)
        {
            int i = 0;
            int measLength = waveList.Count;
            foreach (string filePath in waveList)
            {
                string saveFilePath = Path.Combine(savePath, deviceID);
                FileSystemHelper.CopyFile(saveFilePath, filePath);
                i++;

                // Thread.Sleep(10);
                ALog.Debug($"{deviceID}波形文件COPY中。进度：{i}/{measLength}");
            }
        }
        #endregion
    }
}
