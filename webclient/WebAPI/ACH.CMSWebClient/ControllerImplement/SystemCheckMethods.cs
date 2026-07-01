using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.BoltAxialForceDB;
using ACH.BoltAxialForceDB.BFMADUData;
using ACH.BoltAxialForceDB.BFMMeasData;
using ACH.CMSWebClient.ControllerModel.SystemCheck;
using ACH.DataEntity.Common;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.SD;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Component;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Dm.util;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class SystemCheckMethods
    {
        IDevTreeRepsitory devTreeRepsitory = DevTreeRepsitory.Instance;
        BFMDeviceInfoRepository bfmInfoRepository = BFMDeviceInfoRepository.Instance;
        IDeviceWaveRead deviceWaveRead;
        IAlarmStatusRepository alarmStatusRepository;
        ComponentHelper componentHelper = new ComponentHelper();
        DeviceBFMReadWaveDB deviceBFMReadWaveDB;

        private double upperLine = 18.0; // 直流分量上限
        private double lowerLine = 6.0; // 直流分量下限
                                        // private double BFMNormalLine = 6.0; // 超声采集单元回波峰峰值的正常阈值限

        public SystemCheckMethods(IConfiguration _configuration)
        {
            upperLine = double.Parse(_configuration["DCUpperLine"] ?? "18");
            lowerLine = double.Parse(_configuration["DCLowerLine"] ?? "6");
            // BFMNormalLine = double.Parse(_configuration["BFMNormalLine"] ?? "6");

            deviceWaveRead = new ACHDeviceReadWaveDB(_configuration);
            alarmStatusRepository = new AlarmStatusRepository(_configuration);
            deviceBFMReadWaveDB = new DeviceBFMReadWaveDB(_configuration);
        }

        #region GetStationMonitorList接口实现
        /// <summary>
        /// 1、获取风场下的采集器类型列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<StationMonitorTypeDTO> GetStationMonitorTypeList(string stationID)
        {
            List<StationMonitorTypeDTO> res = new List<StationMonitorTypeDTO>();
            // 根据设备树获取部件列表
            var measData = devTreeRepsitory.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);

            // 检查数据是否存在
            if (!measData.Any())
            {
                return res.OrderBy(o => o.Sort).ToList();
            }

            // 获取该风场下实体部件列表
            var monitorList = measData.FirstOrDefault()?.Select(o => o.MeasDataType).Distinct() ?? Enumerable.Empty<EnumMonitorType>();

            var addedNames = new HashSet<string>(); // 使用HashSet提高查找效率

            var cmsMonitor = new StationMonitorTypeDTO
            {
                MonitorCompName = "传动链监测",
                Sort = 1,
                MonitorTypeList = new List<MonitorTypeDTO>()
            };
            var bmsMonitor = new StationMonitorTypeDTO
            {
                MonitorCompName = "叶片监测",
                Sort = 2,
                MonitorTypeList = new List<MonitorTypeDTO>()
            };
            var tmsMonitor = new StationMonitorTypeDTO
            {
                MonitorCompName = "塔筒监测",
                Sort = 3,
                MonitorTypeList = new List<MonitorTypeDTO>()
            };

            foreach (EnumMonitorType type in monitorList)
            {
                switch (type)
                {
                    case EnumMonitorType.CVM:
                        MonitorTypeDTO cvmObj = AddADUMonitorType(stationID, EnumMonitorType.CVM, "传动链采集单元");
                        if (cvmObj != null && cvmObj.MonitorTypeCode != null)
                            cmsMonitor.MonitorTypeList.Add(cvmObj);
                        break;
                    case EnumMonitorType.BVM:
                        MonitorTypeDTO bvmObj = AddADUMonitorType(stationID, EnumMonitorType.BVM, "叶片采集单元");
                        if (bvmObj != null && bvmObj.MonitorTypeCode != null)
                            bmsMonitor.MonitorTypeList.Add(bvmObj);
                        break;
                    case EnumMonitorType.TVM_STE:
                        MonitorTypeDTO tvmsteObj = AddModbusMonitorType(stationID, EnumMonitorType.TVM_STE, "塔筒倾角传感器");
                        if (tvmsteObj != null && tvmsteObj.MonitorTypeCode != null)
                            tmsMonitor.MonitorTypeList.Add(tvmsteObj);
                        break;
                    case EnumMonitorType.TVM_FLG_GAP:
                        MonitorTypeDTO flggapObj = AddModbusMonitorType(stationID, EnumMonitorType.TVM_FLG_GAP, "法兰间隙传感器");
                        if (flggapObj != null && flggapObj.MonitorTypeCode != null)
                            tmsMonitor.MonitorTypeList.Add(flggapObj);
                        break;
                    case EnumMonitorType.TVM_CBF:
                        MonitorTypeDTO cbfObj = AddADUMonitorType(stationID, EnumMonitorType.TVM_CBF, "索力采集单元");
                        if (cbfObj != null && cbfObj.MonitorTypeCode != null)
                            tmsMonitor.MonitorTypeList.Add(cbfObj);
                        break;
                    case EnumMonitorType.TVM_BFM:
                        MonitorTypeDTO bfmObj = AddADUMonitorType(stationID, EnumMonitorType.TVM_BFM, "螺栓超声采集单元");
                        if (bfmObj != null && bfmObj.MonitorTypeCode != null)
                            tmsMonitor.MonitorTypeList.Add(bfmObj);
                        break;
                }
            }

            if (cmsMonitor.MonitorTypeList.Count != 0) { res.Add(cmsMonitor); }
            if (bmsMonitor.MonitorTypeList.Count != 0) { res.Add(bmsMonitor); }
            if (tmsMonitor.MonitorTypeList.Count != 0) { res.Add(tmsMonitor); }


            return res.OrderBy(o => o.Sort).ToList();
        }

        /// <summary>
        /// 通用方法：添加HADU监测类型
        /// </summary>
        private MonitorTypeDTO AddADUMonitorType(string stationID, EnumMonitorType monitorType, string typeName)
        {
            MonitorTypeDTO obj = new MonitorTypeDTO();
            List<HADUStatusDTO> data = GetHADUStatusList(stationID, monitorType);
            if (data.Count != 0)
            {
                obj.MonitorTypeName = typeName;
                obj.MonitorTypeCode = monitorType.toString();
                obj.FaultMonitorNum = data.Count(o => o.MonitorStatus == "故障");
            }
            return obj;
        }

        /// <summary>
        /// 通用方法：添加Modbus监测类型
        /// </summary>
        private MonitorTypeDTO AddModbusMonitorType(string stationID, EnumMonitorType monitorType, string typeName)
        {
            MonitorTypeDTO obj = new MonitorTypeDTO();
            List<ModbusStatusDTO> data = GetModbusStatusList(stationID, monitorType);
            if (data.Count != 0)
            {
                obj.MonitorTypeName = typeName;
                obj.MonitorTypeCode = monitorType.toString();
                obj.FaultMonitorNum = data.Count(o => o.MonitorStatus == "故障");
            }
            return obj;
        }
        #endregion


        #region GetHADUStatusList接口实现
        /// <summary>
        /// 2、根据获取采集单元状态列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal List<HADUStatusDTO> GetHADUStatusList(string stationID, EnumMonitorType monitorType)
        {
            List<HADUStatusDTO> res = new List<HADUStatusDTO>();
            // 获取设备树信息
            List<DevMeasLocation> measlocs = devTreeRepsitory.GetAllMeasLocation(stationID).Where(O => O.MeasDataType == monitorType).ToList();
            // 获取全部通道状态
            List<ChannelStatusAlarm> stationAlarms = alarmStatusRepository.GetChannelStatusByStationId(stationID, monitorType);

            // 根据采集事件类型判定从哪个采集器Info表中获取数据
            if (monitorType == EnumMonitorType.TVM_BFM)
            {
                // 获取风场下，该采集器事件类型的超声采集器infoList
                List<BFMADUInfo> bfmInfoList = bfmInfoRepository.GetBFMADUInfoList(stationID);

                // 遍历采集器列表
                foreach (BFMADUInfo bfmItem in bfmInfoList)
                {
                    // 根据ADUID查询机组ID
                    string deviceID = bfmItem.ChannelMappers.First().DeviceID;

                    // 获取设备树对象
                    DevMeasLocation measloc = measlocs.First(o => deviceID == o.DeviceID);
                    // 获取该采集器下全部的通道状态
                    List<ChannelStatusAlarm> ADUAlarms = stationAlarms.Where(o => o.ADUID == bfmItem.ADUID).ToList();

                    HADUStatusDTO HADUObj = new HADUStatusDTO(
                        guid: Guid.NewGuid().ToString("N"),
                        deviceName: measloc.DeviceName,
                        monitorId: bfmItem.ADUID,
                        monitorIp: bfmItem.ADUIP,
                        alarms: ADUAlarms
                    );
                    res.Add(HADUObj);
                }
            }
            else
            {
                // 获取风场下，该采集器事件类型的振动采集单元infoList
                List<HADUInfo> HADUInfoList = devTreeRepsitory.GetHADUSList(stationID, monitorType);

                // 遍历采集器列表
                foreach (HADUInfo haduItem in HADUInfoList)
                {
                    // 获取设备树对象
                    DevMeasLocation? measloc = measlocs.First(o => haduItem.HADUID.Contains(o.DeviceID));

                    // 获取该采集器下全部的通道状态(兼容旧数据没有存储ADUID的通道状态)
                    List<ChannelStatusAlarm> ADUAlarms = stationAlarms.Where(o => o.ADUID == haduItem.HADUID).ToList().Count == 0 ? stationAlarms.Where(o => haduItem.HADUID.Contains(o.DeviceID)).ToList() : stationAlarms.Where(o => o.ADUID == haduItem.HADUID).ToList();

                    HADUStatusDTO HADUObj = new HADUStatusDTO(
                        guid: Guid.NewGuid().ToString("N"),
                        deviceName: measloc.DeviceName,
                        monitorId: haduItem.HADUID,
                        monitorIp: haduItem.HADUIP,
                        alarms: ADUAlarms
                    );
                    res.Add(HADUObj);
                }
            }
            return res;
        }

        #endregion


        #region GetModbusStatusList接口实现
        /// <summary>
        /// 3、获取Modbus状态列表
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        internal List<ModbusStatusDTO> GetModbusStatusList(string stationID, EnumMonitorType monitorType)
        {
            List<ModbusStatusDTO> res = new List<ModbusStatusDTO>();

            // 获取modbusInfo列表
            List<ModbusSensorConfig> modbusInfoList = DevTreeRepsitory.Instance.GetModbusSensorConfigs(stationID, monitorType);
            // 获取设备树列表
            List<DevMeasLocation> measlocs = devTreeRepsitory.GetAllMeasLocation(stationID).Where(O => O.MeasDataType == monitorType).ToList();
            // 获取通道状态列表 
            List<ChannelStatusAlarm> stationAlarms = alarmStatusRepository.GetChannelStatusByStationId(stationID, monitorType);

            foreach (ModbusSensorConfig modbusSensor in modbusInfoList)
            {
                // 获取对应的测点列表
                DevMeasLocation? measloc = measlocs.First(o => modbusSensor.BusConfig.DeviceID == o.DeviceID);

                string section = componentHelper.ConvertSectionBySensorType(modbusSensor.SensorType);

                // 根据截面获取Alarm表中的全部状态数据
                List<ChannelStatusAlarm> ADUAlarms = stationAlarms.Where(o => o.ADUID == modbusSensor.ModBusSensorID && o.MeaslocID.contains(section)).ToList();

                ModbusStatusDTO obj = new ModbusStatusDTO(
                        guid: Guid.NewGuid().ToString("N"),
                        deviceName: measloc.DeviceName,
                        monitorId: modbusSensor.ModBusSensorID,
                        monitorIp: modbusSensor.BusConfig.TcpIp,
                        monitorName: $"{EnumHelper.GetDescription(modbusSensor.SensorType)}_{modbusSensor.Address}",
                        monitorConnType: EnumHelper.GetDescription(modbusSensor.BusConfig.Type),
                        data: "--", // 当前列表的【数据】字段给空
                        alarms: ADUAlarms
                    );
                res.Add(obj);
            }
            return res;
        }
        #endregion


        #region GetHADUChannelStatus 接口实现
        /// <summary>
        /// 4、获取采集单元通道状态列表
        /// </summary>
        /// <param name="monitorID">采集器ID</param>
        /// <param name="monitorType">测量事件类型</param>
        /// <returns></returns>
        internal List<HADUChannelStatusDTO> GetHADUChannelStatusList(string monitorID, EnumMonitorType monitorType)
        {
            List<HADUChannelStatusDTO> res = new List<HADUChannelStatusDTO>();
            if (monitorType == EnumMonitorType.TVM_BFM)
            {
                BFMADUInfo ultInfo = bfmInfoRepository.GetBFMADUInfoByID(monitorID);
                if (ultInfo == null)
                {
                    ALog.Debug($"不存在{monitorID}的UltrasonicDeviceInfo");
                    return res;
                }

                // 后续超声表中新增风场ID后就不用根据设备树获取风场ID
                string stationID = devTreeRepsitory.GetMeasLocationByMeaslocID(ultInfo.ChannelMappers.First().MeasLocationID).StationID;

                // 获取该采集器下全部的通道状态
                List<ChannelStatusAlarm> alarms = alarmStatusRepository.GetChannelStatusByHADUId(stationID, monitorID);

                foreach (var ult in ultInfo.ChannelMappers)
                {
                    // 获取该测点的设备树信息
                    DevMeasLocation? measloc = devTreeRepsitory.GetMeasLocationByMeaslocID(ult.MeasLocationID);
                    // 根据测点ID获取通道状态
                    ChannelStatusAlarm? alarm = alarms.FirstOrDefault(o => o.MeaslocID == ult.MeasLocationID);

                    HADUChannelStatusDTO obj = new HADUChannelStatusDTO(
                        guid: Guid.NewGuid().ToString("N"),
                        channel: ult.Channel,
                        measLoctionName: measloc.MeasLoctionName,
                        measlocID: ult.MeasLocationID,
                        unit: "",
                        alarm: alarm
                    );
                    res.Add(obj);
                }
            }
            else
            {
                HADUInfo HADUInfo = devTreeRepsitory.GetHADUInfoByID(monitorID);
                if (HADUInfo == null)
                {
                    ALog.Debug($"不存在{monitorID}的HADUInfo");
                    return res;
                }

                // 获取该采集器下全部的通道状态
                List<ChannelStatusAlarm> alarms = alarmStatusRepository.GetChannelStatusByHADUId(HADUInfo.StationID, monitorID);

                foreach (HADUChannelInfo channelItem in HADUInfo.Channels)
                {
                    // 由于将塔顶和塔基属于Modbus自检，故在此统计时将不统计塔顶和塔基
                    if (channelItem.MeasLocID.Contains("TOWTOP") || channelItem.MeasLocID.Contains("TOWFDN"))
                    {
                        continue;
                    }

                    // 获取该测点的设备树信息
                    DevMeasLocation? measloc = devTreeRepsitory.GetMeasLocationByMeaslocID(channelItem.MeasLocID);
                    // 根据测点ID获取通道状态
                    ChannelStatusAlarm? alarm = alarms.FirstOrDefault(o => o.MeaslocID == channelItem.MeasLocID);

                    HADUChannelStatusDTO obj = new HADUChannelStatusDTO(
                        guid: Guid.NewGuid().ToString("N"),
                        channel: channelItem.ChannelID,
                        measLoctionName: measloc.MeasLoctionName,
                        measlocID: channelItem.MeasLocID,
                        unit: "V",
                        alarm: alarm
                    );
                    res.Add(obj);
                }
            }
            return res.OrderBy(o => o.ChannelNum).ToList();
        }
        #endregion


        #region GetValueTrend接口实现
        /// <summary>
        /// 6、获取趋势
        /// </summary>
        /// <param name="measlocID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        internal TimeframeMonitorDataDTO GetTimeframeMonitorData(EnumMonitorType monitorType, List<string> measIDList, DateTime startTime, DateTime endTime)
        {
            if (monitorType == EnumMonitorType.TVM_BFM)
            {
                return GetBfmHADUMonitorData(measIDList, startTime, endTime);
            }
            else
            {
                return GetVibHADUMonitorData(measIDList, startTime, endTime);
            }
        }

        /// <summary>
        /// 获取螺栓轴力的峰峰值趋势
        /// </summary>
        /// <param name="measIDList">测点ID列表</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        private TimeframeMonitorDataDTO GetBfmHADUMonitorData(List<string> measIDList, DateTime startTime, DateTime endTime)
        {
            TimeframeMonitorDataDTO res = new TimeframeMonitorDataDTO();
            res.UpperLine = null;
            res.LowerLine = 0;
            res.UnitY = "";
            res.Datas = new List<MonitorMeaslocDataDTO>();

            foreach (string measID in measIDList)
            {
                DevMeasLocation? devMeas = devTreeRepsitory.GetMeasLocationByMeaslocID(measID);
                if (devMeas == null) continue;

                // 在Config表中获取峰峰值阈值
                BFMSysCheckConfig bfmSysCheckConfig = BFMSysCheckConfigDB.Instance.GetBFMSysCheckConfig(devMeas.StationID, devMeas.DeviceID, measID);
                if (bfmSysCheckConfig != null)
                {
                    res.LowerLine = bfmSysCheckConfig.EWPPKThreshold;
                }

                MonitorMeaslocDataDTO obj = new MonitorMeaslocDataDTO();
                obj.measlocName = devMeas.MeasLoctionName;
                obj.data = new List<object[]>();


                // 查询轴力波形
                List<TWAxialForceData> AxialForceDatas = deviceBFMReadWaveDB.GetMeasWaveData(devMeas.StationID, devMeas.DeviceID, startTime, endTime).OfType<TWAxialForceData>().Where(o => o.MeasLocID == measID).ToList();
                // 查询回波波形
                List<TWEchoData> echoDatas = deviceBFMReadWaveDB.GetEchoWaveData(devMeas.StationID, devMeas.DeviceID, startTime, endTime).OfType<TWEchoData>().Where(o => o.MeasLocID == measID).ToList();


                if (AxialForceDatas != null && AxialForceDatas.Count != 0)
                {
                    // 列表中已经添加轴力波形
                    obj.data.AddRange(GetAxialFroceData(AxialForceDatas));

                    if (echoDatas.Count != 0)
                    {
                        AddEchoWaveData(obj.data, echoDatas);
                    }
                }
                obj.data = obj.data.OrderBy(o => o[0]).ToList();

                res.Datas.Add(obj);
            }

            return res;
        }

        /// <summary>
        /// 根据轴力波形添加趋势数据
        /// </summary>
        /// <param name="waveData">轴力数据</param>
        /// <returns></returns>
        internal List<object[]> GetAxialFroceData(List<TWAxialForceData> waveData)
        {
            List<object[]> res = waveData.GroupBy(v => v.AcqTime)
                .Select(g => g.OrderByDescending(v => v.AcqTime).First())
                .OrderBy(v => v.AcqTime)
                .Select(v => new object[]
                {
                    v.AcqTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Math.Round(v.EchoPPK.Value, 2),
                    false
                }).ToList();
            return res;
        }

        /// <summary>
        /// 根据回波波形添加趋势数据
        /// </summary>
        /// <param name="res">特征值趋势结果列表</param>
        /// <param name="echoDatas">回波波形列表</param>
        private void AddEchoWaveData(List<object[]> res, List<TWEchoData> echoDatas)
        {
            var data = echoDatas.GroupBy(v => v.AcqTime).Select(g => g.OrderByDescending(v => v.AcqTime).First()).OrderBy(v => v.AcqTime);

            foreach (var item in data)
            {
                string time = item.AcqTime.ToString("yyyy-MM-dd HH:mm:ss");
                // 查找已存在的项
                var existingItem = res.FirstOrDefault(v => v[0].ToString() == time);

                if (existingItem != null)
                {
                    existingItem[2] = true;
                }
                else
                {
                    res.Add(new object[] { time, Math.Round(item.PPK.Value, 2), true });
                }
            }
        }

        /// <summary>
        /// 获取直流分量趋势
        /// </summary>
        /// <param name="measIDList">测点ID列表</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        internal TimeframeMonitorDataDTO GetVibHADUMonitorData(List<string> measIDList, DateTime startTime, DateTime endTime)
        {
            TimeframeMonitorDataDTO res = new TimeframeMonitorDataDTO();
            res.UpperLine = upperLine;
            res.LowerLine = lowerLine;
            res.UnitY = "V";
            res.Datas = new List<MonitorMeaslocDataDTO>();

            foreach (string measID in measIDList)
            {
                DevMeasLocation? devMeas = devTreeRepsitory.GetMeasLocationByMeaslocID(measID);
                if (devMeas == null) continue;

                MonitorMeaslocDataDTO obj = new MonitorMeaslocDataDTO();
                obj.measlocName = devMeas.MeasLoctionName;
                obj.data = new List<object[]>();

                // 取测点对应机组的全部波形
                List<TWVibData> twVibDatas = deviceWaveRead.GetMeasWaveData(devMeas.StationID, devMeas.DeviceID, devMeas.MeasDataType, startTime, endTime).OfType<TWVibData>().ToList();
                if (twVibDatas.Count != 0)
                {
                    var groupVib = twVibDatas.Where(o => o.MeasLocID == measID).ToList();
                    obj.data.AddRange(GetDCDatas(groupVib));
                }
                res.Datas.Add(obj);
            }

            return res;
        }

        /// <summary>
        /// 根据振动波形数据添加趋势
        /// </summary>
        /// <param name="waveData">振动波形</param>
        /// <returns></returns>
        internal List<object[]> GetDCDatas(List<TWVibData> waveData)
        {
            if (waveData == null) return new List<object[]>();

            List<object[]> res = waveData.GroupBy(v => v.AcqTime)
                .Select(g => g.OrderByDescending(v => v.AcqTime).First())
                .OrderBy(v => v.AcqTime)
                .Select(v => new object[]
                {
                    v.AcqTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    Math.Round(v.SensorVoltage, 2)
                }).ToList();
            return res;
        }
        #endregion

        #region 获取BFM回波波形
        /// <summary>
        /// 获取BFM回波波形
        /// </summary>
        /// <param name="measlocID">测点ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <returns></returns>
        internal BFMEchoWaveDTO GetBFMEchoWaveData(string measlocID, string acqTime)
        {
            try
            {
                BFMEchoWaveDTO waveDatas = new BFMEchoWaveDTO(); //返回对象数据
                waveDatas.UnitX = "";
                waveDatas.UnitY = "V";
                waveDatas.Datas = new List<BFMEchoWaveDataDTO>();

                // 根据测点ID获取设备树
                DevMeasLocation measObj = devTreeRepsitory.GetMeasLocationByMeaslocID(measlocID) ?? new();

                // 获取标准回波波形
                TWDataBase basedata = deviceBFMReadWaveDB.GetBaseEchoWaveData(measObj.StationID, measObj.DeviceID, measObj.MeasLoctionID);
                if (basedata != null)
                {
                    waveDatas.Datas.Add(ConvertToBFMEchoWaveDTO(basedata, "基准回波"));
                }

                // 获取某个时间点的回波波形
                TWDataBase data = deviceBFMReadWaveDB.GetEchoWaveData(measObj.StationID, measObj.DeviceID, measObj.MeasLoctionID, DateTime.Parse(acqTime));
                if (data != null)
                {
                    waveDatas.Datas.Add(ConvertToBFMEchoWaveDTO(data, $"{measObj.MeasLoctionName}_{data.AcqTime.ToString("yyyy-MM-dd HH:mm:ss")}"));
                }

                return waveDatas;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"ConvertToWave-波形获取报错");
                return new BFMEchoWaveDTO();
            }
        }

        /// <summary>
        /// 将回波波形/基准回波波形转化为接口返回值
        /// </summary>
        /// <param name="data">波形数据</param>
        /// <param name="legend">图例描述信息</param>
        /// <returns></returns>
        private BFMEchoWaveDataDTO ConvertToBFMEchoWaveDTO(TWDataBase data, string legend)
        {
            BFMEchoWaveDataDTO res = new BFMEchoWaveDataDTO();
            res.Legend = legend;
            res.data = new List<List<double>>();
            List<List<double>> datas = new List<List<double>>();
            if (data.WaveData != null && data.WaveData.Length != 0)
            {
                double intevel = 1;
                double xi = 0;
                for (int i = 0; i < data.WaveData.Length; i++)
                {
                    List<double> wave = new List<double>();
                    xi += intevel;
                    double yi = data.WaveData[i];
                    double.TryParse(xi.ToString("G9"), out double xi1);
                    double.TryParse(yi.ToString("G5"), out double yi1);
                    wave.Add(xi1);
                    wave.Add(yi1);
                    datas.Add(wave);
                }
            }
            res.data = datas;
            return res;
        }
        #endregion
    }
}
