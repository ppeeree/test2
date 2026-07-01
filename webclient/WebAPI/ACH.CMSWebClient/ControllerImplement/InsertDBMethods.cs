using ACH.ACHLog.SeriLog;
using ACH.Alarm.Entity;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.AnylzerData;
using ACH.DataEntity.App;
using ACH.DataEntity.Bladex;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.Enum;
using ACH.DataEntity.EnumType;
using ACH.DataEntity.DevTree;
using ACH.DataEntity.TheWeave;
using ACH.DataRepository.DevTree;
using ACH.DBConn.Bladex;
using ACH.DBConn.Dat;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using Dm.util;
using Microsoft.Extensions.Configuration.Json;
using SqlSugar;
using EnumSignalType = ACH.DataEntity.Enum.EnumSignalType;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class InsertDBMethods
    {
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        static BladexDBContext bladexDBContext = new BladexDBContext();
        WindParkAPIMethods windParkAPIMethods;
        StatusDBContext statusDBContext;

        private readonly IConfiguration configuration;
        public InsertDBMethods()
        {
            configuration = new ConfigurationBuilder()
                .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = false })
                .Build();

            statusDBContext = new StatusDBContext(configuration);
            windParkAPIMethods = new WindParkAPIMethods(configuration);
        }

        /// <summary>
        /// 按照01机组的设备树创建卓盛风场的其余N个机组
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal static void CreatDevLocation(string deviceID, int startNum, int endNum)
        {
            List<DevMeasLocation> res = new List<DevMeasLocation>();
            var turbineList = _devTreeRepository.GetMeaslocationByDeviceID(deviceID);
            for (int i = startNum; i <= endNum; i++)
            {
                string deviceCode = i.ToString("D4");
                foreach (var meas in turbineList)
                {
                    string devID = $"{meas.StationID}{deviceCode}";
                    DevMeasLocation obj = new DevMeasLocation();
                    obj.StationID = meas.StationID;
                    obj.StationName = meas.StationName;
                    obj.DeviceID = devID;
                    obj.DeviceName = i.ToString("D2");
                    obj.ComponentID = meas.ComponentID.Replace(meas.DeviceID, devID);
                    obj.ComponentName = meas.ComponentName;
                    obj.MeasLoctionID = meas.MeasLoctionID.Replace(meas.DeviceID, devID);
                    obj.MeasLoctionName = meas.MeasLoctionName;
                    obj.MeasLoctionNickName = meas.MeasLoctionNickName;
                    obj.MeasDataType = meas.MeasDataType;
                    obj.SignalType = meas.SignalType;
                    obj.Section = meas.Section;
                    obj.Orientation = meas.Orientation;
                    obj.Remark = meas.Remark;
                    obj.sort = meas.sort;
                    res.Add(obj);
                    // _devTreeRepository.AddMeasLocation(obj);
                }
            }
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteCommand();
            }
        }


        // 9.1、SQL：查询该机组下的设备信息
        public List<account_space_basic> GetTurbineSpaceBasicList(string turbineID)
        {
            try
            {
                using (SqlSugarClient db = new SqlSugarClient(bladexDBContext.GetJAVAConnection()))
                {
                    List<account_space_basic> list = db.Queryable<account_space_basic>().Where(o => o.space_guid == turbineID).ToList();
                    return list;
                }
            }
            catch (Exception ex)
            {
                ALog.Debug($"account_space_basic表查询报错：{ex.Message}");
                return new List<account_space_basic>();
            }
        }

        /// <summary>
        /// 往DeviceInfo中添加数据
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal void AddDeviceInfo(string stationID)
        {
            List<DeviceInfo> res = new List<DeviceInfo>();
            // 获取全部设备树
            var deviceList = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);

            foreach (var item in deviceList)
            {
                var first = item.First();
                // 获取java数据库中的配置信息
                List<account_space_basic> basics = GetTurbineSpaceBasicList(item.Key);

                if (basics.Count == 0)
                {
                    continue;
                }

                // 将属性存储到字典中，方便快速查找
                Dictionary<string, account_space_basic> basicDict = basics.GroupBy(b => b.space_basic_code).ToDictionary(g => g.Key, g => g.First());

                /*DeviceInfo obj = new DeviceInfo();
                obj.StationId = first.StationID;
                obj.DeviceId = item.Key;
                obj.DeviceName = first.DeviceName;
                obj.OperationlDate = windParkAPIMethods.GetValueFromDict(basicDict, "operational_date") == "" ? DateTime.MinValue : DateTime.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "operational_date"));
                obj.DeviceModel = windParkAPIMethods.GetValueFromDict(basicDict, "windturbine_model");
                obj.DeviceVindor = windParkAPIMethods.GetValueFromDict(basicDict, "manufacturer");
                obj.Longitude = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "longitude"));
                obj.Latitude = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "latitude"));
                obj.Elevation = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "elevation"));
                obj.FactedPower = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "power_rating"));
                obj.TransmissionForm = "双馈";
                obj.TransmissionRatio = 100;
                obj.RatedGeneratorSpeed = 1800;
                obj.GridConnectedGeneratorSpeed = 1500;
                obj.MeaslocPlanName = windParkAPIMethods.GetValueFromDict(basicDict, "measureTypeName");
                obj.MeaslocPlan = windParkAPIMethods.GetValueFromDict(basicDict, "measlocDeviceProperty");
                obj.DeviceModelPlan = windParkAPIMethods.GetValueFromDict(basicDict, "modelDeviceProperty");
                obj.DeviceCompPlan = windParkAPIMethods.GetValueFromDict(basicDict, "deviceParentProperty");
                obj.Remark = "";
                obj.Sort = null;

                res.Add(obj);*/
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        /// <summary>
        /// 修改DevMeasLocation中的风场名称
        /// </summary>
        /// <param name="stationID">风场ID，用于查询</param>
        /// <param name="changeStationName">修改的风场名称</param>
        /// <exception cref="NotImplementedException"></exception>
        internal static void ChangeStationName(string stationID, string changeStationName)
        {
            List<DevMeasLocation> list = _devTreeRepository.GetAllMeasLocation(stationID);

            List<DevMeasLocation> res = new List<DevMeasLocation>();

            foreach (var item in list)
            {
                item.StationName = changeStationName;
                res.Add(item);
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Updateable(res).ExecuteCommand();
            }
        }


        internal void AddStationInfo(string stationID)
        {
            SqlSugarClient db1 = DevTreeDBContext.GetDevTreeDBConn();
            List<StationInfo> res = new List<StationInfo>();
            // 获取全部设备树
            var stationList = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.StationID);

            foreach (var item in stationList)
            {
                var first = item.First();
                // 获取java数据库中的配置信息
                List<account_space_basic> basics = GetTurbineSpaceBasicList(first.DeviceID);

                StationInfo obj = new StationInfo();

                if (basics.Count == 0)
                {
                    obj.StationId = first.StationID;
                    obj.StationName = first.StationName;
                    obj.Longitude = 0;
                    obj.Latitude = 0;
                    obj.Elevation = 0;
                    obj.OperationlDate = DateTime.Now;
                    obj.DeviceModel = "";
                    obj.DeviceMaker = "";
                    obj.StationAddress = "";
                    obj.TransmissionForm = "";
                    obj.DetectionObject = "传动链";

                }
                else
                {
                    /*// 将属性存储到字典中，方便快速查找
                    Dictionary<string, account_space_basic> basicDict = basics.GroupBy(b => b.space_basic_code).ToDictionary(g => g.Key, g => g.First());

                    // StationInfo obj = new StationInfo();
                    obj.StationId = first.StationID;
                    obj.StationName = first.StationName;
                    obj.Longitude = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "longitude"));
                    obj.Latitude = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "latitude"));
                    obj.Elevation = double.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "elevation"));
                    obj.OperationlDate = DateTime.Parse(windParkAPIMethods.GetValueFromDict(basicDict, "operational_date"));
                    obj.DeviceModel = windParkAPIMethods.GetValueFromDict(basicDict, "windturbine_model");
                    obj.DeviceVindor = windParkAPIMethods.GetValueFromDict(basicDict, "manufacturer");
                    obj.StationAddress = windParkAPIMethods.GetValueFromDict(basicDict, "province");
                    obj.TransmissionForm = "";
                    obj.DetectionObject = "传动链";*/
                }
                res.Add(obj);
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        internal void AddUserInfo()
        {
            List<UserInfo> res = new List<UserInfo>();
            using (SqlSugarClient db = new SqlSugarClient(bladexDBContext.GetJAVAConnection()))
            {
                List<blade_user> list = db.Queryable<blade_user>().ToList();

                foreach (blade_user user in list)
                {
                    UserInfo obj = new UserInfo();
                    obj.Id = user.name;
                    obj.Name = user.name;
                    obj.Account = user.account;
                    obj.Password = user.password;
                    res.add(obj);
                }

            }

            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        internal static void AddUserStationMap(string stationID, string userAcount)
        {
            List<UserStationMapper> res = new List<UserStationMapper>();
            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                List<UserInfo> userList = db.Queryable<UserInfo>().Where(o => o.Account == userAcount).ToList();
                foreach (var user in userList)
                {
                    UserStationMapper obj = new UserStationMapper();

                    obj.UserId = user.Id;
                    obj.StationID = stationID;
                    res.Add(obj);
                }

                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        internal void AddSystemMenu()
        {
            List<SystemMenu> res = new List<SystemMenu>();
            using (SqlSugarClient db = new SqlSugarClient(bladexDBContext.GetJAVAConnection()))
            {
                List<blade_menu> list = db.Queryable<blade_menu>().ToList();

                foreach (var item in list)
                {
                    SystemMenu obj = new SystemMenu();
                    obj.ID = item.id;
                    obj.ParentID = item.parent_id;
                    obj.Name = item.name;
                    obj.Code = item.code;
                    obj.Alias = item.alias;
                    obj.Path = item.path;
                    obj.Source = item.source;
                    obj.IsDelete = item.is_deleted == 0 ? true : false;
                    obj.IsOpen = item.is_open == 0 ? true : false;
                    obj.Sort = item.sort;
                    res.Add(obj);
                }

            }

            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }


        /// <summary>
        /// 同步theweave数据库和Dat数据库
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        internal static void SyncDevTree(string stationID)
        {
            List<DevMeasLocation> res = new List<DevMeasLocation>();
            List<DevMeasLocation> data = _devTreeRepository.GetAllMeasLocation(stationID);

            // 1、获取theweave数据库中全部设备树信息
            using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
            {
                List<Measurement> list = db.Queryable<Measurement>().Where(o => o.StationID == stationID).ToList().DistinctBy(o => o.MeasLoctionID).ToList();

                foreach (Measurement item in list)
                {
                    List<DevMeasLocation> obj = data.Where(o => o.MeasLoctionID == item.MeasLoctionID).ToList();
                    if (obj == null || obj.Count == 0)
                    {
                        // 需新增
                        DevMeasLocation devMeasLocation = new DevMeasLocation();
                        devMeasLocation.StationID = item.StationID;
                        devMeasLocation.StationName = item.StationName;
                        devMeasLocation.DeviceID = item.DeviceID;
                        devMeasLocation.DeviceName = item.DeviceName;
                        devMeasLocation.ComponentID = item.ComponentID;
                        devMeasLocation.ComponentName = item.ComponentName;
                        devMeasLocation.MeasLoctionID = item.MeasLoctionID;
                        devMeasLocation.MeasLoctionName = item.MeasLoctionName;
                        devMeasLocation.MeasLoctionNickName = item.MeasLoctionNickName;
                        devMeasLocation.MeasDataType = GetMeasDataType(item.MeasLoctionID);
                        devMeasLocation.SignalType = DataEntity.Common.EnumSignalType.VT;//GetSingalType(item.MeasLoctionID);
                        devMeasLocation.Section = GetSection(item.MeasLoctionID);
                        devMeasLocation.Orientation = "";
                        devMeasLocation.Remark = "";
                        devMeasLocation.sort = 0;
                        res.Add(devMeasLocation);

                        //  _devTreeRepository.AddMeasLocation(devMeasLocation);
                    }
                }

            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        private static EnumMonitorType GetMeasDataType(string measLoctionID)
        {
            if (measLoctionID.contains("FL1") || measLoctionID.contains("FL2") || measLoctionID.contains("FL3"))
            {
                return EnumMonitorType.TVM_FLG_GAP;
            }
            else if (measLoctionID.contains("PL1"))
            {
                return EnumMonitorType.TVM_CBF;
            }
            else if (measLoctionID.contains("TOP") || measLoctionID.contains("FDN"))
            {
                return EnumMonitorType.TVM_STE;
            }
            else if (measLoctionID.contains("BL"))
            {
                return EnumMonitorType.BVM;
            }
            else
            {
                return EnumMonitorType.CVM;
            }
        }

        private static string GetSection(string MeasLoctionID)
        {
            if (MeasLoctionID.contains("PL1"))
            {
                return "PL1";
            }
            else if (MeasLoctionID.contains("FL1"))
            {
                return "FL1";
            }
            else if (MeasLoctionID.contains("FL2"))
            {
                return "FL2";
            }
            else if (MeasLoctionID.contains("FL3"))
            {
                return "FL3";
            }
            else if (MeasLoctionID.contains("TOP"))
            {
                return "TOP";
            }
            else if (MeasLoctionID.contains("FDN"))
            {
                return "FDN";
            }
            else if (MeasLoctionID.contains("MST"))
            {
                return "MST";
            }
            else if (MeasLoctionID.contains("GBX"))
            {
                return "GBX";
            }
            else if (MeasLoctionID.contains("GEN"))
            {
                return "GEN";
            }
            else if (MeasLoctionID.contains("BL1"))
            {
                return "BL1";
            }
            else if (MeasLoctionID.contains("BL2"))
            {
                return "BL2";
            }
            else if (MeasLoctionID.contains("BL3"))
            {
                return "BL3";
            }
            else
            {
                return "";
            }
        }





        internal void AddAModbusBusConfig(string stationID)
        {
            List<ModbusBusConfig> res = new List<ModbusBusConfig>();
            var data = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);

            int i = 1;
            foreach (var item in data)
            {
                ModbusBusConfig obj1 = new ModbusBusConfig();
                obj1.BusID = 1;
                obj1.DeviceID = item.Key;
                obj1.SampleRate = 64;
                obj1.SampleInterval = 5;
                obj1.SampleTime = 5;
                obj1.SampleTime = 128;
                obj1.Type = EnumModbusBusType.Serial;
                obj1.SerialName = "/dev/ttymxc1";
                obj1.SerialBauRate = 19200;
                obj1.TcpIp = $"192.168.0.{i}";
                obj1.TcpPort = null;


                ModbusBusConfig obj2 = new ModbusBusConfig();
                obj2.BusID = 2;
                obj2.DeviceID = item.Key;
                obj2.SampleRate = 64;
                obj2.SampleInterval = 5;
                obj2.SampleTime = 5;
                obj2.SampleTime = 128;
                obj2.Type = EnumModbusBusType.TCP;
                obj2.SerialName = null;
                obj2.SerialBauRate = null;
                obj2.TcpIp = $"192.168.0.{i}";
                obj2.TcpPort = 1031;

                res.Add(obj1);
                res.Add(obj2);
                i++;
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }



        internal void AddMeasLoc2DConfig()
        {
            List<MeaslocCircleModelConfig> res = new List<MeaslocCircleModelConfig>();
            var index = 30;
            // 一个模型一个ID
            var id = Guid.NewGuid().ToString("N");
            var si = 360 / index;
            for (int i = 0; i < index; i++)
            {
                MeaslocCircleModelConfig obj = new MeaslocCircleModelConfig();
                obj.ConfigID = id;
                obj.ConfigName = "30个钢索方案";
                obj.CircleType = EnumCircleType.CBF;
                obj.MeaslocCode = (i).ToString();
                obj.AngleDegree = si * i;
                obj.CircleMeaslocName = (i + 1).ToString();
                res.Add(obj);
            }

            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        internal void AddMeasLoc2DConfigMapper()
        {
            List<MeaslocCircleConfigMapper> res = new List<MeaslocCircleConfigMapper>();
            var data = _devTreeRepository.GetAllMeasLocation("XJDL001").GroupBy(o => o.DeviceID).OrderBy(o => o.Key);
            int i = 0;
            foreach (var device in data)
            {
                i++;
                MeaslocCircleConfigMapper obj = new MeaslocCircleConfigMapper();
                obj.StationID = "";
                obj.CircleType = EnumCircleType.BFM;
                obj.DeviceID = device.Key;

                if (i <= 21)
                {
                    obj.ConfigID = "ecc315b656a74d58bdf3d4e96b3d9390";
                }
                else
                {
                    obj.ConfigID = "2f1a00630a884e9eb2e6af1fa8c1212a";
                }

                res.Add(obj);
            }


            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        internal void AddUltrasonicChannelInfo(string stationID)
        {
            // 1. 参数化 SQL（SqlSugar 用 {xxx} 占位）
            const string sql = @"INSERT INTO main.UltrasonicChannelInfo
        (DeviceID,Channel,BoltLength,EchoWidth,Gain,RefTime,RefTemp,ProbeFreq,Duty,Forward,Reverse,Threshold,C0,C1)
        VALUES ({DeviceID},{Channel},90.0,10,15.0,32318.0,24.67,2.0,50,2,1,0,-0.000145800923633033,1.11495167896608);";

            // 2. 一次构造 720 行参数列表
            var inserts = new List<UltrasonicChannelInfo>(30 * 24);
            for (int dev = 3; dev <= 32; dev++)
            {
                string deviceId = $"{stationID}{dev:D4}";
                for (int ch = 1; ch <= 24; ch++)
                {
                    inserts.Add(new UltrasonicChannelInfo
                    {
                        DeviceID = deviceId,
                        Channel = ch,
                        BoltLength = 90.0,
                        EchoWidth = 10,
                        Gain = 15.0,
                        RefTime = 32318.0,
                        RefTemp = 24.67,
                        ProbeFreq = 2.0,
                        Duty = 50,
                        Forward = 2,
                        Reverse = 1,
                        Threshold = 0,
                        C0 = -0.000145800923633033,
                        C1 = 1.11495167896608
                    });
                }
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(inserts).ExecuteCommand();
            }
        }

        internal void AddUltrasonicChannelMapper(string stationID)
        {
            List<UltrasonicChannelMapper> res = new List<UltrasonicChannelMapper>();
            var data = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID).OrderBy(o => o.Key);

            foreach (var item in data)
            {
                if (item.Key == "XJDL0010001" || item.Key == "XJDL0010002")
                {
                    continue;
                }

                int i = 1;
                foreach (var meas in item)
                {
                    if (meas.MeasLoctionID.Contains("BOL"))
                    {
                        UltrasonicChannelMapper obj = new UltrasonicChannelMapper();
                        obj.DeviceID = meas.DeviceID;
                        obj.Channel = i;
                        obj.MeasLocationID = meas.MeasLoctionID;
                        obj.StationID = meas.StationID;
                        obj.ComponentID = meas.ComponentID;
                        res.Add(obj);
                        i++;
                    }
                }
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        internal void AddUltrasonicDeviceInfo(string stationID)
        {
            List<UltrasonicDeviceInfo> res = new List<UltrasonicDeviceInfo>();
            var data = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID).OrderBy(o => o.Key);

            int i = 3;
            foreach (var item in data)
            {
                if (item.Key == "XJDL0010001" || item.Key == "XJDL0010002")
                {
                    continue;
                }

                UltrasonicDeviceInfo obj = new UltrasonicDeviceInfo();
                obj.DeviceID = item.Key;
                obj.DeviceIP = $"192.168.0.{i}";
                obj.DevicePort = 23;
                obj.DeviceAddress = 1;
                obj.Frequency = 10;
                obj.MultiChannelSampleCount = 1;
                obj.MultiChannelSampleInterval = 5;
                obj.SingleChannelSampleCount = 10;
                obj.SingleChannelSampleInterval = 60;
                res.Add(obj);
                i++;
            }

            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        internal void AddChannelStatusAlarm(string stationID)
        {
            List<ChannelStatusAlarm> res = new List<ChannelStatusAlarm>();
            List<UltrasonicChannelMapper> mapper = new List<UltrasonicChannelMapper>();
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                mapper = db.Queryable<UltrasonicChannelMapper>().Where(o => o.StationID == stationID).ToList();
            }

            var data = _devTreeRepository.GetAllMeasLocation(stationID);
            foreach (var meas in data)
            {
                if (meas.MeasLoctionID.contains("TOWFDNNaA"))
                {
                    //  var select = mapper.Where(o => o.MeasLocationID == meas.MeasLoctionID).First();
                    ChannelStatusAlarm obj = new ChannelStatusAlarm();
                    obj.MeaslocID = $"{meas.DeviceID}TOWFDNNaA";
                    obj.DeviceID = meas.DeviceID;
                    obj.DeviceType = meas.MeasDataType;
                    obj.ChannelNumber = 0;
                    obj.ChannelStatus = EnumChannelStatus.Normal;
                    obj.Voltage = 0;
                    obj.AcqTime = DateTime.Now;

                    res.Add(obj);
                }
            }


            using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        internal void AddEigenValueAlarm(string stationID)
        {
            List<EigenValueAlarm> res = new List<EigenValueAlarm>();
            var data = _devTreeRepository.GetAllMeasLocation(stationID);
            Random _rnd = new Random();
            foreach (var item in data)
            {
                if (item.MeasLoctionID.Contains("BOL"))
                {
                    EigenValueAlarm obj = new EigenValueAlarm();
                    obj.MeasLocID = item.MeasLoctionID;
                    obj.EvCode = "AVG";
                    obj.AcqTime = DateTime.Now;
                    obj.MeasType = item.MeasDataType;
                    obj.DeviceID = item.DeviceID;
                    obj.EvStatus = EnumAlarmStatus.Normal;
                    obj.Value = _rnd.NextDouble() * (1.0 - 0.8) + 0.8;
                    obj.Unit = "kN";
                    res.Add(obj);
                }
            }
            using (SqlSugarClient db = statusDBContext.GetStatusDBConn(stationID))
            {
                db.Insertable(res).ExecuteCommand();
            }
        }

        /*internal void AddHADUChannelMapperHADUID(string stationID)
        {
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                List<HADUChannelMapper> data = db.Queryable<HADUChannelMapper>().Where(o => o.StationID == stationID).ToList();

                foreach (var item in data)
                {
                    if (item.MeasLocationID.Contains("TOWTOP"))
                    {
                        item.ADUID = $"{item.DeviceID}_TOP";
                    }
                    else if (item.MeasLocationID.Contains("TOWFDN"))
                    {
                        item.ADUID = $"{item.DeviceID}_FDN";
                    }
                    else if (item.MeasLocationID.Contains("PL"))
                    {
                        item.ADUID = $"{item.DeviceID}_PL";
                    }
                    else if (item.MeasLocationID.Contains("FL"))
                    {
                        item.ADUID = $"{item.DeviceID}_FL";
                    }
                    else if (item.MeasLocationID.Contains("BL"))
                    {
                        item.ADUID = $"{item.DeviceID}_BMS";
                    }
                    else
                    {
                        item.ADUID = $"{item.DeviceID}_CMS";
                    }
                }

                db.Updateable(data).ExecuteCommand();
            }
        }*/

        internal void AddHADUChannelInfo(string stationID)
        {
            List<HADUChannelInfo> res = new List<HADUChannelInfo>();
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                List<HADUChannelMapper> mappers = db.Queryable<HADUChannelMapper>().Where(o => o.MeasLocationID.Contains(stationID)).ToList();

                foreach (var item in mappers)
                {
                    HADUChannelInfo obj = new HADUChannelInfo();
                    obj.HADUID = $"{item.DeviceID}_CMS";
                    obj.ChannelID = item.MapChno;
                    obj.MeasLocID = item.MeasLocationID;

                    res.Add(obj);
                }

                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        internal void AddHADUInfo(string stationID)
        {
            List<HADUInfo> res = new List<HADUInfo>();
            var data = _devTreeRepository.GetAllMeasLocation(stationID).GroupBy(o => o.DeviceID);
            var i = 0;
            foreach (var item in data)
            {
                i++;
                var type = item.GroupBy(o => o.MeasDataType);
                foreach (var typeItem in type)
                {
                    if (typeItem.Key == EnumMonitorType.BVM)
                    {
                        HADUInfo obj = new HADUInfo();
                        obj.HADUID = $"{item.Key}_BMS";
                        obj.HADUIP = $"192.168.124.{i + 100}";
                        obj.MonitorType = EnumMonitorType.BVM;
                        obj.StartDelayMin = 0;
                        obj.StationID = stationID;
                        res.Add(obj);
                    }
                    if (typeItem.Key == EnumMonitorType.CVM)
                    {
                        HADUInfo obj = new HADUInfo();
                        obj.HADUID = $"{item.Key}_CMS";
                        obj.HADUIP = $"192.168.124.{i}";
                        obj.MonitorType = EnumMonitorType.CVM;
                        obj.StartDelayMin = 0;
                        obj.StationID = stationID;
                        res.Add(obj);
                    }
                }
            }
            using (SqlSugarClient db = DevTreeDBContext.GetDevTreeDBConn())
            {
                db.Insertable(res).ExecuteReturnIdentity();
            }
        }

        public class HADUChannelMapper
        {
            public string StationID { get; set; }

            public string MapStationID { get; set; }

            public string DeviceID { get; set; }

            public string MapDeviceID { get; set; }

            public string ComponentID { get; set; }

            public EnumSignalType SingalType { get; set; }

            [SugarColumn(IsPrimaryKey = true)]
            public string MeasLocationID { get; set; }

            public int MapChno { get; set; }

            public int HADUChno { get; set; }
        }
    }
}
