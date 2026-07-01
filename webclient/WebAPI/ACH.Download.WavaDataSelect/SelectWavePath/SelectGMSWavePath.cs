using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Download.WaveDataSelect
{
    /// <summary>
    /// 筛选法兰数据
    /// </summary>
    public class SelectGMSWavePath : IWaveFileSelect
    {
        private readonly IConfiguration _configuration;
        private readonly DeviceWaveDBFactory _deviceWaveReadFactory;
        private readonly EnumMonitorType monitorType = EnumMonitorType.TVM_FLG_GAP; // 测量事件类型
        public SelectGMSWavePath(IConfiguration configuration)
        {
            _configuration = configuration;
            _deviceWaveReadFactory = new DeviceWaveDBFactory(_configuration);
        }

        /// <summary>
        /// 根据传参筛选波形地址
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选数量</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measLocs, DateTime bgTime, DateTime endTime, int num)
        {
            try
            {
                ALog.Debug($"开始筛选{deviceID}法兰波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");

                string stationID = measLocs.First().StationID;
                List<string> res = new List<string>();

                // 获取全部索力波形
                List<TWDataBase> refData = _deviceWaveReadFactory.GetDeviceWaveRead(EnumMonitorType.TVM_CBF).GetMeasWaveData(stationID, deviceID, EnumMonitorType.TVM_CBF, bgTime, endTime).Where(o => o.MeasLocID.Contains(ConstStr.PL)).ToList();

                // 获取全部法兰波形
                List<TWDataBase> gvmData = _deviceWaveReadFactory.GetDeviceWaveRead(monitorType).GetMeasWaveData(stationID, deviceID, monitorType, bgTime, endTime).OrderBy(o => o.AcqTime).ToList();

                // 索力数据为空，时间抽取
                if (refData.Count != 0)
                {
                    // 根据索力数据筛选法兰
                    res = SelectGVMByFVM(gvmData, refData, num);
                }

                // 上述结果筛选为0，按照时间筛选
                if (res.Count == 0)
                {
                    res = WaveSelectHelper.HandlerGVMTimeSampling(gvmData, num);
                }


                ALog.Debug($"{deviceID}法兰根据波形索引获取波形地址完成，共{res.Count}组。");


                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectWavePath-法兰下载数据筛选报错");
                return new List<string>();
            }
        }




        /// <summary>
        /// 根据索力数据筛选法兰下载数据
        /// </summary>
        /// <param name="gvmData">原始法兰数据</param>
        /// <param name="refData">原始索力波形</param>
        /// <param name="num">下载个数</param>
        /// <returns></returns>
        private List<string> SelectGVMByFVM(List<TWDataBase> gvmData, List<TWDataBase> refData, int num)
        {
            try
            {
                var result = new HashSet<string>();
                // 索力数据筛选结果
                List<TWDataBase> refVibList = refData.GroupBy(o => o.MeasLocID).First().OrderByDescending(o => o.EV).Take(num).ToList();

                if (!gvmData.Any()) return new List<string>();

                // 取索力数据前后5分钟内的波形
                var arr = gvmData.ToArray();
                int n = arr.Length;
                foreach (var r in refVibList)
                {
                    DateTime left = r.AcqTime.AddMinutes(-5);
                    DateTime right = r.AcqTime.AddMinutes(5);

                    int lo = LowerBound(arr, left);
                    int hi = UpperBound(arr, right);

                    for (int i = lo; i < hi; i++)
                    {
                        if (arr[i].WavePath != null)
                        {
                            result.Add(arr[i].WavePath);
                        }
                    }
                }

                return result.ToList();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectGVMByFVM-根据索力数据筛选法兰数据异常，使用时间抽样筛选");
                return WaveSelectHelper.HandlerGVMTimeSampling(gvmData, num);
            }
        }

        #region 二分辅助
        private static int LowerBound(TWDataBase[] arr, DateTime t)
        {
            int l = 0, r = arr.Length;
            while (l < r)
            {
                int m = (l + r) >> 1;
                if (arr[m].AcqTime < t) l = m + 1;
                else r = m;
            }
            return l;
        }

        private static int UpperBound(TWDataBase[] arr, DateTime t)
        {
            int l = 0, r = arr.Length;
            while (l < r)
            {
                int m = (l + r) >> 1;
                if (arr[m].AcqTime <= t) l = m + 1;
                else r = m;
            }
            return l;
        }
        #endregion
    }
}
