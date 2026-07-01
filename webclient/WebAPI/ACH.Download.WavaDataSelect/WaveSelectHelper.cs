using ACH.ACHLog.SeriLog;
using ACH.MeasData.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.Download.WaveDataSelect
{
    public class WaveSelectHelper
    {
        /// <summary>
        /// 根据筛选出的测量事件获取波形地址
        /// </summary>
        /// <param name="data">波形数据</param>
        /// <param name="selectMeasevent">筛选的测量事件</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<string> GetFilePathByMeasevent(List<TWDataBase> data, List<MeasEventBase> selectMeasevent)
        {
            try
            {
                if (selectMeasevent == null || data == null) return new List<string>();

                // 使用Ticks比较，精确到秒级别，避免字符串转换
                const long TicksPerSecond = TimeSpan.TicksPerSecond;
                // 计算事件时间的秒级Ticks值
                var evtSecondTicks = new HashSet<long>(selectMeasevent.Select(e => e.AcqTime.Ticks / TicksPerSecond));
                // 查找时间匹配的波形文件（精确到秒）
                var res = data.Where(d => d.WavePath != null && evtSecondTicks.Contains(d.AcqTime.Ticks / TicksPerSecond)).Select(d => d.WavePath!).Distinct().ToList();

                // 如果上述找不到波形文件，则在1秒误差内重新查找波形
                if (res == null || res.Count == 0)
                {
                    const long Tolerance = TimeSpan.TicksPerSecond;
                    var evtTicks = selectMeasevent.Where(m => m.AcqTime != default).Select(m => m.AcqTime.ToUniversalTime().Ticks).Distinct().OrderBy(t => t).ToList();
                    return data.Where(d => d.WavePath != null && evtTicks.Any(t => Math.Abs(t - d.AcqTime.ToUniversalTime().Ticks) <= Tolerance)).Select(d => d.WavePath!).Distinct().ToList();
                }

                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetFilePathByMeasevent-根据波形索引获取波形地址异常，返回空列表");
                return new List<string>();
            }
        }

        /// <summary>
        /// MeasDataEvent时间抽取
        /// </summary>
        /// <param name="rawData">原始数据</param>
        /// <param name="num">筛选个数</param>
        /// <returns></returns>
        public static List<MeasEventBase> HandlerTimeSampling(List<MeasEventBase> rawData, int num)
        {
            if (rawData == null || rawData.Count == 0 || num <= 0)
            {
                ALog.Debug($"HandlerTimeSampling-波形索引列表为0或者下载个数为0，返回空列表");
                return new List<MeasEventBase>();
            }

            // 1. 按时间去重并排序
            var unique = rawData
                         .GroupBy(x => x.AcqTime.Ticks)
                         .Select(g => g.First())
                         .OrderByDescending(x => x.AcqTime)
                         .ToList();

            try
            {
                if (num == 1)
                {
                    ALog.Debug($"HandlerTimeSampling - num==1，返回最新的波形索引");
                    return new List<MeasEventBase> { unique[0] };
                }
                if (unique.Count <= num)
                {
                    ALog.Debug($"HandlerTimeSampling - unique.Count<=num，返回全部波形索引");
                    return unique;
                }

                // 2. 改进的均匀抽样算法：确保至少抽取num个，优先包含首尾元素
                var result = new List<MeasEventBase>();

                // 计算步长，使用浮点数确保精度
                double step = (double)(unique.Count - 1) / (num - 1);

                // 从0到num-1，计算每个抽样点的索引
                for (int i = 0; i < num; i++)
                {
                    int index = (int)Math.Round(i * step);
                    result.Add(unique[index]);
                }

                // 3. 去重（避免由于四舍五入导致的重复）
                result = result
                         .GroupBy(x => x.AcqTime.Ticks)
                         .Select(g => g.First())
                         .ToList();

                // 4. 确保至少返回num个，如果去重后不足，补充最新的
                if (result.Count < num)
                {
                    var additional = unique
                                     .Where(x => !result.Any(r => r.AcqTime.Ticks == x.AcqTime.Ticks))
                                     .Take(num - result.Count)
                                     .ToList();
                    result.AddRange(additional);
                }

                ALog.Debug($"HandlerTimeSampling-时间抽样执行完成，输入{rawData.Count}组，目标{num}组，输出{result.Count}组");
                return result;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"HandlerTimeSampling-时间抽样异常，取最新{num}组");
                // 异常时返回最新的num个，确保不会少抽
                return unique.Take(num).ToList();
            }
        }


        /// <summary>
        /// 法兰数据时间抽样下载
        /// </summary>
        /// <param name="gvmData">法兰数据</param>
        /// <param name="num">下载个数</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static List<string> HandlerGVMTimeSampling(List<TWDataBase> gvmData, int num)
        {
            if (gvmData == null || gvmData.Count == 0 || num <= 0)
            {
                ALog.Debug($"HandlerGVMTimeSampling-法兰数据为空或者下载个数为0，返回空数组");
                return new List<string>();
            }

            // 1. 按时间去重并排序
            var unique = gvmData
                         .GroupBy(x => x.AcqTime.Ticks)   // 用 Ticks 避免毫秒误差
                         .Select(g => g.First())
                         .OrderByDescending(x => x.AcqTime)
                         .ToList();
            try
            {
                List<string> paths = unique.Select(x => x.WavePath).OfType<string>().ToList();

                if (num == 1)
                {
                    ALog.Debug($"HandlerGVMTimeSampling - num==1，返回最新的法兰数据");
                    return new List<string> { paths[0] };
                }
                if (unique.Count <= num)
                {
                    ALog.Debug($"HandlerGVMTimeSampling - unique.Count<=num，波形数据全部返回");
                    return paths;
                }

                // 2. 改进的均匀抽样算法：确保至少抽取num个，优先包含首尾元素               
                var result = new List<string>();

                // 计算步长，使用浮点数确保精度
                double step = (double)(paths.Count - 1) / (num - 1);

                // 从0到num-1，计算每个抽样点的索引
                for (int i = 0; i < num; i++)
                {
                    int index = (int)Math.Round(i * step);
                    result.Add(paths[index]);
                }

                // 3. 去重（避免由于四舍五入导致的重复）
                result = result.Distinct().ToList();

                // 4. 确保至少返回num个，如果去重后不足，补充最新的
                if (result.Count < num)
                {
                    var additional = paths
                                     .Where(x => !result.Contains(x))
                                     .Take(num - result.Count)
                                     .ToList();
                    result.AddRange(additional);
                }

                ALog.Debug($"HandlerGVMTimeSampling-时间抽样执行完成，输入{gvmData.Count}组，目标{num}组，输出{result.Count}组");
                return result;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"HandlerGVMTimeSampling-时间抽样异常，取最新{num}组");
                var data = unique.Take(num).ToList();
                List<string> paths = data.Select(x => x.WavePath).OfType<string>().ToList();
                return paths;
            }
        }
    }
}
