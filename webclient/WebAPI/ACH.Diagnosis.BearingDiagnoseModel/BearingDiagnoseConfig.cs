using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ACH.ACHLog.SeriLog;
using Newtonsoft.Json;


namespace ACH.Diagnosis.BearingDamageModel
{
    public class BearingDiagnoseConfig
    {
        public string ModeId { get; set; }// 模型id
        public string ModeParaId { get; set; }// 模型参数id
        public int FrequencyDoublingNumber { get; set; }//倍频个数
        public int FrequencyDoublingErrorPoint { get; set; } // 倍频误差点数
        public double BandAvgThreshold { get; set; } // 频带均值阈值
        public int FrequencyDoublingRangePoint { get; set; } // 倍频范围点数
        public int FrequencyDoublingSiftNumber { get; set; } // 倍频筛选个数

        public int EnvSpecFreqSpecErrorPoint { get; set; } // 包络谱基频与频谱基频误差点数
        public double SideFreDoublingFreRatioCoeff { get; set; } // 边频幅值相对于倍频幅值设定的占比系数
        public int ContinuousFrequencyDoubling { get; set; }// 倍频筛选后，连续倍频个数

        /// <summary>
        ///  获取轴承诊断配置参数
        /// </summary>
        /// <param name="modeParaId">模型参数id</param>
        public static BearingDiagnoseConfig getBearingDiagnoseConfigPara(string modeParaId)
        {
            BearingDiagnoseConfig bearingDiagnoseConfigPara = null;
            // 1.根据模型id获取模型 参数
            List<BearingDiagnoseConfig> bearingDiagnoseConfig = new List<BearingDiagnoseConfig>(getBearingDiagnoseConfig());
            if (bearingDiagnoseConfig != null && bearingDiagnoseConfig.Count != 0)
            {
                bearingDiagnoseConfig = bearingDiagnoseConfig.Where(vo => vo.ModeParaId == modeParaId).ToList();
                if (bearingDiagnoseConfig.Count != 0)
                {
                    bearingDiagnoseConfigPara = bearingDiagnoseConfig[0];
                }
                else
                {
                    ALog.Information("根据" + modeParaId + "未读取到数据,使用默认配置参数");
                }
            }
            else
            {
                ALog.Information("BearingDamageConfig.json未读取到数据，使用默认配置参数");
            }

            return bearingDiagnoseConfigPara;
        }

        /// <summary>
        ///  读取BearingDiagnoseConfig.json 配置文件
        /// </summary>
        public static List<BearingDiagnoseConfig> bearingDiagnoseConfig = new List<BearingDiagnoseConfig>();
        public static IEnumerable<BearingDiagnoseConfig> getBearingDiagnoseConfig()
        {
            if (bearingDiagnoseConfig.Count == 0)
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BearingDiagnoseConfig.json");
                if (File.Exists(path))
                {
                    try
                    {
                        bearingDiagnoseConfig = JsonConvert.DeserializeObject<List<BearingDiagnoseConfig>>(File.ReadAllText(path));
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, "BearingDiagnoseConfig.json");
                    }
                }
            }
            return bearingDiagnoseConfig;
        }


        /// <summary>
        /// 根据参数id 获取倍频个数 和倍频误差点数，包络谱与频谱误差点数
        /// </summary>
        /// <param name="ModeParaId">模型参数id</param>
        /// <param name="frequencyDoublingErrorPoint">倍频误差点数</param>
        /// <param name="frequencyDoublingNumber">倍频个数</param>
        /// <param name="envSpecFreqSpecErrorPoint"> 包络谱与频谱误差点数</param> 
        public static void paraAssignment(string ModeParaId, ref int frequencyDoublingErrorPoint, ref int frequencyDoublingNumber, ref int envSpecFreqSpecErrorPoint)
        {
            if (ModeParaId != null && ModeParaId != "")
            {
                // 获取轴承模型参数
                BearingDiagnoseConfig bearingDiagnoseConfig = getBearingDiagnoseConfigPara(ModeParaId);
                if (bearingDiagnoseConfig != null)
                {
                    frequencyDoublingNumber = bearingDiagnoseConfig.FrequencyDoublingNumber;
                    frequencyDoublingErrorPoint = bearingDiagnoseConfig.FrequencyDoublingErrorPoint;
                    envSpecFreqSpecErrorPoint = bearingDiagnoseConfig.EnvSpecFreqSpecErrorPoint;
                }
            }
            else
            {
                ALog.Information(ModeParaId + "模型参数不存在使用默认数据");
            }
        }

    }
}
