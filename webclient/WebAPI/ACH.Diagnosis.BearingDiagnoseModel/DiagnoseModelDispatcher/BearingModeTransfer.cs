using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ACH.ACHLog.SeriLog;
using ACH.AppFramework.Analysis.ZXHCEVCalculate;
using ACH.Diagnosis.BearingDamageModel.GenBearingDamage;

namespace ACH.Diagnosis.BearingDiagnoseModel.DiagnoseModelDispatcher
{
    public class BearingModeTransfer
    {
        /// <summary>
        /// 轴承诊断模型缓存 为key为模型类型，value 为 队列为3个模型结果 
        /// </summary>
        public static ConcurrentDictionary<string, FixedSizeConcurrentQueue<DiagnosisResult>> diagnoseModecacheQueue = new ConcurrentDictionary<string, FixedSizeConcurrentQueue<DiagnosisResult>>();


        /// <summary>
        ///  轴承诊断模型调用
        /// </summary>
        /// <param name="operatingData">运行数据</param>
        /// <param name="bpfiFaultCoefficient">内圈故障系数</param>
        /// <param name="bpfoFaultCoefficient">外圈故障系数</param>
        /// <param name="bsfFaultCoefficient">滚动体故障系数</param>
        /// <param name="ftfFaultCoefficient">保持架故障系数</param>
        /// <param name="bearBpfiModeParaId ">轴承内圈模型参数id</param>
        /// <param name="bearBpfoModeParaId">轴承外圈模型参数id</param>
        /// <param name="bearBsfiModeParaId">轴承滚动体模型参数id</param>
        /// <param name="bearFtfModeParaId">轴承保持架模型参数id</param>
        /// <param name="constantSpeed">恒定转速</param>
        /// 
        /// <returns></returns>
        public static List<DiagnosisResult> BearingModeDiagnose(List<OperatingData> operatingData, double bpfiFaultCoefficient, double bpfoFaultCoefficient, double bsfFaultCoefficient, double ftfFaultCoefficient,
            string bearBpfiModeParaId, string bearBpfoModeParaId, string bearBsfiModeParaId, string bearFtfModeParaId, double constantSpeed)
        {
            List<DiagnosisResult> results = new List<DiagnosisResult>();
            // 1.数据校验
            if (operatingData == null || operatingData.Count == 0)
            {
                ALog.Debug("数据为null");
                return results;
            }
            // 2.过滤非发电机数据
            operatingData = operatingData.Where(vo => vo.MeasLocID.Contains("GEN")).ToList();
            operatingData = operatingData.OrderByDescending(vo => vo.AcqTime).ToList();// 倒序

            // 3.执行轴承诊断模型
            List<DiagnosisResult> diagnoseRetuen = ParallelExecutionBearingDiagnose(operatingData, bpfiFaultCoefficient, bpfoFaultCoefficient, bsfFaultCoefficient, ftfFaultCoefficient,
                                       bearBpfiModeParaId, bearBpfoModeParaId, bearBsfiModeParaId, bearFtfModeParaId, constantSpeed, 12);
            if (diagnoseRetuen == null || diagnoseRetuen.Count == 0)
            {
                ALog.Debug("诊断数据为null");
                return results;
            }

            // 4.根据缓存队列综合执行诊断结果 模型结果综合判断 ： 聚合次数最多的结果为当前波形的结果
            var faultTypeResults = diagnoseRetuen.ToList().GroupBy(r => r.FaultType).Select(g => new DiagnosisResult
            {
                FaultType = g.Key,
                TrueCount = g.Count(r => r.result),
                FalseCount = g.Count(r => !r.result),
                MajorityResult = g.Count(r => r.result) >= g.Count(r => !r.result)
            }).ToList();

            // 结果封装
            List<DiagnosisResult> resultsList = new List<DiagnosisResult>();
            foreach (var item in faultTypeResults)
            {
                resultsList.Add(new DiagnosisResult() { MeasLocID = operatingData[0].MeasLocID, AcqTime = operatingData[0].AcqTime, SampleRate = operatingData[0].SampleRate, FaultType = item.FaultType, result = item.MajorityResult });
            }
            return resultsList;
        }

