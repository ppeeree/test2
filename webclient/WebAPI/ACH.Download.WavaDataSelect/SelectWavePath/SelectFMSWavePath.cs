using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using ACH.DevTree.Entity;

namespace ACH.Download.WaveDataSelect
{
    /// <summary>
    /// 筛选索力数据
    /// </summary>
    public class SelectFMSWavePath : IWaveFileSelect
    {
        private readonly IDeviceWaveRead _deviceWaveRead;
        private readonly IConfiguration _configuration;
        private readonly EnumMonitorType monitorType = EnumMonitorType.TVM_CBF; // 测量事件类型
        public SelectFMSWavePath(IConfiguration configuration)
        {
            _configuration = configuration;
            _deviceWaveRead = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(monitorType);
        }

        /// <summary>
        /// 根据传参筛选波形
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选数量</param>
        /// <returns></returns>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measLocs, DateTime bgTime, DateTime endTime, int num)
        {
            try
            {
                ALog.Debug($"开始筛选{deviceID}索力波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");

                string stationID = measLocs.First().StationID;

                // 获取所有索力波形 - _deviceWaveRead.GetMeasWaveData方法将会获取塔筒和索力的全部波形
                List<TWDataBase> data = _deviceWaveRead.GetMeasWaveData(stationID, deviceID, monitorType, bgTime, endTime).Where(o => o.MeasLocID.Contains(ConstStr.PL)).ToList();
                if (data.Count == 0)
                {
                    ALog.Debug($"{deviceID}，从{bgTime}到{endTime}的索力波形数据为空。没有波形数据，返回空列表");
                    return new List<string>();
                }

                // 索力数据按照测点分组，取第一个测点下的波形进行筛选
                var group = data.GroupBy(o => o.MeasLocID);
                List<TWDataBase> vibList = group.First().ToList();

                // 按照EV值从大->小排序，取前num个
                vibList = vibList.OrderByDescending(o => o.EV).Take(num).ToList();
                ALog.Debug($"索力按照第一个测点的EV值从大->小排序，取前{num}个");


                // 对筛选出的波形取文件地址并返回
                List<string> results = new List<string>();
                foreach (var item in vibList)
                {
                    if (item.WavePath != null)
                    {
                        results.Add(item.WavePath);
                    }
                    else
                    {
                        ALog.Debug($"{item.MeasLocID}表中不存在波形地址");
                    }
                }

                ALog.Debug($"{deviceID}索力根据波形索引获取波形地址完成，共{results.Count}组。");

                return results;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectWavePath-索力下载数据筛选失败");
                return new List<string>();
            }
        }
    }
}
