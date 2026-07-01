using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.Common;
using ACH.DataEntity.StatusTree;
using ACH.DBRepository.SD;
using ACH.Helper.Comparer;
using ACH.Helper.Component;
using System.Text.RegularExpressions;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    public class DeviceTreeMethods
    {
        ComponentHelper componentHelper = new ComponentHelper();
        IAlarmStatusRepository alarmStatusRepository;
        public DeviceTreeMethods(IConfiguration configuration)
        {
            alarmStatusRepository = new AlarmStatusRepository(configuration);
        }

        /// <summary>
        /// 1、/Analysis/DeviceTrees 接口处理 
        /// </summary>
        /// <returns></returns>
        internal List<DTStationStatusDTO> ConvertToDeviceTrees(string userId)
        {
            List<DTStationStatusDTO> res = new List<DTStationStatusDTO>();

            // 获取该用户下绑定的全部风场ID
            List<DTStationInfo> deviceTrees = alarmStatusRepository.GetStations(userId);

            foreach (var deviceTree in deviceTrees)
            {
                if (deviceTree == null || deviceTree.DeviceList == null || deviceTree.DeviceList.Count == 0)
                {
                    continue;
                }

                DTStationStatusDTO stationObj = new()
                {
                    Name = deviceTree.WindParkName,
                    Type = "windpark",
                    Code = "",
                    Id = deviceTree.WindParkId,
                    Children = ConvertTurbineTrees(deviceTree.DeviceList) //机组聚合
                };

                res.Add(stationObj);
            }

            return res.SortByName(ascending: true, dictType: EnumSortType.StationName);
        }


        /// <summary>
        /// 1.1、处理接口机组返回层 
        /// </summary>
        /// <param name="deviceList">机组列表</param>
        /// <returns></returns>
        private List<DTDeviceStatusDTO> ConvertTurbineTrees(List<DTDeviceStatus> deviceList)
        {
            List<DTDeviceStatusDTO> res = new List<DTDeviceStatusDTO>();

            foreach (var item in deviceList)
            {
                DTDeviceStatusDTO obj = new()
                {
                    Id = item.WindTurbuineId,
                    Name = item.WindTurbuineName,
                    Code = "",
                    Type = "turbine",
                    Status = item.WindTurbuineStatus.ToString().ToLower(),
                    Time = item.WindTurbuineStatusTime.ToString(),
                    Children = ConvertBigCompTree(item.CompList)
                };
                res.Add(obj);
            }

            return res.SortByName(ascending: true, dictType: EnumSortType.Alphabetical);
        }


        /// <summary>
        /// 1.2、处理接口大部件返回层
        /// </summary>
        /// <param name="compList">实体部件列表</param>
        /// <returns></returns>
        private List<DTBigCompStatusDTO> ConvertBigCompTree(List<DTCompStatus> compList)
        {
            // 根据部件聚合大部件
            List<DTBigCompStatusDTO> pageCompRes = new List<DTBigCompStatusDTO>();

            if (compList != null && compList.Count != 0)
            {
                // 传动链
                List<DTCompStatus> cvm = compList.Where(vo => vo.CompCode == "MST" || vo.CompCode == "GBX" || vo.CompCode == "GEN").ToList();
                if (cvm != null && cvm.Count != 0)
                {
                    string acquisitionComponentType = EnumMonitorType.CVM.ToString();
                    var type = componentHelper.GetComponentTypeDic()[acquisitionComponentType];
                    pageCompRes.Add(ConvertPageCompTree(cvm, type, type, acquisitionComponentType, "传动链", false));
                }

                // 叶片
                List<DTCompStatus> bvm = compList.Where(vo => vo.CompCode.Contains("BL") || vo.CompCode.Contains("PB")).ToList();
                if (bvm != null && bvm.Count != 0)
                {
                    string acquisitionComponentType = EnumMonitorType.BVM.ToString();
                    var type = componentHelper.GetComponentTypeDic()[acquisitionComponentType];
                    pageCompRes.Add(ConvertPageCompTree(bvm, type, type, acquisitionComponentType, "叶片", false));
                }

                // 塔筒下全部测点
                List<DTCompStatus> tvm = compList.Where(vo => vo.CompCode == "TOW").ToList();

                // 将塔筒结构下全部实体部件按照塔顶/塔基/法兰/索力......等进行区分
                List<DTCompStatus> newTvm = TowStrutureeReconsitution(tvm);
                if (newTvm != null && newTvm.Count != 0)
                {
                    string acquisitionComponentType = EnumMonitorType.TVM_STE.ToString();
                    var type = componentHelper.GetComponentTypeDic()[acquisitionComponentType];
                    pageCompRes.Add(ConvertPageCompTree(newTvm, type, type, acquisitionComponentType, "塔筒", false));
                }
            }
            return pageCompRes.SortByName(ascending: true, dictType: EnumSortType.PageCompName);
        }

        /// <summary>
        /// 1.3、将塔筒下的全部实体部件重构
        /// </summary>
        /// <param name="tvm">塔筒全部实体部件数据</param>
        /// <returns></returns>
        private List<DTCompStatus> TowStrutureeReconsitution(List<DTCompStatus> tvm)
        {
            List<DTCompStatus> res = new List<DTCompStatus>();
            if (tvm != null && tvm.Count != 0)
            {
                DTCompStatus topCompItem = new DTCompStatus { CompCode = "TVM_STE_TOP", CompName = "塔顶" }; // 塔顶部件对象
                List<DTMeaslocStatus> topMeaslocList = new List<DTMeaslocStatus>(); // 塔顶测点数据
                DTCompStatus fdnCompItem = new DTCompStatus { CompCode = "TVM_STE_FDN", CompName = "塔基" }; // 塔基部件对象
                List<DTMeaslocStatus> fdnMeaslocList = new List<DTMeaslocStatus>(); // 塔基测点数据
                DTCompStatus cbfCompItem = new DTCompStatus { CompCode = EnumMonitorType.TVM_CBF.ToString(), CompName = "索力" }; // 索力部件对象
                List<DTMeaslocStatus> cbfMeaslocList = new List<DTMeaslocStatus>(); // 索力测点数据
                DTCompStatus glggapCompItem = new DTCompStatus { CompCode = EnumMonitorType.TVM_FLG_GAP.ToString(), CompName = "法兰间隙" }; // 法兰间隙部件对象
                List<DTMeaslocStatus> glggapMeaslocList = new List<DTMeaslocStatus>(); // 法兰间隙测点数据
                DTCompStatus bfmCompItem = new DTCompStatus { CompCode = EnumMonitorType.TVM_BFM.ToString(), CompName = "法兰螺栓" }; // 法兰螺栓部件对象
                List<DTMeaslocStatus> bfmMeaslocList = new List<DTMeaslocStatus>(); // 法兰螺栓测点数据

                foreach (DTCompStatus compItem in tvm)
                {
                    string compId = compItem.CompId;
                    List<DTMeaslocStatus> measdata = compItem.MeaslocList;

                    if (measdata == null || measdata.Count == 0)
                    {
                        continue;
                    }

                    foreach (DTMeaslocStatus meas in measdata)
                    {
                        if (meas.MeaslocId.Contains("TOWTOP"))
                        {
                            topCompItem.CompId = compId + "TOP";
                            topMeaslocList.Add(meas);
                        }
                        else if (meas.MeaslocId.Contains("TOWFDN"))
                        {
                            fdnCompItem.CompId = compId + "FDN";
                            fdnMeaslocList.Add(meas);
                        }
                        else if (meas.MeaslocId.Contains("PL"))
                        {
                            cbfCompItem.CompId = compId + "CBF";
                            cbfMeaslocList.Add(meas);
                        }
                        else if (meas.MeaslocId.Contains("BOL"))
                        {
                            bfmCompItem.CompId = compId + "BFM";
                            bfmMeaslocList.Add(meas);
                        }
                        else if (meas.MeaslocId.Contains("FL"))
                        {
                            glggapCompItem.CompId = compId + "FLG";
                            glggapMeaslocList.Add(meas);
                        }
                    }
                }

                // 塔顶对象
                if (topMeaslocList != null && topMeaslocList.Count != 0)
                {
                    // 过滤掉温度测点
                    topMeaslocList = topMeaslocList.Where(vo => !vo.MeaslocId.EndsWith("T")).ToList();
                    topCompItem.MeaslocList = topMeaslocList;
                    res.Add(topCompItem);
                }
                // 塔基对象
                if (fdnMeaslocList != null && fdnMeaslocList.Count != 0)
                {
                    // 过滤掉温度测点
                    fdnMeaslocList = fdnMeaslocList.Where(vo => !vo.MeaslocId.EndsWith("T")).ToList();
                    fdnCompItem.MeaslocList = fdnMeaslocList;
                    res.Add(fdnCompItem);
                }
                // 索力对象
                if (cbfMeaslocList != null && cbfMeaslocList.Count != 0)
                {
                    // 过滤掉切向和温度测点
                    cbfMeaslocList = cbfMeaslocList.Where(vo => !vo.MeaslocId.EndsWith("T") && !vo.MeaslocId.EndsWith("Ti")).ToList();
                    cbfCompItem.MeaslocList = cbfMeaslocList;
                    res.Add(cbfCompItem);
                }
                // 法兰间隙对象
                if (glggapMeaslocList != null && glggapMeaslocList.Count != 0)
                {
                    glggapCompItem.MeaslocList = glggapMeaslocList;
                    res.Add(glggapCompItem);
                }
                // 法兰间隙对象
                if (bfmMeaslocList != null && bfmMeaslocList.Count != 0)
                {
                    bfmCompItem.MeaslocList = bfmMeaslocList;
                    res.Add(bfmCompItem);
                }
            }
            return res;
        }


        /// <summary>
        /// 1.4、将聚合大部件下数据按照实体部件聚合
        /// </summary>
        /// <param name="compDatas"></param> 
        /// <param name="compCode">部件类型过滤code</param>
        /// <param name="aggregateCode">聚合部件id替换code列表</param>
        /// <param name="pageCompCode">聚合部件Code</param>
        /// <param name="pageCompName">聚合部件类型名称</param>
        /// <param name="isMeaslocChildren">大部件的子级为实体部件层或是测点层（true为测点层）</param>
        private DTBigCompStatusDTO ConvertPageCompTree(List<DTCompStatus> compDatas, List<string> compCode, List<string> aggregateCode, string pageCompCode, string pageCompName, bool isMeaslocChildren)
        {
            // 处理实体部件列表
            List<DTCompStatusDTO> compList = ConvertCompTree(compDatas, compCode);

            // 如果有部件数据返回对应结果
            if (compList == null && compList.Count == 0)
            {
                return new DTBigCompStatusDTO();
            }

            // 大部件id
            string id = compDatas[0].CompId;
            // 替换聚合部件id
            if (aggregateCode != null && aggregateCode.Count != 0)
            {
                foreach (var code in aggregateCode)
                {
                    id = Regex.Replace(id, code, pageCompCode, RegexOptions.IgnoreCase);
                }
            }
            DTBigCompStatusDTO obj = new()
            {
                Name = pageCompName,
                Code = pageCompCode,
                Type = "bigComponent",
                Id = id,
                Children = compList
            };
            return obj;

        }

        /// <summary>
        /// 1.5、处理接口部件返回层
        /// </summary>
        /// <param name="compList"></param>
        /// <param name="compCode"></param>
        /// <returns></returns>
        private List<DTCompStatusDTO> ConvertCompTree(List<DTCompStatus> compList, List<string> compCode)
        {
            List<DTCompStatusDTO> res = new List<DTCompStatusDTO>();
            if (compList == null || compList.Count == 0)
                return res;

            foreach (var item in compList)
            {
                DTCompStatusDTO objs = new DTCompStatusDTO();
                // 部件名称修改
                objs.Name = componentHelper.HandlerCompName(item.CompId, item.CompName);
                objs.Code = item.CompCode;
                objs.Type = "component";
                objs.Id = item.CompId;
                objs.Status = item.CompStatus.ToString().ToLower();
                objs.Time = item.CompStatusTime.ToString();
                if (item.MeaslocList != null && item.MeaslocList.Count != 0 && compCode != null && compCode.Count != 0)
                {
                    // 根据部件类型过滤code过滤测点数据
                    List<DTMeaslocStatus> measLocItems = new List<DTMeaslocStatus>();
                    foreach (DTMeaslocStatus meas in item.MeaslocList)
                    {
                        bool noFiltering = false;
                        foreach (var code in compCode)
                        {
                            // 如果该测点在部件类型过滤code 中包含返回ture
                            if (meas.MeaslocId.Contains(code))
                            {
                                noFiltering = true;
                                break;
                            }

                        }
                        if (noFiltering)
                        {
                            meas.MeaslocName = componentHelper.HandlerMeaslocName(meas.MeaslocId, meas.MeaslocName);
                            measLocItems.Add(meas);
                        }
                    }
                    // 测点聚合
                    objs.Children = ConvertMeaslocTree(measLocItems);
                }


                // 如果有测点返回结果
                if (objs.Children != null && objs.Children.Count != 0)
                {
                    res.Add(objs);
                }
            }
            return res.SortByName(ascending: true, dictType: EnumSortType.CompName);
        }

        /// <summary>
        /// 1.6、处理接口测点返回层
        /// </summary>
        /// <param name="measlocList">测点列表</param>
        /// <returns></returns>
        private List<DTMeaslocStatusDTO> ConvertMeaslocTree(List<DTMeaslocStatus> measlocList)
        {
            List<DTMeaslocStatusDTO> res = new List<DTMeaslocStatusDTO>();

            foreach (var item in measlocList)
            {
                if (item.MeaslocId.Contains("RSPD")) continue;

                string status = item.DiagnosisStatus == null ? EnumAlarmStatus.Nostate.ToString().ToLower() : item.DiagnosisStatus.ToLower();
                DTMeaslocStatusDTO obj = new()
                {
                    Name = item.MeaslocName,
                    Code = "",
                    Type = "measloc",
                    Id = item.MeaslocId,
                    Status = item.MeaslocStatus.ToString().ToLower(),
                    Time = item.MeaslocStatusTime.ToString(),

                    IsDiagnosticResults = item.IsDiagnosticResults,
                    DiagnosisStatus = status,
                    DiagnosisConclusion = item.DiagnosisConclusion,
                    DiagnosisTime = item.DiagnosisTime,
                    Children = new List<DTEvStatusDTO>()
                    // Children = ConvertEvTree(item.EigenValueList)
                };

                res.Add(obj);
            }

            return res.SortByName(ascending: true, dictType: EnumSortType.MeaslocName);
        }

    }
}
