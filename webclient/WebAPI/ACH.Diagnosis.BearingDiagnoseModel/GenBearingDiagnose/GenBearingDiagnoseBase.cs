using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ACH.ACHLog.SeriLog;
using ACH.AppFramework.Analysis.peakSearch;
using ACH.AppFramework.Analysis.peakSearch.entity;
using ACH.Diagnosis.BearingDiagnoseModel;
using OfficeOpenXml;


namespace ACH.Diagnosis.BearingDamageModel.GenBearingDamage
{
    public abstract class GenBearingDiagnoseBase
    {
        /// <summary>
        ///  轴承诊断
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

        public abstract DiagnosisResult BearingDiagnosis(double[] waveData,SortedDictionary<double, double> frequencyDomainData, SortedDictionary<double, double> envelopeSpectrumData, double sampleRate, double spd, double bpfiFaultCoefficient, double bpfoFaultCoefficient, double bsfFaultCoefficient, double ftfFaultCoefficient, string modeParaId, double constantSpeed, DataDescription dataDescription);


        /// <summary>
        ///  轴承倍频计算
        /// </summary>
        /// <param name="spd">转速</param>
        /// <param name="frequencyConversion">峰值提取转频</param>
        /// <param name="presentFaultCoefficient">当前故障系数</param>
        /// <param name="additionalFaultCoefficient1">其余故障系数1</param>
        /// <param name="additionalFaultCoefficient2">其余故障系数2</param>
        /// <param name="additionalFaultCoefficient3">其余故障系数3</param>
        /// <param name="frequencyDoublingNumber"> 倍频个数</param>
        /// <param name="frequencyDomainData"> 频域数据</param>
        /// <param name="envelopeSpectrumData"> 包络谱数据</param>
        /// <param name="intervalFrequency"> 频域间隔</param>
        /// <param name="frequencyDoublingErrorPoint"> 倍频误差点数</param>
        /// <param name="acquisitionCorrection"> 采集修正: 不是采集修正默认峰值修正</param>
        /// <param name="envSpecFreqSpecErrorPoint"> 包络谱与频谱误差点数</param>
        /// <param name="frequencyDoublingErrorRange"> 倍频误差范围数据</param>
        /// 
        public static List<PkData> BearingFrequencyDoublingCalculate(double spd, double frequencyConversion,
            double presentFaultCoefficient, double additionalFaultCoefficient1, double additionalFaultCoefficient2, double additionalFaultCoefficient3,
            int frequencyDoublingNumber, SortedDictionary<double, double> frequencyDomainData, SortedDictionary<double, double> envelopeSpectrumData, double intervalFrequency, int frequencyDoublingErrorPoint,
            ref bool acquisitionCorrection, int envSpecFreqSpecErrorPoint, List<List<double>> frequencyDoublingErrorRange)
        {
            List<PkData> peakExtractionVOs = new List<PkData>();
            // 倍频误差范围数据校验
            if (frequencyDoublingErrorRange == null)
            {
                frequencyDoublingErrorRange = new List<List<double>>();
            }
            if (frequencyDomainData.Count != 0)
            {
                // 1.查询到的转速数据，折算到转频
                spd = spd / 60.0;

                // 2. 获取频带误差范围间隔 
                double bandErrorRangeSpace = GetBandErrorRangeInterval(frequencyConversion, presentFaultCoefficient, additionalFaultCoefficient1, additionalFaultCoefficient2, additionalFaultCoefficient3, intervalFrequency, frequencyDoublingErrorPoint);

                // 3.根据包络谱进行 基频计算
                acquisitionCorrection = true; //采集修正: 不是采集修正默认峰值修正
                double fundamentalFrequency = FundamentalFrequencyCalculation(spd, frequencyConversion, presentFaultCoefficient, envelopeSpectrumData, peakExtractionVOs, bandErrorRangeSpace, ref acquisitionCorrection);

                // 4. 根据包络谱计算的基频，在频域数据上修正基频
                peakExtractionVOs = new List<PkData>();
                var result = PeakSearch.DictionaryToArray(frequencyDomainData);
                PkData baseFrequencyData = PeakSearch.GetPeaks(result.dataX, result.dataY, fundamentalFrequency - envSpecFreqSpecErrorPoint * intervalFrequency, fundamentalFrequency + envSpecFreqSpecErrorPoint * intervalFrequency);
                peakExtractionVOs.Add(baseFrequencyData);
                fundamentalFrequency = baseFrequencyData.X;

                // 5.存储基频误差范围数据
                frequencyDoublingErrorRange.Add(new List<double>() { fundamentalFrequency - envSpecFreqSpecErrorPoint * intervalFrequency, fundamentalFrequency + envSpecFreqSpecErrorPoint * intervalFrequency });

                // 6.多倍基频修正,每阶自动修正基频
                for (int i = 2; i <= frequencyDoublingNumber; i++)
                {
                    // 6.1获取故障频率参数范围
                    double value = fundamentalFrequency * i;
                    double bandStart = value - bandErrorRangeSpace;
                    if (bandStart <= 0)
                    {
                        bandStart = 0;
                    }
                    double bandEnd = value + bandErrorRangeSpace;


                    // 6.2判断倍频是否超过频域数据最大频率
                    List<double> frequencyDomainFrequencyData = frequencyDomainData.Keys.ToList();
                    if (bandEnd > frequencyDomainFrequencyData[frequencyDomainFrequencyData.Count - 1])
                    {
                        break;
                    }

                    // 6.3 存储倍频误差范围数据
                    frequencyDoublingErrorRange.Add(new List<double>() { bandStart, bandEnd });

                    // 6.4 获取误差范围内幅值最大的频率
                    var resultData = PeakSearch.DictionaryToArray(frequencyDomainData);
                    PkData frequencyData = PeakSearch.GetPeaks(resultData.dataX, resultData.dataY, bandStart, bandEnd);//峰值提取法
                    peakExtractionVOs.Add(frequencyData);

                    // 6.5 赋值修正基频
                    fundamentalFrequency = frequencyData.X / i;
                }
                return peakExtractionVOs;
            }
            else
            {
                ALog.Information("频域数据为null");
                return peakExtractionVOs;
            }
        }


