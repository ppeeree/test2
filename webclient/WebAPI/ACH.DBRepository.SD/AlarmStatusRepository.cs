using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.Common;
using ACH.DataEntity.StatusTree;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Component;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StatusDBContext = ACH.DBConn.Dat.StatusDBContext;

namespace ACH.DBRepository.SD
{
    public class AlarmStatusRepository : IAlarmStatusRepository
    {
        private readonly IConfiguration _configuration;
        private readonly StatusDBContext statusDBContext;
        private readonly ComponentHelper convertRepository = new ComponentHelper();
        private readonly IAppDatRepository appDatRepository = new AppDatRepository();
        private readonly IDevTreeRepsitory devTreeRepsitory = DevTreeRepsitory.Instance;

        /// <summary>
        /// 构造函数
        /// </summary>
        public AlarmStatusRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            statusDBContext = new StatusDBContext(configuration);
        }

        #region 系统自检
        public List<ChannelStatusAlarm> GetChannelStatusByStationId(string stationID, EnumMonitorType monitorType)
        {
            try
            {
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    try
                    {
                        // 方案1：尝试直接查询（新表结构）
                        return db.Queryable<ChannelStatusAlarm>().Where(o => o.DeviceType == monitorType).ToList();
                    }
                    catch (Exception sqlEx) when (sqlEx.Message.Contains("ADUID") && sqlEx.Message.Contains("no such column"))
                    {
                        ALog.Debug($"表缺少ADUID字段，使用兼容查询: {stationID}");

                        // 方案2：兼容查询（旧表结构）
                        List<ChannelStatusAlarm> res = new List<ChannelStatusAlarm>();
                        var oldData = db.Queryable<OldChannelStatusAlarm>().Where(o => o.DeviceType == monitorType).ToList();

                        foreach (var item in oldData)
                        {
                            // 根据测点ID获取采集器ID
                            HADUChannelInfo channelInfoDict = devTreeRepsitory.GetHADUChannelInfoByMeasID(item.MeaslocID);

                            ChannelStatusAlarm obj = new ChannelStatusAlarm
                            {
                                ADUID = channelInfoDict.HADUID ?? item.DeviceID,
                                DeviceID = item.DeviceID,
                                DeviceType = item.DeviceType,
                                ChannelNumber = item.ChannelNumber,
                                MeaslocID = item.MeaslocID,
                                ChannelStatus = item.ChannelStatus,
                                Voltage = item.Voltage,
                                AcqTime = item.AcqTime
                            };
                            res.Add(obj);
                        }
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SQL-GetChannelStatusByStationId-获取{stationID}风场采集器状态异常");
                return new List<ChannelStatusAlarm>();
            }
        }

        /// <summary>
        /// 获取采集器ID下所有通道状态列表 
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorID">采集器ID</param>
        /// <returns></returns>
        public List<ChannelStatusAlarm> GetChannelStatusByHADUId(string stationID, string monitorID)
        {
            try
            {
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    try
                    {
                        // 方案1：尝试使用ADUID查询（新表结构）
                        List<ChannelStatusAlarm> data = db.Queryable<ChannelStatusAlarm>().Where(o => monitorID == o.ADUID).ToList();
                        return data;
                    }
                    catch (Exception sqlEx) when (sqlEx.Message.Contains("ADUID") && sqlEx.Message.Contains("no such column"))
                    {
                        ALog.Debug($"表缺少ADUID字段，使用兼容查询: {stationID}, monitorID={monitorID}");

                        // 方案2：兼容查询（旧表结构）
                        var oldData = db.Queryable<OldChannelStatusAlarm>().Where(o => monitorID.Contains(o.DeviceID)).ToList();

                        List<ChannelStatusAlarm> res = new List<ChannelStatusAlarm>();

                        foreach (var item in oldData)
                        {
                            // 根据测点ID获取采集器ID
                            HADUChannelInfo channelInfoDict = devTreeRepsitory.GetHADUChannelInfoByMeasID(item.MeaslocID);

                            // 判断采集器ID是否和入参ID一致
                            if (!string.IsNullOrEmpty(channelInfoDict.HADUID) && channelInfoDict.HADUID == monitorID)
                            {
                                ChannelStatusAlarm obj = new ChannelStatusAlarm
                                {
                                    ADUID = channelInfoDict.HADUID ?? item.DeviceID,
                                    DeviceID = item.DeviceID,
                                    DeviceType = item.DeviceType,
                                    ChannelNumber = item.ChannelNumber,
                                    MeaslocID = item.MeaslocID,
                                    ChannelStatus = item.ChannelStatus,
                                    Voltage = item.Voltage,
                                    AcqTime = item.AcqTime
                                };
                                res.Add(obj);
                            }
                        }
                        return res;
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"SQL-GetChannelStatusByHADUId-获取{stationID}风场的{monitorID}采集器状态异常");
                return new List<ChannelStatusAlarm>();
            }
        }

        [SugarTable("ChannelStatusAlarm")]
        public class OldChannelStatusAlarm
        {
            [SugarColumn(IsNullable = true, IsIgnore = true)]
            public string ADUID { get; set; }

            public string DeviceID { get; set; }

            public EnumMonitorType DeviceType { get; set; }

            public int ChannelNumber { get; set; }

            [SugarColumn(IsPrimaryKey = true)]
            public string MeaslocID { get; set; }

            public EnumChannelStatus ChannelStatus { get; set; }

            [SugarColumn(IsNullable = true)]
            public double? Voltage { get; set; }

            public DateTime AcqTime { get; set; }
        }
        #endregion


        #region 设备预警
        /// <summary>
        /// 1、获取用户下全部风场列表 
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        /*public List<DTStationInfo> GetStations(string userID)
        {
            // 根据用户ID获取stationMapper
            List<UserStationMapper> stationMappers = appDatRepository.GetStationListByUserID(userID);
            if (stationMappers.Count == 0)
            {
                return new List<DTStationInfo>();
            }
            ALog.Debug($"GetStations - 1");
            List<DTStationInfo> res = new List<DTStationInfo>();
            var i = 0;
            foreach (UserStationMapper item in stationMappers)
            {
                i++;
                ALog.Debug($"GetStations - 2.{i} - {item.StationID}");
                // 获取风场下所有测点
                List<DevMeasLocation> stationMeaslocList = DevTreeRepsitory.Instance.GetAllMeasLocation(item.StationID);
                ALog.Debug($"GetStations - 2.{i} - 1");

                // 获取风场下所有特征值报警数据
                List<DTEvStatus> evDatas = GetEvAlarmListByStation(item.StationID, null);
                ALog.Debug($"GetStations - 2.{i} - 2");

                // 创建风场状态树并返回
                res.Add(CreatedStationStatusList(stationMeaslocList, evDatas));
                ALog.Debug($"GetStations - 2.{i} - 3");
            }
            return res;
        }*/
        public List<DTStationInfo> GetStations(string userID)
        {
            var stationMappers = appDatRepository.GetStationListByUserID(userID);
            if (stationMappers.Count == 0) return new List<DTStationInfo>();

            // 限制并发数，避免连接池耗尽
            var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
            var results = new ConcurrentBag<(int index, DTStationInfo data)>();

            Parallel.ForEach(stationMappers.Select((m, i) => (m, i)), options, item =>
            {
                var (mapper, index) = item;
                try
                {
                    // 获取测点（可缓存优化）
                    var measLocs = DevTreeRepsitory.Instance.GetAllMeasLocation(mapper.StationID);

                    // 获取报警数据（已优化为单次SQL）
                    var evDatas = GetEvAlarmListByStation(mapper.StationID, null);

                    var stationInfo = CreatedStationStatusList(measLocs, evDatas);
                    results.Add((index, stationInfo));
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"GetStations-处理风场{mapper.StationID}异常");
                }
            });

            return results.OrderBy(r => r.index).Select(r => r.data).ToList();
        }


