using ACH.DataEntity.Common;
using ACH.DataEntity.Enum;
using ACH.DataEntity.EnumType;
using ACH.MeasData.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using ACH.DataEntity.DevTree;

namespace ACH.Helper.Component
{
    public class ComponentHelper
    {
        // 实体部件和聚合部件关系字典
        private readonly Dictionary<(string, string), ComponentMapping> pageCompDir = new Dictionary<(string, string), ComponentMapping>
        {
            [("windturbine", "BL1")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "叶片一", compType: "BL1", sort: 1),
            [("windturbine", "BL2")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "叶片二", compType: "BL2", sort: 1),
            [("windturbine", "BL3")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "叶片三", compType: "BL3", sort: 1),
            [("windturbine", "MST")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "主轴", compType: "MST", sort: 1),
            [("windturbine", "GEN")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "发电机", compType: "GEN", sort: 1),
            [("windturbine", "GBX")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "齿轮箱", compType: "GBX", sort: 1),
            [("windturbine", "TOW")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "塔筒", compType: "TOW", sort: 1),
            [("windturbine", "YBG")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "偏航轴承", compType: "YBG", sort: 1),
            [("windturbine", "PB1")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "变桨轴承一", compType: "PB1", sort: 1),
            [("windturbine", "PB2")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "变桨轴承二", compType: "PB2", sort: 1),
            [("windturbine", "PB3")] = new ComponentMapping(pageCompName: "整机", pageCompType: "windturbine", compName: "变桨轴承三", compType: "PB3", sort: 1),

            [("ROT", "BL1")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "叶片一", compType: "BL1", sort: 2),
            [("ROT", "BL2")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "叶片二", compType: "BL2", sort: 2),
            [("ROT", "BL3")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "叶片三", compType: "BL3", sort: 2),
            [("ROT", "PB1")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "变桨轴承一", compType: "PB1", sort: 2),
            [("ROT", "PB2")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "变桨轴承二", compType: "PB2", sort: 2),
            [("ROT", "PB3")] = new ComponentMapping(pageCompName: "风轮", pageCompType: "ROT", compName: "变桨轴承三", compType: "PB3", sort: 2),

            [("NAC", "GBX")] = new ComponentMapping(pageCompName: "机舱", pageCompType: "NAC", compName: "叶片一", compType: "GBX", sort: 3),
            [("NAC", "MST")] = new ComponentMapping(pageCompName: "机舱", pageCompType: "NAC", compName: "叶片一", compType: "MST", sort: 3),
            [("NAC", "GEN")] = new ComponentMapping(pageCompName: "机舱", pageCompType: "NAC", compName: "叶片一", compType: "GEN", sort: 3),

            [("TWW", "YBG")] = new ComponentMapping(pageCompName: "塔筒", pageCompType: "TWW", compName: "偏航轴承", compType: "YBG", sort: 4),
            [("TWW", "TOW")] = new ComponentMapping(pageCompName: "塔筒", pageCompType: "TWW", compName: "塔筒", compType: "TOW", sort: 4),

            [("YPB", "PB1")] = new ComponentMapping(pageCompName: "变桨与偏航", pageCompType: "YPB", compName: "变桨轴承一", compType: "PB1", sort: 5),
            [("YPB", "PB2")] = new ComponentMapping(pageCompName: "变桨与偏航", pageCompType: "YPB", compName: "变桨轴承二", compType: "PB2", sort: 5),
            [("YPB", "PB3")] = new ComponentMapping(pageCompName: "变桨与偏航", pageCompType: "YPB", compName: "变桨轴承三", compType: "PB3", sort: 5),
            [("YPB", "YBG")] = new ComponentMapping(pageCompName: "变桨与偏航", pageCompType: "YPB", compName: "偏航轴承", compType: "YBG", sort: 5),
        };