        /// <summary>
        /// 获取频带误差范围间隔 (倍频误差点数自动修正)
        /// </summary>
        /// <param name="frequencyConversion">峰值提取转频</param>
        /// <param name="presentFaultCoefficient">当前故障系数</param>
        /// <param name="additionalFaultCoefficient1">其余故障系数1</param>
        /// <param name="additionalFaultCoefficient2">其余故障系数2</param>
        /// <param name="additionalFaultCoefficient3">其余故障系数3</param>
        /// <param name="intervalFrequency"> 频域间隔</param>
        /// <param name="frequencyDoublingErrorPoint"> 倍频误差点数</param>
        /// <returns></returns>
        public static double GetBandErrorRangeInterval(double frequencyConversion, double presentFaultCoefficient, double additionalFaultCoefficient1, double additionalFaultCoefficient2, double additionalFaultCoefficient3, double intervalFrequency, int frequencyDoublingErrorPoint)
        {
            // 1..校验倍频误差点数是否小于，当前故障频率与其他故障频率范围之间最小点数(向下取整)
            // 1.1计算当前故障系数与每个附加故障系数的绝对差值
            double diff1 = Math.Abs(presentFaultCoefficient - additionalFaultCoefficient1);
            double diff2 = Math.Abs(presentFaultCoefficient - additionalFaultCoefficient2);
            double diff3 = Math.Abs(presentFaultCoefficient - additionalFaultCoefficient3);
            // 1.2 找出最小的差值
            double minDiff = Math.Min(diff1, Math.Min(diff2, diff3));
            int failureMinDiff = (int)((minDiff * frequencyConversion) / intervalFrequency);
            if (frequencyDoublingErrorPoint > failureMinDiff)
            {
                frequencyDoublingErrorPoint = failureMinDiff;
            }
            // 2 自动修正，频带误差范围间隔
            double bandErrorRangeSpace = frequencyDoublingErrorPoint * intervalFrequency;
            return bandErrorRangeSpace;
        }