        /// <summary>
        ///  并法执行轴承诊断
        /// </summary>
        /// <param name="operatingData">运行数据</param>
        /// <param name="bpfiFaultCoefficient">内圈故障系数</param>
        /// <param name="bpfoFaultCoefficient">外圈故障系数</param>
        /// <param name="bsfFaultCoefficient">滚动体故障系数</param>
        /// <param name="ftfFaultCoefficient">保持架故障系数</param>
        /// <param name="bearBpfiModeParaId ">轴承内圈模型参数id</param>
        /// <param name="bearBpfoModeParaId">轴承外圈模型参数id</param>
        /// <param name="bearBsfiModeParaId">轴承滚动体模型参数id</param>
        /// <param name="bearFtfModeParaId">轴承保持架模型参数id</param>
        /// <param name="constantSpeed">恒定转速</param>
        /// <param name="cacheSize">缓存个数</param>
        /// <returns></returns>
        public static List<DiagnosisResult> ParallelExecutionBearingDiagnose(List<OperatingData> operatingData, double bpfiFaultCoefficient, double bpfoFaultCoefficient, double bsfFaultCoefficient, double ftfFaultCoefficient, string bearBpfiModeParaId, string bearBpfoModeParaId, string bearBsfiModeParaId, string bearFtfModeParaId, double constantSpeed, int cacheSize)
        {
            List<DiagnosisResult> results = new List<DiagnosisResult>();
            if (operatingData != null || operatingData.Count != 0)
            {
                // 1.并行调用模型
                var diagnosisResults = new ConcurrentBag<DiagnosisResult>();
                var tasks = new List<Task>();// 任务队列
                for (int i = 0; i < operatingData.Count; i++)
                {
                    OperatingData data = operatingData[i];
                    if (data == null || data.waveData == null || data.waveData.Length == 0)
                    {
                        continue;
                    }
                    // 1.1 数据获取
                    double[] waveData = data.waveData;
                    SortedDictionary<double, double> frequencyDomainData = data.frequencyDomainData;//频谱
                    SortedDictionary<double, double> envelopeSpectrumData = data.envelopeSpectrumData; //包络
                    double spd = data.Spd;// 转速
                    string measLocID = data.MeasLocID;// 测点id
                    DateTime acTime = data.AcqTime;// 时间
                    double sampleRate = data.SampleRate;// 采样率

                    // 1.2 数据说明填充
                    DataDescription dataDescription = new DataDescription() { MeasLocID = measLocID, AcqTime = acTime, SampleRate = sampleRate };

                    // 1.3 将模型添加到任务队列
                    tasks.Add(Task.Run(() =>
                    {
                        var diagnose = new GenBearingBpfoDiagnose();
                        DiagnosisResult result = diagnose.BearingDiagnosis(waveData, frequencyDomainData, envelopeSpectrumData, sampleRate, spd, bpfiFaultCoefficient, bpfoFaultCoefficient, bsfFaultCoefficient, ftfFaultCoefficient, bearBpfoModeParaId, constantSpeed, dataDescription);
                        diagnosisResults.Add(result);
                    }));

                    tasks.Add(Task.Run(() =>
                    {
                        var diagnose = new GenBearingFtfDiagnose();
                        DiagnosisResult result = diagnose.BearingDiagnosis(waveData, frequencyDomainData, envelopeSpectrumData, sampleRate, spd, bpfiFaultCoefficient, bpfoFaultCoefficient, bsfFaultCoefficient, ftfFaultCoefficient, bearFtfModeParaId, constantSpeed, dataDescription);
                        diagnosisResults.Add(result);
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        var diagnose = new GenBearingBpfiDiagnose();
                        DiagnosisResult result = diagnose.BearingDiagnosis(waveData, frequencyDomainData, envelopeSpectrumData, sampleRate, spd, bpfiFaultCoefficient, bpfoFaultCoefficient, bsfFaultCoefficient, ftfFaultCoefficient, bearBpfiModeParaId, constantSpeed, dataDescription);
                        diagnosisResults.Add(result);
                    }));
                    tasks.Add(Task.Run(() =>
                    {
                        var diagnose = new GenBearingBsfDiagnose();
                        DiagnosisResult result = diagnose.BearingDiagnosis(waveData, frequencyDomainData, envelopeSpectrumData, sampleRate, spd, bpfiFaultCoefficient, bpfoFaultCoefficient, bsfFaultCoefficient, ftfFaultCoefficient, bearBsfiModeParaId, constantSpeed, dataDescription);
                        diagnosisResults.Add(result);
                    }));
                }
                Task.WaitAll(tasks.ToArray());//等待所有 12 个任务完成（3 模型 × 4 波形数据）

                // 2.将执行结果存储到缓存队列
                if (diagnosisResults != null && diagnosisResults.Count != 0)
                {
                    // 模型缓存数据存储
                    StorageCacheData(diagnosisResults);
                    // 不使用模型缓存
                    //results = diagnosisResults.ToList();
                }
            }
            // 查询缓存队列数据
            List<DiagnosisResult> cachResults = new List<DiagnosisResult>();
            cachResults.AddRange(GetModeCacheQueue(EnumBearingFaultType.INNERCIRCLE.ToString()));// 内圈数据
            cachResults.AddRange(GetModeCacheQueue(EnumBearingFaultType.OUTSIDECIRCLE.ToString())); //外圈数据
            cachResults.AddRange(GetModeCacheQueue(EnumBearingFaultType.ROLLINGELEMENT.ToString()));// 滚动体数据
            cachResults.AddRange(GetModeCacheQueue(EnumBearingFaultType.CAGE.ToString()));// 保持架数据

            return cachResults;
        }

