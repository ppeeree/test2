using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Download.WaveDataSelect
{
    public class SelectCMSWavePath : IWaveFileSelect
    {
        private readonly IDeviceWaveRead _deviceWaveRead;
        private readonly IConfiguration _configuration;
        private readonly EnumMonitorType monitorType = EnumMonitorType.CVM; // 测量事件类型
        public SelectCMSWavePath(IConfiguration configuration)
        {
            _configuration = configuration;
            _deviceWaveRead = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(monitorType);
        }

        /// <summary>
        /// 根据传参筛选波形，返回波形地址
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选数量</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measLocs, DateTime bgTime, DateTime endTime, int num)
        {
            ALog.Debug($"开始筛选{deviceID}传动链波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");

            string stationID = measLocs.First().StationID;
            // 全部测量事件 
            List<MeasEventBase> meas = _deviceWaveRead.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime).Where(o => o.MeasType == monitorType).ToList();
            if (meas.Count == 0)
            {
                ALog.Debug($"筛选{deviceID}传动链，从{bgTime}到{endTime}获取波形索引为空，认为没有波形文件，返回空列表");

                return new List<string>();
            }

            // 全部波形
            List<TWDataBase> data = _deviceWaveRead.GetMeasWaveData(stationID, deviceID, monitorType, bgTime, endTime);

            // 测量事件筛选结果
            List<MeasEventBase> selectMeasevent = new List<MeasEventBase>();
            selectMeasevent = SelectCMSMeasEvent(num, meas);
            ALog.Debug($"{deviceID}传动链波形索引形筛选完成，目标{num}组，实际{selectMeasevent.Count}组。");


            // 根据测量事件获取波形
            List<string> res = WaveSelectHelper.GetFilePathByMeasevent(data, selectMeasevent);
            ALog.Debug($"{deviceID}传动链根据波形索引获取波形地址完成，共{res.Count}组。");

            return res;
        }

        /// <summary>
        /// 传动链测量事件筛选
        /// </summary>
        /// <param name="num">筛选个数</param>
        /// <param name="meas">传动链测量事件列表</param>
        /// <returns></returns>
        private static List<MeasEventBase> SelectCMSMeasEvent(int num, List<MeasEventBase> meas)
        {
            try
            {
                List<MeasEventBase> selectMeasevent = new List<MeasEventBase>();
                meas = meas.OrderByDescending(o => o.RotSpd).ToList();

                if (meas.First().RotSpd != null && meas.First().RotSpd != 0)
                {
                    // 取转速值最大的num组
                    int take = Math.Min(num, meas.Count);
                    ALog.Debug($"传动链有转速值，取转速值大的{take}组波形索引");
                    selectMeasevent.AddRange(meas.Take(take));
                }
                else
                {
                    ALog.Debug($"传动链没有转速值，时间抽样取波形索引");

                    // 时间抽样
                    selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(meas, num));
                }

                return selectMeasevent;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectBMSMeasEvent-传动链按照转速筛选异常，使用时间抽样");
                return WaveSelectHelper.HandlerTimeSampling(meas, num);
            }
        }
    }
}
