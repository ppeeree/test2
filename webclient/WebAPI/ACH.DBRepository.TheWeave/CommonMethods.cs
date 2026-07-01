using ACH.DataEntity.Enum;
using ACH.DataEntity.TheWeave;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.DBRepository.TheWeave
{
    public class CommonMethods
    {
        private static IConfiguration _configuration;
        public CommonMethods(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 根据地址读取波形
        /// </summary>
        /// <param name="measEvent"></param>
        /// <returns></returns>
        public DataEntity.TheWeave.MeasWaveData GetMeasWaveData(DataEntity.TheWeave.MeasWaveData measEvent)
        {
            if (measEvent.WavePath != null)
            {
                string paths = measEvent.WavePath.Replace(@"\", "/");
                byte[]? bytes = DataConvertHelp.GetWaveData(paths);
                if (bytes != null && bytes.Length > 0)
                {
                    measEvent.WaveData = DataConvertHelp.ConvertByteToDoubles(bytes);
                    measEvent.SamplePoint = measEvent.WaveData.Length;
                }
            }
            return measEvent;
        }

        public PulseWaveData GetMeasWaveData(PulseWaveData waveData)
        {
            if (waveData.WavePath != null)
            {
                string paths = waveData.WavePath.Replace(@"\", "/");
                byte[]? bytes = DataConvertHelp.GetWaveData(paths);
                if (bytes != null && bytes.Length > 0)
                {
                    waveData.WaveData = DataConvertHelp.ConvertByteToDoubles(bytes);
                    waveData.SamplePoint = waveData.WaveData.Length;
                }
            }
            return waveData;
        }


        /// <summary>
        /// 切割自然月
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<DataQueryTimeDTO> SplitQueryTime(DateTime startTime, DateTime endTime)
        {
            var result = new List<DataQueryTimeDTO>();
            DateTime currentStart = startTime;
            while (currentStart < endTime)
            {
                // 当前段的开始日期
                DateTime segmentStart = currentStart;
                // 当前段的结束日期
                var segmentEnd = SegmentEndTime(segmentStart);
                if (segmentEnd >= endTime)
                {
                    result.Add(new DataQueryTimeDTO { StartTime = segmentStart, EndTime = endTime });
                    break;
                }
                else
                {
                    result.Add(new DataQueryTimeDTO { StartTime = segmentStart, EndTime = segmentEnd });
                    currentStart = segmentEnd.AddMinutes(1);
                }
            }
            return result;
        }

        private DateTime SegmentEndTime(DateTime segmentStartTime)
        {
            // 获取该月最后一天，时间设为 23:59:59
            return new DateTime(
                segmentStartTime.Year,
                segmentStartTime.Month,
                DateTime.DaysInMonth(segmentStartTime.Year, segmentStartTime.Month),
                23, 59, 59);
        }

        /// <summary>
        /// 根据测点ID获取特征值单位等信息
        /// </summary>
        /// <param name="MeasLocID"></param>
        /// <returns></returns>
        public List<EvCalcConfig> GetCVMEvRoule(string MeasLocID)
        {
            List<EvCalcConfig> evs = InitCVMEvRoule().ToList();
            // 获取额外计算特征值列表
            // 根据测点id 获取对应的特征值
            evs = evs.Where(ev => MeasLocID.Contains(ev.CompCode.ToString()) && MeasLocID.Contains(ev.LocCode)).ToList();

            if (MeasLocID.Contains("Ti"))//过滤温度
            {
                evs = evs.Where(ev => ev.EigenValueCode != "AvgT").ToList();
            }
            return evs;
        }
        /// <summary>
        /// 塔顶-X加速度
        /// </summary>
        public readonly static string TOPX = "Ti";
        /// <summary>
        /// 塔顶-Y加速度
        /// </summary>
        public readonly static string TOPY = "Na";
        /// <summary>
        /// 塔顶-Z加速度
        /// </summary>
        public readonly static string TOPZ = "Ri";
        /// <summary>
        /// 塔顶-X倾角
        /// </summary>
        public readonly static string TOPXPIT = "Pi";
        /// <summary>
        /// 塔顶-Y倾角
        /// </summary>
        public readonly static string TOPYPIT = "Ro";
        /// <summary>
        /// 塔基-X倾角
        /// </summary>
        public readonly static string FDNX = "TiA";
        /// <summary>
        /// 塔基-Y倾角
        /// </summary>
        public readonly static string FDNY = "NaA";
        /// <summary>
        /// 传动链特征值定义
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<EvCalcConfig> InitCVMEvRoule()
        {
            // 获取额外计算特征值列表
            List<EvCalcConfig> evs = new List<EvCalcConfig>
            {
                // 主轴承前轴承
                new EvCalcConfig(EnumCompType.MST, ConstStr.MSTMB1, "0_10-RMS","BRMS",0.0,10.0,"0.1-10有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.MST, ConstStr.MSTMB1, "10_1K-VMS","VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 主轴承后轴承
                new EvCalcConfig(EnumCompType.MST, ConstStr.MSTMB2, "0_10-RMS","BRMS",0.0,10.0,"0.1-10有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.MST, ConstStr.MSTMB2, "10_1K-VMS", "VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 齿轮箱输入端
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXINS, "0_10-RMS","BRMS", 0.0,10.0,"0.1-10有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXINS, "10_1K-VMS","VRMS", 10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 齿轮箱一级内齿圈
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX1PS, "0_10-RMS","BRMS",0.0,10.0,"0.1-10有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX1PS, "10_1K-VMS", "VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 齿轮箱二级内齿圈
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX2PS, "10_2K-RMS","BRMS",10.0,2000.0,"10-2000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX2PS, "10_1K-VMS", "VRMS", 10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // GBXISTRi0O
                // 齿轮箱三级内齿圈
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX3PS, "10_2K-RMS","BRMS",10.0,2000.0,"10-2000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBX3PS, "10_1K-VMS", "VRMS", 10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),

                // 低速轴
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXLSS, "10_2K-RMS", "BRMS",10.0,2000.0,"10-2000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXLSS, "10_1K-VMS", "VRMS", 10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),

                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXINS, "10_2K-RMS", "BRMS",10.0,2000.0,"10-2000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXLSS, "10_1K-VMS", "VRMS", 10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 齿轮箱输出轴
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXHSS, "10_2K-RMS", "BRMS",10.0,2000.0,"10-2000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GBX, ConstStr.GBXHSS, "10_1K-VMS", "VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                // 发电机
                new EvCalcConfig(EnumCompType.GEN, ConstStr.GENDEE, "10_5K-RMS", "BRMS", 10.0,5000.0,"10-5000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GEN, ConstStr.GENDEE, "10_1K-VMS","VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                new EvCalcConfig(EnumCompType.GEN, ConstStr.GENNDE, "10_5K-RMS", "BRMS", 10.0,5000.0,"10-5000有效值","m/s^2","加速度","A"),
                new EvCalcConfig(EnumCompType.GEN, ConstStr.GENNDE, "10_1K-VMS", "VRMS",10.0,1000.0,"10-1000速度有效值","mm/s","加速度","A"),
                //叶片
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "FaTDK","FaTDK",null,null,"冲击系数","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "FaTDR","FaTDR",null,null,"有效能量","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "1NF","NF",1.02,1.94,"一阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "2NF","NF",2.02,2.97,"二阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "3NF","NF",4.02,4.96,"三阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "4NF","NF",5.02,5.99,"四阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL1, ConstStr.MPT, "Avg","Avg",null,null,"平均值","","加速度","A"),


                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "FaTDK","FaTDK",null,null,"冲击系数","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "FaTDR","FaTDR",null,null,"有效能量","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "1NF","NF",1.02,1.94,"一阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "2NF","NF",2.02,2.97,"二阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "3NF","NF",4.02,4.96,"三阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "4NF","NF",5.02,5.99,"四阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL2, ConstStr.MPT, "Avg","Avg",null,null,"平均值","","加速度","A"),


                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "FaTDK","FaTDK",null,null,"冲击系数","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "FaTDR","FaTDR",null,null,"有效能量","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "1NF","NF",1.02,1.94,"一阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "2NF","NF",2.02,2.97,"二阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "3NF","NF",4.02,4.96,"三阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "4NF","NF",5.02,5.99,"四阶故障频率","","加速度","A"),
                 new EvCalcConfig(EnumCompType.BL3, ConstStr.MPT, "Avg","Avg",null,null,"平均值","","加速度","A"),

                 //塔筒塔顶
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPY, "AA_Axisl","AA_Axisl",null,null,"垂直加速度","°","加速度","A"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPZ, "VA_Vertical","VA_Vertical",null,null,"轴向加速度","°","加速度","A"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPX, "LA_Horizontal","LA_Horizontal",null,null,"水平加速度","°","加速度","A"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPX, "Actrual", "Actrual",null,null,"合成角度","°","角度","DEG"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPXPIT, "Pitch", "SA",null,null,"基础Pi","°","角度","DEG"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+TOPYPIT , "Roll","BA",null,null,"基础Ro","°","角度","DEG"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.TOP+ConstStr.TPE, "AvgT","AvgT",null,null,"温度","℃","加速度","A"),

                 //new EvCalcConfig(EnumCompType.TOW,  ConstStr.TOP+ConstStr.TOPY,"CBF","CBF",null,null,"拉索力","kN","加速度","A"),//测试


                 //塔筒塔基
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.FDN+FDNX, "Actrual","Actrual",null,null,"合成角度","°","角度","DEG"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.FDN+FDNX, "XI","SA", null,null,"X倾角","°","角度","DEG"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.FDN+ConstStr.TPE, "AvgT", "AvgT",null,null,"温度","℃","加速度","A"),
                 new EvCalcConfig(EnumCompType.TOW, ConstStr.FDN+FDNY, "YI","BA",null,null,"Y倾角","°","角度","DEG"),

                 //塔筒法兰
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri0ODS","Avg","Avg",null,null,"平均值","mm","位移","S"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri3ODS","Max","Max",null,null,"最大值","mm","位移","S"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri6ODS","Min","Min",null,null,"最小值","mm","位移","S"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri9ODS","Min","Min",null,null,"最小值","mm","位移","S"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri0OT","Avg","Avg",null,null,"平均值","℃","温度","T"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri3OT","Max","Max",null,null,"最大值","℃","温度","T"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri6OT","Min","Min",null,null,"最小值","℃","温度","T"),
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.FL+"Ri9OT","Min","Min",null,null,"最小值","℃","温度","T"),

                 //塔筒拉索
                  new EvCalcConfig(EnumCompType.TOW, ConstStr.PL1,"CBF","CBF",null,null,"拉索力","kN","加速度","A"),

            };
            return evs;
        }
    }
}
