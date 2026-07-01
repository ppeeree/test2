using ACH.ACHLog.SeriLog;
using ACH.AppFramework.Analysis;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Analysis;
using ACH.Helper.Component;
using ACH.MeasData.Entity;
using SqlSugar;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    public class WaveListMethods
    {
        IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        DBSelect dbSelect;
        ComponentHelper componentHelper = new ComponentHelper();
        public WaveListMethods(IConfiguration _configuration)
        {
            dbSelect = new DBSelect(_configuration);
        }


        /// <summary>
        /// 4、GetWaveformData接口实现
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="measlocId">测点ID</param>
        /// <param name="waveCategory">波形类型</param>
        /// <param name="sampleRate">采样频率</param>
        /// <param name="takeDataVOS"></param>
        /// <param name="dataZoomXValue">缩放范围</param>
        /// <param name="takeFilterWaveData">是否稀释</param>
        /// <param name="filteringWaveRange">滤波上下限</param>
        /// <returns></returns>
        internal List<WaveDataDTO> ConvertToWave(string deviceID, DateTime acqTime, string measlocId, string waveCategory, double? sampleRate, bool takeDataVOS, string? dataZoomXValue, bool takeFilterWaveData)
        {
            try
            {
                List<WaveDataDTO> waveDatas = new List<WaveDataDTO>(); //返回对象数据

                // 根据测点ID获取设备树
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(measlocId) ?? new();

                // 获取通用波形对象
                List<TWDataBase> data = dbSelect.GetMeasWaveData(measObj, acqTime, sampleRate);

                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        // 根据波形类型转换波形对象
                        WaveDataDTO obj = ConvertToWaveDataModel(item, measObj, waveCategory);

                        if (takeFilterWaveData)
                        {
                            // 根据条件稀释波形
                            WaveformDilution(obj, waveCategory, dataZoomXValue);
                        }
                        waveDatas.Add(obj);
                    }
                }

                return waveDatas;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"ConvertToWave-波形获取报错");
                return new List<WaveDataDTO>();
            }
        }


        /// <summary>
        /// 将TWDataBase转化为WaveDataModel对象
        /// </summary>
        /// <param name="wave"></param>
        /// <param name="measObj"></param>
        /// <param name="waveCategory"></param>
        /// <returns></returns>
        public WaveDataDTO ConvertToWaveDataModel(TWDataBase wave, DevMeasLocation measObj, string waveCategory)
        {
            //根据测点返回对应测点定义表中的数据
            WaveDataDTO res = new WaveDataDTO();

            if (wave.WaveData == null)
            {
                ALog.Debug($"ConvertToWaveDataModel-该{wave.MeasLocID}~{wave.AcqTime}WaveData为空");
                return res;
            }

            // 将float[]转化为double[]
            double[] waveDataDouble = wave.WaveData.Select(f => (double)f).ToArray();

            List<List<double>> waveDatas = ConvertWaveByCategory(measObj, res, waveDataDouble, wave, waveCategory);
            if (waveDatas != null && waveDatas.Count != 0)
            {
                res.DataVOS = waveDatas;
            }
            else
            {
                res.DataVOS = new List<List<double>>();
            }

            res.BandWidth = "";
            res.CompName = measObj.ComponentName;
            res.dataKv = new Dictionary<string, string>();
            res.MeaslocId = measObj.MeasLoctionID;
            res.MeaslocName = componentHelper.HandlerMeaslocName(measObj.MeasLoctionID, measObj.MeasLoctionName); // 叶片测点名称特殊处理
            res.Peak = 0;
            res.power = "0.0kW";
            res.SampleRate = wave.SampleRate;
            res.Samplingtimelength = ((1.0 / wave.SampleRate) * wave.WaveData.Length) + "";
            res.Temp = "";
            res.Time = wave.AcqTime.ToString("yyyy-MM-dd HH:mm:ss");
            res.WaveCategory = waveCategory;
            res.WaveLength = wave.WaveData.Length;
            res.WindParkName = measObj.StationName;
            res.WindturbineName = measObj.DeviceName;
            res.WindturbineId = wave.DeviceID;
            res.WindSpd = "";
            res.Rotatespeed = 0;

            double.TryParse(wave.EV.ToString("G5"), out double RotRmsValue);
            res.Rms = RotRmsValue;

            return res;
        }



        /// <summary>
        /// 根据波形类型转化原始波形数据
        /// </summary>
        /// <param name="measObj">设备树对象</param>
        /// <param name="res">接口返回对象</param>
        /// <param name="waveData">原始波形数据</param>
        /// <param name="samplePoint">采样点数</param>
        /// <param name="sampleRate">采样频率</param>
        /// <param name="waveCategory">波形类型</param>
        /// <returns></returns>
        private List<List<double>> ConvertWaveByCategory(DevMeasLocation measObj, WaveDataDTO res, double[] waveData, TWDataBase wave, string waveCategory)
        {
            List<List<double>> datas = new List<List<double>>();

            string unity = DataEntity.Common.EnumHelper.GetUnit(measObj.SignalType);
            DateTime AcqTime = wave.AcqTime;

            switch (waveCategory)
            {
                // 时域 + 
                case CommonConstantData.WAVE_TYPE_TIME_DOMAIN:
                case CommonConstantData.WAVE_TYPE_ORDER_BEAR_ANALYSIS:
                    if (waveData != null && waveData.Length != 0)
                    {
                        double intevel = 1 / (double)wave.SampleRate;
                        double xi = 0;
                        for (int i = 0; i < waveData.Length; i++)
                        {
                            List<double> data = new List<double>();
                            xi += intevel;
                            double yi = waveData[i];
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }
                    }
                    res.UnitX = "s";
                    res.UnitY = unity;
                    break;
                // 频域 + 瀑布图
                case CommonConstantData.WAVE_TYPE_FREQ_DOMAIN:
                case CommonConstantData.WAVE_TYPE_WATERFALL:
                    if (waveData != null && waveData.Length != 0)
                    {
                        SortedDictionary<double, double> DATAFFT = FFTW.traditionFFT(waveData, wave.SampleRate * 1.0, waveData.Length);// 传统FFT
                        foreach (var item in DATAFFT)
                        {
                            List<double> data = new List<double>();
                            double xi = item.Key;
                            double yi = item.Value;
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }
                    }
                    res.UnitX = "Hz";
                    res.UnitY = unity;
                    break;
                // 阶次波形
                case CommonConstantData.WAVE_TYPE_ORDER:
                    if (waveData != null && waveData.Length != 0)
                    {
                        List<double> spdDatasa = GetSpdDatas(measObj.StationID, measObj.DeviceID, AcqTime);// 获取转速数据
                        if (spdDatasa != null && spdDatasa.Count != 0)
                        {
                            SortedDictionary<double, double> orderWave = OrderFunctions.OrderWaveform(new List<double>(waveData), spdDatasa, wave.SampleRate * 1.0, null);
                            foreach (var item in orderWave)
                            {
                                List<double> data = new List<double>();
                                double xi = item.Key;
                                double yi = item.Value;
                                double.TryParse(xi.ToString("G9"), out double xi1);
                                double.TryParse(yi.ToString("G5"), out double yi1);
                                data.Add(xi1);
                                data.Add(yi1);
                                datas.Add(data);
                            }
                        }

                    }
                    res.UnitX = "n";
                    res.UnitY = unity;
                    break;
                // 阶次谱
                case CommonConstantData.WAVE_TYPE_ORDER_SPECTRUM:
                    if (waveData != null && waveData.Length != 0)
                    {
                        List<double> spdDatasb = GetSpdDatas(measObj.StationID, measObj.DeviceID, AcqTime);// 获取转速数据
                        if (spdDatasb != null && spdDatasb.Count != 0)
                        {
                            SortedDictionary<double, double> orderSpectrum = OrderFunctions.OrderSpectrum(new List<double>(waveData), spdDatasb, wave.SampleRate * 1.0);
                            foreach (var item in orderSpectrum)
                            {
                                List<double> data = new List<double>();
                                double xi = item.Key;
                                double yi = item.Value;
                                double.TryParse(xi.ToString("G9"), out double xi1);
                                double.TryParse(yi.ToString("G5"), out double yi1);
                                data.Add(xi1);
                                data.Add(yi1);
                                datas.Add(data);
                            }
                        }

                    }
                    res.UnitX = "Order";
                    res.UnitY = unity;
                    break;
                // 包络
                case CommonConstantData.WAVE_TYPE_ENVELOPE:
                    if (waveData != null && waveData.Length != 0)
                    {
                        SortedDictionary<double, double> hdata = HilbertTransformEnvelope.HilbertTransformEnvelopeWaveMethod(new List<double>(waveData), wave.SampleRate * 1.0, false, null);
                        foreach (var item in hdata)
                        {
                            List<double> data = new List<double>();
                            double xi = item.Key;
                            double yi = item.Value;
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }
                    }
                    res.UnitX = "s";
                    res.UnitY = unity;
                    break;
                // 包络谱
                case CommonConstantData.WAVE_TYPE_ENVELOPE_SPECTRUM:
                    if (waveData != null && waveData.Length != 0)
                    {
                        SortedDictionary<double, double> hsdata = HilbertTransformEnvelope.hilbertTransformEnvelopeSpectrum(new List<double>(waveData), wave.SampleRate * 1.0, null);
                        foreach (var item in hsdata)
                        {
                            List<double> data = new List<double>();
                            double xi = item.Key;
                            double yi = item.Value;
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }
                    }
                    res.UnitX = "Hz";
                    res.UnitY = unity;
                    break;
                // 阶次包络
                case CommonConstantData.WAVE_TYPE_ORDER_ENVELOPE:
                    if (waveData != null && waveData.Length != 0)
                    {
                        List<double> spdDatasc = GetSpdDatas(measObj.StationID, measObj.DeviceID, AcqTime);// 获取转速数据
                        if (spdDatasc != null && spdDatasc.Count != 0)
                        {
                            SortedDictionary<double, double> zdata = Cepstrum.CepstrumCorrect(new List<double>(waveData), wave.SampleRate * 1.0);
                            SortedDictionary<double, double> orderEnvelopeWaveform = OrderFunctions.OrderEnvelopeWaveform(new List<double>(waveData), spdDatasc, wave.SampleRate * 1.0);
                            foreach (var item in orderEnvelopeWaveform)
                            {
                                List<double> data = new List<double>();
                                double xi = item.Key;
                                double yi = item.Value;
                                double.TryParse(xi.ToString("G9"), out double xi1);
                                double.TryParse(yi.ToString("G5"), out double yi1);
                                data.Add(xi1);
                                data.Add(yi1);
                                datas.Add(data);
                            }
                        }

                    }
                    res.UnitX = "s";
                    res.UnitY = unity;
                    break;
                // 阶次包络谱
                case CommonConstantData.WAVE_TYPE_ORDER_ENVELOPE_SPECTRUM:
                    if (waveData != null && waveData.Length != 0)
                    {
                        List<double> spdDatasd = GetSpdDatas(measObj.StationID, measObj.DeviceID, AcqTime);// 获取转速数据
                        if (spdDatasd != null && spdDatasd.Count != 0)
                        {
                            SortedDictionary<double, double> orderEnvelopeSpectrum = OrderFunctions.OrderEnvelopeSpectrum(new List<double>(waveData), spdDatasd, wave.SampleRate * 1.0);
                            foreach (var item in orderEnvelopeSpectrum)
                            {
                                List<double> data = new List<double>();
                                double xi = item.Key;
                                double yi = item.Value;
                                double.TryParse(xi.ToString("G9"), out double xi1);
                                double.TryParse(yi.ToString("G5"), out double yi1);
                                data.Add(xi1);
                                data.Add(yi1);
                                datas.Add(data);
                            }
                        }

                    }
                    res.UnitX = "Order";
                    res.UnitY = unity;
                    break;
                // 倒谱
                case CommonConstantData.WAVE_TYPE_CEPSTRUM:
                    if (waveData != null && waveData.Length != 0)
                    {
                        SortedDictionary<double, double> zdata = Cepstrum.CepstrumCorrect(new List<double>(waveData), wave.SampleRate * 1.0);
                        foreach (var item in zdata)
                        {
                            List<double> data = new List<double>();
                            double xi = item.Key;
                            double yi = item.Value;
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }
                    }
                    res.UnitX = "s";
                    res.UnitY = unity;
                    break;
                // 转速
                case CommonConstantData.SPD:
                    List<double> spdDatas = GetSpdDatas(measObj.StationID, measObj.DeviceID, AcqTime);// 获取转速数据
                    if (spdDatas != null && spdDatas.Count != 0)
                    {
                        double intevel = 1;
                        double xi = 1;
                        for (int i = 0; i < spdDatas.Count; i++)
                        {
                            List<double> data = new List<double>();
                            xi += intevel;
                            double yi = spdDatas[i];
                            double.TryParse(xi.ToString("G9"), out double xi1);
                            double.TryParse(yi.ToString("G5"), out double yi1);
                            data.Add(xi1);
                            data.Add(yi1);
                            datas.Add(data);
                        }

                    }
                    res.UnitX = "t";
                    res.UnitY = unity;
                    break;
                default:
                    break;
            }
            return datas;
        }


        /// <summary>
        /// 获取转速数据
        /// </summary>
        /// <param name="windParkID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <returns></returns>
        public List<double> GetSpdDatas(string windParkID, string deviceID, DateTime acqTime)
        {
            List<double> spdDatas = new List<double>();
            // 查询转速数据
            List<RotSpdWaveData> rotSpdWaveDatas = dbSelect.GetSPDatas(windParkID, deviceID, acqTime);
            if (rotSpdWaveDatas != null && rotSpdWaveDatas.Count != 0)
            {
                spdDatas = rotSpdWaveDatas[0].WaveData.Select(data => (double)data).ToList();
            }
            return spdDatas;
        }

        /// <summary>
        /// 波形稀释
        /// </summary>
        /// <param name="result"></param>
        /// <param name="waveCategory"></param>
        /// <param name="dataZoomXValue"></param>
        private void WaveformDilution(WaveDataDTO result, string waveCategory, string? dataZoomXValue)
        {
            if (result.DataVOS != null && result.DataVOS.Count != 0)
            {
                // 下标数据封装
                List<double> dataZoomXValues = new List<double>();
                if (dataZoomXValue != null)
                {
                    dataZoomXValues.Add(double.Parse(dataZoomXValue.Split(',')[0]));
                    dataZoomXValues.Add(double.Parse(dataZoomXValue.Split(',')[1]));
                }
                List<List<double>> dilutionDatas = new FilterWaveData().FilterWaveDataV2(waveCategory, result.DataVOS, dataZoomXValues);
                result.DataVOS = dilutionDatas;
            }
        }
    }
}
