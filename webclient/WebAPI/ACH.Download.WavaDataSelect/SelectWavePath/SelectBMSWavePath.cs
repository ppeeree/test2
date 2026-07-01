using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DataRepository.DevTree;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Download.WaveDataSelect
{
    public class SelectBMSWavePath : IWaveFileSelect
    {
        private readonly IDeviceWaveRead _deviceWaveRead;
        private readonly IConfiguration _configuration;
        private readonly EnumMonitorType monitorType = EnumMonitorType.BVM; // 测量事件类型

        public SelectBMSWavePath(IConfiguration configuration)
        {
            _configuration = configuration;
            _deviceWaveRead = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(monitorType);
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
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measList, DateTime bgTime, DateTime endTime, int num)
        {
            ALog.Debug($"开始筛选{deviceID}叶片波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");
            List<DevMeasLocation> measLocs = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(deviceID);
            string stationID = measLocs.First().StationID;

            // 获取叶片的测量事件
            List<MeasEventBase> meas = _deviceWaveRead.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime).Where(o => o.MeasType == monitorType).ToList();
            if (meas.Count == 0)
            {
                ALog.Debug($"筛选{deviceID}叶片，从{bgTime}到{endTime}获取波形索引为空，认为没有波形文件，返回空列表");
                return new List<string>();
            }

            // 该机组时间范围内全部叶片波形
            List<TWDataBase> data = _deviceWaveRead.GetMeasWaveData(stationID, deviceID, monitorType, bgTime, endTime);

            List<MeasEventBase> selectMeasevent = SelectBMSMeasEvent(num, meas);
            ALog.Debug($"{deviceID}叶片波形索引形筛选完成，目标{num}组，实际{selectMeasevent.Count}组。");

            List<string> res = WaveSelectHelper.GetFilePathByMeasevent(data, selectMeasevent);
            ALog.Debug($"{deviceID}叶片根据波形索引获取波形地址完成，共{res.Count}组。");

            return res;
        }

        /// <summary>
        /// 叶片测量事件筛选
        /// </summary>
        /// <param name="num">筛选个数</param>
        /// <param name="meas">叶片测量事件列表</param>
        /// <returns></returns>
        private static List<MeasEventBase> SelectBMSMeasEvent(int num, List<MeasEventBase> meas)
        {
            try
            {
                List<MeasEventBase> selectMeasevent = new List<MeasEventBase>();

                // 对数据按照运行状态分组
                List<MeasEventBase> runList = new List<MeasEventBase>();
                List<MeasEventBase> stopList = new List<MeasEventBase>();
                List<MeasEventBase> otherList = new List<MeasEventBase>();
                foreach (var dataItem in meas)
                {
                    switch (dataItem.RunStatus)
                    {
                        case EnumRunStatus.Run:
                            runList.Add(dataItem);
                            break;
                        case EnumRunStatus.Stop:
                            stopList.Add(dataItem);
                            break;
                        default:
                            otherList.Add(dataItem);
                            break;
                    }
                }
                ALog.Debug($"叶片数据按运行状态分组完成：运行状态{runList.Count}组，停机状态{stopList.Count}组，其他状态{otherList.Count}组");

                // 数据筛选
                if (runList.Count > 0 || stopList.Count > 0)
                {
                    // 有运行数据 
                    selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(runList, num));
                    // 有停机数据 
                    selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(stopList, num));
                }
                else
                {
                    // 没有数据质量正常数据 - 时间筛选
                    selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(otherList, num));
                }
                return selectMeasevent;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectBMSMeasEvent-叶片筛选异常，使用时间抽样");
                return WaveSelectHelper.HandlerTimeSampling(meas, num);
            }
        }
    }
}
