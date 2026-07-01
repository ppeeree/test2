using System.Collections.Generic;
using ACH.AppFramework.Analysis;
using ACH.AppFramework.Analysis.peakSearch;
using ACH.AppFramework.Analysis.peakSearch.entity;
using ACH.Diagnosis.BearingDiagnoseModel;

namespace ACH.Diagnosis.BearingDamageModel.GenBearingDamage
{
    public class GenBearingBsfDiagnose : GenBearingDiagnoseBase
    {
        /// <summary>
        ///  滚动体损伤诊断   
        /// </summary>
        /// <param name="waveData">数据</param>
        /// <param name="frequencyDomainData">频谱数据</param>
        /// <param name="envelopeSpectrumData">包络数据</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="spd">转速</param>
        /// <param name="bpfiFaultCoefficient">内圈故障系数</param>
        /// <param name="bpfoFaultCoefficient">外圈故障系数</param>
        /// <param name="bsfFaultCoefficient">滚动体故障系数</param>
        /// <param name="ftfFaultCoefficient">保持架故障系数</param>
        /// <param name="modeParaId">模型参数id</param>
        /// <param name="constantSpeed">恒定转速</param>
        /// <param name="dataDescription">数据说明</param>

        public override DiagnosisResult BearingDiagnosis(double[] waveData,SortedDictionary<double, double> frequencyDomainData, SortedDictionary<double, double> envelopeSpectrumData, double sampleRate, double spd, double bpfiFaultCoefficient, double bpfoFaultCoefficient, double bsfFaultCoefficient, double ftfFaultCoefficient, string modeParaId, double constantSpeed, DataDescription dataDescription)
        {
            BearingDiagnoseConfig bearingDiagnoseConfig = new BearingDiagnoseConfig()
            {
                ModeParaId = modeParaId,//模型参数id
                FrequencyDoublingNumber = 20,//倍频个数
                FrequencyDoublingErrorPoint = 8,// 倍频误差点数
                BandAvgThreshold = 4,// 频带均值阈值
                FrequencyDoublingRangePoint = 60,// 倍频范围点数
                FrequencyDoublingSiftNumber = 8,// 倍频筛选个数
                EnvSpecFreqSpecErrorPoint = 1,//包络谱基频与频谱基频误差点数
                SideFreDoublingFreRatioCoeff = 0.4,//边频幅值相对于倍频幅值设定的占比系数
                ContinuousFrequencyDoubling = 3//// 倍频筛选后，连续倍频个数
            };
            double intervalFrequency = sampleRate / waveData.Length; // 频率间隔
            // 1.模型配置参数赋值
            ParaAssignment(bearingDiagnoseConfig);

            // 2.获取频域数据
            if (frequencyDomainData == null ||  frequencyDomainData.Count == 0)
            {
                frequencyDomainData = FFTW.traditionFFT(CopyData(waveData), sampleRate, waveData.Length * 1.0);
            }

            // 3.获取峰值提取法得到的转频
            var result = PeakSearch.DictionaryToArray(frequencyDomainData);
            double frequencyConversion = PeakSearch.GetPeaks(result.dataX, result.dataY, (spd / 60) * 0.7, (constantSpeed / 60) * 1.1).X;

            // 4. 获取包络谱数据
            if (envelopeSpectrumData== null || envelopeSpectrumData.Count == 0)
            {
                envelopeSpectrumData = HilbertTransformEnvelope.hilbertTransformEnvelopeSpectrum(new List<double>(waveData), sampleRate, null);
            }

            // 5.轴承倍频计算
            bool acquisitionCorrection = true;//采集修正: 不是采集修正默认峰值修正
            List<List<double>> frequencyDoublingErrorRange = new List<List<double>>();// 倍频误差范围数据
            List<PkData> frequencyDoublingData = BearingFrequencyDoublingCalculate(spd, frequencyConversion, bsfFaultCoefficient, bpfoFaultCoefficient, bpfiFaultCoefficient, ftfFaultCoefficient,
                bearingDiagnoseConfig.FrequencyDoublingNumber, frequencyDomainData, envelopeSpectrumData, intervalFrequency, bearingDiagnoseConfig.FrequencyDoublingErrorPoint, ref acquisitionCorrection, bearingDiagnoseConfig.EnvSpecFreqSpecErrorPoint, frequencyDoublingErrorRange);

            // 6.筛选转频
            double screeningFrequencyConversion = spd / 60.0;
            if (!acquisitionCorrection)
            {
                screeningFrequencyConversion = frequencyConversion;
            }

            // 7.筛选倍频个数
            List<FrequencyDoublingData> siftFrequencyDoublingData = SiftFrequencyDoubling(bearingDiagnoseConfig.BandAvgThreshold, bearingDiagnoseConfig.FrequencyDoublingRangePoint, frequencyDomainData, intervalFrequency, frequencyDoublingData, screeningFrequencyConversion, bearingDiagnoseConfig.SideFreDoublingFreRatioCoeff, frequencyDoublingErrorRange, true);

            // 8.根据筛选倍频数据，进行轴承故障判断
            return BearingFaultJudgment(bearingDiagnoseConfig.FrequencyDoublingSiftNumber, bearingDiagnoseConfig.ContinuousFrequencyDoubling, siftFrequencyDoublingData, dataDescription, EnumBearingFaultType.ROLLINGELEMENT);
        }



    
    }
}