        /// <summary>
        /// EnumMonitorType和测点Code之间的对应关系
        /// </summary>
        private readonly Dictionary<string, List<string>> MonitorTypeDir = new Dictionary<string, List<string>>
        {
            [EnumMonitorType.CVM.ToString()] = new List<string> { "MST", "GBX", "GEN" },
            [EnumMonitorType.BVM.ToString()] = new List<string> { "BL1", "BL2", "BL3", "PB1", "PB2", "PB3" },
            [EnumMonitorType.TVM_STE.ToString()] = new List<string> { "TOWTOP", "TOWFDN", "TOWFL", "TOWPL" },
            [EnumMonitorType.TVM_FLG_GAP.ToString()] = new List<string> { "TOWFL" },
            [EnumMonitorType.TVM_CBF.ToString()] = new List<string> { "TOWPL" },
            [EnumMonitorType.TVM_BFM.ToString()] = new List<string> { "TOWFL1BOL", "TOWFL2BOL", "TOWFL3BOL" },
        };

        public ComponentHelper()
        {
            pageCompNameType = pageCompDir.Values.GroupBy(v => v.PageCompName)
               .ToDictionary(
                   g => g.Key,
                   g => g.First().PageCompType,
                   StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 根据上述字典，获取聚合部件code和名称的新字典
        /// </summary>
        private readonly Dictionary<string, string> pageCompNameType = new Dictionary<string, string>();

        #region 部件相关实现

        public Dictionary<(string, string), ComponentMapping> GetComponentDirc()
        {
            return pageCompDir;
        }


        /// <summary>
        /// 根据聚合部件获取实体部件
        /// </summary>
        /// <param name="pagecomp">聚合部件Code</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<string> GetCompType(string pagecomp)
        {
            List<string> res = new List<string>();
            foreach (var kv in pageCompDir)
            {
                if (kv.Key.Item1 == pagecomp)
                    res.Add(kv.Key.Item2);
            }
            return res;
        }


        public Dictionary<string, List<string>> GetPageCompTypeDir()
        {
            Dictionary<string, List<string>> CompTypeDir = new Dictionary<string, List<string>>
            {
                ["NAC"] = new List<string> { "MST", "GBX", "GEN" },
                ["ROT"] = new List<string> { "BL1", "BL2", "BL3", "PB1", "PB2", "PB3" },
                ["TWW"] = new List<string> { "TOWTOP", "TOWFDN", "TOWFL1", "TOWFL2", "TOWFL3", "TOWPL1" }
            };
            return CompTypeDir;
        }



        /// <summary>
        /// 根据聚合部件名称获取code
        /// </summary>
        /// <param name="compName"></param>"
        /// <returns></returns>
        public string GetBigCompCodeByName(string compName)
        {
            return pageCompNameType.TryGetValue(compName, out var t) ? t : "";
        }


        /// <summary>
        /// 根据聚合部件获取实体部件字典
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, List<string>> GetComponentTypeDic()
        {
            return MonitorTypeDir;
        }


        public Dictionary<List<string>, EnumMonitorType> GetComponentTypeDicReverse()
        {
            Dictionary<List<string>, EnumMonitorType> acquisitionComponentTypeReverse = new Dictionary<List<string>, EnumMonitorType>();
            Dictionary<string, List<string>> acquisitionComponentType = GetComponentTypeDic();
            if (acquisitionComponentType != null && acquisitionComponentType.Count != 0)
            {
                foreach (var type in acquisitionComponentType)
                {
                    acquisitionComponentTypeReverse[type.Value] = (EnumMonitorType)Enum.Parse(typeof(EnumMonitorType), type.Key);
                }
            }

            return acquisitionComponentTypeReverse;
        }


        public EnumCircleType ConvertToCircleType(string code, string measlocId)
        {
            switch (code)
            {
                case string idx when (code.Contains("TOWPL")):
                    return EnumCircleType.CBF;
                case string idx when (code.Contains("TOWFL")):
                    if (measlocId.Contains("BOL"))
                    {
                        return EnumCircleType.BFM;
                    }
                    else
                    {
                        return EnumCircleType.FLG_GAP;
                    }
                default:
                    return EnumCircleType.Others;
            }
        }
        #endregion

        #region 部件名称处理
        /// <summary>
        /// 特殊处理：处理部件名称
        /// </summary>
        /// <param name="compID"></param>
        /// <param name="compName"></param>
        /// <returns></returns>
        public string HandlerCompName(string compID, string compName)
        {
            switch (compID)
            {
                case string code when (code.Contains(EnumCompType.BL1.ToString())):
                    return "叶片一(B1)";
                case string code when (code.Contains(EnumCompType.BL2.ToString())):
                    return "叶片二(B2)";
                case string code when (code.Contains(EnumCompType.BL3.ToString())):
                    return "叶片三(B3)";
                default:
                    return compName;
            }
        }

        public string HandlerCompName(string compCode)
        {
            switch (compCode)
            {
                case "MST":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.MST);
                case "GBX":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.GBX);
                case "GEN":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.GBX);
                case "BL1":
                    return "B1";
                case "BL2":
                    return "B2";
                case "BL3":
                    return "B3";
                case "PB1":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.PB1);
                case "PB2":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.PB2);
                case "PB3":
                    return EnumHelper.GetDescription(EnumCompIDKeyWords.PB3);
                case "TOWTOP":
                    return "塔筒塔顶";
                case "TOWFDN":
                    return "塔筒塔基";
                case "TOWFL":
                    return "塔筒法兰";
                case "TOWPL":
                    return "塔筒索力";
                default:
                    return compCode;
            }
        }

        #endregion


        #region 处理卡片标题
        public string ConvertEVCardTitle(string title)
        {
            switch (title)
            {
                case string code when (code.Contains("叶片一")):
                    return "B1";
                case string code when (code.Contains("叶片二")):
                    return "B2";
                case string code when (code.Contains("叶片三")):
                    return "B3";
                default:
                    return title;
            }
        }


        public string ConvertEVName(string evid)
        {
            switch (evid)
            {
                case string code when (code.Contains("Fa")):
                    return "挥舞";
                case string code when (code.Contains("La")):
                    return "摆振";
                case string code when (code.Contains("Ri0O")):
                    return "零点";
                case string code when (code.Contains("Ri3O")):
                    return "三点";
                case string code when (code.Contains("Ri6O")):
                    return "六点";
                case string code when (code.Contains("Ri9O")):
                    return "九点";
                default:
                    return "";
            }
        }

        #endregion



        #region 测点名称处理
        /// <summary>
        /// 特殊处理：处理测点名称
        /// </summary>
        /// <param name="measlocID">测点ID</param>
        /// <param name="measlocName">原测点名称</param>
        /// <returns></returns>
        public string HandlerMeaslocName(string measlocID, string measlocName)
        {
            switch (measlocID)
            {
                case string code when (code.Contains("BL1MPTFa")):
                    return "B1挥舞";
                case string code when (code.Contains("BL1MPTLa")):
                    return "B1摆振";
                case string code when (code.Contains("BL2MPTFa")):
                    return "B2挥舞";
                case string code when (code.Contains("BL2MPTLa")):
                    return "B2摆振";
                case string code when (code.Contains("BL3MPTFa")):
                    return "B3挥舞";
                case string code when (code.Contains("BL3MPTLa")):
                    return "B3摆振";
                default:
                    return measlocName;
            }
        }
        #endregion



        #region 特征值名称处理
        /// <summary>
        /// 根据特征值Code获取特征值名称
        /// </summary>
        /// <param name="evCode">特征值Code</param>
        /// <returns></returns>
        public string ConvertEVNameByCode(string evCode)
        {
            string name = evCode;
            switch (evCode)
            {
                case string code when (code.Contains("VRMS")):
                    string[] vrmsCode = evCode.Split("-");
                    name = $"{HandlerRMSName(vrmsCode[0])}速度有效值";
                    break;
                case "DC":
                    name = "直流分量";
                    break;
                case string code when (code.Contains("RMS")):
                    string[] rmsCode = evCode.Split("-");
                    name = $"{HandlerRMSName(rmsCode[0])}有效值";
                    break;
                case "PK":
                    name = "峰值";
                    break;
                case "PPK":
                    name = "峰峰值";
                    break;
                case "KTS":
                    name = "峭度指标";
                    break;
                case "CF":
                    name = "峰值指标";
                    break;
                case "SK":
                    name = "偏斜度";
                    break;
                case "IF":
                    name = "脉冲指标";
                    break;
                case "LF":
                    name = "裕度指标";
                    break;
                case "SF":
                    name = "波形指标";
                    break;
                case "gPKm":
                    name = "包络峰值";
                    break;
                case "gPKc":
                    name = "包络地毯值";
                    break;
                case "gPKmean":
                    name = "包络均值";
                    break;
                case "ECF":
                    name = "包络峰值因数";
                    break;
                case "EKTS":
                    name = "包络峭度";
                    break;
                case "EIF":
                    name = "包络脉冲指标";
                    break;
                case "ELF":
                    name = "包络裕度指标";
                    break;
                case "ESF":
                    name = "包络波形指标";
                    break;
                case "Pitch":
                    name = "基础Pi";
                    break;
                case "Roll":
                    name = "基础Ro";
                    break;
                case "Actrual":
                    name = "合成角度";
                    break;
                case "VA_Vertical":
                    name = "垂直加速度";
                    break;
                case "AA_Axisl":
                    name = "轴向加速度";
                    break;
                case "TRE":
                    name = "倾斜率";
                    break;
                case "CBF":
                    name = "拉索力";
                    break;
                case "MAX":
                    name = "最大值";
                    break;
                case "MIN":
                    name = "最小值";
                    break;
                case "AVG":
                    name = "平均值";
                    break;
                case "XI":
                    name = "X倾角";
                    break;
                case "YI":
                    name = "Y倾角";
                    break;
                default:
                    break;
            }
            return name;
        }

        /// <summary>
        /// 特殊处理：为保证全景监视页面的特征值名称一致，对该名称做处理
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private string HandlerRMSName(string code)
        {
            switch (code)
            {
                case "0_10":
                    return "0.1-10";
                case "10_1K":
                    return "10-1000";
                case "10_2K":
                    return "10-2000";
                case "10_5K":
                    return "10-5000";
                default:
                    return code;
            }
        }

        #endregion



        #region 特征值单位处理

        /// <summary>
        /// 特殊处理：特征值趋势单位
        /// </summary>
        /// <param name="first"></param>
        /// <returns></returns>
        public string GetEigenValueTrendUnitY(EigenValueData first)
        {
            string res = "";
            switch (first.EigenValueID)
            {
                case string code when (first.EigenValueID.Contains(EnumCompType.BL1.ToString())):
                case string code1 when (first.EigenValueID.Contains(EnumCompType.BL2.ToString())):
                case string code2 when (first.EigenValueID.Contains(EnumCompType.BL3.ToString())):
                    if (first.EigenValueID.Contains("NF"))
                    {
                        res = "Hz";
                    }
                    break;
                default:
                    res = first.Unit;
                    break;
            }
            return res;
        }

        public string GetUnitDescription(string unit)
        {
            switch (unit)
            {
                case "m/s^2":
                    return "加速度";
                case "mm/s":
                    return "速度";
                case "mm":
                    return "位移";
                case "V":
                    return "电压";
                case "kA":
                    return "电流";
                default:
                    return "";
            }
        }

        public string GetEvCardNameByMonitorType(EnumMonitorType type)
        {
            switch (type)
            {
                case EnumMonitorType.TVM_FLG_GAP:
                    return "间隙";
                default:
                    return "";
            }
        }


        public string ConvertSectionBySensorType(EnumSensorType sensorType)
        {
            switch (sensorType)
            {
                case EnumSensorType.TowSvm:
                    return ConstStr.TOW + ConstStr.TOP;
                case EnumSensorType.TowTilt:
                    return ConstStr.TOW + ConstStr.FDN;
                case EnumSensorType.FlangeLvdt:
                    return ConstStr.TOW + ConstStr.FL;
                case EnumSensorType.TowSvm4in1:
                    return ConstStr.TOW + ConstStr.TOP;
                default:
                    return "";
            }
        }
        #endregion
    }
}