        /// <summary>
        ///  基频计算
        /// </summary>
        /// <param name="spd"></param>
        /// <param name="frequencyConversion">峰值提取转频</param>
        /// <param name="bearFaultCoefficient">当前故障系数</param>
        /// <param name="data">数据</param>
        /// <param name="peakExtractionVOs">频率存储对象</param>
        /// <param name="bandErrorRangeSpace">频带误差范围间隔</param>
        /// <param name="acquisitionCorrection">采集修正: 不是采集修正默认峰值修正</param>
        /// <returns></returns>
        public static double FundamentalFrequencyCalculation(double spd, double frequencyConversion, double bearFaultCoefficient, SortedDictionary<double, double> data,
            List<PkData> peakExtractionVOs, double bandErrorRangeSpace, ref bool acquisitionCorrection)
        {
            // 1.采集基频修正
            PkData acquisitionFundamentalFrequencyCorrection = FundamentalFrequencyCorrection(spd, bearFaultCoefficient, data, bandErrorRangeSpace);

            // 2.峰值提取法基频修正
            PkData peakExtractionFundamentalFrequencyCorrection = FundamentalFrequencyCorrection(frequencyConversion, bearFaultCoefficient, data, bandErrorRangeSpace);

            // 3.基频根据幅值大小选取
            double fundamentalFrequency = 0.0;
            if (acquisitionFundamentalFrequencyCorrection.Y <= peakExtractionFundamentalFrequencyCorrection.Y)
            {
                acquisitionCorrection = false;
                peakExtractionVOs.Add(peakExtractionFundamentalFrequencyCorrection);
                fundamentalFrequency = peakExtractionFundamentalFrequencyCorrection.X;
            }
            else
            {
                acquisitionCorrection = true;
                peakExtractionVOs.Add(acquisitionFundamentalFrequencyCorrection);
                fundamentalFrequency = acquisitionFundamentalFrequencyCorrection.X;
            }
            return fundamentalFrequency;
        }

        /// <summary>
        ///  基频修正
        /// </summary>
        /// <param name="spd">转速</param>
        /// <param name="bearFaultCoefficient">轴承故障系数</param>
        /// <param name="data"> 数据</param>
        /// <param name="bandErrorRangeSpace">频带误差范围间隔</param>
        /// <returns></returns>
        public static PkData FundamentalFrequencyCorrection(double spd, double bearFaultCoefficient, SortedDictionary<double, double> data, double bandErrorRangeSpace)
        {
            PkData acquisitionSpdCorrection = new PkData();
            for (int i = 1; i <= 1; i++)
            {
                // 1.获取故障频率误差范围
                double Value = bearFaultCoefficient * (spd) * i;
                double BandStart = Value - bandErrorRangeSpace;
                if (BandStart < 0)
                {
                    BandStart = 0;
                }
                double BandEnd = Value + bandErrorRangeSpace;
                // 2. 获取误差范围内幅值最大的频率
                var result = PeakSearch.DictionaryToArray(data);
                acquisitionSpdCorrection = PeakSearch.GetPeaks(result.dataX, result.dataY, BandStart, BandEnd);
            }

            return acquisitionSpdCorrection;
        }


