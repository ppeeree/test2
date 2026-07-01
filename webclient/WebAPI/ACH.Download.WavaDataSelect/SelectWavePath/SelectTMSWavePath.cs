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
    public class SelectTMSWavePath : IWaveFileSelect
    {
        private readonly IConfiguration _configuration;
        private readonly IDeviceWaveRead _deviceWaveReadTvm;
        private readonly IDeviceWaveRead _deviceWaveReadCvm;
        private readonly EnumMonitorType monitorType = EnumMonitorType.TVM_STE; // 测量事件类型

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configuration"></param>
        public SelectTMSWavePath(IConfiguration configuration)
        {
            _configuration = configuration;
            _deviceWaveReadTvm = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(monitorType);
            _deviceWaveReadCvm = new DeviceWaveDBFactory(_configuration).GetDeviceWaveRead(EnumMonitorType.CVM);
        }


        /// <summary>
        /// 根据传参获取波形地址
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选数量</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measLocs, DateTime bgTime, DateTime endTime, int num)
        {
            ALog.Debug($"开始筛选{deviceID}塔筒结构波形数据，时间范围：{bgTime}到{endTime}，目标数量：{num}组");

            string stationID = measLocs.First().StationID;
            // 获取塔筒的测量事件
            List<MeasEventBase> tvmMeas = _deviceWaveReadTvm.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime).Where(o => o.MeasType == monitorType).ToList();
            if (tvmMeas.Count == 0)
            {
                ALog.Debug($"筛选到塔筒结构的波形为空，返回空列表");
                return new List<string>();
            }

            // 获取传动链的测量事件
            List<MeasEventBase> cvmMeas = _deviceWaveReadCvm.GetMeasEventByDeviceID(stationID, deviceID, bgTime, endTime).Where(o => o.MeasType == EnumMonitorType.CVM).ToList();


            // 该机组时间范围内全部塔筒波形
            List<TWDataBase> data = _deviceWaveReadTvm.GetMeasWaveData(stationID, deviceID, monitorType, bgTime, endTime);


            List<MeasEventBase> selectMeasevent = new List<MeasEventBase>();
            if (cvmMeas.Count > 0)
            {
                ALog.Debug($"有传动链波形索引，根据传动链波形索引来获取塔筒波形索引");
                selectMeasevent.AddRange(SelectTMSWithCMS(cvmMeas, tvmMeas, num));
            }
            else
            {
                ALog.Debug($"没有传动链波形索引，按照塔筒运行状态筛选波形索引");

                selectMeasevent.AddRange(SelectTMS(tvmMeas, num));
            }

            // 若上述均没筛选到测量事件，时间抽样
            if (selectMeasevent.Count == 0)
            {
                selectMeasevent = WaveSelectHelper.HandlerTimeSampling(tvmMeas, num);
            }

            List<string> res = WaveSelectHelper.GetFilePathByMeasevent(data, selectMeasevent);
            ALog.Debug($"{deviceID}塔筒结构根据波形索引获取波形地址完成，共{res.Count}组。");

            return res;
        }

        /// <summary>
        /// 根据传动链数据筛选塔筒数据
        /// </summary>
        /// <param name="cvmMeas">传动链测量事件</param>
        /// <param name="tvmMeas">塔筒测量事件</param>
        /// <param name="num">筛选个数</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<MeasEventBase> SelectTMSWithCMS(List<MeasEventBase> cvmList, List<MeasEventBase> tvmList, int waveNum)
        {
            List<MeasEventBase> cvmSelect = new List<MeasEventBase>();

            cvmList = cvmList.OrderByDescending(n => n.RotSpd).ToList();
            if (cvmList.First().RotSpd != null && cvmList.First().RotSpd != 0)
            {
                // 判定传动链运行，获取转速值大的2*waveNum组数据作为正常数据
                int take = Math.Min(2 * waveNum, cvmList.Count);
                cvmSelect.AddRange(cvmList.Take(take));
            }
            else
            {
                // 判定传动链质量异常，时间抽样作为异常数据
                cvmSelect.AddRange(WaveSelectHelper.HandlerTimeSampling(cvmList, waveNum));
            }

            // 获取对应时间的塔筒数据
            var matchedItems = tvmList.Join(cvmSelect, tvm => tvm.AcqTime.ToString("yyyyMMddHHmmss"), cvm => cvm.AcqTime.ToString("yyyyMMddHHmmss"), (tvm, _) => tvm).ToList();

            return matchedItems;
        }


        /// <summary>
        /// 塔筒数据单独筛选
        /// </summary>
        /// <param name="tvmMeas"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private IEnumerable<MeasEventBase> SelectTMS(List<MeasEventBase> tvmMeas, int num)
        {
            List<MeasEventBase> selectMeasevent = new List<MeasEventBase>();

            // 对数据按照运行状态分组
            List<MeasEventBase> runList = new List<MeasEventBase>();
            List<MeasEventBase> stopList = new List<MeasEventBase>();
            List<MeasEventBase> otherList = new List<MeasEventBase>();
            foreach (var dataItem in tvmMeas)
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
            ALog.Debug($"塔筒数据按运行状态分组完成：运行状态{runList.Count}组，停机状态{stopList.Count}组，其他状态{otherList.Count}组");


            // 数据筛选
            if (runList.Count > 0 || stopList.Count > 0)
            {
                // 有数据质量正常数据 
                selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(runList, num));
                selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(stopList, num));
            }
            else
            {
                // 没有数据质量正常数据 - 时间筛选
                selectMeasevent.AddRange(WaveSelectHelper.HandlerTimeSampling(otherList, num));
            }

            return selectMeasevent;
        }
    }
}
