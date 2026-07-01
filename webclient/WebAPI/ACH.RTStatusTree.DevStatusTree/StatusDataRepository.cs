using ACH.ACHLog.SeriLog;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DBConn.Dat;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ACH.RTStatusTree.DevStatusTree
{
    public class StatusDataRepository : IStatusRepository
    {
        private IConfiguration configuration;
        private readonly IDevTreeRepsitory devTreeRepsitory = DevTreeRepsitory.Instance;
        private StatusDBContext statusDBContext;
        private static StatusDataRepository _instance;
        private static readonly object instanceLock = new object();
        private IComponentRepository convertRepository = new ComponentRepository();

        /// <summary>
        /// 内存变量：实时特征值列表
        /// </summary>
        private ConcurrentDictionary<string, List<EigenValueData>> _evDataRT;
        /// <summary>
        /// 内存变量：设备实时状态树
        /// </summary>
        private ConcurrentBag<StationItem> _deviceStatusTree = new ConcurrentBag<StationItem>();

        /// <summary>
        /// 构造函数
        /// </summary>
        private StatusDataRepository()
        {
            configuration = new ConfigurationBuilder()
                   .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false).Build();
            statusDBContext = new StatusDBContext(configuration);


            // 读取设备树，初始化设备状态树
            var station = devTreeRepsitory.GetAllMeasLocation().GroupBy(o => o.StationID);
            foreach (var item in station)
            {
                // 获取特征值数据 
                CreatedEVDataRT(item.Key);

                // 初始化站点信息
                CreatedDeviceStatusTree(item.ToList());
            }

        }

        /// <summary>
        /// 单例对象
        /// </summary>
        public static StatusDataRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new StatusDataRepository();
                    }
                }
                return _instance;
            }
        }


        /// <summary>
        /// 读取数据库，获取特征值实时状态列表
        /// </summary>
        /// <returns></returns>
        private void CreatedEVDataRT(string stationID)
        {
            try
            {
                _evDataRT = new ConcurrentDictionary<string, List<EigenValueData>>();

                List<EigenValueData> evData = new List<EigenValueData>();
                List<EigenValueAlarm> evAlarms = new List<EigenValueAlarm>();

                // 从EigenValueAlarm表中获取数据
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    // 查询近XX天的报警数据
                    int RTTime = int.Parse(configuration["RTStatusTreeTime"] ?? "3");
                    DateTime dateTime = DateTime.Now.AddDays(-RTTime);
                    evAlarms = db.Queryable<EigenValueAlarm>().Where(o => o.AcqTime >= dateTime).ToList();
                }

                // evAlarms 按照DeviceID分组
                Dictionary<string, List<EigenValueAlarm>> evAlarmsDic = evAlarms.GroupBy(o => o.DeviceID).ToDictionary(o => o.Key, o => o.ToList());
                foreach (var item in evAlarmsDic)
                {
                    // 按照测量位置分组
                    Dictionary<string, List<EigenValueAlarm>> measLocAlarms = item.Value.GroupBy(o => o.MeasLocID).ToDictionary(o => o.Key, o => o.ToList());
                    foreach (var measLocAlarm in measLocAlarms)
                    {
                        // 每组测量位置取时间最新的一批 
                        List<EigenValueAlarm> alarms = measLocAlarm.Value.OrderByDescending(o => o.AcqTime).ToList();
                        EigenValueAlarm ev = alarms.FirstOrDefault();
                        if (ev != null)
                        {
                            // 添加到特征列表中
                            evData.AddRange(ConvertToEigenValueData(alarms.Where(o => o.AcqTime == ev.AcqTime).ToList()));
                        }
                    }
                }
                foreach (var item in evData)
                {
                    if (!_evDataRT.ContainsKey(item.DeviceID))
                    {
                        _evDataRT.TryAdd(item.DeviceID, new List<EigenValueData>());
                    }
                    _evDataRT[item.DeviceID].Add(item);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CreatedEVDataRT-构建内存中特征值状态列表异常");
            }
        }

        /// <summary>
        /// 数据对象转换：将表中对象转化为RT列表对象
        /// </summary>
        /// <param name="eigenValues">特征值列表</param>
        /// <returns></returns>
        private List<EigenValueData> ConvertToEigenValueData(List<EigenValueAlarm> eigenValues)
        {
            List<EigenValueData> result = new List<EigenValueData>();
            foreach (var item in eigenValues)
            {
                var evName = convertRepository.ConvertEVNameByCode(item.EvCode);
                result.Add(new EigenValueData(item.DeviceID, item.MeasLocID, item.EvCode, $"{item.MeasLocID}&&{item.EvCode}", evName,
                    item.EvStatus, item.AcqTime, item.Unit, item.Value, item.MeasType));
            }

            return result;
        }


        /// <summary>
        /// 构建设备实时状态树
        /// </summary>
        /// <param name="measLocs">测点列表</param>
        private void CreatedDeviceStatusTree(List<DevMeasLocation> measLocs)
        {
            if (measLocs.Count() > 0)
            {
                StationItem? station = InitStation(measLocs);
                if (station != null)
                {
                    _deviceStatusTree.Add(station);
                }
            }
        }


        /// <summary>
        /// 构建某个风场下的设备状态树
        /// </summary>
        /// <param name="measLoc">该风场下测点列表</param>
        /// <returns></returns>
        private StationItem? InitStation(List<DevMeasLocation> measLoc)
        {
            StationItem station = new StationItem();
            station.DeviceList = new List<DeviceItem>();
            station.WindParkId = measLoc.First().StationID;
            station.WindParkName = measLoc.First().StationName;
            station.WindParkCode = "WindPark";

            var deviceGroup = measLoc.GroupBy(o => o.DeviceID);
            foreach (var device in deviceGroup)
            {
                List<EigenValueData> deviceEvs = new List<EigenValueData>();
                if (_evDataRT.ContainsKey(device.Key))
                {
                    _evDataRT.TryGetValue(device.Key, out deviceEvs);
                }
                DeviceItem deviceObj = new DeviceItem();
                deviceObj.WindTurbuineId = device.Key;
                deviceObj.WindTurbuineName = device.First().DeviceName;
                deviceObj.CompList = new List<CompItem>();
                var compGroup = device.GroupBy(o => o.ComponentID);
                foreach (var comp in compGroup)
                {
                    CompItem compObj = new CompItem();
                    compObj.CompId = comp.Key;
                    compObj.CompName = comp.First().ComponentName;
                    compObj.CompCode = comp.First().ComponentID.Replace(device.Key, "");
                    compObj.MeaslocList = new List<MeasLocItem>();
                    var measLocGroup = comp.GroupBy(o => o.MeasLoctionID);
                    // 构建测量位置状态及其挂在特征值
                    foreach (var lco in measLocGroup)
                    {
                        MeasLocItem measLocObj = new MeasLocItem();
                        measLocObj.MeaslocId = lco.Key;
                        measLocObj.MeaslocCode = lco.Key.Replace(device.Key, "");
                        measLocObj.MeaslocName = lco.First().MeasLoctionName;
                        measLocObj.EigenValueList = deviceEvs.Where(o => o.MeasLocID == lco.Key).ToList();
                        // 状态取最高
                        if (measLocObj.EigenValueList.Count > 0)
                        {
                            measLocObj.MeaslocStatus = measLocObj.EigenValueList.Max(o => o.EvStatus);
                            measLocObj.MeaslocStatusTime = measLocObj.EigenValueList[0].EvStatusTime;
                        }
                        compObj.MeaslocList.Add(measLocObj);
                    }
                    compObj.CompStatus = compObj.MeaslocList.Max(o => o.MeaslocStatus);
                    compObj.CompStatusTime = compObj.MeaslocList.Max(o => o.MeaslocStatusTime);
                    deviceObj.CompList.Add(compObj);
                }
                deviceObj.WindTurbuineStatus = deviceObj.CompList.Max(o => o.CompStatus);
                deviceObj.WindTurbuineStatusTime = deviceObj.CompList.Max(o => o.CompStatusTime);
                station.DeviceList.Add(deviceObj);
            }
            return station;
        }


        /// <summary>
        /// post接口实现
        /// </summary>
        /// <param name="alarms">推送的状态数据</param>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        public void SaveEigenValueAlarm(List<EigenValueAlarm> alarms, string stationID, string deviceID)
        {
            try
            {
                // 更新实时特征值
                SaveEigenValueDatas(ConvertToEigenValueData(alarms), stationID, deviceID);

                // 推送的特征值状态数据存储
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    db.Insertable(alarms).ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SaveEigenValueAlarm-存储{alarms[0].DeviceID}状态数据报错");
            }
        }

        /// <summary>
        /// 更新内存中的设备实时状态树
        /// </summary>
        /// <param name="eigenValues">特征值列表</param>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        public void SaveEigenValueDatas(List<EigenValueData> eigenValues, string stationID, string deviceID)
        {
            try
            {
                // 更新内存中的实时特征值列表
                UpdateEvDataRT(eigenValues);

                // 更新内存中设备状态树
                var station = _deviceStatusTree.FirstOrDefault(item => item.WindParkId == stationID);
                if (station == null)
                {
                    // 该风场在实时状态树中不存在，从风场层开始构建
                    CreatedDeviceStatusTree(devTreeRepsitory.GetAllMeasLocation(stationID));
                }
                else
                {
                    // 根据机组ID，从机组层开始更新
                    var device = station.DeviceList.FirstOrDefault(item => item.WindTurbuineId == deviceID);
                    UpdateDeviceStatus(stationID, device, eigenValues);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SaveEigenValueDatas-更新实时特征值报错");
            }
        }

        /// <summary>
        /// 更新内存中特征值状态列表
        /// </summary>
        /// <param name="eigenValues">特征值列表</param>
        private void UpdateEvDataRT(List<EigenValueData> eigenValues)
        {
            List<EigenValueData> updateEigenValues = new List<EigenValueData>();
            foreach (var item in eigenValues)
            {
                if (_evDataRT.ContainsKey(item.DeviceID))
                {
                    // 从内存中对比需要更新的特征值
                    var evData = _evDataRT[item.DeviceID].FirstOrDefault(o => o.EvCode == item.EvCode && o.MeasLocID == item.MeasLocID);
                    if (evData != null)
                    {
                        // 最新数据则更新
                        if (evData.EvStatusTime < item.EvStatusTime)
                        {
                            evData.EvStatus = item.EvStatus;
                            evData.EvStatusTime = item.EvStatusTime;
                            evData.Value = item.Value;
                            updateEigenValues.Add(evData);
                        }
                    }
                    else
                    {
                        // 新增
                        _evDataRT[item.DeviceID].Add(item);
                    }
                }
                else
                {
                    _evDataRT.TryAdd(item.DeviceID, eigenValues);
                }
            }
        }


        /// <summary>
        /// 更新设备状态
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="device">设备层对象</param>
        /// <param name="evDates">特征值列表</param>
        private void UpdateDeviceStatus(string stationID, DeviceItem device, List<EigenValueData> evDates)
        {
            try
            {
                bool isDeviceUpdate = false;
                List<CompItem> upDatecomps = new List<CompItem>();
                foreach (var compItem in device.CompList)
                {
                    // 更新状态
                    bool isUpdate = UpdateDeviceMeasLocStatus(compItem, evDates);
                    if (isUpdate)
                    {
                        compItem.CompStatus = compItem.MeaslocList.Max(o => o.MeaslocStatus);
                        compItem.CompStatusTime = compItem.MeaslocList.Max(o => o.MeaslocStatusTime);
                        upDatecomps.Add(compItem);
                        isDeviceUpdate = true;
                    }
                }
                if (isDeviceUpdate)
                {
                    device.WindTurbuineStatus = device.CompList.Max(o => o.CompStatus);
                    device.WindTurbuineStatusTime = device.CompList.Max(o => o.CompStatusTime);
                    if (upDatecomps.Count > 0)
                    {
                        SaveDeviceStatus(stationID, device, upDatecomps);
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"UpdateDeviceStatus-更新机组、部件状态失败");
            }
        }

        /// <summary>
        /// 更新部件状态
        /// </summary>
        /// <param name="compItem"></param>
        /// <param name="evDates"></param>
        /// <returns></returns>
        private bool UpdateDeviceMeasLocStatus(CompItem compItem, List<EigenValueData> evDates)
        {
            bool isUpdate = false;
            foreach (MeasLocItem measLocItem in compItem.MeaslocList)
            {
                // 更新部件下测点状态
                bool _isUpdate = UpdateMeasLocEvStatus(measLocItem, evDates.Where(o => o.MeasLocID == measLocItem.MeaslocId));
                // 如果测量位置有更新，则标记部件状态为需更新
                if (_isUpdate)
                {
                    isUpdate = _isUpdate;
                }
            }
            return isUpdate;
        }

        /// <summary>
        /// 更新测点状态
        /// </summary>
        /// <param name="measLocItem"></param>
        /// <param name="evs"></param>
        /// <returns></returns>
        private bool UpdateMeasLocEvStatus(MeasLocItem measLocItem, IEnumerable<EigenValueData> evs)
        {
            if (evs.Count() > 0)
            {
                measLocItem.MeaslocStatus = evs.Max(o => o.EvStatus);
                measLocItem.MeaslocStatusTime = evs.Max(o => o.EvStatusTime);
                measLocItem.EigenValueList = evs.ToList();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 同步到实时表中
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="device">机组对象</param>
        /// <param name="upDatecomps">部件状态列表</param>
        private void SaveDeviceStatus(string stationID, DeviceItem device, List<CompItem> upDatecomps)
        {
            try
            {
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    db.Storageable(device).WhereColumns(c => new { c.WindTurbuineId, c.WindTurbuineStatusTime }).ExecuteCommand();

                    // 对部件状态列表去重
                    var distinct = upDatecomps.GroupBy(c => new { c.CompId, c.CompStatusTime }).Select(g => g.First()).ToList();
                    db.Storageable(distinct).WhereColumns(c => new { c.CompId, c.CompStatusTime }).ExecuteCommand();
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SaveDeviceStatus-同步DeviceItem，CompItem表异常");
            }
        }

        /// <summary>
        /// 1、接口实现：获取设备树全部内容
        /// </summary>
        /// <returns></returns>
        public List<DataEntity.Alarm.StationItem> GetStations()
        {
            return _deviceStatusTree.ToList();
        }


        /// <summary>
        /// 2、接口实现：根据风场ID获取设备状态树内容
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        public DataEntity.Alarm.StationItem GetStationsById(string stationId)
        {
            var station = _deviceStatusTree.FirstOrDefault(o => o.WindParkId == stationId);
            // 若设备状态树中不存在该风场下数据，构建
            if (station == null)
            {
                CreatedDeviceStatusTree(devTreeRepsitory.GetAllMeasLocation(stationId));
            }
            return _deviceStatusTree.FirstOrDefault(o => o.WindParkId == stationId);
        }

        /// <summary>
        /// 3、接口实现：根据机组ID获取状态树内容
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <returns></returns>
        public DeviceItem GetDeviceId(string deviceID)
        {
            // 获取机组对应的风场信息
            var measLocs = devTreeRepsitory.GetMeaslocationByDeviceID(deviceID);
            if (measLocs != null && measLocs.Count > 0)
            {
                var station = _deviceStatusTree.FirstOrDefault(o => o.WindParkId == measLocs[0].StationID);
                if (station != null)
                {
                    return station.DeviceList.FirstOrDefault(o => o.WindTurbuineId == deviceID);
                }
            }
            return null;
        }
    }
}
