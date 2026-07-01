using ACH.CMSWebClient.ControllerModel.WindPark;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.Common;
using ACH.DataEntity.StatusTree;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DevTree;
using ACH.DBRepository.SD;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using Dm.util;
using Newtonsoft.Json;
using SqlSugar;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class WindParkAPIMethods
    {
        private readonly IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        private readonly IAlarmStatusRepository alarmStatusRepository;
        private readonly IDevTreeRepository webDevTreeRepository = new DevTreeRepository();
        public WindParkAPIMethods(IConfiguration configuration)
        {
            alarmStatusRepository = new AlarmStatusRepository(configuration);
        }
        // 获取地图配置信息的文件夹地址
        private readonly string UserMapInfoFolderPath = "MapConfig";
        // 当前页面给的状态等级
        // private readonly string[] statusCodeList = new string[] { "normal", "attention", "warning" };

        private readonly EnumAlarmStatus[] statusCodeList = new EnumAlarmStatus[] { EnumAlarmStatus.Normal, EnumAlarmStatus.Attention, EnumAlarmStatus.Warning };

        #region GetMapInfoByUserID接口实现
        internal ControlMapInfoDTO GetMapInfoByUserID(string userID)
        {
            // 根据风场ID判断高亮什么风场信息，是需要一个json配置文件还是添加到表中
            string filePath = Path.Combine(UserMapInfoFolderPath, "UserMapInfo.json");
            string jsonString = File.ReadAllText(filePath);

            List<ControlMapInfoDTO> data = JsonConvert.DeserializeObject<List<ControlMapInfoDTO>>(jsonString);
            if (data == null || data.Count == 0) { return new ControlMapInfoDTO(); }

            ControlMapInfoDTO res = data.FirstOrDefault(o => o.UserID == userID);

            // 在json文件中没有该用户信息，按照json文件的第一条重新构建返回
            if (res == null)
            {
                ControlMapInfoDTO obj = new ControlMapInfoDTO();
                obj.UserID = userID;
                obj.Longitude = data.First().Longitude;
                obj.Latitude = data.First().Latitude;
                obj.Elevation = data.First().Elevation;
                string firstmapPath = Path.Combine(UserMapInfoFolderPath, data.First().JsonFile);
                obj.JsonFile = File.ReadAllText(firstmapPath);
                return obj;
            }
            else
            {
                // 读取json文件，并重新赋值 
                string mapPath = Path.Combine(UserMapInfoFolderPath, res.JsonFile);
                res.JsonFile = File.ReadAllText(mapPath);
                return res;
            }
        }
        #endregion


        #region GetStationInfoList接口实现
        internal List<ControlStationStatusDTO> GetStationInfoList(string userID)
        {
            List<ControlStationStatusDTO> res = new List<ControlStationStatusDTO>();

            // 获取该风场下绑定的所有风场
            List<DTStationInfo> deviceTrees = alarmStatusRepository.GetStations(userID);

            // 读取json文件，获取地图中风场的虚拟坐标
            string filePath = Path.Combine(UserMapInfoFolderPath, "StationMapPosition.json");
            string jsonString = File.ReadAllText(filePath);
            List<StationMapPositionJsonDTO> data = JsonConvert.DeserializeObject<List<StationMapPositionJsonDTO>>(jsonString);

            foreach (var deviceTree in deviceTrees)
            {
                StationMapPositionJsonDTO pos = new StationMapPositionJsonDTO();
                if (data != null && data.Count > 0)
                {
                    pos = data.FirstOrDefault(o => o.StationID == deviceTree.WindParkId);
                }

                // 获取该风场info
                StationInfo stationInfo = _devTreeRepository.GetStationInfoByID(deviceTree.WindParkId);
                if (stationInfo == null) continue;

                ControlStationStatusDTO obj = new ControlStationStatusDTO();
                obj.StationID = stationInfo.StationId;
                obj.StationName = stationInfo.StationName;
                obj.Longitude = pos == null ? stationInfo.Longitude : pos.Longitude;
                obj.Latitude = pos == null ? stationInfo.Latitude : pos.Latitude;
                obj.Elevation = pos == null ? stationInfo.Elevation : pos.Elevation;
                obj.ViewLongitude = stationInfo.Longitude;
                obj.ViewLatitude = stationInfo.Latitude;
                obj.ViewElevation = stationInfo.Elevation;
                obj.DeviceNum = deviceTree.DeviceList.Count;
                obj.AttentionDeviceNum = deviceTree.DeviceList.Count(o => o.WindTurbuineStatus == EnumAlarmStatus.Attention);
                obj.WarningDeviceNum = deviceTree.DeviceList.Count(o => o.WindTurbuineStatus == EnumAlarmStatus.Warning);
                obj.DangerDeviceNum = deviceTree.DeviceList.Count(o => o.WindTurbuineStatus == EnumAlarmStatus.Danger);
                obj.NormalDeviceNum = deviceTree.DeviceList.Count - obj.AttentionDeviceNum - obj.WarningDeviceNum - obj.DangerDeviceNum;

                res.Add(obj);
            }
            return res.SortByName(ascending: true, dictType: EnumSortType.StationName);
        }
        #endregion


        #region WindParkMonitorNum接口实现
        internal StationMonitorInfoDTO GetWindParkMonitorNum(string stationId)
        {
            // 获取风场下设备状态树
            DTStationInfo DeviceTree = alarmStatusRepository.GetStationById(stationId);

            if (DeviceTree == null || DeviceTree.DeviceList == null || DeviceTree.DeviceList.Count == 0)
            {
                return new StationMonitorInfoDTO();
            }

            StationMonitorInfoDTO res = new()
            {
                Name = DeviceTree.WindParkName,
                Code = DeviceTree.WindParkCode,
                Id = DeviceTree.WindParkId,
                OfflineNumber = 0,
                OfflineNumberIds = new List<string?>(),
                MonitorNumber = 0,
                MonitorNumberIds = new List<string>(),
                MonitoringRate = 0
            };


            foreach (var device in DeviceTree.DeviceList)
            {
                int timeSpan = (device.WindTurbuineStatusTime - DateTime.Now).Days;

                if (timeSpan <= -1)
                {
                    // 数据更新时间超过一天 - 离线
                    res.OfflineNumber += 1;
                    res.OfflineNumberIds.Add(device.WindTurbuineId);
                }
                else
                {
                    // 数据更新时间未超过一天 - 在线
                    res.MonitorNumber += 1;
                    res.MonitorNumberIds.Add(device.WindTurbuineId);
                }
            }

            res.MonitoringRate = (double)res.MonitorNumber / DeviceTree.DeviceList.Count(); // 计算在线率

            return res;
        }
        #endregion


        #region WindTurbineWeekStatus 接口实现
        internal StationDeviceStatusDTO GetWindTurbineWeekStatusList(List<DTStationInfo> deviceTrees)
        {
            // 过滤掉无效的风场数据
            deviceTrees = deviceTrees.Where(dt => dt != null && dt.DeviceList != null && dt.DeviceList.Count > 0).ToList();

            if (deviceTrees.Count == 0)
            {
                return new StationDeviceStatusDTO();
            }

            StationDeviceStatusDTO res = new()
            {
                DayStatusList = HandlerTimeState(deviceTrees),
                WeekStatusList = HandlerWeekTurbineList(deviceTrees)
            };
            return res;
        }

        // 2.2、统计机组实时状态
        private List<StationDayStatusDTO> HandlerTimeState(List<DTStationInfo> deviceTrees)
        {

            List<StationDayStatusDTO> res = new List<StationDayStatusDTO>();

            // 获取所有风场的机组列表
            var allDevices = deviceTrees.SelectMany(dt => dt.DeviceList).ToList();
            // 对所有机组状态按照状态分组
            var turbineStateGroup = allDevices.GroupBy(o => o.WindTurbuineStatus).ToList();

            foreach (var item in statusCodeList)
            {

                var turbineState = turbineStateGroup.FirstOrDefault(o => o.Key == item);

                StationDayStatusDTO obj = new()
                {
                    StatusName = EnumHelper.GetDescription(item),
                    StatusCode = item.toString().ToLower(),
                    DeviceCount = turbineState == null ? 0 : turbineState.Count(),
                };
                res.Add(obj);
            }
            return res;
        }

        // 2.3、获取一周内的全部机组健康状态
        private List<StationWeekStatusDTO> HandlerWeekTurbineList(List<DTStationInfo> deviceTrees)
        {
            List<StationWeekStatusDTO> res = new List<StationWeekStatusDTO>();

            // 时间范围为近一周
            DateTime endTime = DateTime.Today.AddDays(1);
            DateTime bgTime = endTime.AddDays(-7);

            // 获取所有风场ID
            var stationIds = deviceTrees.Select(dt => dt.WindParkId).Distinct().ToList();

            // 获取所有机组近一周状态
            List<DeviceItem> allWeekStateList = new List<DeviceItem>();
            foreach (var stationId in stationIds)
            {
                // 取该风场近一周的机组状态趋势
                List<DeviceItem> weekStateList = alarmStatusRepository.GetDeviceStateTrend(stationId, bgTime, endTime);
                if (weekStateList != null && weekStateList.Count > 0)
                {
                    allWeekStateList.AddRange(weekStateList);
                }
            }

            // 获取所有风场的机组列表
            var allDevices = deviceTrees.SelectMany(dt => dt.DeviceList).ToList();

            // 对时间按照天进行遍历
            int day = (endTime - bgTime).Days;
            for (int i = 0; i < day; i++)
            {
                // 计算循环的当前日期
                DateTime _time = bgTime.AddDays(i);

                StationWeekStatusDTO obj = new()
                {
                    StatusTime = _time.ToString("yyyy-MM-dd"),
                    Data = new List<List<object>>()
                };

                // 取一天时间段的机组状态
                List<DeviceItem> dayDeviceStatusList = allWeekStateList.Where(o => o.WindTurbuineStatusTime > _time && o.WindTurbuineStatusTime < _time.AddDays(1)).ToList();

                // 如果没有历史机组状态数据，则默认所有机组正常
                if (dayDeviceStatusList == null || dayDeviceStatusList.Count() == 0)
                {
                    obj.Data.Add(new List<object>() { EnumAlarmStatus.Normal.ToString().ToLower(), allDevices.Count() });
                }
                else
                {
                    List<DeviceItem> new_data = new List<DeviceItem>();

                    foreach (var turbine in allDevices)
                    {
                        // 对机组状态去重，取每个机组的最高状态。
                        DeviceItem deviceStatus = dayDeviceStatusList.Where(e => e.WindTurbuineId == turbine.WindTurbuineId).OrderByDescending(e => e.WindTurbuineStatus).FirstOrDefault() ?? new DeviceItem() { WindTurbuineStatus = EnumAlarmStatus.Normal };
                        new_data.Add(deviceStatus);
                    }

                    // 对状态数组group分组，将不同状态添加到res中 
                    var group = new_data.GroupBy(o => o.WindTurbuineStatus);
                    foreach (var ele in group)
                    {
                        var stateObj = new List<object>()
                        {
                            ele.Key.ToString().ToLower(),
                            ele.Count()
                        };

                        obj.Data.Add(stateObj);
                    }
                }
                res.Add(obj);
            }
            return res;
        }
        #endregion


        #region WindTurbineStatusTrend接口实现
        internal List<StationDeviceStatusTrendDTO> GetWindTurbineTrendList(List<DTStationInfo> deviceTrees, DateTime startTime, DateTime endTime)
        {
            List<StationDeviceStatusTrendDTO> result = new List<StationDeviceStatusTrendDTO>();

            // 过滤掉无效的风场数据
            deviceTrees = deviceTrees.Where(dt => dt != null && dt.DeviceList != null && dt.DeviceList.Count > 0).ToList();

            if (deviceTrees.Count == 0)
            {
                // 如果没有设备数据，返回与statusCodeList数量一致的空对象
                foreach (var statusCode in statusCodeList)
                {
                    result.Add(new StationDeviceStatusTrendDTO
                    {
                        StatusCode = statusCode.ToString().ToLower(),
                        StatusName = EnumHelper.GetDescription(statusCode),
                        Data = new List<List<object>>()
                    });
                }
                return result;
            }

            // 1、获取所有风场的机组列表和状态数据
            List<DTDeviceStatus> allDevices = deviceTrees.SelectMany(dt => dt.DeviceList).ToList();
            List<string> stationIds = deviceTrees.Select(dt => dt.WindParkId).Distinct().ToList();

            // 2、根据风场ID获取时间段内全部机组状态
            List<DeviceItem> allStatusData = new List<DeviceItem>();
            foreach (var stationId in stationIds)
            {
                List<DeviceItem> statusData = alarmStatusRepository.GetDeviceStateTrend(stationId, startTime, endTime);
                if (statusData != null && statusData.Count > 0)
                {
                    allStatusData.AddRange(statusData);
                }
            }

            // 3、创建状态等级对象
            foreach (var statusCode in statusCodeList)
            {
                StationDeviceStatusTrendDTO statusObject = new StationDeviceStatusTrendDTO
                {
                    StatusCode = statusCode.ToString().ToLower(),
                    StatusName = EnumHelper.GetDescription(statusCode),
                    Data = new List<List<object>>()
                };
                result.Add(statusObject);
            }

            // 4、处理时间范围数据
            DateTime endDate = new DateTime(endTime.Year, endTime.Month, endTime.Day).AddDays(1);
            DateTime startDate = new DateTime(startTime.Year, startTime.Month, startTime.Day);
            int dayCount = (endDate - startDate).Days;

            for (int i = 0; i < dayCount; i++)
            {
                DateTime currentDate = startDate.AddDays(i);
                string dateString = currentDate.ToString("yyyy-MM-dd");

                // 获取当天时间内的机组历史状态
                var dailyStatusData = allStatusData.Where(o => o.WindTurbuineStatusTime > currentDate && o.WindTurbuineStatusTime < currentDate.AddDays(1)).ToList();

                // 处理当天数据
                if (dailyStatusData == null || dailyStatusData.Count == 0)
                {
                    // 无状态数据，默认所有机组为正常
                    AddDefaultStatusData(result, dateString, allDevices.Count);
                }
                else
                {
                    // 获取每个机组的最高状态
                    var deviceMaxStatusList = GetDeviceMaxStatusList(allDevices, dailyStatusData);
                    AddCalculatedStatusData(result, dateString, allDevices.Count, deviceMaxStatusList);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取每个机组的最高状态
        /// </summary>
        private List<DeviceItem> GetDeviceMaxStatusList(List<DTDeviceStatus> devices, List<DeviceItem> statusData)
        {
            var result = new List<DeviceItem>();
            foreach (var device in devices)
            {
                // 假设device有WindTurbuineId属性
                var deviceId = device.GetType().GetProperty("WindTurbuineId")?.GetValue(device);
                if (deviceId != null)
                {
                    var maxStatus = statusData
                        .Where(e => e.WindTurbuineId == (string)deviceId)
                        .OrderByDescending(e => e.WindTurbuineStatus)
                        .FirstOrDefault() ?? new DeviceItem { WindTurbuineStatus = EnumAlarmStatus.Normal };
                    result.Add(maxStatus);
                }
            }
            return result;
        }

        /// <summary>
        /// 添加默认状态数据
        /// </summary>
        private void AddDefaultStatusData(List<StationDeviceStatusTrendDTO> statusObjects, string dateString, int totalDevices)
        {
            foreach (var statusObj in statusObjects)
            {
                int count = statusObj.StatusCode == EnumAlarmStatus.Normal.ToString().ToLower() ? totalDevices : 0;
                statusObj.Data.Add(new List<object> { dateString, count });
            }
        }

        /// <summary>
        /// 添加计算后的状态数据
        /// </summary>
        private void AddCalculatedStatusData(List<StationDeviceStatusTrendDTO> statusObjects, string dateString, int totalDevices, List<DeviceItem> maxStatusList)
        {
            int normalCount = maxStatusList.Count(o => o.WindTurbuineStatus == EnumAlarmStatus.Normal || o.WindTurbuineStatus == EnumAlarmStatus.Nostate);

            foreach (var statusObj in statusObjects)
            {
                if (statusObj.StatusCode == EnumAlarmStatus.Normal.ToString().ToLower())
                {
                    statusObj.Data.Add(new List<object> { dateString, normalCount });
                }
                else
                {
                    try
                    {
                        EnumAlarmStatus status = (EnumAlarmStatus)Enum.Parse(typeof(EnumAlarmStatus), statusObj.StatusCode, true);
                        int count = maxStatusList.Count(o => o.WindTurbuineStatus == status);
                        statusObj.Data.Add(new List<object> { dateString, count });
                    }
                    catch
                    {
                        statusObj.Data.Add(new List<object> { dateString, 0 });
                    }
                }
            }
        }
        #endregion

        /* #region WindTurbineStatusTrend接口实现
         internal List<StationDeviceStatusTrendDTO> GetWindTurbineTrendList(List<DTStationInfo> deviceTrees, DateTime startTime, DateTime endTime)
         {
             List<StationDeviceStatusTrendDTO> res = new List<StationDeviceStatusTrendDTO>();

             // 过滤掉无效的风场数据
             deviceTrees = deviceTrees.Where(dt => dt != null && dt.DeviceList != null && dt.DeviceList.Count > 0).ToList();
             if (deviceTrees.Count == 0)
             {
                 return res;
             }


             // 1、获取所有风场列表和全部机组列表
             var allDevices = deviceTrees.SelectMany(dt => dt.DeviceList).ToList();
             var stationIds = deviceTrees.Select(dt => dt.WindParkId).Distinct().ToList();

             // 2、根据风场ID获取时间段内全部机组状态
             List<DeviceItem> allMonthStateList = new List<DeviceItem>();
             foreach (var stationId in stationIds)
             {
                 List<DeviceItem> monthStateList = alarmStatusRepository.GetDeviceStateTrend(stationId, startTime, endTime);
                 if (monthStateList != null && monthStateList.Count > 0)
                 {
                     allMonthStateList.AddRange(monthStateList);
                 }
             }


             // 3、创建状态等级对象
             foreach (var statusCode in statusCodeList)
             {
                 // 获取该状态下的全部机组状态列表
                 List<DeviceItem> StateCodeList = allMonthStateList.Where(o => o.WindTurbuineStatus == statusCode).ToList();

                 StationDeviceStatusTrendDTO obj = new StationDeviceStatusTrendDTO();
                 obj.StatusCode = statusCode.toString().ToLower();
                 obj.StatusName = EnumHelper.GetDescription(statusCode);
                 obj.Data = new List<List<object>>();
                 // obj.Data = HandlerDeviceStatusTrendData(statusCode, allDevices, startTime, endTime, StateCodeList);
                 res.Add(obj);
             }


             DateTime _end = new DateTime(endTime.Year, endTime.Month, endTime.Day).AddDays(1);
             DateTime _begin = new DateTime(startTime.Year, startTime.Month, startTime.Day);

             // 根据时间范围遍历每天
             int day = (_end - _begin).Days;
             for (int i = 0; i < day; i++)
             {
                 DateTime _time = _begin.AddDays(i);
                 string _timeStr = _time.ToString("yyyy-MM-dd");

                 // 获取当天时间内的机组历史状态
                 var deviceDayStatusList = allMonthStateList.Where(o => o.WindTurbuineStatusTime > _time && o.WindTurbuineStatusTime < _time.AddDays(1)).ToList();

                 // 如果当前没有任何机组状态，默认所有机组状态为正常
                 if (deviceDayStatusList == null || deviceDayStatusList.Count() == 0)
                 {
                     foreach (var statusObj in res)
                     {
                         if (statusObj.StatusCode == EnumAlarmStatus.Normal.ToString().ToLower())
                         {
                             statusObj.Data.Add(new List<object>() { _timeStr, allDevices.Count() });
                         }
                         else
                         {
                             statusObj.Data.Add(new List<object>() { _timeStr, 0 });
                         }
                     }
                 }
                 else
                 {
                     // 对组机状态去重，取每个机组的最高状态。
                     var deviceMaxStatusList = new List<DeviceItem>();
                     foreach (var turbine in allDevices)
                     {
                         var newState = deviceDayStatusList.Where(e => e.WindTurbuineId == turbine.WindTurbuineId).OrderByDescending(e => e.WindTurbuineStatus).FirstOrDefault() ?? new();
                         deviceMaxStatusList.Add(newState);
                     }
                     int abnormalCount = deviceMaxStatusList.Where(o => o.WindTurbuineStatus != EnumAlarmStatus.Normal && o.WindTurbuineStatus != EnumAlarmStatus.Nostate).Count();

                     foreach (var statusObj in res)
                     {
                         var statusCount = deviceMaxStatusList.Where(o => o.WindTurbuineStatus == (EnumAlarmStatus)Enum.Parse(typeof(EnumAlarmStatus), statusObj.StatusCode, true)).Count();

                         if (statusObj.StatusCode == EnumAlarmStatus.Normal.ToString().ToLower())
                         {
                             statusObj.Data.Add(new List<object>() { _timeStr, allDevices.Count() - abnormalCount });
                         }
                         else
                         {
                             statusObj.Data.Add(new List<object>() { _timeStr, statusCount });
                         }
                     }
                 }
             }


             return res;
         }

         private void GetDeviceStatusTrendData()
         {
             throw new NotImplementedException();
         }

         /// <summary>
         /// 获取该时间段内该状态的机组数量
         /// </summary>
         /// <param name="statusCode">状态Code</param>
         /// <param name="startTime">开始时间</param>
         /// <param name="endTime">结束时间</param>
         /// <returns></returns>
         private List<List<object>> HandlerDeviceStatusTrendData(EnumAlarmStatus status, List<DTDeviceStatus> allDevices, DateTime startTime, DateTime endTime, List<DeviceItem> allMonthStateList)
         {
             List<List<object>> res = new List<List<object>>();
             DateTime _end = new DateTime(endTime.Year, endTime.Month, endTime.Day).AddDays(1);
             DateTime _begin = new DateTime(startTime.Year, startTime.Month, startTime.Day);

             // 根据时间范围遍历每天
             int day = (_end - _begin).Days;
             for (int i = 0; i < day; i++)
             {
                 DateTime _time = _begin.AddDays(i);
                 string _timeStr = _time.ToString("yyyy-MM-dd");

                 // 获取当天时间内的机组历史状态
                 var _data = allMonthStateList.Where(o => o.WindTurbuineStatusTime > _time && o.WindTurbuineStatusTime < _time.AddDays(1)).ToList();

                 // 如果当前没有机组状态=status的数据，
                 if (_data == null || _data.Count() == 0)
                 {
                     if (status == EnumAlarmStatus.Normal)
                     {
                         res.Add(new List<object>() { _timeStr, allDevices.Count() });
                     }
                     else
                     {
                         res.Add(new List<object>() { _timeStr, 0 });
                     }
                 }
                 else
                 {
                     res.Add(new List<object>() { _timeStr, _data.Count() });
                 }
             }
             return res;
         }
         #endregion*/


        #region WindParkCompStatusList接口实现
        internal List<StationCompStatusDTO> GetCompStatusRingList(List<DTStationInfo> deviceTrees, DateTime startTime, DateTime endTime)
        {
            // 过滤掉无效的风场数据
            deviceTrees = deviceTrees.Where(dt => dt != null && dt.DeviceList != null && dt.DeviceList.Count > 0).ToList();

            if (deviceTrees.Count == 0)
            {
                return new List<StationCompStatusDTO>();
            }

            // 1、获取所有风场下的所有部件
            var allComp = deviceTrees
                .SelectMany(dt => dt.DeviceList)
                .Where(device => device.CompList != null)
                .SelectMany(device => device.CompList)
                .ToList();

            // 2、按照部件类型分组
            var compGroup = allComp.GroupBy(comp => comp.CompCode).ToList();

            List<StationCompStatusDTO> result = new();

            foreach (var comp in compGroup)
            {
                var compFirst = comp.First();
                var stationCompStatus = new StationCompStatusDTO
                {
                    CompCode = compFirst.CompCode,
                    CompName = compFirst.CompName,
                    CompTotal = comp.Count(),
                    FaultCompCount = 0,
                    FaultStatusList = new List<StationCompStatusNumDTO>()
                };

                // 3、每种部件内按照状态分组
                var stateGroup = comp.GroupBy(x => x.CompStatus).ToList();

                foreach (var state in stateGroup)
                {
                    string stateName = state.Key.ToString();

                    // 统计该部件下所有状态名称及个数
                    var faultStatus = new StationCompStatusNumDTO
                    {
                        Name = stateName.ToLower(),
                        Count = state.Count()
                    };

                    stationCompStatus.FaultStatusList.Add(faultStatus);

                    // 统计异常状态的部件个数（排除正常和无状态）
                    if (stateName != "Nostate" && stateName != "Normal")
                    {
                        stationCompStatus.FaultCompCount += state.Count();
                    }
                }

                result.Add(stationCompStatus);
            }

            return result.SortByName(ascending: true, dictType: EnumSortType.CompName);
        }
        #endregion


        #region WindParkCompTrend接口实现
        internal StationCompStatusTrendDTO GetCompTrendList(List<DTStationInfo> deviceTrees, DateTime startTime, DateTime endTime)
        {
            var result = new StationCompStatusTrendDTO();
            result.FaultStatusList = new List<CompTrendList>();

            // 过滤掉无效的风场数据
            deviceTrees = deviceTrees.Where(dt => dt != null && dt.DeviceList != null && dt.DeviceList.Count > 0).ToList();

            if (deviceTrees.Count == 0)
            {
                result.Time = new List<string>();
                return result;
            }

            // 1、获取所有风场的部件状态趋势数据
            var allCompStateList = new List<CompItem>();
            var stationIds = deviceTrees.Select(dt => dt.WindParkId).Distinct().ToList();

            foreach (var stationId in stationIds)
            {
                var compStateList = alarmStatusRepository.GetCompStateTrendByStationID(stationId, startTime, endTime);
                if (compStateList != null && compStateList.Count > 0)
                {
                    allCompStateList.AddRange(compStateList);
                }
            }

            // 2、更新部件名称和代码
            UpdateCompItemCodes(allCompStateList);

            // 3、按部件类型分组
            var compGroup = allCompStateList.GroupBy(d => d.CompCode)
                .Where(g => !string.IsNullOrEmpty(g.Key))
                .ToDictionary(g => g.Key, g => g.ToList());

            // 4、时间范围处理
            var endDate = new DateTime(endTime.Year, endTime.Month, endTime.Day).AddDays(1);
            var beginDate = new DateTime(startTime.Year, startTime.Month, startTime.Day);
            int totalDays = (endDate - beginDate).Days;

            // 生成时间列表
            var timeList = Enumerable.Range(0, totalDays)
                .Select(i => beginDate.AddDays(i).ToString("yyyy-MM-dd"))
                .ToList();

            // 5、处理每个部件的趋势数据
            foreach (var comp in compGroup)
            {
                var compItems = comp.Value;

                // 跳过没有数据的部件
                if (compItems == null || compItems.Count == 0)
                {
                    continue;
                }

                var compTrend = new CompTrendList
                {
                    CompCode = comp.Key,
                    CompName = compItems.FirstOrDefault()?.CompName ?? "",
                    FaultCount = new List<List<int>>()
                };

                // 处理每天的数据
                for (int i = 0; i < totalDays; i++)
                {
                    var currentDate = beginDate.AddDays(i);
                    var nextDate = currentDate.AddDays(1);

                    // 获取当天的部件状态数据
                    var dailyCompDatas = compItems.Where(o => o.CompStatusTime > currentDate && o.CompStatusTime < nextDate).ToList();

                    int dangerCount = 0;
                    int warningCount = 0;
                    int attentionCount = 0;

                    if (dailyCompDatas.Count > 0)
                    {
                        // 按部件ID分组
                        var compIdGroup = dailyCompDatas.GroupBy(d => d.CompId)
                            .Where(g => g.Key != null)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        foreach (var item in compIdGroup)
                        {
                            // 获取该部件当天的所有有效状态
                            var validStatusData = item.Value.Where(item => item.CompStatus != null).ToList();
                            if (validStatusData.Count == 0)
                            {
                                continue;
                            }

                            // 获取当天最高状态
                            var maxStatus = validStatusData.Max(item => item.CompStatus);

                            switch (maxStatus)
                            {
                                case EnumAlarmStatus.Danger:
                                    dangerCount++;
                                    break;
                                case EnumAlarmStatus.Warning:
                                    warningCount++;
                                    break;
                                case EnumAlarmStatus.Attention:
                                    attentionCount++;
                                    break;
                            }
                        }
                    }

                    // 添加当天统计数据
                    var dailyCounts = new List<int>
                    {
                        dangerCount + warningCount + attentionCount, // 总异常数
                        dangerCount,                                  // 危险数
                        warningCount,                                 // 警告数
                        attentionCount                                // 注意数
                    };
                    compTrend.FaultCount.Add(dailyCounts);
                }

                result.FaultStatusList.Add(compTrend);
            }

            result.Time = timeList;
            return result;
        }

        private void UpdateCompItemCodes(List<CompItem> datas)
        {
            if (datas != null && datas.Count != 0)
            {
                foreach (var item in datas)
                {
                    // 获取部件名称和code
                    var compNameList = DevTreeRepsitory.Instance.GetMeaslocationByCompID(item.CompId);
                    if (compNameList != null && compNameList.Count > 0)
                    {
                        var CompNameObj = compNameList.FirstOrDefault();
                        item.CompCode = CompNameObj.ComponentID.Replace(CompNameObj.DeviceID, "");
                        item.CompName = CompNameObj.ComponentName;
                    }
                }
            }
        }
        #endregion


        #region WindTurbineStatusList接口实现
        // 7.1、WindTurbineStatusListAPI接口 - 处理机组列表
        internal List<StationDeviceStatusCardDTO> GetWindTurbineCardList(string stationId)
        {
            // 获取该风场下的所有机组列表
            DTStationInfo deviceTree = alarmStatusRepository.GetStationById(stationId);
            List<StationDeviceStatusCardDTO> res = new List<StationDeviceStatusCardDTO>();
            if (deviceTree == null || deviceTree.DeviceList == null || deviceTree.DeviceList.Count == 0)
            {
                return new List<StationDeviceStatusCardDTO>();
            }

            foreach (var item in deviceTree.DeviceList)
            {
                StationDeviceStatusCardDTO obj = new()
                {
                    WindparkId = deviceTree.WindParkId,
                    WindturbineId = item.WindTurbuineId,
                    WindturbineName = item.WindTurbuineName,
                    WindturbineStatus = item.WindTurbuineStatus.ToString().ToLower(),
                    StatusLastTime = item.WindTurbuineStatusTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    HealthStatusEntityVo = GetCompList(item.CompList)
                };
                res.Add(obj);
            }
            return res;
        }

        // 7.2、WindTurbineStatusListAPI接口 - 处理部件列表
        private List<StationDeviceCardCompStatusDTO> GetCompList(List<DTCompStatus> compList)
        {
            List<StationDeviceCardCompStatusDTO> res = new List<StationDeviceCardCompStatusDTO>();
            foreach (var item in compList)
            {
                StationDeviceCardCompStatusDTO obj = new()
                {
                    EntityId = item.CompId,
                    EntityName = item.CompName,
                    EntityStatus = item.CompStatus.ToString().ToLower(),
                    EntityType = item.CompCode,
                    StatusTime = item.CompStatusTime.ToString("yyyy-MM-dd HH:mm:ss")
                };
                res.Add(obj);
            }
            return res;
        }
        #endregion


        #region AllDeviceList接口实现
        internal List<StationInfoTreeDTO> GetDeviceListByUserId(string userId)
        {
            List<StationInfoTreeDTO> res = new List<StationInfoTreeDTO>();

            // 获取该用户下绑定的全部风场的设备状态树
            List<DTStationInfo> stationIDList = alarmStatusRepository.GetStations(userId);
            foreach (DTStationInfo obj in stationIDList)
            {
                StationInfoTreeDTO item = new StationInfoTreeDTO();
                StationInfo station = _devTreeRepository.GetStationInfoByID(obj.WindParkId); // 从数据库获取风场信息
                if (station == null)
                {
                    continue;
                }
                item.id = station.StationId;
                item.name = station.StationName;
                item.code = station.StationId;
                item.isImageryLayer = station.StationMapType == DataEntity.EnumType.EnumStationMapType.WGS84 ? true : false;
                item.childNode = HandlerWindTurbineTree(obj); // 处理机组列表

                res.Add(item);
            }

            var dat = res.Where(o => o.childNode != null && o.childNode.Count != 0).ToList();
            return dat.SortByName(ascending: true, dictType: EnumSortType.StationName);
        }


        // 8.3、根据风场ID获取设备树接口的机组列表
        private List<DeviceInfoTreeDTO> HandlerWindTurbineTree(DTStationInfo DeviceTree)
        {
            try
            {
                List<DeviceInfoTreeDTO> res = new List<DeviceInfoTreeDTO>();

                if (DeviceTree == null || DeviceTree.DeviceList == null || DeviceTree.DeviceList.Count == 0)
                    return res;

                foreach (DTDeviceStatus item in DeviceTree.DeviceList)
                {
                    DeviceInfoTreeDTO obj = new()
                    {
                        deptCode = item.WindTurbuineId,
                        entityCode = item.WindTurbuineId.Replace(DeviceTree.WindParkId, ""),
                        entityId = item.WindTurbuineId,
                        entityName = item.WindTurbuineName,
                        healthStatus = item.WindTurbuineStatus.ToString().ToLower()
                    };
                    res.Add(obj);
                }

                return res.SortByName(ascending: true, dictType: EnumSortType.Alphabetical);
            }
            catch (Exception ex)
            {
                return new List<DeviceInfoTreeDTO>();
            }
        }
        #endregion


        #region WindParkInfo接口实现
        /// <summary>
        /// 9、WindParkInfo接口处理：获取风场投运信息
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<StationInfoDTO> GetWindParkInfo(string stationId)
        {
            try
            {
                // 获取风场信息
                StationInfo stationInfo = _devTreeRepository.GetStationInfoByID(stationId);
                // 获取风场下所有机组信息
                List<DeviceInfo> turbineList = webDevTreeRepository.GetDeviceInfoByStationID(stationId);

                StationInfoDTO obj = new StationInfoDTO();
                obj.Id = stationInfo.StationId;
                obj.Name = stationInfo.StationName;
                obj.Number = turbineList.Count;

                // 定义分组键
                Func<DeviceInfo, string>[] groupKeys = new Func<DeviceInfo, string>[]
                {
                    o => o.DeviceMaker, // 厂商
                    o => o.OperationlDate.ToString("yyyy-MM-dd"), // 投运时长
                    o => o.DeviceModel // 风机型号
                };

                // 处理分组数据并设置子节点
                obj.childNode = HandlerTurbineGroupBasic(turbineList.GroupBy(groupKeys[0]).ToList(), obj.Number);
                obj.childNode1 = HandlerTurbineGroupBasic(turbineList.GroupBy(groupKeys[1]).ToList(), obj.Number);
                obj.childNode2 = HandlerTurbineGroupBasic(turbineList.GroupBy(groupKeys[2]).ToList(), obj.Number);

                return new List<StationInfoDTO> { obj };
            }
            catch (Exception ex)
            {
                return new List<StationInfoDTO>();
            }
        }

        // 9.4、对设备进行分组处理
        private List<StationInfoDetailDTO> HandlerTurbineGroupBasic(List<IGrouping<string, DeviceInfo>> groupDatas, int deviceNum)
        {
            List<StationInfoDetailDTO> res = groupDatas.Select(group =>
            {
                string key = group.Key;
                int size = group.Count();
                double percentage = Math.Round((double)size / deviceNum, 2); // 计算并保留两位小数
                List<string> ids = group.Select(o => o.DeviceId).ToList();

                return new StationInfoDetailDTO
                {
                    Name = key,
                    Size = size,
                    Ratio = percentage,
                    Ids = ids
                };
            }).ToList();

            return res;
        }

        // 9.5、计算投运时间距现在的年份
        public static string CalculateYears(string time)
        {
            DateTime pastDate = DateTime.Parse(time);
            DateTime now = DateTime.Now;

            // 计算两个日期之间的总天数
            double totalDays = (now - pastDate).TotalDays;

            // 将总天数转换为年数，并确保至少为1年
            int years = (int)Math.Ceiling(totalDays / 365.0);

            return $"{years}年";
        }
        #endregion


        #region
        internal ControlStationInfoDTO GetStationControlInfo(string userID)
        {
            ControlStationInfoDTO res = new ControlStationInfoDTO();

            // 统计机组状态更新时间在一天内的机组个数（更新时间在一天内视为在线）
            int controlDeviceNum = 0;

            // 获取该风场下绑定的所有风场
            List<DTStationInfo> deviceTrees = alarmStatusRepository.GetStations(userID);

            res.StationNum = deviceTrees.Count;
            foreach (var deviceTree in deviceTrees)
            {
                if (deviceTree.DeviceList == null || deviceTree.DeviceList.Count == 0)
                    continue;

                res.DeviceNum += deviceTree.DeviceList.Count;
                controlDeviceNum += deviceTree.DeviceList.Count(o => o.WindTurbuineStatusTime > DateTime.Now.AddDays(-1));
            }

            res.StationControlRate = Math.Round(controlDeviceNum / (double)res.DeviceNum, 2);

            return res;
        }
        #endregion


        #region GetDeviceCoordinatesList接口实现
        /// <summary>
        /// 获取机组经纬度信息
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<StationDeviceStatusInfoDTO> GetDeviceCoordinatesList(string stationId)
        {
            List<StationDeviceStatusInfoDTO> res = new List<StationDeviceStatusInfoDTO>();

            // 在设备树中获取该风场下所有机组的状态树
            DTStationInfo deviceTree = alarmStatusRepository.GetStationById(stationId);
            if (deviceTree == null || deviceTree.DeviceList == null || deviceTree.DeviceList.Count == 0)
            {
                return new List<StationDeviceStatusInfoDTO>();
            }

            foreach (DTDeviceStatus item in deviceTree.DeviceList)
            {
                // 获取该机组的详细信息
                DeviceInfo deviceInfo = _devTreeRepository.GetDeviceInfoByID(item.WindTurbuineId);

                StationDeviceStatusInfoDTO obj = new StationDeviceStatusInfoDTO();

                obj.DeviceID = item.WindTurbuineId;
                obj.DeviceName = item.WindTurbuineName;
                obj.Latitude = deviceInfo.Latitude.ToString();
                obj.Longitude = deviceInfo.Longitude.ToString();
                obj.Elevation = deviceInfo.Elevation.ToString();
                obj.DeviceModel = deviceInfo.DeviceModel;
                obj.DeviceVindor = deviceInfo.DeviceMaker;
                obj.DeviceStatus = item.WindTurbuineStatus.ToString().ToLower();
                obj.OperationalDate = deviceInfo.OperationlDate.ToString("yyyy-MM-dd");
                obj.PoweringRate = deviceInfo.FactedPower.ToString() ?? "0";
                obj.DeviceModelType = EnumHelper.GetDescription(deviceInfo.DeviceModelType);
                res.Add(obj);
            }
            return res;

        }
        #endregion

    }
}
