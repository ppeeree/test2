using ACH.ACHLog.SeriLog;
using ACH.AppFramework.Analysis.ZXHCEVCalculate.entity;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using ACH.ModelConfig.DB;
using SqlSugar;


namespace ACH.CMSWebClient.ControllerImplement
{
    public class CBFOnlineToolMethods
    {
        IDevTreeRepsitory treeRepsitory = DevTreeRepsitory.Instance;
        IModelConfigDB modelConfigDB = ModelConfigSqliteContext.Instance;
        IDeviceEVRead deviceEVRead;
        public CBFOnlineToolMethods(IConfiguration config)
        {
            deviceEVRead = new EvTrendDBFactory(config).GetDeviceEVRead(EnumMonitorType.TVM_CBF);
        }

        /// <summary>
        ///  CBF在线工具执行
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="CBFValue">CBF校准值</param>
        /// <param name="StartTime">特征查询开始时间</param>
        /// <param name="EndTime">特征值查询结束时间</param>
        /// <returns></returns>
        public string ExecuteCBFTool(string StationID, double CBFValue, string StartTime, string EndTime)
        {
            string cBFInfoData = "";
            // 1.根据风场id,找到cbf配置表中改风场的所有数据
            List<CBFConfig> cbfConfigData = modelConfigDB.GetCBFConfigByStationID(StationID);

            // 2.查询测点定义表中该风场的所有钢索测点数据 JDGF0010021
            List<DevMeasLocation> measurements = treeRepsitory.GetAllMeasLocation(StationID);
            if (measurements != null && measurements.Count != 0)
            {
                measurements = measurements.Where(vo => vo.DeviceID.Contains(StationID) && vo.MeasLoctionID.Contains("PL") && !vo.MeasLoctionID.EndsWith("T")).ToList();
            }
            ALog.Information($"1.CBF配置数据查询：{cbfConfigData.Count}-测定定义表查询{measurements.Count}");

            if (cbfConfigData != null && cbfConfigData.Count != 0 && measurements != null && measurements.Count != 0)
            {
                // 3.筛选该风场CBF机组配置数据,  按照机组分组后，个数为1的是机组配置
                Dictionary<string, CBFConfig> siftCbfConfigData = new Dictionary<string, CBFConfig>();
                List<string> wid = new List<string>();
                Dictionary<string, List<CBFConfig>> cBFConGroupedByDeviceID = cbfConfigData.GroupBy(d => d.DeviceID).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var item in cBFConGroupedByDeviceID)
                {
                    string DeviceID = item.Key;// 机组id
                    List<CBFConfig> cBFConfigs = item.Value;// 该机组CBF配置数据
                    if (cBFConfigs.Count == 1)// 只配置了机组,配置到测点，跳过
                    {
                        siftCbfConfigData[DeviceID] = cBFConfigs[0];
                        wid.Add(DeviceID);
                    }
                }

                // 4.如果存在机组配置
                if (siftCbfConfigData.Count != 0)
                {
                    // 4.1测点定义表筛选，需要进行测点配置的机组,按照测点分组
                    measurements = measurements.Where(vo => wid.Contains(vo.DeviceID)).ToList();
                    Dictionary<string, List<DevMeasLocation>> measGroupedData = measurements.GroupBy(d => d.DeviceID).ToDictionary(g => g.Key, g => g.ToList());

                    // 4.2 获取机组配置级 当前时间近1个月的所有CBF特征值数据
                    ALog.Information("2.特征值趋势数据查询进行中：");
                    DateTime startTimes = DateTime.Now.AddMonths(-1);// 默认当前时间少1个月
                    DateTime endTimes = DateTime.Now;
                    if (!string.IsNullOrEmpty(StartTime) && !string.IsNullOrEmpty(EndTime))//输入时间已输入时间为准
                    {
                        startTimes = DateTime.Parse(StartTime);
                        endTimes = DateTime.Parse(EndTime);
                    }
                    // 根据机组 查询CBF特征值数据 
                    List<EigenValueData> eigenValueDatas = new List<EigenValueData>();
                    foreach (var id in wid)
                    {
                        var cbfData = deviceEVRead.GetEigenValueHis(StationID, id, new string[] { "CBF" }, startTimes, endTimes);
                        eigenValueDatas.AddRange(cbfData);
                    }

                    // 根据机组  时间 查询索力特征值数据
                    ALog.Information("特征值趋势数据查询个数：" + eigenValueDatas.Count);
                    if (eigenValueDatas != null && eigenValueDatas.Count != 0)
                    {
                        List<CBFConfig> _config = new List<CBFConfig>();

                        // 按照机组分组
                        // 4.3 机组配置转为测点配置数据
                        AddCBFConfigData(CBFValue, siftCbfConfigData, eigenValueDatas, _config);

                        ALog.Information("3.初次新加的测点配置个数：" + _config.Count);

                        // 4.4 由于特征值数据可能有些测点没有，对已经计算的测点配置数据，补充其他没有特征值数据的CBF配置数据
                        if (_config.Count != 0)
                        {
                            //4.4.1 补充测点配置数据
                            SupplementCBFConfigData(measGroupedData, _config);
                            ALog.Information("4.最终新加的测点配置个数：" + _config.Count);
                            // 4.4.2 CBF配置数据存储
                            cBFInfoData = modelConfigDB.AddCBFConfig(_config);// CBFConfigDataStorage(db, _config);
                        }
                        else
                        {
                            cBFInfoData = "根据特征值数据生成的测点配置数据为null";
                        }
                    }
                    else
                    {
                        cBFInfoData = "CBF特征值数据查询为null,没有更新该测点配置";
                    }
                }
                else
                {
                    cBFInfoData = "该风场所有CBF配置已经配置到测点级";
                }
                return cBFInfoData;
            }
            else
            {
                cBFInfoData = "CBFConfig||测点定义表没有该" + StationID + "风场数据";
                return cBFInfoData;
            }
        }

