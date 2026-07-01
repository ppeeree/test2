using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.CMSWebClient.ControllerModel.Config;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataEntity.StatusTree;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using ACH.Helper.Component;
using Newtonsoft.Json;
using static ACH.CMSWebClient.ControllerModel.Analysis.GroupTrendFromBodyDTO;
using ConstStr = ACH.MeasData.Entity.ConstStr;
namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    /// <summary>
    /// 专项分析接口处理
    /// </summary>
    public class GroupTrendMethods
    {
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        static ComponentHelper componentHelper = new ComponentHelper();



        /// <summary>
        /// 读取EigenValueConfigMap配置文件：获取每个测点展示哪些特征值
        /// </summary>
        /// <returns></returns>
        public static List<EvConfigMapperJsonDTO> GetEigenConfigs()
        {
            List<EvConfigMapperJsonDTO> eigenConfig = new List<EvConfigMapperJsonDTO>();
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EigenValueConfigMap.json");
            if (File.Exists(path))
            {
                try
                {
                    eigenConfig = JsonConvert.DeserializeObject<List<EvConfigMapperJsonDTO>>(File.ReadAllText(path));
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, "EigenValueConfigMap.json");
                }

            }
            return eigenConfig;
        }

        /// <summary>
        /// 专项分析接口处理
        /// </summary>
        /// <param name="groupTrendChartDto">接口传参</param>
        /// <returns></returns>
        internal static List<GroupTrendChartDTO> GroupTrend(GroupTrendFromBodyDTO groupTrendChartDto)
        {
            // 1. 参数校验
            List<GroupTrendChartDTO> res = new List<GroupTrendChartDTO>();
            if (string.IsNullOrEmpty(groupTrendChartDto.GTCType))
            {
                ALog.Information("分组趋势类型字段为空或未设置");
            }

            if (groupTrendChartDto.GTCAttributes == null || groupTrendChartDto.GTCAttributes.Count == 0)
            {
                ALog.Information("分组趋势数据为null");
                return res;
            }

            // 获取报警模型配置
            List<EvConfigMapperJsonDTO> eigenConfigList = GetEigenConfigs();

            // 分组趋势类型字段
            string groupTrendType = groupTrendChartDto.GTCType;

            // 获取该测点id
            List<string> measlocIds = groupTrendChartDto.GTCAttributes.Select(attr => attr.MeasLoctionID).ToList();

            // 获取采集事件列表部件类型,与专项分析code 匹配
            Dictionary<string, List<string>> componentType = componentHelper.GetComponentTypeDic();


            // 趋势分析
            if (groupTrendType == "Trend")
            {
                return TrendAnalysis(eigenConfigList, measlocIds);

            }
            // 专项分析
            else if (componentType.ContainsKey(groupTrendType) || groupTrendType.Contains("TVM_STE_TOP") || groupTrendType.Contains("TVM_STE_FDN"))
            {
                return SpecialAnalysis(groupTrendChartDto, eigenConfigList, groupTrendType, measlocIds, componentType);
            }
            // 其他未知类型返回null
            else
            {
                return res;
            }
        }


        /// <summary>
        /// 趋势分析（Trend）
        /// </summary>
        /// <param name="eigenConfigList"></param>
        /// <param name="measlocIds"></param>
        /// <returns></returns>
        private static List<GroupTrendChartDTO> TrendAnalysis(List<EvConfigMapperJsonDTO> eigenConfigList, List<string> measlocIds)
        {
            // 2.1 获取配置文件中的特征值列表，并转化对象
            List<DTEvStatus> allMapDatas = HandlerMapEigenValue(measlocIds, eigenConfigList);

            // 特殊处理：删除叶片的平均值特征值，页面只展示文档中包含的特征值类型
            var blEvs = allMapDatas.Where(o => o.EvId.Contains("BL") && o.EvId.Contains("Avg")).ToList();
            allMapDatas.RemoveAll(o => blEvs.Contains(o));

            // 2.3 聚合特征值，并返回接口结果
            List<GroupTrendChartDTO> res = EigenvalueAggregation(allMapDatas);

            return res;
        }


        /// <summary>
        /// 专项分析：
        /// 传动链（CVM）、叶片分析（BVM）、塔筒结构（TVM_STE）
        /// 塔筒结构-塔基（TVM_STE_FDN）、塔筒结构-塔顶（TVM_STE_TOP）、塔筒法兰间隙(TVM_FLG_GAP)、塔筒_索力(TVM_CBF)
        /// </summary>
        /// <param name="groupTrendChartDto">获取专项分析入参</param>
        /// <param name="eigenConfigList">json文件中的特征值配置项</param>
        /// <param name="groupTrendType">专项分析Code</param>
        /// <param name="measlocIds">测点ID</param>
        /// <param name="componentType">聚合部件Code</param>
        /// <returns></returns>
        private static List<GroupTrendChartDTO> SpecialAnalysis(GroupTrendFromBodyDTO groupTrendChartDto, List<EvConfigMapperJsonDTO> eigenConfigList, string groupTrendType, List<string> measlocIds, Dictionary<string, List<string>> componentType)
        {
            // 2.1 根据聚合部件code获取实体部件code 
            List<string> componentTypeCode = ComponentFilteringCode(groupTrendType, componentType);

            // 筛选出该专项分析下的测点ID
            var newMeas = measlocIds.Where(id => componentTypeCode.Any(code => id.IndexOf(code, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();

            // 2.2 根据测点 和 部件过滤code 获取特征值数据 todo：按说是不需要生成DTEvStatus，只要另个一对象中也有这些字段也可以用
            List<DTEvStatus> evDatas = HandlerMapEigenValue(newMeas, eigenConfigList);

            // 2.3 筛选设备树
            List<DevMeasLocation> measurementsDatas = screeningMeasuringPointData(groupTrendChartDto);


            // 索力专项分析特殊处理：不需要按照测点Code做聚合，且返回索力个数
            if (groupTrendType == EnumMonitorType.TVM_CBF.ToString())
            {
                return CBFSpecialAnalysis(groupTrendChartDto, evDatas, measurementsDatas, newMeas);
            }

            // 2.4 测点，特征值数据 聚合
            List<GroupTrendChartDTO> data = measuringPointPolymerization(evDatas, measurementsDatas, groupTrendType);

            // 叶片特殊处理：按照配置文件中的特征值，根据挥舞/摆振聚合返回
            if (groupTrendType == EnumMonitorType.BVM.ToString())
            {
                List<compItem> compItems = eigenConfigList.Where(o => o.compType == groupTrendChartDto.GTCType).First().compList;
                var newcompItems = compItems.Where(o => groupTrendChartDto.GTCAttributes.Any(e => e.MeasLoctionID.Contains(o.compType))).ToList();
                return BVMSpecialAnalysis(newcompItems, data, groupTrendChartDto.GTCAttributes);
            }
            else
            {
                return data.SortByName(ascending: true, dictType: EnumSortType.MeaslocName);
            }
        }



        /// <summary>
        /// 特征值趋势处理
        /// </summary>
        /// <param name="measlocIds"></param>
        /// <param name="eigenConfigList"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static List<DTEvStatus> HandlerMapEigenValue(List<string> measlocIds, List<EvConfigMapperJsonDTO> eigenConfigList)
        {
            List<DTEvStatus> res = new List<DTEvStatus>();
            // 取配置文件中全部部件的特征值项
            List<compItem> compItems = eigenConfigList.SelectMany(o => o.compList).ToList();

            if (compItems == null || compItems.Count == 0)
            {
                return new List<DTEvStatus>();
            }

            foreach (var mapItem in compItems)
            {
                foreach (var measID in measlocIds)
                {
                    // 特殊处理，如果是塔筒塔顶，精准匹配
                    if (measID.Contains(ConstStr.TOW + ConstStr.TOP))
                    {
                        string windturbineID = measID.Substring(0, measID.IndexOf(ConstStr.TOW + ConstStr.TOP));
                        if (measID == windturbineID + mapItem.compType)
                        {
                            foreach (var item in mapItem.compList)
                            {
                                DTEvStatus obj = new DTEvStatus();
                                obj.EvCode = item.code;
                                obj.EvId = $"{measID}&&{item.code}";
                                obj.EvName = $"{item.name}({item.code})";
                                res.Add(obj);
                            }
                        }
                    }// 其余模糊匹配
                    else
                    {
                        if (measID.Contains(mapItem.compType))
                        {
                            foreach (var item in mapItem.compList)
                            {
                                DTEvStatus obj = new DTEvStatus();
                                obj.EvCode = item.code;
                                obj.EvId = $"{measID}&&{item.code}";
                                obj.EvName = $"{item.name}({item.code})";

                                res.Add(obj);
                            }
                        }
                    }
                }
            }

            return res.DistinctBy(o => o.EvId).ToList();
        }


        #region 叶片专项分析特殊处理
        /// <summary>
        /// 处理叶片专项分析返回结果
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static List<GroupTrendChartDTO> BVMSpecialAnalysis(List<compItem> compItems, List<GroupTrendChartDTO> data, List<GroupTrendAttribute> gTCAttributes)
        {
            // 按照叶片挥舞和叶片摆振对数据分组
            List<GroupTrendChartDTO> bvmData = new List<GroupTrendChartDTO>();
            GroupTrendChartDTO faObj = new()
            {
                Name = "叶片挥舞",
                Code = "MPTFa",
                Ids = new List<string>(),
                Children = new List<GroupTrendChartDTO>()
            };

            GroupTrendChartDTO laObj = new()
            {
                Name = "叶片摆振",
                Code = "MPTLa",
                Ids = new List<string>(),
                Children = new List<GroupTrendChartDTO>()
            };

            List<string> faIDs = gTCAttributes.Where(x => x.MeasLoctionID.Contains(faObj.Code)).Select(x => x.MeasLoctionID).ToList();
            List<string> laIDs = gTCAttributes.Where(x => x.MeasLoctionID.Contains(laObj.Code)).Select(x => x.MeasLoctionID).ToList();

            // 对配置文件中的符合传参的列表按照挥舞摆振分组
            foreach (compItem obj in compItems)
            {
                if (obj.compType.Contains(faObj.Code))
                {
                    List<GroupTrendChartDTO> list1 = handlerToGroupTrendChartVO(obj.compList, faIDs);

                    faObj.Children.AddRange(list1);

                }
                else if (obj.compType.Contains(laObj.Code))
                {
                    List<GroupTrendChartDTO> list1 = handlerToGroupTrendChartVO(obj.compList, laIDs);
                    laObj.Children.AddRange(list1);
                }
            }

            // 对faobj和laobj去重
            faObj.Children = faObj.Children.DistinctBy(o => o.Name).ToList();
            laObj.Children = laObj.Children.DistinctBy(o => o.Name).ToList();

            // 设置返回对象的排序：如果一阶固有频率没有数据，按照有数据的ID排序。如果有数据，将一阶固有频率放最前方，其余按照ID个数排序
            faObj.Children = faObj.Children.OrderByDescending(o => o.Ids.Count).ToList();
            var _one = faObj.Children.Find(o => o.Code == "NF1");
            if (_one != null && _one.Ids.Count != 0)
            {
                faObj.Children.Remove(_one);
                faObj.Children.Insert(0, _one);
            }
            laObj.Children = laObj.Children.OrderByDescending(o => o.Ids.Count).ToList();
            var one = laObj.Children.Find(o => o.Code == "NFD1");
            if (one != null && one.Ids.Count != 0)
            {
                laObj.Children.Remove(one);
                laObj.Children.Insert(0, one);
            }


            if (faObj.Children.Count > 0)
            {
                bvmData.Add(faObj);
            }

            if (laObj.Children.Count > 0)
            {
                bvmData.Add(laObj);
            }

            return bvmData;
        }

        // 叶片专项分析：将配置中内容修改为接口返回内容
        private static List<GroupTrendChartDTO> handlerToGroupTrendChartVO(List<compValueItem> compList, List<string> ids)
        {
            List<GroupTrendChartDTO> res = new List<GroupTrendChartDTO>();
            foreach (var item in compList)
            {
                GroupTrendChartDTO obj = new GroupTrendChartDTO();
                obj.Name = $"{item.name}({item.code})";
                obj.Code = item.code;
                obj.Ids = ids.Select(id => $"{id}&&{item.code}").ToList();
                res.Add(obj);

            }
            return res;
        }
        #endregion

        #region 塔筒专项分析特殊处理
        private static List<GroupTrendChartDTO> CBFSpecialAnalysis(GroupTrendFromBodyDTO groupTrendChartDto, List<DTEvStatus> evDatas, List<DevMeasLocation> measurementsDatas, List<string> newMeas)
        {
            List<GroupTrendChartDTO> cbfData = new List<GroupTrendChartDTO>();
            foreach (var item in groupTrendChartDto.GTCAttributes)
            {
                if (!newMeas.Contains(item.MeasLoctionID))
                {
                    continue;
                }
                List<DTEvStatus> evs = evDatas.Where(o => o.EvId.Contains(item.MeasLoctionID)).ToList();
                DevMeasLocation meas = measurementsDatas.Where(o => o.MeasLoctionID == item.MeasLoctionID).First();
                List<DevMeasLocation> measlocs = _devTreeRepository.GetMeaslocationByDeviceID(item.WindturbineID).Where(o => o.MeasDataType == EnumMonitorType.TVM_CBF).OrderByDescending(o => o.MeasLoctionID).ToList();
                // 取钢索ID最大的一条
                var first = measlocs.First();
                // 获取钢索名称
                string measCode = first.MeasLoctionID.Replace($"{first.DeviceID}TOWPL1", "");
                string digits = new string(measCode.Where(char.IsDigit).ToArray());

                GroupTrendChartDTO obj = new GroupTrendChartDTO();
                obj.Code = item.MeasLoctionID.Replace(item.WindturbineID, "");
                obj.Name = meas.MeasLoctionName;
                obj.Ids = new List<string>();
                obj.CBFNumber = int.Parse(digits);
                obj.Children = new List<GroupTrendChartDTO>();

                foreach (var ev in evs)
                {
                    GroupTrendChartDTO evObj = new GroupTrendChartDTO();
                    evObj.Code = ev.EvCode;
                    evObj.Name = ev.EvName;
                    evObj.Ids = new List<string>() { ev.EvId };

                    obj.Children.Add(evObj);
                }
                cbfData.Add(obj);
            }
            return cbfData;
        }
        #endregion

        /// <summary>
        /// 根据分组趋势类型获取对应部件过滤code
        /// </summary>
        /// <param name="analysisType"></param>
        /// <returns></returns>
        private static List<string> ComponentFilteringCode(string analysisType, Dictionary<string, List<string>> componentType)
        {
            // Dictionary<string, List<string>> componentType = PHM.AppFramework.ACHPHMData.getAcquisitionComponentType();

            // 塔顶塔基 特殊处理
            string filterType = analysisType;//过滤字段
            if (analysisType == "TVM_STE_TOP" || analysisType == "TVM_STE_FDN")
            {
                analysisType = "TVM_STE";
            }
            List<string> componentTypeCode = new List<string>();
            if (componentType.ContainsKey(analysisType))
            {
                componentTypeCode = componentType[analysisType];
            }

            if (filterType == "TVM_STE_TOP")// 过滤对应字段
            {
                componentTypeCode = componentTypeCode.Where(vo => vo.Contains("TOWTOP")).ToList();
            }
            if (filterType == "TVM_STE_FDN")// 过滤对应字段
            {
                componentTypeCode = componentTypeCode.Where(vo => vo.Contains("TOWFDN")).ToList();
            }
            return componentTypeCode;
        }




        /// <summary>
        ///  筛选测点数据 
        /// </summary>
        /// <param name="groupTrendChartDto"></param>
        /// <returns></returns>
        private static List<DevMeasLocation> screeningMeasuringPointData(GroupTrendFromBodyDTO groupTrendChartDto)
        {
            // 获取测量位置
            List<DevMeasLocation> measLocs = DevTreeRepsitory.Instance.GetAllMeasLocation();
            // 设备树列表数据
            Dictionary<string, List<DevMeasLocation>> measurementsMap = measLocs.GroupBy(d => d.DeviceID).ToDictionary(g => g.Key, g => g.ToList());
            // 根据测点ids 获取对应风测点数据
            Dictionary<string, List<GroupTrendAttribute>> groupTrendAttribute = groupTrendChartDto.GTCAttributes.GroupBy(d => d.WindturbineID).ToDictionary(g => g.Key, g => g.ToList());

            List<DevMeasLocation> measurementsDatas = new List<DevMeasLocation>();
            // 筛选 输入测点的测点属性数据
            foreach (var item in groupTrendAttribute)
            {
                if (measurementsMap.ContainsKey(item.Key))
                {// 在传入风数据中 过滤该输入的测点信息数据
                 //传入的测点数据
                    List<GroupTrendAttribute> groupTrends = item.Value;
                    // 数据存在的测点数据
                    List<DevMeasLocation> measurements = measurementsMap[item.Key];
                    foreach (var item1 in measurements)
                    {
                        foreach (var item2 in groupTrends)
                        {

                            if (item1.MeasLoctionID == item2.MeasLoctionID)
                            {
                                measurementsDatas.Add(item1);
                            }
                        }

                    }

                }
            }

            return measurementsDatas;
        }




        /// <summary>
        ///  测点聚合
        /// </summary>
        /// <param name="evDtas">特征值数据</param>
        /// <param name="measurementsDatas">测点数据</param>
        /// <param name="groupTrendType"></param>
        private static List<GroupTrendChartDTO> measuringPointPolymerization(List<DTEvStatus> evDtas, List<DevMeasLocation> measurementsDatas, string groupTrendType)
        {
            List<GroupTrendChartDTO> res = new List<GroupTrendChartDTO>();
            if (evDtas == null || evDtas.Count == 0)
            {
                ALog.Information("分组趋势类型" + groupTrendType + "特征值数据为null");
                return res;
            }

            if (measurementsDatas == null || measurementsDatas.Count == 0)
            {

                ALog.Information("分组趋势类型" + groupTrendType + "测点筛选数据为null");
                return res;
            }

            // 2.4 对筛选的测点数据按照测点名称分组，然后，根据测点code 封装对应的特征值数据
            Dictionary<string, List<DevMeasLocation>> groupedByMeasName = measurementsDatas.
                Where(d => !string.IsNullOrEmpty(d.MeasLoctionName)).GroupBy(d => d.MeasLoctionName).ToDictionary(g => g.Key, g => g.ToList());

            if (groupedByMeasName != null && groupedByMeasName.Count != 0)
            {
                // 获取到对应的测点名称和测点code数据
                foreach (var measName in groupedByMeasName)
                {
                    if (measName.Value != null && measName.Value.Count != 0)
                    {
                        DevMeasLocation measurement = measName.Value[0];
                        // 获取该第一个测点和对应的测点code
                        string name = measurement.MeasLoctionName;//
                        string code = "";
                        if (!string.IsNullOrEmpty(measurement.DeviceID) && !string.IsNullOrEmpty(measurement.MeasLoctionID))
                        {    // 根据测点id 和机组id 获取测点code
                            code = measurement.MeasLoctionID.Replace(measurement.DeviceID, "");
                        }
                        //封装数据
                        GroupTrendChartDTO measDatas = new GroupTrendChartDTO();
                        measDatas.Name = name;
                        measDatas.Code = code;
                        measDatas.Ids = new List<string>();
                        //根据测点code,过滤对应的特征值数据
                        List<DTEvStatus> eigenValues = evDtas.Where(vo => vo.EvId.Contains(code)).ToList();
                        if (eigenValues != null && eigenValues.Count != 0)
                        {
                            // 封装特征值code 数据结构
                            measDatas.Children = EigenvalueAggregation(eigenValues);
                            res.Add(measDatas);
                        }

                    }
                }

            }

            return res;
        }


        /// <summary>
        ///  特征值聚合
        /// </summary>
        /// <param name="evDtas">特征值数据</param>
        /// <returns></returns>
        public static List<GroupTrendChartDTO> EigenvalueAggregation(List<DTEvStatus> evDtas)
        {
            List<GroupTrendChartDTO> specialAnalysisVOs = new List<GroupTrendChartDTO>();
            if (evDtas == null || evDtas.Count == 0)
            {
                ALog.Information("特征值数据为null");
                return specialAnalysisVOs;
            }

            // 查询到的特征值按照特征值code进行分组
            Dictionary<string, List<DTEvStatus>> groupedByEvCode = evDtas.GroupBy(d => d.EvCode).ToDictionary(g => g.Key, g => g.ToList());
            if (groupedByEvCode != null && groupedByEvCode.Count > 0)
            {
                foreach (var item in groupedByEvCode)
                {
                    // 特征值code
                    string evCode = item.Key;
                    // 特征值 name
                    string evNmae = "";
                    // 特征值id列表
                    List<string> eids = new List<string>();
                    if (item.Value != null && item.Value.Count != 0)
                    {
                        foreach (DTEvStatus eigenValueItem in item.Value)
                        {  //赋值该code名称
                            evNmae = eigenValueItem.EvName;
                            //赋值eid
                            eids.Add(eigenValueItem.EvId);
                        }
                    }
                    //封装数据
                    GroupTrendChartDTO evDatas = new GroupTrendChartDTO();
                    evDatas.Name = evNmae;
                    evDatas.Code = evCode;
                    evDatas.Ids = eids;
                    specialAnalysisVOs.Add(evDatas);
                }
            }
            return specialAnalysisVOs;
        }
    }
}