        /// <summary>
        ///  模型参数赋值
        /// </summary>
        /// <param name="diagnoseConfig">轴承配置</param>
        public static void ParaAssignment(BearingDiagnoseConfig diagnoseConfig)
        {
            // 1.根据模型id获取模型 参数
            BearingDiagnoseConfig bearingDamageConfig = BearingDiagnoseConfig.getBearingDiagnoseConfigPara(diagnoseConfig.ModeParaId);
            if (bearingDamageConfig != null)
            {
                diagnoseConfig.FrequencyDoublingNumber = bearingDamageConfig.FrequencyDoublingNumber;
                diagnoseConfig.FrequencyDoublingErrorPoint = bearingDamageConfig.FrequencyDoublingErrorPoint;
                diagnoseConfig.BandAvgThreshold = bearingDamageConfig.BandAvgThreshold;
                diagnoseConfig.FrequencyDoublingRangePoint = bearingDamageConfig.FrequencyDoublingRangePoint;
                diagnoseConfig.FrequencyDoublingSiftNumber = bearingDamageConfig.FrequencyDoublingSiftNumber;
                diagnoseConfig.EnvSpecFreqSpecErrorPoint = bearingDamageConfig.EnvSpecFreqSpecErrorPoint;
                diagnoseConfig.SideFreDoublingFreRatioCoeff = bearingDamageConfig.SideFreDoublingFreRatioCoeff;
                diagnoseConfig.ContinuousFrequencyDoubling = bearingDamageConfig.ContinuousFrequencyDoubling;
            }
            else
            {
                ALog.Information("BearingDamageConfig.json未读取到数据，使用默认配置参数");
            }
        }

        /// <summary>
        ///  筛选倍频数据
        /// </summary>
        /// <param name="bandAvgThreshold">频带均值阈值</param>
        /// <param name="frequencyDoublingRangePoint">倍频范围点数</param>
        /// <param name="frequencyDomainData">频域数据</param>
        /// <param name="intervalFrequency">频率间隔</param>
        /// <param name="frequencyDoublingData">倍频数据</param>
        /// <param name="screeningFrequencyConversion">筛选转频</param>
        /// <param name="sideFreDoublingFreRatioCoeff">边频幅值相对于倍频幅值设定的占比系数</param>
        /// <param name="frequencyDoublingErrorRange"> 倍频误差范围数据</param>
        /// <param name="rangePointCorrection"> 倍频范围点数修正</param>
        /// <returns></returns>
        public static List<FrequencyDoublingData> SiftFrequencyDoubling(double bandAvgThreshold, int frequencyDoublingRangePoint, SortedDictionary<double, double> frequencyDomainData,
            double intervalFrequency, List<PkData> frequencyDoublingData, double screeningFrequencyConversion, double sideFreDoublingFreRatioCoeff, List<List<double>> frequencyDoublingErrorRange, bool rangePointCorrection)
        {
            List<FrequencyDoublingData> siftFrequencyDoublingData = new List<FrequencyDoublingData>();
            // 倍频误差范围数据个数和倍频个数是否一致
            if (frequencyDoublingErrorRange.Count != frequencyDoublingData.Count && frequencyDoublingData.Count != 0)
            {
                return siftFrequencyDoublingData;
            }

            int i = 1;
            foreach (PkData frequencyDoubling in frequencyDoublingData)
            {
                double frequency = frequencyDoubling.X;// 频率
                double amplitude = frequencyDoubling.Y; // 幅值

                // 1.倍频显著判断
                double avg = 0;
                bool hasSignificantFrequencyDoubling = DoublingFrequencySignificantjudgment(bandAvgThreshold, frequencyDoublingRangePoint, frequencyDomainData, intervalFrequency, frequency,
                    amplitude, ref avg, screeningFrequencyConversion, frequencyDoublingErrorRange[i - 1], rangePointCorrection);

                if (hasSignificantFrequencyDoubling)
                {
                    // 2.如果输入了边频系数，再进行边频是否明显
                    if (sideFreDoublingFreRatioCoeff != 0)
                    {
                        // 2.1 边频显著判断
                        bool hasSignificantSideFrequencies = SidefrequencySaliencyJudgment(frequencyDomainData, intervalFrequency, screeningFrequencyConversion, frequency, amplitude, sideFreDoublingFreRatioCoeff);
                        if (hasSignificantSideFrequencies)
                        {
                            siftFrequencyDoublingData.Add(new FrequencyDoublingData { SerialNumber = i, Frequency = frequency, Amplitude = amplitude, AmplitudeAvg = avg });

                        }
                    }
                    else
                    {
                        siftFrequencyDoublingData.Add(new FrequencyDoublingData { SerialNumber = i, Frequency = frequency, Amplitude = amplitude, AmplitudeAvg = avg });
                    }
                }
                i = i + 1;
            }
            return siftFrequencyDoublingData;
        }