        /// <summary>
        /// 2、根据风场ID获取设备状态树
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public DTStationInfo GetStationById(string stationID)
        {
            // 获取风场下所有测点
            List<DevMeasLocation> stationMeaslocList = DevTreeRepsitory.Instance.GetAllMeasLocation(stationID);
            // 获取风场下所有特征值报警数据
            List<DTEvStatus> evDatas = GetEvAlarmListByStation(stationID, null);

            // 创建风场状态树并返回
            return CreatedStationStatusList(stationMeaslocList, evDatas);
        }

        /// <summary>
        /// 根据风场ID或用户ID获取设备树对象
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<DTStationInfo> GetStationsByUserIDOrStationID(string? stationId, string? userID)
        {
            if (!string.IsNullOrEmpty(stationId) && string.IsNullOrEmpty(userID))
            {
                // 获取单个风场数据
                return new List<DTStationInfo> { GetStationById(stationId) };
            }
            else if (string.IsNullOrEmpty(stationId) && !string.IsNullOrEmpty(userID))
            {
                // 获取所有风场数据
                return GetStations(userID);
            }
            return new List<DTStationInfo>();
        }

        /// <summary>
        /// 3、根据机组ID获取机组状态树
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public DTDeviceStatus GetDeviceId(string deviceID)
        {
            // 获取机组下所有测点
            List<DevMeasLocation> deviceMeaslocList = DevTreeRepsitory.Instance.GetMeaslocationByDeviceID(deviceID);
            string stationID = deviceMeaslocList.First().StationID;
            // 获取机组下所有特征值报警数据
            List<DTEvStatus> evDatas = GetEvAlarmListByStation(stationID, deviceID);

            // 构建风场状态树
            DTStationInfo data = CreatedStationStatusList(deviceMeaslocList, evDatas);

            // 返回风场下指定机组的状态树
            return data.DeviceList.FirstOrDefault(o => o.WindTurbuineId == deviceID);
        }

