using ACH.ACHLog.SeriLog;
using ACH.BoltAxialForceDB;
using ACH.DevTree.Entity;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Download.WaveDataSelect.SelectWavePath
{
    /// <summary>
    /// 筛选螺栓波形文件
    /// </summary>
    public class SelectTVMBFMWavePath : IWaveFileSelect
    {
        private DeviceBFMReadWaveDB _bfmReadWave;
        public SelectTVMBFMWavePath(IConfiguration configuration)
        {
            _bfmReadWave = new DeviceBFMReadWaveDB(configuration);
        }

        /// <summary>
        /// 根据传参获取螺栓轴力波形地址
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选数量</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measLocs, DateTime bgTime, DateTime endTime, int num)
        {
            ALog.Debug($"开始筛选{deviceID}塔筒法兰螺栓波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");

            string stationID = measLocs.First().StationID;
            // 获取塔筒超声螺栓的测量事件
            List<MeasEventBase> bfmMeas = _bfmReadWave.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime);

            if (bfmMeas.Count == 0)
            {
                ALog.Debug($"筛选{deviceID}塔筒法兰螺栓，从{bgTime}到{endTime}获取波形索引为空，认为没有波形文件，返回空列表");
                return new List<string>();
            }

            // 全部轴力波形
            List<TWDataBase> data = _bfmReadWave.GetMeasWaveData(stationID, deviceID, bgTime, endTime);

            // 测量事件筛选结果
            ALog.Debug($"螺栓轴力按照时间抽样取波形索引");
            List<MeasEventBase> selectMeasevent = WaveSelectHelper.HandlerTimeSampling(bfmMeas, num);
            ALog.Debug($"{deviceID}法兰螺栓波形索引形筛选完成，目标{num}组，实际{selectMeasevent.Count}组。");

            List<string> res = WaveSelectHelper.GetFilePathByMeasevent(data, selectMeasevent);
            ALog.Debug($"{deviceID}法兰螺栓根据波形索引获取波形地址完成，共{res.Count}组。");

            return res;
        }



        /// <summary>
        /// 筛选螺栓轴力测量事件
        /// </summary>
        /// <param name="num">筛选个数</param>
        /// <param name="bfmMeas">全部波形索引</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        /*private List<MeasEventBase> SelectBFMMeasEvent(int num, List<MeasEventBase> bfmMeas)
        {
            try
            {
                ALog.Debug($"螺栓轴力按照时间抽样取波形索引");
                return WaveSelectHelper.HandlerTimeSampling(bfmMeas, num);
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SelectBFMMeasEvent-筛选螺栓轴力测量事件发生异常，使用时间抽样");
                return WaveSelectHelper.HandlerTimeSampling(bfmMeas, num);
            }
        }*/
    }
}