        /// <summary>
        ///  补充测点配置数据
        /// </summary>
        /// <param name="measGroupedData">测点定义表测点数据</param>
        /// <param name="_config">存储的CBF测点配置数据</param>
        private void SupplementCBFConfigData(Dictionary<string, List<DevMeasLocation>> measGroupedData, List<CBFConfig> _config)
        {
            // 1.补充没有特征值其他测点数据,按照机组分组，后的个数与测点定义表中的个数不一样的时候补充测点配置
            // 机组配黑转为测点配置的数据按照机组进行分组
            Dictionary<string, List<CBFConfig>> measConfigData = _config.GroupBy(d => d.DeviceID).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var item in measConfigData)
            {
                string key = item.Key;
                List<CBFConfig> cbfValue = item.Value;
                // 2. 生成的测点配置数据不为0，并且在测点定义表中存在该机组
                if (cbfValue.Count != 0 && measGroupedData.ContainsKey(key))
                {
                    // 3. 如果生成的机组 测点个数和 测点定义表中数据不一致，根据 测点定义表测点补充配置信息
                    List<DevMeasLocation> measurementsData = measGroupedData[key];
                    if (cbfValue.Count != measurementsData.Count)
                    { // 4. 获取该机组 拉索长度出现次数最多的数据
                        CBFConfig supplementConfig = GetMostFrequentCBFConfig(cbfValue);

                        // 5. 获取该缺失测点信息
                        List<string> loseMid = GetMeasurementMidsNotInCBFConfig(measurementsData, cbfValue);
                        foreach (var mid in loseMid)
                        {
                            // 替换缺失mid 后添加 
                            supplementConfig.MeasLoctionID = mid;
                            // 6.补充确实测点配置信息
                            _config.Add(supplementConfig);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  添加CBF测点配置数据
        /// </summary>
        /// <param name="CBFValue"> 特征值校准值</param>
        /// <param name="siftCbfConfigData">筛选的CBF机组配置数据</param>
        /// <param name="eigenValueDatas">特征值数据</param>
        /// <param name="_config">存储的CBF测点配置数据</param>
        private void AddCBFConfigData(double CBFValue, Dictionary<string, CBFConfig> siftCbfConfigData, List<EigenValueData> eigenValueDatas, List<CBFConfig> _config)
        {
            // 1. 特征值数据按照测点id进行分组
            Dictionary<string, List<EigenValueData>> evGroupedData = eigenValueDatas.GroupBy(d => d.MeasLocID).ToDictionary(g => g.Key, g => g.ToList());
            foreach (var item in evGroupedData)
            {
                string mid = item.Key;// 测点id
                List<EigenValueData> evDatas = item.Value;// 该测点对应的特征值数据
                if (evDatas.Count != 0)
                {
                    // 2. 统计 EigenValue 出现次数最多的值
                    List<EigenValueData> mostFrequentItems = GetMostFrequentEigenValueItems(evDatas);

                    // 3. 特征值数据 在机组配置级中存在,获取机组配置信息  主要使用 拉索长度 单位质量 上下限
                    CBFConfig config = new CBFConfig();
                    if (siftCbfConfigData.ContainsKey(evDatas[0].DeviceID))
                    {
                        config = siftCbfConfigData[evDatas[0].DeviceID];
                    }
                    int order = 1;
                    double quality = config.Quality;// 单位质量
                    double eigenValue = mostFrequentItems[0].EigenValue * 1000; // 已知 CBF 的值
                    double cBFLength = config.CBFLength;// 查询已配置机组长度
                    double irf = Math.Sqrt(eigenValue * Math.Pow(order, 2) / (4 * quality * Math.Pow(cBFLength, 2)));

                    // 4. 根据倒退出的固有频率 按照默认索力值 计算拉索长度
                    double CBF = CBFValue * 1000; // 假设 F 的值是 180
                    double l = Math.Sqrt(CBF * Math.Pow(order, 2) / (4 * quality * Math.Pow(irf, 2)));
                    _config.Add(new CBFConfig { DeviceID = evDatas[0].DeviceID, MeasLoctionID = item.Key, Quality = quality, CBFLength = l, Upper = config.Upper, Low = config.Low, IRF = irf });
                }
            }
        }

        /// <summary>
        ///  查询缺失的测点id
        /// </summary>
        /// <param name="measurements"></param>
        /// <param name="cbfConfigs"></param>
        /// <returns></returns>
        public List<string> GetMeasurementMidsNotInCBFConfig(List<DevMeasLocation> measurements, List<CBFConfig> cbfConfigs)
        {
            // 1. 检查输入是否为空
            if (measurements == null || cbfConfigs == null)
            {
                return new List<string>();
            }

            // 2. 提取 CBFConfig 的所有 mid（用 HashSet 提高查询效率）
            var cbfConfigMids = new HashSet<string>(cbfConfigs.Select(c => c.MeasLoctionID));

            // 3. 找出 Measurement 中 mid 不在 CBFConfig 的 mid 列表中的项
            var uniqueMids = measurements
                .Where(m => !cbfConfigMids.Contains(m.MeasLoctionID))
                .Select(m => m.MeasLoctionID)
                .Distinct() // 去重（如果需要）
                .ToList();

            return uniqueMids;
        }



        /// <summary>
        /// 返回 CBFLength 出现次数最多的 CBFConfig 对象（如果有多个，返回第一个）
        /// </summary>
        /// <param name="configs">CBFConfig 列表</param>
        /// <returns>CBFLength 最频繁的 CBFConfig 对象（如果输入为空，返回 null）</returns>
        public CBFConfig GetMostFrequentCBFConfig(List<CBFConfig> configs)
        {
            // 1. 检查输入是否为空或无数据
            if (configs == null || !configs.Any())
            {
                return null;
            }

            // 2. 找到出现次数最多的 CBFLength
            var mostFrequentLength = configs
                .GroupBy(config => config.CBFLength)
                .OrderByDescending(group => group.Count())
                .Select(group => group.Key)
                .FirstOrDefault();

            // 3. 返回第一个 CBFLength == mostFrequentLength 的对象
            return configs.FirstOrDefault(config => config.CBFLength == mostFrequentLength);
        }

        /// <summary>
        ///  获取该特征值数据中出现次数最多特征值数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<EigenValueData> GetMostFrequentEigenValueItems(List<EigenValueData> data)
        {
            if (data == null || data.Count == 0)
            {
                return new List<EigenValueData>();
            }

            // 1. 按 EigenValue 分组，并统计每组的数量
            var groupedData = data
                .GroupBy(item => item.EigenValue)
                .Select(group => new
                {
                    EigenValue = group.Key,
                    Count = group.Count(),
                    Items = group.ToList() // 保存该分组下的所有对象
                })
                .OrderByDescending(g => g.Count) // 按出现次数降序排序
                .ToList();

            // 2. 获取出现次数最多的分组
            var mostFrequentGroup = groupedData.FirstOrDefault();

            // 3. 返回该分组下的所有 DTEvStatus 对象
            return mostFrequentGroup?.Items ?? new List<EigenValueData>();
        }

    }
}
