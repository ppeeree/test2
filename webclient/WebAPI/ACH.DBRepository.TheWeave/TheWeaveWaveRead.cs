using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Common;
using ACH.DataEntity.Enum;
using ACH.DataEntity.TheWeave;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.DBRepository.TheWeave
{
    public class TheWeaveWaveRead : IDeviceWaveRead
    {
        ITheWeaveRepository theweaveRepository;
        CommonMethods commonMethods;
        private static IConfiguration _configuration;
        public TheWeaveWaveRead(IConfiguration configuration)
        {
            _configuration = configuration;
            theweaveRepository = new TheWeaveRepository(_configuration);
            commonMethods = new CommonMethods(_configuration);
        }

        /// <summary>
        /// 获取测量事件
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<MeasEventBase> GetMeasEventByDeviceID(string stationID, string deviceID, DateTime timeBegin, DateTime timeEnd)
        {
            List<MeasEventBase> res = new List<MeasEventBase>();
            try
            {
                // 获取theweave数据库的测量事件
                List<MeasDataEvent> measDataEvents = theweaveRepository.GetMeasDataEvent(deviceID, timeBegin, timeEnd, null, null);

                foreach (var item in measDataEvents)
                {
                    MeasEventBase obj = new MeasEventBase();
                    obj.StationID = stationID;
                    obj.DeviceID = deviceID;
                    obj.AcqTime = item.AcqTime;
                    obj.MeasType = HandlerMeasType(item.CompType);
                    obj.Count = item.Count;
                    obj.RotSpd = item.RotSpd;
                    obj.Power = item.Power;
                    obj.RunStatus = EnumRunStatus.Unknow; // 是否需要根据直流分量计算状态
                    obj.workConditionData = new List<WorkCondData>(); //
                    res.Add(obj);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"TheWeave-GetMeasEventByDeviceID-获取测量事件异常");
            }
            return res;
        }

        /// <summary>
        /// 根据部件类型转化为测量事件类型
        /// </summary>
        /// <param name="compType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private EnumMonitorType HandlerMeasType(EnumCompCategory compType)
        {
            switch (compType)
            {
                case EnumCompCategory.CVM:
                    return EnumMonitorType.CVM;
                case EnumCompCategory.BVM:
                    return EnumMonitorType.BVM;
                case EnumCompCategory.TVM:
                    return EnumMonitorType.TVM_STE;
                case EnumCompCategory.FLG:
                    return EnumMonitorType.TVM_FLG_GAP;
                case EnumCompCategory.CBF:
                    return EnumMonitorType.TVM_CBF;
                default:
                    return EnumMonitorType.CVM;
            }
        }



        /// <summary>
        /// 获取波形数据，返回结果带有详细波形信息
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TWDataBase> GetMeasWaveData(string stationID, string deviceID, EnumMonitorType monitorType, string measLocID, DataEntity.Common.EnumSignalType signalType, DateTime acqTime, double? sampleRate = null)
        {
            List<TWDataBase> res = new List<TWDataBase>();
            // 查询TheWeave数据库获取波形数据
            List<DataEntity.TheWeave.MeasWaveData> waveData = theweaveRepository.GetMeasWaveData(deviceID, measLocID, acqTime, sampleRate);

            foreach (DataEntity.TheWeave.MeasWaveData waveObj in waveData)
            {
                // 根据地址读取波形
                var item = commonMethods.GetMeasWaveData(waveObj);

                TWDataBase obj = new TWDataBase();
                obj.DeviceID = deviceID;
                obj.ComponentID = item.CompId;
                obj.MeasLocID = measLocID;
                obj.SampleRate = item.SampleRate;
                obj.AcqTime = item.AcqTime;
                obj.WaveData = Array.ConvertAll(item.WaveData, d => (float)d);
                obj.WavePath = item.WavePath;
                obj.SamplePoint = item.SamplePoint;
                obj.Chno = 0; // 
                obj.EV = 0.0; // 

                res.Add(obj);
            }
            return res;
        }

        /// <summary>
        /// 获取波形数据，返回结果不带有详细波形信息 
        /// 应用场景：数据打包时，根据测量事件对数据筛选。但在获取历史波形的需求下不常用
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="monitorType">测量事件类型</param>
        /// <param name="measLocID">测点ID</param>
        /// <param name="signalType">信号类型</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="sampleRate">采样频率</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TWDataBase> GetMeasWaveData(string stationID, string deviceID, EnumMonitorType monitorType, DateTime startTime, DateTime endTime)
        {
            List<TWDataBase> res = new List<TWDataBase>();
            // 查询TheWeave数据库获取波形数据
            List<DataEntity.TheWeave.MeasWaveData> waveData = theweaveRepository.GetMeasWaveData(deviceID, startTime, endTime);

            foreach (var item in waveData)
            {
                TWDataBase obj = new TWDataBase();
                obj.DeviceID = deviceID;
                obj.ComponentID = item.CompId;
                obj.MeasLocID = obj.MeasLocID;
                obj.SampleRate = item.SampleRate;
                obj.AcqTime = item.AcqTime;
                obj.WaveData = new float[0];
                obj.WavePath = item.WavePath;
                obj.SamplePoint = item.SamplePoint;
                obj.Chno = 0; // 
                obj.EV = 0.0; // 

                res.Add(obj);
            }
            return res;
        }


        /// <summary>
        /// 获取振动波形数据
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<TWVibData> GetTWVibDatas(string stationID, string deviceID, DateTime acqTime)
        {
            ALog.Information($"TheWeave-GetTWVibDatas-获取振动波形数据暂未使用，未实现");
            return new List<TWVibData>();
        }


        /// <summary>
        /// 获取该机组采集时间点内的转速数据
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="deviceID"></param>
        /// <param name="acqTime"></param>
        /// <returns></returns>
        public List<RotSpdWaveData> GetSPDatas(string stationID, string deviceID, DateTime acqTime)
        {
            List<RotSpdWaveData> res = new List<RotSpdWaveData>();

            // 从TheWeave数据库中获取原始波形数据
            List<PulseWaveData> data = theweaveRepository.GetPulseWaveData(deviceID, acqTime);

            // 将PulseWaveData对象转化为RotSpdWaveData对象
            foreach (PulseWaveData ite in data)
            {
                // 根据地址读取波形
                PulseWaveData item = commonMethods.GetMeasWaveData(ite);

                // 将波形的double[]转化为float[]
                float[] waveData = Array.ConvertAll(item.WaveData, d => (float)d);

                RotSpdWaveData obj = new RotSpdWaveData();
                obj.DeviceID = item.DeviceID;
                obj.ComponentID = item.ComponentID;
                obj.MeasLocID = item.MeasLocID;
                obj.SamplePoint = item.SamplePoint;
                obj.SingleType = DataEntity.Common.EnumSignalType.RPM; // 可以固定为RPM
                obj.AcqTime = item.AcqTime;
                obj.WaveData = waveData;
                obj.WavePath = item.WavePath;
                obj.SensorAlarm = item.SensorAlarm;
                obj.DataQuality = EnumDataQuality.Unknow;
                obj.MAX = waveData.Max();
                obj.MIN = waveData.Min();
                obj.AVG = waveData.Average();
                obj.MImplus = item.MImplus;
                obj.Type = item.Type;
                res.Add(obj);
            }
            return res;
        }
    }
}
