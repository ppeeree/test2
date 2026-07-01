using ACH.ACHLog.SeriLog;
using ACH.BoltAxialForceDB;
using ACH.DBRepository.TheWeave;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.DBRepository.DBSelect
{
    /// <summary>
    /// 根据风场ID和时间确定调用哪个数据库
    /// </summary>
    public class DBSelect
    {
        private readonly IConfiguration _configuration;
        private EvTrendDBFactory _measDataRepo;
        private DeviceWaveDBFactory _deviceWaveFactory;
        private IDeviceEVRead _theWeaveEVRead;
        private IDeviceWaveRead _theWeaveWaveRead;
        private ACHDeviceReadWaveDB achDeviceReadWaveDB;
        private BFMEVTrendDatDB _bfmEvTrendRead;
        private DeviceBFMReadWaveDB _bfmReadWave;

        public DBSelect(IConfiguration configuration)
        {
            _configuration = configuration;
            _measDataRepo = new EvTrendDBFactory(configuration);
            _theWeaveEVRead = new TheWeaveEVRead(_configuration);
            _theWeaveWaveRead = new TheWeaveWaveRead(_configuration);
            _deviceWaveFactory = new DeviceWaveDBFactory(_configuration);
            achDeviceReadWaveDB = new ACHDeviceReadWaveDB(_configuration);
            _bfmEvTrendRead = new BFMEVTrendDatDB(_configuration);
            _bfmReadWave = new DeviceBFMReadWaveDB(_configuration);
        }

        /// <summary>
        /// 根据风场ID在json文件中获取时间节点
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string GetDBTypeByStatinID(string stationID, DateTime endTime)
        {
            string res = "DAT";
            try
            {
                // 读取JSON配置文件
                List<StationTimeSelect> data = new List<StationTimeSelect>();
                string filePath = "StationTime.json";
                string jsonString = System.IO.File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<List<StationTimeSelect>>(jsonString);
                if (data == null)
                {
                    return res;
                }

                // 根据JSON文件中配置的时间节点获取查询的数据库列表
                StationTimeSelect obj = data.Where(o => o.StationID == stationID).FirstOrDefault();
                if (obj != null)
                {
                    DateTime time = DateTime.Parse(obj.Time);
                    // 切换时间之前的数据查询 TheWeave，之后的数据查询 DAT。
                    if (time > endTime)
                    {
                        res = "TheWeave";
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"根据时间节点判断数据库异常，默认查询DAT数据库");
                return res;
            }
        }


        /// <summary>
        /// 获取数据库类型列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <returns></returns>
        private Dictionary<string, List<DateTime>> GetDBTypesByStatinID(string stationID, DateTime timeBegin, DateTime timeEnd)
        {
            Dictionary<string, List<DateTime>> res = new Dictionary<string, List<DateTime>>();
            res.TryAdd("DAT", new List<DateTime>() { timeBegin, timeEnd });

            try
            {
                // 读取JSON配置文件
                List<StationTimeSelect> data = new List<StationTimeSelect>();
                string filePath = "StationTime.json";
                string jsonString = System.IO.File.ReadAllText(filePath);
                data = JsonConvert.DeserializeObject<List<StationTimeSelect>>(jsonString);
                if (data == null)
                {
                    return res;
                }

                StationTimeSelect obj = data.Where(o => o.StationID == stationID).FirstOrDefault();
                if (obj != null)
                {
                    // 风场数据库变更时间点
                    DateTime changeDBTime = DateTime.Parse(obj.Time);

                    if (changeDBTime > timeBegin)
                    {
                        if (timeEnd > changeDBTime)
                        {
                            // 查询两个数据库，重新赋值
                            res = new Dictionary<string, List<DateTime>>();
                            res.TryAdd("TheWeave", new List<DateTime>() { timeBegin, changeDBTime.AddDays(1) });
                            res.TryAdd("DAT", new List<DateTime>() { changeDBTime, timeEnd });
                        }
                        else
                        {
                            // 只查询TheWeave数据库，重新赋值
                            res = new Dictionary<string, List<DateTime>>();
                            res.TryAdd("TheWeave", new List<DateTime>() { timeBegin, timeEnd });
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"根据时间节点判断数据库异常，默认查询DAT数据库");
                return res;
            }
        }

        /// <summary>
        /// 获取特征值趋势
        /// </summary>
        /// <param name="measObj">设备树对象</param>
        /// <param name="evCode">特征值Code</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <param name="minRot">最小转速</param>
        /// <param name="maxRot">最大转速</param>
        /// <returns></returns>
        public List<EigenValueData> GetEigenValueHis(DevMeasLocation measObj, string[] evCode, DateTime timeBegin, DateTime timeEnd, double? minRot = null, double? maxRot = null)
        {
            try
            {
                if (measObj == null ||
                    string.IsNullOrWhiteSpace(measObj.StationID) ||
                    string.IsNullOrWhiteSpace(measObj.DeviceID) ||
                    string.IsNullOrWhiteSpace(measObj.MeasLoctionID))
                {
                    ALog.Debug($"GetEigenValueHis-测点设备树信息不完整，跳过特征值趋势查询");
                    return new List<EigenValueData>();
                }

                List<EigenValueData> evData = new List<EigenValueData>();
                // 获取风场数据库变更时间点
                Dictionary<string, List<DateTime>> DBTimes = GetDBTypesByStatinID(measObj.StationID, timeBegin, timeEnd);

                foreach (var item in DBTimes)
                {
                    evData.AddRange(QueryEigenValueHis(measObj, evCode, item.Value[0], item.Value[1], item.Key, minRot, maxRot));
                }

                if (evData.Count == 0)
                {
                    foreach (var dbType in new[] { "TheWeave", "DAT" })
                    {
                        if (HasQueriedFullRange(DBTimes, dbType, timeBegin, timeEnd))
                        {
                            continue;
                        }

                        ALog.Debug($"GetEigenValueHis-按配置查询无数据，尝试使用{dbType}数据库兜底查询{measObj.DeviceID}，{timeBegin}-{timeEnd}的特征值趋势");
                        List<EigenValueData> fallbackData = QueryEigenValueHis(measObj, evCode, timeBegin, timeEnd, dbType, minRot, maxRot);
                        if (fallbackData.Count != 0)
                        {
                            evData.AddRange(fallbackData);
                            break;
                        }
                    }
                }

                return evData;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetEigenValueHis-根据风场+时间判定数据库类型，获取特征值趋势异常");
                return new List<EigenValueData>();
            }
        }

        private List<EigenValueData> QueryEigenValueHis(DevMeasLocation measObj, string[] evCode, DateTime timeBegin, DateTime timeEnd, string dbType, double? minRot = null, double? maxRot = null)
        {
            if (dbType == "DAT")
            {
                ALog.Debug($"GetEigenValueHis-获取{measObj.DeviceID}，{timeBegin}-{timeEnd}的特征值趋势，使用DAT数据库");
                if (measObj.MeasDataType == DataEntity.Common.EnumMonitorType.TVM_BFM)
                {
                    return _bfmEvTrendRead.GetEigenValueHis(measObj.StationID, measObj.DeviceID, evCode, timeBegin, timeEnd, measObj.MeasLoctionID);
                }

                return _measDataRepo.GetDeviceEVRead(measObj.MeasDataType).GetEigenValueHis(measObj.StationID, measObj.DeviceID, evCode, timeBegin, timeEnd, measObj.MeasLoctionID, minRot, maxRot);
            }

            ALog.Debug($"GetEigenValueHis-获取{measObj.DeviceID}，{timeBegin}-{timeEnd}的特征值趋势，使用TheWeave数据库");
            return _theWeaveEVRead.GetEigenValueHis(measObj.StationID, measObj.DeviceID, evCode, timeBegin, timeEnd, measObj.MeasLoctionID, minRot, maxRot);
        }

        private static bool HasQueriedFullRange(Dictionary<string, List<DateTime>> dbTimes, string dbType, DateTime timeBegin, DateTime timeEnd)
        {
            return dbTimes.Count == 1 &&
                   dbTimes.TryGetValue(dbType, out List<DateTime> range) &&
                   range.Count >= 2 &&
                   range[0] == timeBegin &&
                   range[1] == timeEnd;
        }


        /// <summary>
        /// 获取时间范围内测量事件
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <returns></returns>
        public List<MeasEventBase> GetMeasEventByDeviceID(DevMeasLocation measObj, DateTime timeBegin, DateTime timeEnd)
        {
            try
            {
                List<MeasEventBase> measEvents = new List<MeasEventBase>();
                Dictionary<string, List<DateTime>> DBTimes = GetDBTypesByStatinID(measObj.StationID, timeBegin, timeEnd);

                foreach (var item in DBTimes)
                {
                    if (item.Key == "DAT")
                    {
                        ALog.Debug($"GetMeasEventByDeviceID-获取{measObj.DeviceID}，{item.Value[0]}-{item.Value[1]}的测量事件，使用DAT数据库");
                        // 查询DAT数据库
                        if (measObj.MeasDataType == DataEntity.Common.EnumMonitorType.TVM_BFM)
                        {
                            measEvents.AddRange(_bfmReadWave.GetMeasEventByDeviceID(measObj.StationID, measObj.DeviceID, item.Value[0], item.Value[1]));
                        }
                        else
                        {
                            measEvents.AddRange(_deviceWaveFactory.GetMeasEventByDeviceID(measObj.StationID, measObj.DeviceID, item.Value[0], item.Value[1]));
                        }
                    }
                    else
                    {
                        // 查询theweave数据库
                        ALog.Debug($"GetMeasEventByDeviceID-获取{measObj.DeviceID}，{item.Value[0]}-{item.Value[1]}的测量事件，使用TheWeave数据库");
                        measEvents.AddRange(_theWeaveWaveRead.GetMeasEventByDeviceID(measObj.StationID, measObj.DeviceID, item.Value[0], item.Value[1]));
                    }
                }

                return measEvents;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasEventByDeviceID-根据风场+时间判定数据库类型，获取测量事件异常");
                return new List<MeasEventBase>();
            }
        }


        /// <summary>
        /// 获取某个时间点的波形数据 
        /// </summary>
        /// <param name="measObj">设备树对象</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="sampleRate">采样频率</param>
        /// <returns></returns>
        public List<TWDataBase> GetMeasWaveData(DevMeasLocation measObj, DateTime acqTime, double? sampleRate = null)
        {
            try
            {
                List<TWDataBase> data = new List<TWDataBase>();

                string dbType = GetDBTypeByStatinID(measObj.StationID, acqTime);
                ALog.Debug($"GetMeasWaveData-获取{measObj.MeasLoctionID}-{acqTime}的波形信息，使用{dbType}数据库");

                if (dbType == "DAT")
                {
                    if (measObj.MeasDataType == DataEntity.Common.EnumMonitorType.TVM_BFM)
                    {
                        data = _bfmReadWave.GetMeasWaveData(measObj.StationID, measObj.DeviceID, measObj.MeasLoctionID, acqTime);
                    }
                    else
                    {
                        data = _deviceWaveFactory.GetDeviceWaveRead(measObj.MeasDataType).GetMeasWaveData(measObj.StationID, measObj.DeviceID, measObj.MeasDataType, measObj.MeasLoctionID, measObj.SignalType, acqTime, sampleRate);
                    }
                }
                else
                {
                    data = _theWeaveWaveRead.GetMeasWaveData(measObj.StationID, measObj.DeviceID, measObj.MeasDataType, measObj.MeasLoctionID, measObj.SignalType, acqTime, sampleRate);
                }

                return data;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasWaveData-根据风场+时间判定数据库类型，获取波形异常");
                return new List<TWDataBase>();
            }
        }


        /// <summary>
        /// 获取某个时间点的转速数据
        /// </summary>
        /// <param name="windParkID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <returns></returns>
        public List<RotSpdWaveData> GetSPDatas(string windParkID, string deviceID, DateTime acqTime)
        {
            try
            {
                List<RotSpdWaveData> data = new List<RotSpdWaveData>();
                string dbType = GetDBTypeByStatinID(windParkID, acqTime);
                ALog.Debug($"GetMeasWaveData-获取{deviceID}-{acqTime}的转速波形，使用{dbType}数据库");

                if (dbType == "DAT")
                {
                    data = achDeviceReadWaveDB.GetSPDatas(windParkID, deviceID, acqTime);
                }
                else
                {
                    data = _theWeaveWaveRead.GetSPDatas(windParkID, deviceID, acqTime);
                }

                return data;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetSPDatas-根据风场+时间判定数据库类型，获取转速波形异常");
                return new List<RotSpdWaveData>();
            }
        }
    }
}
