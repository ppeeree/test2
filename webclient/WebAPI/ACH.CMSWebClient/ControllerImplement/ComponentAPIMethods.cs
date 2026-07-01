using ACH.CMSWebClient.ControllerModel.Component;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.App;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTree;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.StatusTree;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DBRepository.DevTree;
using ACH.DBRepository.SD;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Component;
using Dm.util;
using NetTaste;
using SqlSugar;
using EnumCircleType = ACH.DataEntity.Enum.EnumCircleType;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class ComponentAPIMethods
    {
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        IAppDatRepository appDatRepository = new AppDatRepository();
        ComponentHelper componentHelper = new ComponentHelper();
        IAlarmStatusRepository alarmStatusRepository;

        IDevTreeRepository webDevTreeRepository = new DevTreeRepository();
        public ComponentAPIMethods(IConfiguration configuration)
        {
            alarmStatusRepository = new AlarmStatusRepository(configuration);
        }

        #region 整机页面特征值卡片
        // 1.1、整机页面 特征值卡片 
        public DeviceEvCardStatusDTO GetTurbineEvCardList(string id, string type)
        {
            DTDeviceStatus DeviceTree = alarmStatusRepository.GetDeviceId(id); // 获取该机组下的所有机组列表
            if (DeviceTree == null || DeviceTree.CompList == null || DeviceTree.CompList.Count == 0)
            {
                return new DeviceEvCardStatusDTO();
            }

            DeviceEvCardStatusDTO res = new DeviceEvCardStatusDTO();

            res.TurbineId = id;
            res.TurbineState = DeviceTree.WindTurbuineStatus.ToString();
            res.TurbineStateTime = DeviceTree.WindTurbuineStatusTime.ToString("yyyy-MM-dd HH:mm:ss");
            res.PagecompList = HandlerPageCompList(id, DeviceTree);


            return res;
        }

        // 1.1、处理聚合部件
        private List<PageCompItemModel> HandlerPageCompList(string id, DTDeviceStatus deviceTree)
        {
            List<PageCompItemModel> res = new List<PageCompItemModel>();

            List<DeviceMeaslocPositopnPlan> planList = webDevTreeRepository.GetDeviceMeaslocPositopnPlan(id); // 机组的所有聚合部件列表
            var pagecompList = planList[0].children.Where(o => o.deviceCode == "windturbine").ToList().First().measlocPositionList; // 聚合部件列表

            // 每个聚合部件下的实体部件列表
            foreach (var pageitem in pagecompList)
            {
                PageCompItemModel obj = new PageCompItemModel(componentHelper.GetBigCompCodeByName(pageitem.measlocCode[0]), pageitem.measlocCode[0]);

                obj.PagecompCode = componentHelper.GetBigCompCodeByName(pageitem.measlocCode[0]);
                obj.PagecompName = pageitem.measlocCode[0];
                obj.CompList = new List<CompItemModel>();

                List<string> List = componentHelper.GetCompType(obj.PagecompCode); // 获取该聚合部件下的实体部件列表

                // 处理聚合部件下的实体部件
                foreach (string str in List)
                {
                    List<DTCompStatus> compList = deviceTree.CompList.Where(o => o.CompCode.Contains(str)).ToList();

                    if (compList == null || compList.Count < 1) continue;

                    DTCompStatus compItem = compList.First();

                    CompItemModel comp = new CompItemModel
                    {
                        CompCode = str,
                        CompFaultList = new List<CompFaultItem>(),
                        CompId = compItem.CompId,
                        CompName = compItem.CompName,
                        CompState = compItem.CompStatus.ToString().ToLower(),
                        CompStateTime = compItem.CompStatusTime.ToString(),
                        CardPosition = pageitem.cardPosition,
                        SpotPosition = pageitem.spot
                    };

                    obj.CompList.Add(comp);
                }

                if (obj.CompList.Count > 0) res.Add(obj);
            }
            return res;
        }
        #endregion


        #region 结构调整后卡片状态
        internal List<EvCardStatusDTO> CompEvStatusList(string deviceID, string pageCompType)
        {
            List<EvCardStatusDTO> res = new List<EvCardStatusDTO>();
            // 根据聚合部件类型获取该聚合部件下的所有实体部件列表 
            List<string> compCodeList = componentHelper.GetPageCompTypeDir()[pageCompType];

            // 获取该机组的该聚合部件下所有测点的三维信息
            List<PlanMeaslocModel> positionList = webDevTreeRepository.GetDeviceMeaslocPositopnPlan(deviceID).First().children.Where(o => o.deviceCode == pageCompType).First().measlocPositionList;

            // 根据机组ID获取该机组的RT状态树
            DTDeviceStatus deviceStatus = alarmStatusRepository.GetDeviceId(deviceID);
            if (deviceStatus == null || deviceStatus.CompList == null || deviceStatus.CompList.Count == 0)
            {
                return res;
            }
            // 获取测点Code包含在compCodeList中的列表
            List<DTMeaslocStatus> measlocList = deviceStatus.CompList.SelectMany(o => o.MeaslocList).ToList();
            List<DTMeaslocStatus> filtered = measlocList.Where(m => compCodeList.Any(c => m.MeaslocCode.Contains(c))).ToList();


            // 对设备树的测点列表进行分组
            if (pageCompType == "NAC")
            {
                Dictionary<string, List<DTMeaslocStatus>> group = GroupBySection(false, filtered);
                foreach (var item in group)
                {
                    // 每个分组一个卡片，卡片名称为测点名称
                    res.Add(ConvertNACEvCard(item.Value.FirstOrDefault(), positionList));
                }
            }
            else if (pageCompType == "ROT")
            {
                Dictionary<string, List<DTMeaslocStatus>> group = GroupBySection(true, filtered);
                foreach (var item in group)
                {
                    // 每个分组一个卡片，卡片名称特殊处理名称
                    res.Add(ConvertROTEvCard(item.Key, item.Value, positionList));
                }
            }
            else
            {
                Dictionary<string, List<DTMeaslocStatus>> group = GroupBySection(true, filtered);
                foreach (var item in group)
                {
                    // 将部件+截面转化为分布圆类型
                    EnumCircleType circleType = componentHelper.ConvertToCircleType(item.Key, item.Value.First().MeaslocId);

                    // 根据机组ID获取测点分布圆信息
                    List<MeaslocCircleModelConfig> circlePosition = appDatRepository.GetMeaslocCircleModelConfig(deviceID, circleType);
                    // 在分布圆配置表中查找同截面的配置信息
                    var circleData = circlePosition.Where(o => o.Section.contains(item.Key)).ToList();

                    // 每个分组一个卡片，卡片名称特殊处理名称
                    res.Add(ConvertTWWEvCard(pageCompType, item.Key, item.Value, positionList, circleData));
                }
            }
            return res;
        }

        /// <summary>
        /// 传动链：卡片名称为测点名称
        /// </summary>
        /// <param name="measlocObj">需要按照测点名称分组的测点数据列表</param>
        /// <param name="positionList">方案中所有测点三维信息</param>
        /// <returns></returns>
        private EvCardStatusDTO ConvertNACEvCard(DTMeaslocStatus measlocObj, List<PlanMeaslocModel> positionList)
        {
            // 在三维信息表中找同测点的信息
            var position = positionList.FirstOrDefault(p => measlocObj.MeaslocId.Contains(string.Join("", p.measlocCode)));

            // 获取设备树对象
            DevMeasLocation meas = _devTreeRepository.GetMeasLocationByMeaslocID(measlocObj.MeaslocId);

            var cardPosition = position == null ? new List<double>() : position.cardPosition;
            var spotPosition = position == null ? new List<double>() : position.spot;
            string iconCode = meas == null ? measlocObj.MeaslocCode : meas.ComponentID.Replace(meas.DeviceID, "");

            EvCardStatusDTO obj = new EvCardStatusDTO(
                title: measlocObj.MeaslocName,
                iconCode: iconCode,
                cardPosition: cardPosition,
                spotPosition: spotPosition
            );

            if (measlocObj.EigenValueList != null && measlocObj.EigenValueList.Count > 0)
            {
                obj.EvList = ConvertNACEVData(measlocObj.EigenValueList);
                obj.EvSummaryStatus = measlocObj.EigenValueList.Max(o => o.EvStatus).ToString().ToLower();
                obj.EvSummaryStatusTime = measlocObj.EigenValueList.Max(o => o.EvStatusTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return obj;
        }

        /// <summary>
        /// 传动链：特征值卡片不存在分布圆，且特征值名称不用特殊处理
        /// </summary>
        /// <param name="eigenValueList"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<EvStatusItemDTO> ConvertNACEVData(List<DTEvStatus> eigenValueList)
        {
            List<EvStatusItemDTO> res = new List<EvStatusItemDTO>();

            // 如果没有分布圆配置，只根据已有特征值处理
            foreach (var evItem in eigenValueList)
            {
                double raw = evItem.Value;
                double rounded = Math.Round(raw, 3, MidpointRounding.AwayFromZero);
                double.TryParse(evItem.Value.ToString("G5"), out double parsedValue);

                EvStatusItemDTO obj = new EvStatusItemDTO(evItem.EvName, evItem.EvCode, evItem.EvId, evItem.EvStatus.toString().ToLower(), evItem.EvStatusTime.ToString("yyyy-MM-dd HH:mm:ss"), parsedValue, evItem.Unit);

                res.Add(obj);
            }

            return res;
        }


        /// <summary>
        /// 风轮：卡片名称特殊处理
        /// </summary>
        /// <param name="measlocDir"></param>
        /// <returns></returns>
        private EvCardStatusDTO ConvertROTEvCard(string code, List<DTMeaslocStatus> measlocList, List<PlanMeaslocModel> positionList)
        {
            // 在三维信息表中找同测点的信息
            var position = positionList.FirstOrDefault(p => string.Join("", p.measlocCode).Contains(code));

            // 获取该卡片下的所有特征值
            List<DTEvStatus> evData = measlocList.SelectMany(o => o.EigenValueList).ToList();
            List<string> measlocName = measlocList.Select(o => o.MeaslocName).ToList();

            var cardPosition = position == null ? new List<double>() : position.cardPosition;
            var spotPosition = position == null ? new List<double>() : position.spot;
            var title = componentHelper.ConvertEVCardTitle(LongestCommonPrefix(measlocName));

            EvCardStatusDTO obj = new EvCardStatusDTO(title: title, iconCode: code, cardPosition: cardPosition,
    spotPosition: spotPosition);
            obj.EvList = ConvertROTEVData(code, evData);

            if (evData.Count > 0)
            {
                obj.EvSummaryStatus = evData.Max(o => o.EvStatus).ToString().ToLower();
                obj.EvSummaryStatusTime = evData.Max(o => o.EvStatusTime).ToString("yyyy-MM-dd HH:mm:ss");
            }
            return obj;
        }

        /// <summary>
        /// 风轮：特征值卡片不存在分布圆，且特征值名称用特殊处理
        /// </summary>
        /// <param name="name"></param>
        /// <param name="eigenValueList"></param>
        /// <returns></returns>
        private List<EvStatusItemDTO> ConvertROTEVData(string name, List<DTEvStatus> eigenValueList)
        {
            List<EvStatusItemDTO> res = new List<EvStatusItemDTO>();

            // 如果没有分布圆配置，只根据已有特征值处理
            foreach (var evItem in eigenValueList)
            {
       
                double rounded = evItem.Value;
                double parsedValue = rounded == 0 ? 0 : rounded;

                string unit = evItem.Unit == "度" ? "°" : evItem.Unit;
                string evName = $"{componentHelper.ConvertEVName(evItem.EvId)}{evItem.EvName}";
                EvStatusItemDTO obj = new EvStatusItemDTO(evName, evItem.EvCode, evItem.EvId, evItem.EvStatus.toString().ToLower(), evItem.EvStatusTime.ToString("yyyy-MM-dd HH:mm:ss"), parsedValue, unit);

                res.Add(obj);
            }

            return res;
        }


        /// <summary>
        /// 塔筒：卡片名称特殊处理 
        /// </summary>
        /// <param name="measlocDir"></param>
        /// <returns></returns>
        private EvCardStatusDTO ConvertTWWEvCard(string pageCompType, string code, List<DTMeaslocStatus> measlocList, List<PlanMeaslocModel> positionList, List<MeaslocCircleModelConfig> circlePosition)
        {
            if (measlocList.Count == 0)
            {
                return new EvCardStatusDTO();
            }

            // 在三维信息表中找同测点的信息
            var position = positionList.FirstOrDefault(p => string.Join("", p.measlocCode).Contains(code));

            // 获取该卡片下的所有特征值
            List<DTEvStatus> evData = measlocList.SelectMany(o => o.EigenValueList).ToList();

            // 测点名称的最大公共前缀
            List<string> measlocName = measlocList.Select(o => o.MeaslocName).ToList();

            // 获取设备树对象
            DevMeasLocation meas = _devTreeRepository.GetMeasLocationByMeaslocID(measlocList.First().MeaslocId);

            string title = componentHelper.ConvertEVCardTitle(LongestCommonPrefix(measlocName));

            bool isShowCircle = circlePosition.Count() > 0;
            bool isShowRadar = isShowCircle && circlePosition.Any() ? circlePosition.First().IsShowRadar : false;
            double evThreshold = isShowCircle && circlePosition.Any() ? circlePosition.First().EvThreshold : 0.0;
            string finalTitle = isShowCircle ? title + componentHelper.GetEvCardNameByMonitorType(meas.MeasDataType) : title;
            var cardPosition = position == null ? new List<double>() : position.cardPosition;
            var spotPosition = position == null ? new List<double>() : position.spot;

            EvCardStatusDTO obj = new EvCardStatusDTO(
                title: finalTitle,
                iconCode: code,
                threshold: evThreshold,
                isShowCircle: isShowCircle,
                isShowRadar: isShowRadar,
                cardPosition: cardPosition,
                spotPosition: spotPosition
            );
            // 如塔顶和塔基，和风轮页面的卡片处理逻辑一致
            obj.EvList = obj.IsShowCircle == true ? ConvertTWWCircleEVData(evData, circlePosition) : ConvertROTEVData(code, evData);

            // 获取卡片状态和时间
            if (evData.Count > 0)
            {
                // 根据单位取标题_的后缀
                string evName = componentHelper.GetUnitDescription(evData.First().Unit);
                if (evName == "")
                {
                    // 法兰螺栓的特征值卡片名称特殊处理
                    if (evData.First().MeasLocID.Contains("BOL"))
                    {
                        evName = "轴力";
                    }
                    else
                    {
                        evName = evData.First().EvName;
                    }
                }
                obj.EvCardTitle = obj.IsShowCircle == true ? $"{obj.EvCardTitle}_{evName}" : obj.EvCardTitle;

                obj.EvSummaryStatus = evData.Max(o => o.EvStatus).ToString().ToLower();
                obj.EvSummaryStatusTime = evData.Max(o => o.EvStatusTime).ToString("yyyy-MM-dd HH:mm:ss");
            }


            return obj;
        }


        /// <summary>
        /// 塔筒：特征值卡片存在分布圆：根据分布圆配置信息获取特征值列表
        /// </summary>
        /// <param name="eigenValueList"></param>
        /// <param name="circlePosition"></param>
        /// <returns></returns>
        private List<EvStatusItemDTO> ConvertTWWCircleEVData(List<DTEvStatus> eigenValueList, List<MeaslocCircleModelConfig> circlePosition)
        {
            List<EvStatusItemDTO> res = new List<EvStatusItemDTO>();
            foreach (var circle in circlePosition)
            {
                EvStatusItemDTO obj = new EvStatusItemDTO();
                obj.CircleName = circle.CircleMeaslocName; // 有圆环但是没有分布圆，特征值名称需展示测点名称
                obj.AngleDegree = circle.AngleDegree;

                // 该圆环下有配置的测点
                if (circle.MeaslocCode != "")
                {
                    DTEvStatus evObj = eigenValueList.FirstOrDefault(x => x.MeasLocID.Contains(circle.MeaslocCode));
                    if (evObj != null)
                    {
            
                        double rounded = evObj.Value;
                        double parsedValue = rounded == 0 ? 0 : rounded;

                        string evName = $"{componentHelper.ConvertEVName(evObj.EvId)}{evObj.EvName}";
                        string status = evObj.EvStatus.toString().ToLower();
                        string statusTime = evObj.EvStatusTime.ToString("yyyy-MM-dd HH:mm:ss");

                        // 更新现有对象的属性
                        obj.EvName = evName;
                        obj.EvCode = evObj.EvCode;
                        obj.EvID = evObj.EvId;
                        obj.EvStatus = status;
                        obj.EvStatusTime = statusTime;
                        obj.EvValue = parsedValue;
                        obj.EvUnit = evObj.Unit;
                    }
                }
                res.Add(obj);
            }
            return res;
        }

        /// <summary>
        /// 对测点列表分组
        /// </summary>
        /// <param name="isSection">是否按照设备树表中的Section字段分组，false表示按照MeaslocCode分组</param>
        /// <param name="measlocList">原始测点列表</param>
        /// <returns></returns>
        private Dictionary<string, List<DTMeaslocStatus>> GroupBySection(Boolean isSection, List<DTMeaslocStatus> measlocList)
        {
            var dict = new Dictionary<string, List<DTMeaslocStatus>>();

            foreach (var item in measlocList)
            {
                // 过滤转速测点
                if (item.MeaslocCode.Contains("GENRSPD"))
                    continue;

                string key;

                // 按照设备树中的Section字段截取
                if (isSection)
                {
                    var meas = _devTreeRepository.GetMeasLocationByMeaslocID(item.MeaslocId);
                    if (meas?.Section == null)
                        continue;

                    int idx = item.MeaslocCode.IndexOf(meas.Section, StringComparison.Ordinal);
                    if (idx < 0)
                        continue;

                    key = item.MeaslocCode.Substring(0, idx + meas.Section.Length);
                }
                // 直接用完整 Code 当 key
                else
                {
                    key = item.MeaslocCode;
                }

                if (!dict.TryGetValue(key, out var list))
                {
                    list = new List<DTMeaslocStatus>();
                    dict[key] = list;
                }
                list.Add(item);
            }

            return dict;
        }


        /// <summary>
        /// 获取最大公共前缀
        /// </summary>
        /// <param name="ss"></param>
        /// <returns></returns>
        public static string LongestCommonPrefix(IList<string> ss)
        {
            if (ss == null || ss.Count == 0) return string.Empty;

            for (int col = 0; col < ss[0].Length; col++)
            {
                char c = ss[0][col];
                for (int row = 1; row < ss.Count; row++)
                {
                    if (col >= ss[row].Length || ss[row][col] != c)
                        return ss[0][..col];
                }
            }
            return ss[0];   // 全部相等
        }

        #endregion


        #region 部件状态趋势
        // 2、获取聚合部件内的状态趋势
        internal List<CompStatusTrendDTO> GetPageCompStatusList(string entityId, string entityCode, DateTime startTime, DateTime endTime)
        {
            List<CompStatusTrendDTO> res = new List<CompStatusTrendDTO>();

            var allComp = componentHelper.GetCompType(entityCode); // 该聚合部件下全部部件
            DTDeviceStatus DeviceTree = alarmStatusRepository.GetDeviceId(entityId); // 该机组下全部部件

            if (DeviceTree == null || DeviceTree.CompList == null || DeviceTree.CompList.Count == 0)
            {
                return new List<CompStatusTrendDTO>();
            }

            foreach (var item in DeviceTree.CompList)
            {
                if (allComp.Contains(item.CompCode))
                {
                    CompStatusTrendItemDTO entityStatus = GetCompStatusList(item.CompId, startTime, endTime);
                    CompStatusTrendDTO obj = new CompStatusTrendDTO(
                        entityId: item.CompId,
                        entityName: item.CompName,
                        entityType: item.CompCode,
                        status: item.CompStatus.ToString().ToLower(),
                        statusTime: item.CompStatusTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        entityStatus: entityStatus
                    );
                    res.Add(obj);
                }
            }

            return res;
        }

        // 2.1、处理单个部件的状态趋势
        private CompStatusTrendItemDTO GetCompStatusList(string compId, DateTime startTime, DateTime endTime)
        {
            DateTime _end = new DateTime(endTime.Year, endTime.Month, endTime.Day).AddDays(1);
            DateTime _begin = new DateTime(startTime.Year, startTime.Month, startTime.Day);
            List<DevMeasLocation> ids = _devTreeRepository.GetMeaslocationByCompID(compId);
            List<CompItem> allstatus = alarmStatusRepository.GetCompStateTrendByCompID(ids[0].StationID, compId, startTime, endTime);

            CompStatusTrendItemDTO statusList = new CompStatusTrendItemDTO();
            statusList.Time = new List<string>();
            statusList.Status = new List<string>();

            int day = (_end - _begin).Days;

            for (int i = 0; i < day; i++)
            {

                DateTime _time = _begin.AddDays(i);
                statusList.Time.Add(_time.ToString("yyyy-MM-dd"));

                var _data = allstatus.Where(o => o.CompStatusTime > _time && o.CompStatusTime < _time.AddDays(1));
                if (_data == null || _data.Count() == 0)
                {
                    statusList.Status.Add(EnumAlarmStatus.Normal.ToString().ToLower());
                }
                else
                {
                    statusList.Status.Add(_data.Max(o => o.CompStatus).ToString().ToLower());
                }
            }
            return statusList;
        }

        #endregion

    }
}