        /// <summary>
        /// 倍频显著判断(默认不明显)
        /// </summary>
        /// <param name="bandAvgThreshold">频带均值阈值</param>
        /// <param name="frequencyDoublingRangePoint">倍频范围点数</param>
        /// <param name="frequencyDomainData">频域数据</param>
        /// <param name="intervalFrequency">频率间隔</param>
        /// <param name="frequency">频率</param>
        /// <param name="amplitude">幅值</param>
        /// <param name="avg">均值</param>
        /// <param name="screeningFrequencyConversion">筛选转频</param>
        /// <param name="frequencyErrorRange"> 频率误差范围数据</param>
        /// <param name="rangePointCorrection"> 倍频范围点数修正</param>
        /// <returns></returns>
        public static bool DoublingFrequencySignificantjudgment(double bandAvgThreshold, int frequencyDoublingRangePoint, SortedDictionary<double, double> frequencyDomainData, double intervalFrequency, double frequency, double amplitude, ref double avg,
            double screeningFrequencyConversion, List<double> frequencyErrorRange, bool rangePointCorrection)
        {
            // 1.判断倍频是否明显标志
            bool hasSignificantFrequencyDoubling = false;

            if (rangePointCorrection)
            {
                // 2.倍频范围点数修正
                // 2.1 边频间隔点数计算
                int sidebandIntervalPoint = (int)(screeningFrequencyConversion / intervalFrequency);

                // 2.2 如果边频点数小于倍频范围点数，修正倍频范围点数，边频间隔点数-6个点
                if (sidebandIntervalPoint < frequencyDoublingRangePoint)
                {
                    frequencyDoublingRangePoint = sidebandIntervalPoint - 6;
                }
            }


            // 3. 峰值提取法
            var result = PeakSearch.DictionaryToArray(frequencyDomainData);
            PkData sirf = PeakSearch.GetPeaks(result.dataX, result.dataY, frequencyErrorRange[0], frequencyErrorRange[1], bandAvgThreshold, 0,
                frequency - frequencyDoublingRangePoint * intervalFrequency, frequency + frequencyDoublingRangePoint * intervalFrequency, "AVG");

            // 4. 判断数据是否明显
            if (sirf.X != 0)
            {
                avg = sirf.EnergyValue;
                hasSignificantFrequencyDoubling = true;
            }

            return hasSignificantFrequencyDoubling;
        }


