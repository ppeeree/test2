using System;
using System.Collections.Generic;
using System.Text;

namespace ACH.Helper.Analysis
{
    /// <summary>
    /// 波形类型定义
    /// </summary>
    public static class CommonConstantData
    {
        // 假设这些常量是在其他地方定义的
        public const string WAVE_TYPE_TIME_DOMAIN = "TimeDomain";
        public const string WAVE_TYPE_ORDER = "Order";
        public const string WAVE_TYPE_ORDER_ENVELOPE = "OrderEnvelope";
        public const string WAVE_TYPE_ENVELOPE = "Envelope";
        public const string WAVE_TYPE_FREQ_DOMAIN = "FreqDomain";
        public const string WAVE_TYPE_ORDER_SPECTRUM = "OrderSpectrum";
        public const string WAVE_TYPE_CEPSTRUM = "Cepstrum";
        public const string WAVE_TYPE_ENVELOPE_SPECTRUM = "EnvelopeSpectrum";
        public const string WAVE_TYPE_ORDER_BEAR_ANALYSIS = "OrderBearAnalysis";
        public const string WAVE_TYPE_WATERFALL = "Waterfall";
        public const string SPD = "Spd";
        public const string WAVE_TYPE_ORDER_ENVELOPE_SPECTRUM = "OrderEnvelopeSpectrum";

    }
    public class FilterWaveData
    {
        // 使用readonly和static修饰符来创建一个只读且静态的列表
        private static readonly List<string> waveCategoryList = new List<string>
    {
        CommonConstantData.WAVE_TYPE_TIME_DOMAIN,
        CommonConstantData.WAVE_TYPE_ORDER,
        CommonConstantData.WAVE_TYPE_ORDER_ENVELOPE,
        CommonConstantData.WAVE_TYPE_ENVELOPE,
        CommonConstantData.WAVE_TYPE_FREQ_DOMAIN,
        CommonConstantData.WAVE_TYPE_ORDER_SPECTRUM,
        CommonConstantData.WAVE_TYPE_CEPSTRUM,
        CommonConstantData.WAVE_TYPE_ENVELOPE_SPECTRUM,
        CommonConstantData.WAVE_TYPE_WATERFALL,
        CommonConstantData.SPD,
        CommonConstantData.WAVE_TYPE_ORDER_ENVELOPE_SPECTRUM,
        CommonConstantData.WAVE_TYPE_ORDER_BEAR_ANALYSIS
    };

        // 波形稀释方法调用()
        public List<List<double>> FilterWaveDataV2(string waveCategory, List<List<double>> waveData, List<double> dataZoomXValue)
        {
            if (waveCategoryList.Contains(waveCategory))
            {
                if (dataZoomXValue == null || dataZoomXValue.Count != 2)
                {
                    List<double> newZoomValue = new List<double>();
                    newZoomValue.Add(0.0);
                    newZoomValue.Add(waveData[waveData.Count - 1][0]);
                    return HandleFilterPointV2(newZoomValue, waveData);
                }
                else
                {
                    return HandleFilterPointV2(dataZoomXValue, waveData);
                }
            }
            else
            {
                return waveData;
            }
        }

        private List<List<double>> HandleFilterPointV2(List<double> dataZoomValue, List<List<double>> datal)
        {
            List<List<double>> result = new List<List<double>>();

            // Step1 计算Zoom 区间对应的下标
            Dictionary<string, int> IndexList = HandlerStartIndex(dataZoomValue, datal);
            int startx = IndexList["startx"];
            int endx = IndexList["endx"];

            // Step2 计算Zoom 区间内的数据点数
            double barrelCount = 1500.0;
            double zoomDataCount = endx - startx;
            int barrelDataCount = (int)Math.Ceiling(zoomDataCount / barrelCount);

            if (barrelDataCount < 10)
            {
                // Step3 单个桶内数据少于10 - 直接返回，没有筛选必要
                return datal.GetRange(startx, endx - startx);
            }

            // Step4 单个桶内数据大于10 - 对数据进行稀释
            int j = 0;
            while (j < barrelCount)
            {
                int index = startx + j * barrelDataCount;
                if (index >= datal.Count || index > endx)
                {
                    break;
                }
                // Step5 计算第n桶内数据 - 起始点+桶数量*第n桶=桶区间数据
                result.AddRange(GetBarrelData(startx + j * barrelDataCount, barrelDataCount, datal));
                startx++;
                j++;
            }
            return result;
        }

        private List<List<double>> GetBarrelData(int startx, int barrelCount, List<List<double>> datal)
        {
            // Step6 获取区间内第一个值、最后一个值、最大值、最小值
            List<List<double>> result = new List<List<double>>();

            // 边界检查
            if (startx >= datal.Count)
            {
                return result;
            }

            // 添加第一个值
            result.Add(datal[startx]);

            // 边界检查
            int endIndex = Math.Min(startx + barrelCount, datal.Count);

            if (startx + 1 >= endIndex)
            {
                result.Add(datal[endIndex - 1]);
                return result;
            }

            // 初始化最大值和最小值
            List<double> maxValue = datal[startx + 1];
            List<double> minValue = datal[startx + 1];

            for (int x = startx + 1; x < endIndex - 1; x++)
            {
                List<double> current = datal[x];
                if (current[1] > maxValue[1])
                {
                    maxValue = current;
                }
                if (current[1] < minValue[1])
                {
                    minValue = current;
                }
            }

            // 确保最大值和最小值的顺序（这里假设是按照时间戳或其他顺序来决定）
            // 注意：在C#中，List<T>本身没有get方法，因为它是一个索引器类型
            // 如果maxValue和minValue中的第一个元素代表顺序，则按此顺序添加
            if (maxValue[0] > minValue[0])
            {
                result.Add(minValue);
                result.Add(maxValue);
            }
            else
            {
                result.Add(maxValue);
                result.Add(minValue);
            }

            // 添加最后一个值
            result.Add(datal[endIndex - 1]);

            return result;
        }


        // 根据放大区域获取数组的初始和结束下标
        private Dictionary<string, int> HandlerStartIndex(List<double> dataZoomValue, List<List<double>> datal)
        {
            int startx = 0;
            int endx = 0;
            Dictionary<string, int> map = new Dictionary<string, int>();

            if (dataZoomValue[0] > datal[datal.Count - 1][0])
            {
                // 放大区间大于波形范围 -- 返回空数组（注意：这里实际上不会返回空数组，而是设置了一个无效的索引范围）
                startx = datal.Count - 1;
                endx = datal.Count; // 注意：在C#中，这通常不是一个有效的索引，因为它超出了列表的范围。可能需要进一步处理这种情况。
            }
            else if (dataZoomValue[1] > datal[datal.Count - 1][0] && dataZoomValue[0] < datal[datal.Count - 1][0])
            {
                // 放大区间一部分在波形范围内 -- dataZoomValue最小值到波形最大值内数据
                for (int i = 0; i < datal.Count; i++)
                {
                    if (datal[i][0] < dataZoomValue[0])
                    {
                        startx = i + 1;
                    }
                }
                endx = datal.Count - 1;
            }
            else if (dataZoomValue[1] <= datal[datal.Count - 1][0])
            {
                // 放大区间全部在波形范围内 -- 返回范围内全部波形数据
                for (int i = 0; i < datal.Count; i++)
                {
                    if (datal[i][0] < dataZoomValue[0])
                    {
                        startx = i + 1;
                    }
                    if (datal[i][0] >= dataZoomValue[1])
                    {
                        endx = i;
                        break;
                    }
                }
            }

            map["startx"] = startx;
            map["endx"] = endx;
            return map;
        }
    }
}