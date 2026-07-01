
using ACH.ACHLog.SeriLog;
using ACH.Alarm.DB;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Analysis;
using ACH.Helper.Component;
using System.Globalization;
using EigenValueData = ACH.MeasData.Entity.EigenValueData;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    /// <summary>
    /// GetEvAnalyzerData接口处理
    /// </summary>
    public class EvTrendListMethods
    {
        private readonly List<string> OAMeaslocCode = new() { "TOWFDNTiA", "TOWFDNNaA" };
        private readonly List<string> OAEVCode = new() { "XI", "YI" };

        private IDevTreeRepsitory _devTreeRepository;
        private DBSelect evDBSelect;
        private AlarmConfigContext alarmConfigContext;
        private ComponentHelper convertRepository = new ComponentHelper();

        public EvTrendListMethods(IConfiguration configuration)
        {
            _devTreeRepository = DevTreeRepsitory.Instance;
            alarmConfigContext = AlarmConfigContext.Instance;
            evDBSelect = new DBSelect(configuration);
        }


        /// <summary>
        /// 特征值趋势接口处理
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        internal EvTrendDTO ConvertToEvTrend(EvTrendFromBodyDTO param)
        {
            EvTrendDTO res = new EvTrendDTO();
            res.EvdataList = new List<EvTrendItemDTO>();

            List<double?> rot = new List<double?>();
            List<double?> power = new List<double?>();

            if (param.wkCond.ContainsKey("ROTSPEED_MCS"))
            {
                rot = HandlerRange(param.wkCond["ROTSPEED_MCS"]);
            }
            else
            {
                rot.AddRange(new List<double?>() { null, null });
            }

            if (param.wkCond.ContainsKey("POWER"))
            {
                power = HandlerRange(param.wkCond["POWER"]);
            }
            else
            {
                power.AddRange(new List<double?>() { null, null });
            }

            switch (param.analyzeWay)
            {
                case WaveCommonConstant.Trend_Analysis:
                    res.EvdataList.AddRange(GetTAData(param, rot, power));
                    break;
                case WaveCommonConstant.Distributional_Analysis:
                    res.EvdataList.AddRange(GetDAData(param, rot, power));
                    break;
                case WaveCommonConstant.Pwer_Correlation_Analysis:
                    res.EvdataList.AddRange(GetPCAData(param));
                    break;
                case WaveCommonConstant.RotateSpeed_Correlation_Analysis:
                    res.EvdataList.AddRange(GetRCAData(param));
                    break;
                case WaveCommonConstant.WindSpeed_Correlation_Analysis:
                    res.EvdataList.AddRange(GetWCAData(param));
                    break;
                case WaveCommonConstant.Overturn_Analysis:
                    res.EvdataList.AddRange(GetOAData(param));
                    break;
                case WaveCommonConstant.FDN_speciaiItem_Analysis:// 塔基专项分析
                    res.EvdataList.AddRange(GetFDNSAData(param, rot, power));
                    break;
                default:
                    return new EvTrendDTO();
            }

            if (res.EvdataList == null || res.EvdataList.Count == 0)
            {
                ALog.Debug($"ConvertToEvTrend-获取特征值趋时异常，数据库中无数据");
                return res;
            }
            var first = res.EvdataList[0];
            res.Max = System.Convert.ToDouble(res.EvdataList.Max(o => o.Max));
            res.Min = System.Convert.ToDouble(res.EvdataList.Min(o => o.Min));
            res.Mid = first.Mid;
            res.Avg = System.Convert.ToDouble(res.EvdataList.Select(o => o.Avg).Average());
            res.VdiMax = null;
            res.VdiMin = null;
            res.UnitX = "t";
            res.UnitY = first.UnitY;
            var vdiMaxValues = res.EvdataList.Where(o => o.VdiMax.HasValue).Select(o => o.VdiMax.Value).ToList();
            var vdiMinValues = res.EvdataList.Where(o => o.VdiMin.HasValue).Select(o => o.VdiMin.Value).ToList();
            if (vdiMaxValues.Count != 0)
            {
                res.VdiMax = vdiMaxValues.Max();
            }
            if (vdiMinValues.Count != 0)
            {
                res.VdiMin = vdiMinValues.Min();
            }
            return res;
        }

        /// <summary>
        /// 2.1、处理趋势分析 
        /// </summary>
        /// <param name="param"></param>
        /// <param name="rot"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetTAData(EvTrendFromBodyDTO param, List<double?> rot, List<double?> power)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();
            foreach (var item in param.eigenValueIds)
            {
                string[] ids = item.Split("&&");
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(ids[0]) ?? new(); // 根据测点ID获取设备树信息

                // 获取特征值趋势
                List<EigenValueData> evData = evDBSelect.GetEigenValueHis(measObj, new string[] { ids[1] }, DateTime.Parse(param.startTime), DateTime.Parse(param.endTime), rot[0], rot[1]);

                // 塔顶塔基特征过滤 ？todo：是否使用
                evData = TopFdnEvFilter(param.eigenValueIds, measObj.MeasDataType, evData);

                if (evData == null || evData.Count == 0) continue;
                EigenValueData first = evData.FirstOrDefault() ?? new();

                // 返回的特征值趋势对象属性赋值
                EvTrendItemDTO obj = CommonEVDataObj(first, measObj);

                // 将特征值单独组成List<double>，计算最大最小值，避免精度丢失
                List<double> sortEvData = evData.Select(o => o.EigenValue).OrderBy(o => o).ToList();
                obj.Max = double.Parse(sortEvData[sortEvData.Count - 1].ToString("G5"));
                obj.Min = double.Parse(sortEvData[0].ToString("G5"));
                obj.Mid = sortEvData.Count > 1 ? double.Parse(sortEvData[sortEvData.Count / 2 - 1].ToString("G5")) : double.Parse(sortEvData[0].ToString("G5"));
                obj.Avg = double.Parse(sortEvData.Average().ToString("G5"));
                obj.UnitY = convertRepository.GetEigenValueTrendUnitY(first);

                obj.Evdata = HandlerTAEvData(evData);

                res.Add(obj);
            }

            return res;
        }

        /// <summary>
        /// 2.1、处理趋势分析Evdata
        /// </summary>
        /// <param name="evData"></param>
        /// <returns></returns>
        private List<List<object>> HandlerTAEvData(List<EigenValueData> evData)
        {
            List<List<object>> res = new List<List<object>>();
            var evnum = evData.Count();
            if (evnum == 0) return res;
            for (int i = 0; i < evnum; i++)
            {
                var evObj = evData[i];
                /*double raw = evObj.EigenValue;
                double v = Math.Round(raw / Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(raw)))), 5) * Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(raw))));
                string txt = v.ToString("F15");*/
                res.Add(new List<object> { evObj.AcqTime.ToString("yyyy-MM-dd HH:mm:ss"), ToSig5(evObj.EigenValue), "", true, false });
            }
            return res.OrderBy(o => o[0]).ToList();
        }

        public static string ToSig5(double d)
        {
            // 1. 零直接返回
            if (d == 0) return "0";

            // 2. 截断成 5 位有效数字
            int exp = (int)Math.Floor(Math.Log10(Math.Abs(d)));
            double scale = Math.Pow(10, exp);
            double v = Math.Round(d / scale, 5, MidpointRounding.AwayFromZero) * scale;

            // 3. 计算【刚好能露出 5 位有效数字】的最大小数位
            int decimals = Math.Max(0, 4 - exp);   // 5 位 - 1 位整数位
            if (d < 0) decimals--;                 // 负号占一位

            // 4. 用 "F" 格式化后再去尾零
            string s = v.ToString("F" + decimals, CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
            return s;
        }


        /// <summary>
        /// 2.2、处理分布分析
        /// </summary>
        /// <param name="param"></param>
        /// <param name="rot"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetDAData(EvTrendFromBodyDTO param, List<double?> rot, List<double?> power)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();
            foreach (var item in param.eigenValueIds)
            {
                var measlocId = item.Split("&&").ToList()[0];
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(measlocId) ?? new(); // 根据测点ID获取设备树信息
                // 获取特征值趋势
                List<EigenValueData> evData = evDBSelect.GetEigenValueHis(measObj, new string[] { measlocId[1].ToString() }, DateTime.Parse(param.startTime), DateTime.Parse(param.endTime), rot[0], rot[1]);

                if (evData == null || evData.Count == 0) continue;

                // 塔顶塔基特征过滤
                evData = TopFdnEvFilter(param.eigenValueIds, measObj.MeasDataType, evData);

                EigenValueData first = evData.FirstOrDefault() ?? new();

                // 返回的特征值趋势对象属性赋值
                EvTrendItemDTO obj = CommonEVDataObj(first, measObj);

                // 将特征值单独组成List<double>，计算最大最小值，避免精度丢失               
                List<double> sortEvData = evData.Select(o => o.EigenValue).OrderBy(o => o).ToList();
                obj.Max = double.Parse(sortEvData[sortEvData.Count - 1].ToString("G5"));
                obj.Min = double.Parse(sortEvData[0].ToString("G5"));
                obj.Mid = sortEvData.Count > 1 ? double.Parse(sortEvData[sortEvData.Count / 2 - 1].ToString("G5")) : double.Parse(sortEvData[0].ToString("G5"));
                obj.Avg = double.Parse(sortEvData.Average().ToString("G5"));
                obj.UnitY = first.Unit;

                obj.Evdata = HandlerDAEvData(evData);

                res.Add(obj);
            }
            return res;
        }

        /// <summary>
        /// 2.2、处理分布分析Evdata
        /// </summary>
        /// <param name="evData"></param>
        /// <returns></returns>
        private List<List<object>> HandlerDAEvData(List<EigenValueData> evData)
        {
            List<List<object>> res = new List<List<object>>();
            var evnum = evData.Count();
            if (evnum == 0) return res;
            for (int i = 0; i < evnum; i++)
            {
                var evObj = evData[i];
                res.Add(new List<object> { ToSig5(evObj.EigenValue) }); // 是否有波形 是否有故障
            }
            return res;
        }


        /// <summary>
        /// 2.3、处理功率相关性分析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetPCAData(EvTrendFromBodyDTO param)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();

            return res;
        }

        /// <summary>
        /// 2.4、处理转速相关性分析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetRCAData(EvTrendFromBodyDTO param)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();

            return res;
        }

        /// <summary>
        /// 2.5、处理风速相关性分析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetWCAData(EvTrendFromBodyDTO param)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();

            return res;
        }


        /// <summary>
        /// 2.6、倾覆分析
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private List<EvTrendItemDTO> GetOAData(EvTrendFromBodyDTO param)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();
            if (param.windturbineIds == null)
                return res;
            foreach (var turbineId in param.windturbineIds)
            {
                // 新：获取该测点的特征值数据
                string measlocIdX = $"{turbineId}{OAMeaslocCode[0]}";
                DevMeasLocation measObjX = _devTreeRepository.GetMeasLocationByMeaslocID(measlocIdX) ?? new();
                string evIDX = $"{measlocIdX}&&{OAEVCode[0]}";
                // 获取X倾角特征值趋势
                List<EigenValueData> XData = evDBSelect.GetEigenValueHis(measObjX, new string[] { OAEVCode[0] }, DateTime.Parse(param.startTime), DateTime.Parse(param.endTime), null, null);

                string measlocIdY = $"{turbineId}{OAMeaslocCode[1]}";
                DevMeasLocation measObjY = _devTreeRepository.GetMeasLocationByMeaslocID(measlocIdY) ?? new();
                string evID = $"{measlocIdY}&&{OAEVCode[1]}";

                // Y倾角数据
                List<EigenValueData> YData = evDBSelect.GetEigenValueHis(measObjY, new string[] { OAEVCode[1] }, DateTime.Parse(param.startTime), DateTime.Parse(param.endTime), null, null);


                if (XData == null || XData.Count == 0) continue;
                if (YData == null || YData.Count == 0) continue;

                XData = TopFdnEvFilter(param.eigenValueIds, measObjX.MeasDataType, XData);// 塔顶塔基特征过滤
                YData = TopFdnEvFilter(param.eigenValueIds, measObjY.MeasDataType, YData);// 塔顶塔基特征过滤

                EigenValueData first = XData.FirstOrDefault() ?? new();

                // 属性赋值
                EvTrendItemDTO obj = CommonEVDataObj(first, measObjX);

                // 根据double数据计算最大最小值
                List<double> sortEvData = XData.Select(o => o.EigenValue).OrderBy(o => o).ToList();
                obj.Max = double.Parse(sortEvData[sortEvData.Count - 1].ToString("G5"));
                obj.Min = double.Parse(sortEvData[0].ToString("G5"));
                obj.Mid = sortEvData.Count > 1 ? double.Parse(sortEvData[sortEvData.Count / 2 - 1].ToString("G5")) : double.Parse(sortEvData[0].ToString("G5"));
                obj.Avg = double.Parse(sortEvData.Average().ToString("G5"));
                obj.UnitY = first.Unit;

                // 组装特征值趋势返回值
                obj.Evdata = HandlerOAEvData(XData, YData);

                res.Add(obj);
            }

            return res;
        }

        /// <summary>
        /// 2.6、处理趋势分析Evdata
        /// </summary>
        /// <param name="evData"></param>
        /// <param name="evDataY"></param>
        /// <returns></returns>
        private List<List<object>> HandlerOAEvData(List<EigenValueData> evData, List<EigenValueData> evDataY)
        {
            List<List<object>> res = new List<List<object>>();
            var num = evData.Count();
            if (num == 0) return res;
            for (int i = 0; i < num; i++)
            {
                var evObj = evData[i];

                var x = evData.Count > i ? ToSig5(evObj.EigenValue) : "";
                var yobj = evDataY.Find(o => o.AcqTime == evObj.AcqTime);
                var y = yobj == null ? "" : ToSig5(evDataY[i].EigenValue);

                res.Add(new List<object> { evObj.AcqTime.ToString("yyyy-MM-dd HH:mm:ss"), x, y });

            }
            return res;
        }


        // 2.7、塔基专项分析
        private List<EvTrendItemDTO> GetFDNSAData(EvTrendFromBodyDTO param, List<double?> rot, List<double?> power)
        {
            List<EvTrendItemDTO> res = new List<EvTrendItemDTO>();
            if (param.windturbineIds != null && param.windturbineIds.Count != 0)
            {
                foreach (var wid in param.windturbineIds)
                {   // 1. 输入参数处理
                    string mid = wid + "TOWFDNTiA";// 塔筒塔基 横向倾角mid
                    string treCode = "TRE";
                    string sahCode = "SAH";
                    string treName = "";
                    string sahName = "";
                    // 2.根据测点ID获取设备树信息
                    DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(mid) ?? new();

                    // 3.查询倾斜率特征值数据，查询塔基横向倾角测点的特征值即可 配置在了横向倾角
                    List<EigenValueData> evData = evDBSelect.GetEigenValueHis(measObj, new string[] { treCode, sahCode }, DateTime.Parse(param.startTime), DateTime.Parse(param.endTime), rot[0], rot[1]);

                    // 4.结果测点处理 多个测点数据
                    if (evData == null || evData.Count == 0) continue;

                    EigenValueData first = evData.FirstOrDefault() ?? new();

                    // 4.1 辅助信息分封装
                    EvTrendItemDTO obj = CommonEVDataObj(first, measObj);

                    // 特殊处理
                    obj.MeaslocId = mid + "," + mid.Replace("TiA", "NaA");
                    obj.UnitX = "";
                    obj.UnitY = "";
                    obj.EvCode = treCode + "," + sahCode;
                    obj.EvId = mid + "&&" + treCode + "," + mid + "&&" + sahCode;
                    obj.MeaslocCode = "";
                    obj.MeaslocName = "";
                    // 4.3 趋势数据封装
                    obj.Evdata = HandlerTTAEvData(evData, treCode, sahCode, ref treName, ref sahName);
                    obj.EvName = treName + "," + sahName;
                    res.Add(obj);
                }
            }
            return res;
        }

        /// <summary>
        /// 2.7、处理塔基轨迹分析Evdata
        /// </summary>
        /// <param name="evData"></param>
        /// <returns></returns>
        private List<List<object>> HandlerTTAEvData(List<EigenValueData> evData, string treCode, string sahCode, ref string treName, ref string sahName)
        {
            // 1.参数校验
            if (evData == null || evData.Count == 0)
            {
                return new List<List<object>>();
            }
            // 2.对数据按照时间分组
            Dictionary<DateTime, List<EigenValueData>> groupedByTime = evData.GroupBy(d => d.AcqTime).ToDictionary(g => g.Key, g => g.ToList());
            List<List<object>> res = new List<List<object>>();
            foreach (var item in groupedByTime)
            {
                string acqTime = item.Key.ToString("yyyy-MM-dd HH:mm:ss");
                List<EigenValueData> eigenValues = item.Value;
                string? tRE = null;
                string? sAH = null;
                foreach (var ev in eigenValues)
                {
                    if (ev.EigenValueCode == treCode)
                    {
                        tRE = ToSig5(ev.EigenValue);
                        treName = ev.EigenValueName;
                    }
                    else if (ev.EigenValueCode == sahCode)
                    {
                        sAH = ToSig5(ev.EigenValue);
                        sahName = ev.EigenValueName;
                    }

                }
                // 有特征值返回结果
                if (tRE != null && sAH != null)
                {
                    res.Add(new List<object> { tRE, sAH, acqTime }); // 倾斜率  沉降方位角 时间 
                }
            }
            return res.OrderBy(o => o[2]).ToList();
        }



        /// <summary>
        /// 特征值趋势公共方法1：返回一条特征值趋势对象
        /// </summary>
        /// <param name="first"></param>
        /// <param name="measObj"></param>
        /// <returns></returns>
        private EvTrendItemDTO CommonEVDataObj(EigenValueData first, DevMeasLocation measObj)
        {
            // 叶片特征值名称特殊处理
            string evName = first.EigenValueName;
            if (measObj.MeasDataType == EnumMonitorType.BVM)
            {
                evName = first.EigenValueCode;
            }

            // 根据测量事件获取特征值报警线
            double vdiMin = 0;
            double vdiMax = 0;
            double attention = 0;
            double warning = 0;
            double danger = 0;
            List<AlarmConfig> alarms = alarmConfigContext.GetAlarmConfigsByEV(measObj.MeasLoctionID, first.EigenValueCode);
            if (alarms != null && alarms.Count != 0 && alarms.FirstOrDefault() != null)
            {
                vdiMin = alarms.FirstOrDefault().Attention;
                vdiMax = alarms.FirstOrDefault().Warning;
                attention = alarms.FirstOrDefault().Attention;
                warning = alarms.FirstOrDefault().Warning;
                danger = alarms.FirstOrDefault().Danger;
            }


            EvTrendItemDTO obj = new()
            {
                WindParkName = measObj.StationName,
                WindturbineId = measObj.DeviceID,
                WindturbineName = measObj.DeviceName,
                CompName = measObj.ComponentName,
                MeasdefId = "",
                MeaslocId = measObj.MeasLoctionID,
                MeaslocCode = measObj.MeasLoctionID.Replace(measObj.ComponentID, ""),

                MeaslocName = convertRepository.HandlerMeaslocName(measObj.MeasLoctionID, measObj.MeasLoctionName),

                EvCode = first.EigenValueCode,
                EvId = first.EigenValueID,
                EvName = evName,
                SampleRate = first.SampleRate.ToString(),
                Samplingtimelength = "", // 采样时长 ？

                VdiMax = vdiMax,
                VdiMin = vdiMin,
                BandWidth = "", // 带宽 ？

                UnitX = "t", // 特征值趋势X轴单位确定
                UnitY = first.Unit,

                DisVdiMax = 0,
                DisVdiMin = 0,
                DisVdiMaxNum = 0,
                DisVdiMinNum = 0,

                Attention = attention,
                Warning = warning,
                Danger = danger,

                WaveDefId = "", // 波形定义ID ？
                Evdata = new List<List<object>>(),
            };
            return obj;
        }


        /// <summary>
        /// 特征值趋势公共方法2：塔顶塔基特征过滤
        /// </summary>
        /// <param name="eigenValueIds">特征值ID列表</param>
        /// <param name="measDataType">测量事件类型</param>
        /// <param name="evData">特征值趋势数据</param>
        /// <returns></returns>
        private static List<EigenValueData> TopFdnEvFilter(List<string> eigenValueIds, EnumMonitorType measDataType, List<EigenValueData> evData)
        {
            // 塔筒 塔顶 过滤对应特征值 需要过滤，其他不影响  && (measObj.MeasDataType == EnumMonitorType.TVM_STE_TOP || measObj.MeasDataType == EnumMonitorType.TVM_STE_FDN)
            if (eigenValueIds != null && eigenValueIds.Count != 0 && evData != null && evData.Count != 0 && (measDataType == EnumMonitorType.TVM_STE))
            {
                evData = evData.Where(vo => eigenValueIds.Contains(vo.EigenValueID)).ToList();
            }

            return evData;
        }



        /// <summary>
        /// 处理工况信息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<double?> HandlerRange(List<double?> list)
        {
            List<double?> res = new List<double?>();
            foreach (double? item in list)
            {
                if (item != null)
                {
                    res.Add(item.Value);
                }
                else
                {
                    res.Add(null);
                }
            }
            return res;
        }
    }
}