        /// <summary>
        /// 边频显著判断
        /// </summary>
        /// <param name="frequencyDomainData">频域数据</param>
        /// <param name="intervalFrequency">频率间隔</param>
        /// <param name="screeningFrequencyConversion">筛选转频</param>
        /// <param name="frequency">频率</param>
        /// <param name="amplitude">幅值</param>
        /// <param name="sideFreDoublingFreRatioCoeff">边频幅值相对于倍频幅值设定的占比系数</param>
        /// <returns></returns>
        public static bool SidefrequencySaliencyJudgment(SortedDictionary<double, double> frequencyDomainData, double intervalFrequency, double screeningFrequencyConversion, double frequency, double amplitude, double sideFreDoublingFreRatioCoeff)
        {
            // 1.判断边频显著是否明显标志
            bool hasSignificantSideFrequencies = false;

            // 2.根据倍频计算边频理论值
            List<PkData> sideFrequencyData = new List<PkData>();
            // 2.1 边频间隔点数计算
            int sidebandIntervalPoint = (int)(screeningFrequencyConversion / intervalFrequency);
            // 2.2 边频间隔频率计算
            double sidebandIntervalFrequency = sidebandIntervalPoint * intervalFrequency;

            // 2.3 存储间隔频率理论值
            for (int i = 1; i <= 2; i++)
            {
                sideFrequencyData.Add(new PkData { X = (frequency - i * sidebandIntervalFrequency), Y = frequencyDomainData[(frequency - i * sidebandIntervalFrequency)] });
                sideFrequencyData.Add(new PkData { X = (frequency + i * sidebandIntervalFrequency), Y = frequencyDomainData[(frequency + i * sidebandIntervalFrequency)] });
            }

            // 3.判断边频是否可以作为故障特征
            // 3.1 根据边频理论值幅值是否相对与倍频幅值明显，筛选满足条件的边频个数
            sideFrequencyData = sideFrequencyData.Where(vo => vo.Y > (sideFreDoublingFreRatioCoeff * amplitude)).ToList();

            // 3.2 根据筛选后的边频个数是否大于筛选前个数的一半，来判断是否有边频
            if (sideFrequencyData.Count >= 2)
            {
                hasSignificantSideFrequencies = true;
            }
            else
            {
                hasSignificantSideFrequencies = false;
            }
            return hasSignificantSideFrequencies;
        }


        /// <summary>
        /// 轴承故障判断
        /// </summary>
        /// <param name="frequencyDoublingSiftNumber">倍频筛选后，倍频筛选个数阈值</param>
        /// <param name="continuousFrequencyDoubling">倍频筛选后，连续倍频个数阈值</param>
        /// <param name="siftFrequencyDoublingData">筛选后的倍频数据</param>
        /// <param name="dataDescription">数据说明</param>
        /// <param name="faultType">故障类型</param>
        /// <returns></returns>
        public static DiagnosisResult BearingFaultJudgment(int frequencyDoublingSiftNumber, int continuousFrequencyDoubling, List<FrequencyDoublingData> siftFrequencyDoublingData, DataDescription dataDescription, EnumBearingFaultType faultType)
        {
            // 1.数据说明数据赋值
            DiagnosisResult diagnosisResult = new DiagnosisResult();
            diagnosisResult.MeasLocID = dataDescription.MeasLocID;
            diagnosisResult.SampleRate = dataDescription.SampleRate;
            diagnosisResult.AcqTime = dataDescription.AcqTime;
            diagnosisResult.FaultType = faultType;

            // 2. 先根据筛选个数判断
            if (siftFrequencyDoublingData.Count >= frequencyDoublingSiftNumber)
            {
                diagnosisResult.result = true;
            }
            else
            {
                // 8.2 筛选个数不满足条件的，然后根据 是否存在连续倍频数据来判断
                if (ContinuousFrequencyDoublingJudgment(siftFrequencyDoublingData, continuousFrequencyDoubling))
                {
                    diagnosisResult.result = true;//故障
                }
                else
                {
                    diagnosisResult.result = false;//无故障
                }
            }
            return diagnosisResult;
        }

