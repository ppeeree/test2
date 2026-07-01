using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.StatusTree;
using ACH.DataRepository.DevTree;
using ACH.DBConn.Dat;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ACH.RTStatusTree.HADUStatusTree
{
    /// <summary>
    /// 通道状态树构建
    /// </summary>
    public class HADUStatusRepository : IHADUStatusRepository
    {
        private IConfiguration configuration;
        private readonly IDevTreeRepsitory devTreeRepsitory = DevTreeRepsitory.Instance;
        private StatusDBContext statusDBContext;
        private static HADUStatusRepository _instance;
        private static readonly object instanceLock = new object();
        /// <summary>
        /// 设置定时器
        /// </summary>
        private Timer _hourlyTimer;
        /// <summary>
        /// 0=未启动，1=已启动（用原子操作防止并发重复启动）
        /// </summary>
        private int _timerStarted = 0;
        /// <summary>
        /// 内存变量：数据库中的实时通道状态列表
        /// </summary>
        private ConcurrentDictionary<string, List<ChannelStatusAlarm>> _dbChannelStatusList = new ConcurrentDictionary<string, List<ChannelStatusAlarm>>();
        /// <summary>
        /// 内存变量：采集器状态设备树
        /// </summary>
        // private ConcurrentBag<HADUStatusItem> _HADUStatusTree = new ConcurrentBag<HADUStatusItem>();
        private ConcurrentDictionary<string, HADUStatusItem> _HADUStatusTree = new ConcurrentDictionary<string, HADUStatusItem>();


        /// <summary>
        /// 构造函数
        /// </summary>
        public HADUStatusRepository()
        {
            configuration = new ConfigurationBuilder()
                   .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false).Build();
            statusDBContext = new StatusDBContext(configuration);

            // 获取设备树中的风场，读取数据库
            var station = devTreeRepsitory.GetAllMeasLocation().GroupBy(o => o.StationID);
            foreach (var item in station)
            {
                GetChannelStatusListByDB(item.Key);
            }
            // 构建采集器状态
            CreatedSensorStatusTree();
        }


        /// <summary>
        /// 单例对象
        /// </summary>
        public static HADUStatusRepository Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (_instance == null)
                    {
                        _instance = new HADUStatusRepository();
                    }
                }
                return _instance;
            }
        }


        /// <summary>
        /// 初始化：获取数据库数据构建采集器状态树
        /// </summary>
        public void CreatedSensorStatusTree()
        {
            _HADUStatusTree.Clear();
            try
            {
                // 构建HADU的RT树
                CreatedHADUStatus();

                // 构建倾角晃度仪的RT树
                CreatedTVMSTEStatus();

                // 构建超声采集单元RT树
                CreatedBFMStatus();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CreatedHADUStatusTree-初始化设备状态异常");
            }
        }



        #region 根据不同采集器类型构建RT树
        /// <summary>
        /// 1、根据HADU列表构建RT树
        /// </summary>
        private void CreatedHADUStatus()
        {
            // 获取表中配置的HADU信息
            List<HADUInfo> HADUInfoList = devTreeRepsitory.GetHADUSList();

            // 遍历采集器列表
            foreach (HADUInfo haduItem in HADUInfoList)
            {
                HADUStatusItem HADUObj = new HADUStatusItem();
                HADUObj.DeviceID = haduItem.HADUID;
                HADUObj.IP = haduItem.HADUIP;
                HADUObj.CollectorType = haduItem.MonitorType;
                HADUObj.ChannelStatusList = new List<ChannelStatusItem>();

                // 遍历采集器下通道列表
                foreach (HADUChannelInfo channelItem in haduItem.Channels)
                {
                    // 由于将塔顶和塔基的自检区分出去，故在此统计时将不统计塔顶和塔基
                    if (channelItem.MeasLocID.Contains("TOWTOP") || channelItem.MeasLocID.Contains("TOWFDN"))
                    {
                        continue;
                    }

                    // 根据测点ID获取设备树对象
                    DevMeasLocation measloc = devTreeRepsitory.GetMeasLocationByMeaslocID(channelItem.MeasLocID);
                    if (measloc == null)
                    {
                        ALog.Error($"设备树中无该测点信息{channelItem.MeasLocID}");
                        continue;
                    }

                    // 根据设备树对象给采集器对象赋值
                    HADUObj.StationID = measloc.StationID;
                    HADUObj.DeviceName = measloc.DeviceName;

                    // 获取实时通道状态
                    ChannelStatusAlarm dbData = SearchChannelStatus(measloc.StationID, channelItem.MeasLocID);

                    // 转化为ChannelStatusItem对象
                    ChannelStatusItem channelObj = ChangeToChannelStatusItem(haduItem, channelItem, dbData, measloc);
                    HADUObj.ChannelStatusList.Add(channelObj);
                }

                if (HADUObj.ChannelStatusList.Count != 0)
                {
                    // 发电机转速状态不算入统计
                    var statusList = HADUObj.ChannelStatusList.Where(channel => !channel.MeaslocID.Contains("RSPD"));

                    if (statusList == null || statusList.Count() == 0)
                    {
                        HADUObj.DeviceStatus = EnumChannelStatus.Unknow;
                        HADUObj.AcqTime = DateTime.MinValue;
                    }
                    else
                    {
                        var status = statusList.Max(o => o.ChannelStatus);
                        HADUObj.DeviceStatus = status;
                        HADUObj.AcqTime = HADUObj.ChannelStatusList.Max(o => o.AcqTime);
                    }
                }

                _HADUStatusTree.AddOrUpdate(HADUObj.DeviceID, HADUObj, (k, old) => HADUObj);
            }
        }

        /// <summary>
        /// 1.1、将表中对象转化为状态树中通道对象
        /// </summary>
        /// <param name="haduItem">数据库Mapper中的设备信息</param>
        /// <param name="channelItem">数据库Mapper中的通道信息</param>
        /// <param name="dbData">数据库中该测点的RT数据</param>
        /// <param name="devMeas">该测点的设备树对象</param>
        /// <returns></returns>
        private ChannelStatusItem ChangeToChannelStatusItem(HADUInfo haduItem, HADUChannelInfo channelItem, ChannelStatusAlarm dbData, DevMeasLocation devMeas)
        {
            ChannelStatusItem channelObj = new ChannelStatusItem();
            channelObj.DeviceID = haduItem.HADUID;
            channelObj.DeviceType = haduItem.MonitorType;
            channelObj.ChannelNumber = channelItem.ChannelID;
            channelObj.MeaslocName = devMeas.MeasLoctionName;
            channelObj.MeaslocID = channelItem.MeasLocID;
            channelObj.ChannelStatus = dbData.MeaslocID != null ? dbData.ChannelStatus : EnumChannelStatus.Unknow;
            channelObj.Voltage = dbData.MeaslocID != null ? dbData.Voltage : null;
            channelObj.AcqTime = dbData.MeaslocID != null ? dbData.AcqTime : DateTime.MinValue;

            return channelObj;
        }

        /// <summary>
        /// 2、根据倾角仪晃度仪列表构建RT树
        /// 测点状态为塔顶和塔基
        /// </summary>
        private void CreatedTVMSTEStatus()
        {
            // 获取全部总线信息
            List<ModbusBusConfig> allBusConfig = devTreeRepsitory.GetAllModbusBusConfig();

            foreach (ModbusBusConfig modbusItem in allBusConfig)
            {
                HADUStatusItem HADUObj = new HADUStatusItem();

                // 获取采集器类型
                var sensorName = modbusItem.ModbusSensors.FirstOrDefault().SensorType;

                // 获取对应的测点列表
                List<DevMeasLocation> measloc = devTreeRepsitory.GetMeaslocationByDeviceID(modbusItem.DeviceID);
                string deviceName = measloc.FirstOrDefault().DeviceName;
                string stationID = measloc.FirstOrDefault().StationID;

                // 确定采集器对应的截面
                var section = sensorName == DataEntity.EnumType.EnumSensorType.TowSvm ? "TOWTOP" : "TOWFDN";
                // 根据截面获取Alarm表中的全部状态数据
                ChannelStatusAlarm dbData = SearchChannelStatusByCode(stationID, modbusItem.DeviceID, section);

                HADUObj.DeviceID = $"{modbusItem.DeviceID}&&{modbusItem.BusID}"; // 采集器ID
                HADUObj.DeviceName = deviceName;
                HADUObj.SensorName = EnumHelper.GetDescription(sensorName); // 监测设备名称：倾角仪/晃度仪
                HADUObj.LinkType = EnumHelper.GetDescription(modbusItem.Type); // 连接方式
                HADUObj.IP = modbusItem.TcpIp;
                HADUObj.StationID = stationID; // 风场ID
                HADUObj.CollectorType = EnumMonitorType.TVM_STE; // 设备类型，可以定死为塔筒结构
                HADUObj.DeviceStatus = dbData.AcqTime == DateTime.MinValue ? EnumChannelStatus.Abnormal : EnumChannelStatus.Normal;
                HADUObj.AcqTime = dbData.AcqTime;
                HADUObj.ChannelStatusList = new List<ChannelStatusItem>();

                _HADUStatusTree.AddOrUpdate(HADUObj.DeviceID, HADUObj, (k, old) => HADUObj);
            }
        }

        /// <summary>
        /// 3、根据超声采集单元列表构建RT树：
        /// 确定数据源
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CreatedBFMStatus()
        {
            // 获取超声采集单元信息列表
            List<UltrasonicDeviceInfo> bfmInfoList = devTreeRepsitory.GetUltrasonicDeviceInfos();

            foreach (UltrasonicDeviceInfo bfmItem in bfmInfoList)
            {
                HADUStatusItem HADUObj = new HADUStatusItem();
                HADUObj.DeviceID = bfmItem.DeviceID;
                HADUObj.IP = bfmItem.DeviceIP;
                // HADUObj.CollectorType = bfmItem.MonitorType;
                HADUObj.ChannelStatusList = new List<ChannelStatusItem>();
            }

            throw new NotImplementedException();
        }

        #endregion


        /// <summary>
        /// POST接口实现：将接口中获取的实时直流分量数据存储至数据库表中
        /// </summary>
        /// <param name="alarms">新采集的直流分量列表</param>
        /// <param name="stationID">风场ID</param>
        public void SaveChannelStatusAlarm(List<ChannelStatusAlarm> alarms, string stationID)
        {
            try
            {
                // 根据测点去重
                var distinctAlarms = alarms.GroupBy(a => a.MeaslocID).Select(g => g.OrderByDescending(a => a.AcqTime).First()).ToList();

                // 更新数据库表对象_dbChannelStatusList
                UpdateDBChannelStatusList(distinctAlarms);


                // 如果通道状态中有塔筒结构，则更新晃度倾角仪的RT树
                var tvmAlarms = distinctAlarms.Where(o => o.DeviceType == EnumMonitorType.TVM_STE);
                if (tvmAlarms.Count() > 0)
                {
                    CreatedTVMSTEStatus();
                }

                // 如果通道状态中有传动链/叶片/索力的通道数据，则更新HADU状态树
                var HADUAlarms = distinctAlarms.Where(o => o.DeviceType == EnumMonitorType.CVM || o.DeviceType == EnumMonitorType.BVM || o.DeviceType == EnumMonitorType.TVM_CBF);
                if (HADUAlarms.Count() > 0)
                {
                    CreatedHADUStatus();
                }

                ALog.Debug($"内存变量_dbChannelStatusList，_HADUStatusTree已更新");

                // 首次调用时启动定时器
                if (Interlocked.CompareExchange(ref _timerStarted, 1, 0) == 0)
                {
                    // 0：立即更新一次数据库表
                    _hourlyTimer = new Timer(UpdateChannelStatusAlarm, null, 0, 60 * 60 * 1000);
                }
                else
                {
                    ALog.Debug($"不到一小时，暂不更新SD数据库ChannelStatusAlarm表");
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SaveChannelStatusAlarm-post接口实现异常");
            }
        }

        /// <summary>
        /// 根据测点ID筛选RT表对象
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="measlocID">测点ID</param>
        /// <returns></returns>
        private ChannelStatusAlarm SearchChannelStatus(string stationID, string measlocID)
        {
            ChannelStatusAlarm obj = new ChannelStatusAlarm();
            if (_dbChannelStatusList.ContainsKey(stationID))
            {
                obj = _dbChannelStatusList[stationID].FirstOrDefault(alarm => alarm.MeaslocID == measlocID);
            }

            if (obj != null)
            {
                return obj;
            }
            else
            {
                return new ChannelStatusAlarm();
            }
        }

        /// <summary>
        /// 根据测点ID筛选RT表对象
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="section">截面【TOWTOP/TOWFDN】</param>
        /// <returns></returns>
        private ChannelStatusAlarm SearchChannelStatusByCode(string stationID, string deviceID, string section)
        {
            if (_dbChannelStatusList.TryGetValue(stationID, out var alarms))
            {
                var data = alarms.Where(alarm => alarm.DeviceID == deviceID && alarm.MeaslocID.Contains(section)).ToList();
                if (data != null && data.Count > 0)
                {
                    return data.OrderByDescending(o => o.AcqTime).FirstOrDefault();
                }
            }

            return new ChannelStatusAlarm();
        }

        /// <summary>
        /// SQL：将原来表内容删除，重新插入数据
        /// </summary>
        private void UpdateChannelStatusAlarm(object _)
        {
            try
            {
                foreach (var item in _dbChannelStatusList)
                {
                    string stationID = item.Key;
                    var channelAlarmList = item.Value.GroupBy(a => a.MeaslocID).Select(g => g.OrderByDescending(a => a.AcqTime).First()).ToList();

                    using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                    {
                        // 1. 一次性清空整张表
                        db.Deleteable<ChannelStatusAlarm>().ExecuteCommand();

                        // 2. 批量整表插入
                        db.Insertable(channelAlarmList).ExecuteCommand();
                    }
                }
                ALog.Debug($"更新SD数据库ChannelStatusAlarm表完成");
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"UpdateChannelStatusAlarm-实时状态表更新完异常");
            }
        }


        /// <summary>
        /// SQL：获取数据库中的所有通道状态数据
        /// </summary>
        /// <param name="stationID">风场ID</param>
        private void GetChannelStatusListByDB(string stationID)
        {
            try
            {
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    // 取现在表中的全部通道状态数据
                    List<ChannelStatusAlarm> data = db.Queryable<ChannelStatusAlarm>().ToList();
                    if (!_dbChannelStatusList.ContainsKey(stationID))
                    {
                        _dbChannelStatusList.TryAdd(stationID, new List<ChannelStatusAlarm>());
                    }
                    _dbChannelStatusList[stationID].AddRange(data);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SQL-GetChannelStatusListByDB-获取{stationID}风场采集器状态异常");
            }
        }


        /// <summary>
        /// 有新数据后更新内存中的状态列表_dbChannelStatusList
        /// </summary>
        private void UpdateDBChannelStatusList(List<ChannelStatusAlarm> alarms)
        {
            try
            {
                foreach (var item in _dbChannelStatusList)
                {
                    string stationID = item.Key;
                    var channelStatusList = item.Value;

                    // 取alarms中机组ID包含ID中的列表
                    List<ChannelStatusAlarm> alarmList = alarms.Where(alarm => alarm.DeviceID.Contains(stationID)).ToList();
                    if (alarmList == null || alarmList.Count == 0)
                    {
                        continue;
                    }

                    foreach (ChannelStatusAlarm alarmItem in alarmList)
                    {
                        // 获取最新数据和设备树中测点相同的对象
                        ChannelStatusAlarm channelObj = channelStatusList.Where(channel => channel.MeaslocID == alarmItem.MeaslocID).FirstOrDefault();

                        // 如果在内存的实时树中存在该测点，更新状态
                        if (channelObj != null)
                        {
                            // 更新通道数据
                            channelObj.ChannelStatus = alarmItem.ChannelStatus;
                            channelObj.AcqTime = alarmItem.AcqTime;
                            channelObj.Voltage = alarmItem.Voltage;
                        }
                        // 在内存表中加入该测点数据
                        else
                        {
                            _dbChannelStatusList[stationID].Add(alarmItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"UpdateDBChannelStatusList-根据接口新数据更新_dbChannelStatusList异常");
            }
        }


        #region 接口实现
        /// <summary>
        /// 接口实现：根据风场ID获取HADU列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        public HADUStatus GetHADUTypeLists(string stationID)
        {
            try
            {
                var data = _HADUStatusTree.Values.Where(o => o.StationID == stationID && (o.CollectorType == EnumMonitorType.CVM || o.CollectorType == EnumMonitorType.BVM || o.CollectorType == EnumMonitorType.TVM_CBF)).ToList();

                if (data == null || data.Count == 0)
                {
                    return new HADUStatus();
                }
                HADUStatus res = GroupByChannelStatus(data).First();

                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetHADUTypeLists-获取{stationID}下的采集器状态树异常");
                return new HADUStatus();
            }
        }

        /// <summary>
        /// 将通道状态按照风场/采集器类型分组，构建树
        /// </summary>
        /// <param name="_ChannelStatus">采集器状态列表</param>
        private List<HADUStatus> GroupByChannelStatus(List<HADUStatusItem> _ChannelStatus)
        {
            List<HADUStatus> res = new List<HADUStatus>();

            var stationGroup = _ChannelStatus.GroupBy(o => o.StationID);
            foreach (var station in stationGroup)
            {
                HADUStatus stationObj = new HADUStatus();
                stationObj.StationID = station.Key;
                stationObj.CollectorTypeList = new List<HADUTypeItem>();

                var typeGroup = station.GroupBy(o => o.CollectorType);

                foreach (var type in typeGroup)
                {
                    try
                    {
                        HADUTypeItem haduObj = new HADUTypeItem();
                        haduObj.CollectorType = type.Key;
                        haduObj.CollectorTypeName = EnumHelper.GetDescription(type.Key);
                        haduObj.CollectorStatusList = type.ToList();

                        stationObj.CollectorTypeList.Add(haduObj);
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, $"GroupByChannelStatus-初始化状态树异常 MonitorType：{type.Key}");
                    }
                }
                res.Add(stationObj);
            }
            return res;
        }


        /// <summary>
        /// 接口实现：根据设备ID和设备类型获取通道状态列表
        /// </summary>
        /// <param name="deviceID">采集器ID</param>
        /// <param name="deviceType">采集器类型</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<ChannelStatusItem> GetChannelStatusList(string deviceID, string deviceType)
        {
            if (_HADUStatusTree.TryGetValue(deviceID, out var hadu))
                return hadu.ChannelStatusList;
            return new List<ChannelStatusItem>();
        }

        /// <summary>
        /// 根据风场ID获取采集器状态列表
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public List<HADUStatusItem> GetTVMSTEStatusLists(string stationID)
        {
            List<HADUStatusItem> res = new List<HADUStatusItem>();
            try
            {
                // 只统计类型为塔筒结构的数据
                res = _HADUStatusTree.Values.Where(o => o.StationID == stationID && o.CollectorType == EnumMonitorType.TVM_STE).ToList();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetTVMSTEStatusLists-获取倾角晃度传感器状态列表异常");
            }
            return res;
        }


        /// <summary>
        /// 根据风场ID获取超声采集器状态列表
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public List<HADUStatusItem> GetBFMStatusLists(string stationID)
        {
            List<HADUStatusItem> res = new List<HADUStatusItem>();
            try
            {
                // 只统计采集事件类型为塔筒螺栓的数据
                res = _HADUStatusTree.Values.Where(o => o.StationID == stationID && o.CollectorType == EnumMonitorType.TVM_FLG_GAP).ToList();
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetTVMSTEStatusLists-获取倾角晃度传感器状态列表异常");
            }
            return res;
        }

        #endregion
    }
}