        /// <summary>
        ///  存储缓存数据
        /// </summary>
        /// <param name="diagnosisResults"></param>
        /// <returns></returns>
        private static void StorageCacheData(ConcurrentBag<DiagnosisResult> diagnosisResults)
        {
            // 1.按照时间排序 正序
            List<DiagnosisResult> results = diagnosisResults.ToList().OrderByDescending(vo => vo.AcqTime).ToList();

            // 2. 按照类型分组
            Dictionary<string, List<DiagnosisResult>> resultsByType = results.GroupBy(d => d.FaultType.ToString()).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var faultType in resultsByType)
            {
                string type = faultType.Key;// 故障类型
                List<DiagnosisResult> data = faultType.Value; // 该类型数据
                if (data != null && data.Count != 0)
                {
                    foreach (var diagData in data)
                    {
                        DateTime acTime = diagData.AcqTime; //当前数据时间
                        List<DiagnosisResult> cacheData = GetModeCacheQueue(diagData.FaultType.ToString());// 查询当前类型
                        if (cacheData != null && cacheData.Count != 0)// 查看缓存数据是否存在
                        {
                            // 缓存中最新的时间与当前时间相差超过一天，清空该模型的缓存数据，
                            cacheData = cacheData.OrderByDescending(vo => vo.AcqTime).ToList();// 正序
                            DateTime newTime = cacheData[0].AcqTime;
                            // 计算时间差
                            TimeSpan timeDifference = acTime - newTime;
                            if (Math.Abs(timeDifference.TotalDays) > 1)
                            {
                                // 最早的时间与当前时间  大于一天，清空当前类型缓存数据后，加入当前数据结果
                                FixedSizeConcurrentQueue<DiagnosisResult> fixedQueue = new FixedSizeConcurrentQueue<DiagnosisResult>(3);
                                // 入队
                                fixedQueue.Enqueue(diagData);
                                diagnoseModecacheQueue[type.ToString()] = fixedQueue;
                            }
                            else
                            {
                                // 最早的时间与当前时间  小于一天，加入当前数据结果
                                FixedSizeConcurrentQueue<DiagnosisResult> fixedQueue = diagnoseModecacheQueue[type.ToString()];
                                // 入队
                                fixedQueue.Enqueue(diagData);
                                diagnoseModecacheQueue[type.ToString()] = fixedQueue;
                            }
                        }
                        else
                        { //  没有缓存数据
                            FixedSizeConcurrentQueue<DiagnosisResult> fixedQueue = new FixedSizeConcurrentQueue<DiagnosisResult>(3);
                            // 入队
                            fixedQueue.Enqueue(diagData);
                            diagnoseModecacheQueue[type.ToString()] = fixedQueue;

                        }
                    }
                }
            }

        }



        /// <summary>
        ///  缓存队列数据获取
        /// </summary>
        /// <param name="measLocID"></param>
        /// <param name="count"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static List<DiagnosisResult> GetModeCacheQueue(string key)
        {
            List<DiagnosisResult> diagReturn = new List<DiagnosisResult>();
            if (diagnoseModecacheQueue.ContainsKey(key))
            {
                // 根据key查找对应的缓存数据
                FixedSizeConcurrentQueue<DiagnosisResult> cbfCache = diagnoseModecacheQueue[key];
                diagReturn = cbfCache.GetAllElements();
                return diagReturn;
            }
            else
            {
                return diagReturn;
            }
        }

        /// <summary>
        ///  缓存队列存储
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void AddModeCacheQueue(string key, int count, DiagnosisResult value)
        {
            // 如果存在该key 直接根据key获取队列数据 入队
            if (diagnoseModecacheQueue.ContainsKey(key))
            {
                // 根据测点id查找对应的队列
                FixedSizeConcurrentQueue<DiagnosisResult> cbfDats = diagnoseModecacheQueue[key];
                // 入队
                cbfDats.Enqueue(value);
                // 重新更新该测点数据
                diagnoseModecacheQueue[key] = cbfDats;
            }
            else
            {
                // 如果不存在测点 创建一个8数据大小的队列然后入队
                FixedSizeConcurrentQueue<DiagnosisResult> fixedQueue = new FixedSizeConcurrentQueue<DiagnosisResult>(count);
                // 入队
                fixedQueue.Enqueue(value);
                diagnoseModecacheQueue[key] = fixedQueue;
            }
        }

    }


}