        /// <summary>
        /// 连续倍频判断
        /// </summary>
        /// <param name="frequencyDoublingData">倍频数据</param>
        /// <param name="continuousNumber">连续个数</param>
        /// <returns></returns>
        public static bool ContinuousFrequencyDoublingJudgment(List<FrequencyDoublingData> frequencyDoublingData, int continuousNumber)
        {
            // 1. 数据校验
            if (frequencyDoublingData == null || frequencyDoublingData.Count < continuousNumber)
            {
                return false;
            }

            // 2. 检查连续个数是否小于2
            if (continuousNumber < 2)
            {
                return false;
            }

            for (int i = 0; i <= frequencyDoublingData.Count - continuousNumber; i++)
            {
                bool isConsecutive = true;

                // 3.检查从i开始的连续个数是否形成连续序列（升序）
                for (int j = 1; j < continuousNumber; j++)
                {
                    if (frequencyDoublingData[i + j].SerialNumber != frequencyDoublingData[i + j - 1].SerialNumber + 1)
                    {
                        isConsecutive = false;
                        break;
                    }
                }
                if (isConsecutive)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 数组数据复制
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[] CopyData(double[] data)
        {
            double[] dataList = new double[data.Length];
            Array.Copy(data, dataList, data.Length);// 将 data 复制给dataList
            return dataList;
        }

        public static void ExportToExcel(List<DamageModelJudgment> data, string filePath)
        {
            // 设置EPPlus的许可证上下文（社区版）
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // 创建Excel包
            using (var package = new ExcelPackage())
            {
                // 添加工作表
                var worksheet = package.Workbook.Worksheets.Add("DamageData");

                // 添加表头
                worksheet.Cells[1, 1].Value = "MID";
                worksheet.Cells[1, 2].Value = "Time";
                worksheet.Cells[1, 3].Value = "SIP";
                worksheet.Cells[1, 4].Value = "Count";

                // 设置表头样式
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // 填充数据
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    worksheet.Cells[i + 2, 1].Value = item.mid;
                    worksheet.Cells[i + 2, 2].Value = item.time;
                    worksheet.Cells[i + 2, 3].Value = item.sip;
                    worksheet.Cells[i + 2, 4].Value = item.count;
                }

                // 自动调整列宽
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // 保存Excel文件
                FileInfo excelFile = new FileInfo(filePath);
                package.SaveAs(excelFile);
            }

            Console.WriteLine($"数据已成功导出到: {filePath}");
        }

    }

    /// <summary>
    ///  倍频数据
    /// </summary>
    public class FrequencyDoublingData
    {
        public int SerialNumber { get; set; }
        public double Frequency { get; set; }

        public double Amplitude { get; set; }

        public double AmplitudeAvg { get; set; }

        public List<double> AmplitudeAvgData { get; set; }
    }

    /// <summary>
    ///  数据说明
    /// </summary>
    public class DataDescription
    {
        public string MeasLocID { get; set; } // 测量位置(测点/测点+方向)
        public double SampleRate { get; set; } // 采样频率
        public DateTime AcqTime { get; set; } // 采集时间
    }

    
    /// <summary>
    ///  运行数据
    /// </summary>
    public class OperatingData
    {
        public string MeasLocID { get; set; } // 测量位置(测点/测点+方向)
        public double  SampleRate { get; set; } //  采样频率
        public DateTime AcqTime { get; set; } // 采集时间
        public double Spd { get; set; }// 转速
        public double[] waveData { get; set; } // 数据
        public SortedDictionary<double, double> frequencyDomainData { get; set; } //频谱数据(可选)
        public SortedDictionary<double, double> envelopeSpectrumData { get; set; } //包络数据(可选)
    }
    /// <summary>
    /// 诊断结果
    /// </summary>
    public class DiagnosisResult
    {
        public string MeasLocID { get; set; } // 测量位置(测点/测点+方向)
        public double SampleRate { get; set; } // 采样频率
        public DateTime AcqTime { get; set; } // 采集时间
        public EnumBearingFaultType FaultType { get; set; } // 类型  内圈 innerCircle，外圈 outsideCircle，滚动体 rollingElement，保持架 Cage
        public bool result { get; set; } //  结果：是否有故障

        public bool MajorityResult { get; set; } // 该故障类型下 result 出现次数最多的值
        public int TrueCount { get; set; }      // result == true 的次数
        public int FalseCount { get; set; }     // result == false 的次数

        //public bool degree { get; set; } //  程度  轻微 中等 严重

    }


    public class DamageModelJudgment
    {
        public string mid { get; set; }
        public string time { get; set; }
        public double sip { get; set; }

        public int count { get; set; }
        public int status { get; set; }

        public List<FrequencyDoublingData> re { get; set; }

    }

}