        /// <summary>
        /// 获取风场下全部部件状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByStationID(string stationID, DateTime startTime, DateTime endTime)
        {
            List<CompItem> datas = new List<CompItem>();
            try
            {
                // 避免开始时间与结束时间相同查询不到数据 
                if (startTime == endTime)
                {
                    endTime = endTime.AddMilliseconds(1);
                }

                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    // 根据测点 时间找对应数据，可能包含多个采样率
                    datas = db.Queryable<CompItem>().Where(it => it.CompStatusTime >= startTime && it.CompStatusTime <= endTime).ToList();
                }
                datas = datas.OrderBy(item => item.CompStatusTime).ToList();// 排序

                if (datas.Count > 0)
                {
                    // 将状态为无数据的修改为正常
                    foreach (var item in datas)
                    {
                        if (item.CompStatus == EnumAlarmStatus.Nostate)
                        {
                            item.CompStatus = EnumAlarmStatus.Normal;
                        }
                    }
                }

                return datas;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetCompStateTrendByStationID-查询{stationID}内全部部件状态状态数据异常");
                return datas;
            }
        }


        /// <summary>
        /// 获取风场下全部机组状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<DeviceItem> GetDeviceStateTrend(string stationID, DateTime startTime, DateTime endTime)
        {
            List<DeviceItem> datas = new List<DeviceItem>();
            try
            {
                // 避免开始时间与结束时间相同查询不到数据
                if (startTime == endTime)
                {
                    endTime = endTime.AddMilliseconds(1);
                }


                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    datas = db.Queryable<DeviceItem>().Where(it => it.WindTurbuineStatusTime >= startTime && it.WindTurbuineStatusTime <= endTime).ToList();
                }
                datas = datas.OrderBy(item => item.WindTurbuineStatusTime).ToList();

                if (datas.Count > 0)
                {
                    // 将状态为无数据的修改为正常
                    foreach (var item in datas)
                    {
                        if (item.WindTurbuineStatus == EnumAlarmStatus.Nostate)
                        {
                            item.WindTurbuineStatus = EnumAlarmStatus.Normal;
                        }
                    }
                }

                return datas;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetDeviceStateTrend-查询{stationID}下机组状态趋势数据异常");
                return datas;
            }
        }


        /// <summary>
        /// 获取某个部件状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="componentId">部件ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByCompID(string stationID, string componentId, DateTime startTime, DateTime endTime)
        {
            List<CompItem> datas = new List<CompItem>();
            try
            {
                // 避免开始时间与结束时间相同查询不到数据 
                if (startTime == endTime)
                {
                    endTime = endTime.AddMilliseconds(1);
                }

                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    datas = db.Queryable<CompItem>().Where(it => componentId == it.CompId && it.CompStatusTime >= startTime && it.CompStatusTime <= endTime).ToList();
                }
                datas = datas.OrderBy(item => item.CompStatusTime).ToList();

                if (datas.Count > 0)
                {
                    // 将状态为无数据的修改为正常
                    foreach (var item in datas)
                    {
                        if (item.CompStatus == EnumAlarmStatus.Nostate)
                        {
                            item.CompStatus = EnumAlarmStatus.Normal;
                        }
                    }
                }

                return datas;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetCompStateTrendByCompID-查询{componentId}的状态趋势数据异常");
                return datas;
            }

        }
        #endregion


        #region 生成风场的状态树
        /// <summary>
        /// 生成风场的状态树
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        public DTStationInfo CreatedStationStatusList(List<DevMeasLocation> measlocList, List<DTEvStatus> evDatas)
        {
            if (measlocList == null || measlocList.Count == 0)
            {
                ALog.Error($"入参measlocList为空，不能构建设备状态树");
                return new DTStationInfo();
            }


            try
            {
                /* 1. EigenValue 做成字典，方便按照测点查询 */
                var evDict = evDatas?
                    .GroupBy(e => e.MeasLocID)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(e => e.EvStatusTime)
                              .First()
                    ) ?? new Dictionary<string, DTEvStatus>();

                /* 2. 取出风场级信息 */
                var station = new DTStationInfo
                {
                    WindParkId = measlocList.First().StationID,
                    WindParkName = measlocList.First().StationName,
                    DeviceList = new List<DTDeviceStatus>()
                };

                /* 3. 一级分组：Device */
                foreach (var deviceItem in measlocList.GroupBy(m => m.DeviceID))
                {
                    var device = new DTDeviceStatus
                    {
                        WindTurbuineId = deviceItem.First().DeviceID,
                        WindTurbuineName = deviceItem.First().DeviceName,
                        CompList = new List<DTCompStatus>()
                    };

                    /* 4. 二级分组：Component */
                    foreach (var compItem in deviceItem.GroupBy(m => m.ComponentID))
                    {
                        var comp = new DTCompStatus
                        {
                            CompId = compItem.First().ComponentID,
                            CompName = compItem.First().ComponentName,
                            CompCode = compItem.First().ComponentID.Replace(deviceItem.First().DeviceID, ""),
                            MeaslocList = new List<DTMeaslocStatus>()
                        };

                        /* 5. 三级分组：MeasLoc */
                        foreach (var measlocItem in compItem.GroupBy(m => m.MeasLoctionID))
                        {
                            // 在特征值字典中查找该测点下的特征值
                            evDict.TryGetValue(measlocItem.First().MeasLoctionID, out var ev);

                            comp.MeaslocList.Add(new DTMeaslocStatus
                            {
                                MeaslocId = measlocItem.First().MeasLoctionID,
                                MeaslocCode = measlocItem.First().MeasLoctionID.Replace(deviceItem.First().DeviceID, ""),
                                MeaslocName = measlocItem.First().MeasLoctionName,
                                EigenValueList = ev == null ? new List<DTEvStatus>() : new List<DTEvStatus> { ev }
                            });
                        }
                        comp.CompStatus = comp.MeaslocList.Max(o => o.MeaslocStatus);
                        comp.CompStatusTime = comp.MeaslocList.Max(o => o.MeaslocStatusTime);
                        device.CompList.Add(comp);
                    }
                    device.WindTurbuineStatus = device.CompList.Max(o => o.CompStatus);
                    device.WindTurbuineStatusTime = device.CompList.Max(o => o.CompStatusTime);
                    station.DeviceList.Add(device);
                }

                return station;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"CreatedStationStatusList 异常，输入测点数={measlocList?.Count ?? 0}，特征值条数={evDatas?.Count ?? 0}");
                return new DTStationInfo();
            }
        }

        /// <summary>
        /// SQL：获取风场下所有特征值报警数据
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        public List<DTEvStatus> GetEvAlarmListByStation(string stationID, string? deviceID)
        {
            try
            {
                using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
                {
                    int RTTime = int.Parse(_configuration["RTStatusTreeTime"] ?? "3");
                    DateTime dateTime = DateTime.Now.AddDays(-RTTime);

                    // 纯 SQL 窗口函数
                    var sql = @"
                WITH RankedAlarms AS (
                    SELECT *,
                           ROW_NUMBER() OVER (PARTITION BY MeasLocID ORDER BY AcqTime DESC) as rn
                    FROM EigenValueAlarm
                    WHERE AcqTime >= @dateTime
                      AND (@deviceID IS NULL OR DeviceID = @deviceID)
                )
                SELECT MeasLocID, DeviceID, AcqTime, EvCode, Value, Unit, EvStatus
                FROM RankedAlarms
                WHERE rn = 1";

                    var evAlarms = db.Ado.SqlQuery<EigenValueAlarm>(sql, new
                    {
                        dateTime = dateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        deviceID
                    });

                    return ConvertToEigenValueData(evAlarms);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetEvAlarmListByStation-查询{stationID}的{deviceID}状态数据异常");
                return new List<DTEvStatus>();
            }
        }

        /// <summary>
        /// 将SD数据库中的 eigenValueAlarm 转换成RT树中的 DTEvStatus
        /// </summary>
        /// <param name="eigenValues">表中查询到的数据</param>
        /// <returns></returns>
        private List<DTEvStatus> ConvertToEigenValueData(List<EigenValueAlarm> eigenValues)
        {
            List<DTEvStatus> result = new List<DTEvStatus>();
            foreach (var item in eigenValues)
            {
                var evName = convertRepository.ConvertEVNameByCode(item.EvCode);
                result.Add(new DTEvStatus(item.DeviceID, item.MeasLocID, item.EvCode, $"{item.MeasLocID}&&{item.EvCode}", evName,
                    item.EvStatus, item.AcqTime, item.Unit, item.Value, item.MeasType));
            }

            return result;
        }


        #endregion
    }
}
