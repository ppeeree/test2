using ACH.ACHLog.SeriLog;
using ACH.AppFramework.Analysis;
using ACH.AppFramework.Analysis.peakSearch;
using ACH.AppFramework.Analysis.peakSearch.entity;
using ACH.AppFramework.Analysis.ZXHCEVCalculate;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.Common;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.Diagnosis.BearingDamageModel;
using ACH.Diagnosis.BearingDamageModel.GenBearingDamage;
using ACH.Diagnosis.BearingDiagnoseModel;
using ACH.Diagnosis.BearingDiagnoseModel.DiagnoseModelDispatcher;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using ACH.Helper.ApiReponse;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using System.Collections.Concurrent;
using System.Globalization;
using BearingLibrary = ACH.DevTree.Entity.BearingLibrary;
using ACH.DevTree.Entity;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    public class AnalysisMethods
    {
        private static readonly CreateReponse _createReponse = new CreateReponse();
        private static IConfiguration configuration;
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        static DBSelect dbSelect;
        static DeviceWaveDBFactory _waveReadFactory;
        public AnalysisMethods(IConfiguration _configuration)
        {
            dbSelect = new DBSelect(_configuration);
            _waveReadFactory = new DeviceWaveDBFactory(configuration);
        }

        /// <summary>
        /// 获取轴承故障频率
        /// </summary>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <returns></returns>
        internal static BearFaultFrequencyTypeVO GetBearFaultFrequency(string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, string BearFaultFrequencyType)
        {
            BearFaultFrequencyTypeVO BearFaultFrequency = new BearFaultFrequencyTypeVO();


            // 设备树：根据测点id   找到风场id 和部件类型数据
            DevMeasLocation devMeasLocation = DevTreeRepsitory.Instance.GetMeasLocationByMeaslocID(MeasLoctionID);
            string StationID = devMeasLocation.StationID;
            EnumMonitorType MeasDataType = devMeasLocation.MeasDataType;
            EnumSignalType SignalType = devMeasLocation.SignalType;

            // 1.查询发电机转速
            double? Spd = GetGenSpd(StationID, WindturbineID, MeasLoctionID, AcqTime);

            // 2.设备属性参数赋值
            double BPFI = 0;// 内圈故障频率
            double BPFO = 0;// 外圈故障频率
            double BSF = 0;// 滚动体故障频率
            double FTF = 0;// 保持架故障频率
            string BearBpfoModeParaId = null;// 外圈模型参数id
            string BearBpfiModeParaId = null;// 内圈模型参数id
            string BearFtfModeParaId = null;// 轴承保持架模型参数id
            string BearBsfiModeParaId = null;// 轴承滚动体模型参数id
            double ConstantSpeed = 0;// 恒定转速
            double SpdCompare = 0;// 变速比
            bool AttributeExist = DevicePropertieEevaluation(WindturbineID, MeasLoctionID, AcqTime, ref Spd, ref BPFI, ref BPFO, ref BSF, ref FTF, ref BearBpfoModeParaId, ref BearBpfiModeParaId, ref BearFtfModeParaId, ref BearBsfiModeParaId, ref ConstantSpeed, ref SpdCompare);

            if (!AttributeExist) // 判断属性是否存在
            {
                return new BearFaultFrequencyTypeVO();
            }

            // 3.获取频谱数据
            double[] Wavedata = null;
            SortedDictionary<double, double> FrequencyDomainData = GetSpectrumData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, ref Wavedata, SignalType);
            SortedDictionary<double, double> GenFrequencyDomainData = GetGenSpectrumData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, SignalType);
            var result = PeakSearch.DictionaryToArray(GenFrequencyDomainData);
            double FrequencyConversion = PeakSearch.GetPeaks(result.dataX, result.dataY, (Spd.Value / 60) * 0.7, (ConstantSpeed / 60) * 1.1).X;
            // 4.获取包络谱数据
            SortedDictionary<double, double> EnvelopeSpectrumData = HilbertTransformEnvelope.hilbertTransformEnvelopeSpectrum(new List<double>(Wavedata), SampleRate, null);

            // 5.赋值 故障类型
            BearFaultFrequency.BearFaultFrequencyType = BearFaultFrequencyType;

            // 6 根据轴承故障类型封装 倍频数据  
            List<BearFaultFrequencyDoubling> BearFaultFrequencyDoublings = FrequencyDoublingPackage(MeasLoctionID, BearFaultFrequencyType, Spd, FrequencyConversion, BPFI, BPFO, BSF, FTF, FrequencyDomainData, EnvelopeSpectrumData, BearBpfoModeParaId, BearBpfiModeParaId, BearFtfModeParaId, BearBsfiModeParaId);

            // 7.赋值对应故障频率-倍频数据
            BearFaultFrequency.BearFaultFrequencyDoubling = BearFaultFrequencyDoublings;

            return BearFaultFrequency;
        }



        /// <summary>
        /// 根据设备属性数据对需要的参数进行赋值 内圈边频
        /// </summary>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="Spd">转速</param>
        /// <param name="BPFI">内圈故障频率</param>
        /// <param name="BPFO">外圈故障频率</param>
        /// <param name="BSF">滚动体故障频率</param>
        /// <param name="FTF">保持架故障频率</param>
        /// <param name="BearBpfoModeParaId">轴承外圈模型参数id</param>
        /// <param name="BearBpfiModeParaId">轴承内圈模型参数id</param>
        /// <param name="BearFtfModeParaId">轴承保持架模型参数id</param>
        /// <param name="BearBsfiModeParaId">轴承滚动体模型参数id</param>
        /// <param name="ConstantSpeed">恒定转速</param>
        /// <param name="SpdCompare">变速比</param>
        private static bool DevicePropertieEevaluation(string WindturbineID, string MeasLoctionID, string AcqTime, ref double? Spd,
            ref double BPFI, ref double BPFO, ref double BSF, ref double FTF, ref string BearBpfoModeParaId, ref string BearBpfiModeParaId, ref string BearFtfModeParaId, ref string BearBsfiModeParaId, ref double ConstantSpeed, ref double SpdCompare)
        {
            List<DeviceParamEx> DeviceParamExes = DevTreeRepsitory.Instance.GetDeviceParamExByMeaslocID(MeasLoctionID);
            if (DeviceParamExes != null && DeviceParamExes.Count != 0)
            {
                foreach (var deviceParam in DeviceParamExes)
                {
                    // 恒定转速赋值
                    if (deviceParam.ParmKey == "constantSpeed")
                    {
                        ConstantSpeed = double.Parse(deviceParam.ParmValue);
                    }
                    // 2.1.1没查询到对应转速时,或者转速查到的为0, 获取恒定转速
                    if ((Spd == null || Spd <= 60) && deviceParam.ParmKey == "constantSpeed")
                    {
                        Spd = double.Parse(deviceParam.ParmValue);
                    }// 2.1.2 获取模型参数
                    else if (deviceParam.ParmKey == "bearingModel")
                    {
                        // 2.1.2.1根据轴承模型参数获取轴承故障频率
                        List<BearingLibrary> bearingLibraries = DevTreeRepsitory.Instance.GetBearingLibraryByModel(deviceParam.ParmValue);
                        if (bearingLibraries != null && bearingLibraries.Count != 0)
                        {
                            BPFI = bearingLibraries[0].BPFI;
                            BPFO = bearingLibraries[0].BPFO;
                            BSF = bearingLibraries[0].BSF;
                            FTF = bearingLibraries[0].FTF.Value;
                        }
                        else
                        {
                            ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "根据轴承模型参数没有找到对应轴承库的数据");
                        }
                    }
                    else if (deviceParam.ParmKey == "bearBpfoModeParaId" && MeasLoctionID.Contains(deviceParam.Id))
                    { // 2.1.3 轴承外圈模型参数id
                        BearBpfoModeParaId = deviceParam.ParmValue;
                    }
                    else if (deviceParam.ParmKey == "bearBpfiModeParaId" && MeasLoctionID.Contains(deviceParam.Id))
                    { // 2.1.4 轴承内圈模型参数id
                        BearBpfiModeParaId = deviceParam.ParmValue;
                    }
                    else if (deviceParam.ParmKey == "bearFtfiModeParaId" && MeasLoctionID.Contains(deviceParam.Id))
                    { // 2.1.5 轴承保持架模型参数id
                        BearFtfModeParaId = deviceParam.ParmValue;
                    }
                    else if (deviceParam.ParmKey == "bearBsfiModeParaId" && MeasLoctionID.Contains(deviceParam.Id))
                    { // 2.1.6 轴承滚动体模型参数id
                        BearBsfiModeParaId = deviceParam.ParmValue;
                    }
                    else if (deviceParam.ParmKey == "spdCompare")
                    { // 2.1.7 变速比
                        SpdCompare = double.Parse(deviceParam.ParmValue);
                    }

                }
                return true;
            }
            else
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + "未配置设备属性数据");
                return false;
            }
        }


        /* /// <summary>
         /// 根据设备属性数据对需要的参数进行赋值 重载 内圈边频
         /// </summary>
         /// <param name="WindturbineID">机组id</param>
         /// <param name="MeasLoctionID">测点id</param>
         /// <param name="AcqTime">时间</param>
         /// <param name="Spd">转速</param>
         /// <param name="Db">数据库连接字段</param>
         /// <param name="SpdCompare">变速比</param>
         /// <param name="FTF">保持架故障频率</param>
         /// <summary>
         private static bool DevicePropertieEevaluation(string WindturbineID, string MeasLoctionID, string AcqTime, SqlSugarClient Db, ref double? Spd, ref double SpdCompare, ref double FTF)
         {
             List<ACH.DataEntity.DevTreeData.DeviceParamEx> deviceParamExes = Db.Queryable<ACH.DataEntity.DevTreeData.DeviceParamEx>().Where(o => o.Id == WindturbineID || o.Id == MeasLoctionID).ToList();
             if (deviceParamExes != null && deviceParamExes.Count != 0)
             {
                 foreach (var deviceParam in deviceParamExes)
                 {    // 1.没查询到对应转速时,或者转速查到的为0, 获取恒定转速
                     if ((Spd == null || Spd <= 60) && deviceParam.ParmKey == "constantSpeed")
                     {
                         Spd = double.Parse(deviceParam.ParmValue);
                     }// 2.获取变速比
                     else if (deviceParam.ParmKey == "spdCompare")
                     {
                         SpdCompare = double.Parse(deviceParam.ParmValue);
                     }
                     else if (deviceParam.ParmKey == "bearingModel")
                     {
                         // 3.根据轴承模型参数获取轴承故障频率
                         List<BearingLibrary> bearingLibraries = Db.Queryable<BearingLibrary>().Where(o => o.Model == deviceParam.ParmValue).ToList();
                         if (bearingLibraries != null && bearingLibraries.Count != 0)
                         {
                             FTF = bearingLibraries[0].FTF.Value;
                         }
                         else
                         {
                             ALog.Information(WindturbineID + "_" + MeasLoctionID + "_" + AcqTime + "_" + "根据轴承模型参数没有找到对应轴承库的数据");
                         }
                     }
                 }
                 return true;
             }
             else
             {
                 ALog.Information(WindturbineID + "_" + MeasLoctionID + "_" + AcqTime + "_" + "设备属性数据未配置");
                 return false;
             }
         }*/




        /// <summary>
        /// 获取发电机频谱数据
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <param name="MeasDataType">采集类型</param>
        /// <param name="SignalType">数据类型</param>
        /// <returns></returns>
        private static SortedDictionary<double, double> GetGenSpectrumData(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, EnumMonitorType MeasDataType, EnumSignalType SignalType)
        {

            // 1.读取发电机波形数据
            double ReviseSampleRate = SampleRate;
            double[] Wavedata = ReadGenWaveformData(StationID, WindturbineID, MeasLoctionID, AcqTime, ref ReviseSampleRate, SampleRate, MeasDataType, SignalType);

            // 2.FFT
            SortedDictionary<double, double> FrequencyDomainData = new SortedDictionary<double, double>();
            if (Wavedata != null && Wavedata.Length != 0)
            {
                FrequencyDomainData = FFTW.traditionFFT(Wavedata, ReviseSampleRate, Wavedata.Length * 1.0);
            }
            else
            {
                ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + ReviseSampleRate + "频域数据为null");
            }

            return FrequencyDomainData;
        }


        /// <summary>
        ///  转频数据封装
        /// </summary>
        /// <param name="RotorFrequency">封装对象</param>
        /// <param name="Spd">转频</param>
        /// <returns></returns>
        private static RotorFrequencyVO FrequencyConversionEncapsulation(out RotorFrequencyVO RotorFrequency, double? Spd)
        {
            RotorFrequency = new RotorFrequencyVO();
            //1.封装转频类型
            RotorFrequency.RotorFrequencyType = "RF";

            //2.封装转频-倍频数据
            List<RotorFrequencyDoubling> RotorFrequencyDoubling = new List<RotorFrequencyDoubling>();
            for (int i = 1; i <= 3; i++)

            {   // 2.1获取转频-倍频
                double value = Spd.Value * i;
                RotorFrequencyDoubling.Add(new RotorFrequencyDoubling()
                {
                    MultipleLabel = RotorFrequency.RotorFrequencyType + "_" + i + "X",

                    //2.2保留9位有效数字
                    FrequencyValue = double.Parse(value.ToString("G9"))
                });
            }

            // 3. 转频倍频数据赋值
            RotorFrequency.RotorFrequencyDoubling = RotorFrequencyDoubling;

            // 4.返回转频倍频数据
            return RotorFrequency;
        }


        /// <summary>
        ///  根据故障频率封装倍频数据
        /// </summary>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="BearFaultFrequencyType">故障类型</param>
        /// <param name="Spd">转速</param>
        /// <param name="FrequencyConversion">峰值提取转频</param>
        /// <param name="BPFI">内圈故障</param>
        /// <param name="BPFO">外圈故障</param>
        /// <param name="BSF">滚动体故障</param>
        /// <param name="FTF">保持架故障</param>
        /// <param name="FrequencyDomainData">频谱数据</param>
        /// <param name="EnvelopeSpectrumData">包络谱谱数据</param>
        /// <param name="BearBpfoModeParaId">外圈模型参数id</param>
        /// <param name="BearBpfiModeParaId">内圈模型参数id</param>
        /// <param name="BearFtfModeParaId">保持架模型参数id</param>
        /// <param name="BearBsfiModeParaId">滚动体模型参数id</param>
        /// <returns></returns>
        private static List<BearFaultFrequencyDoubling> FrequencyDoublingPackage(string MeasLoctionID, string BearFaultFrequencyType, double? Spd, double FrequencyConversion,
            double BPFI, double BPFO, double BSF, double FTF, SortedDictionary<double, double> FrequencyDomainData, SortedDictionary<double, double> EnvelopeSpectrumData,
            string BearBpfoModeParaId, string BearBpfiModeParaId, string BearFtfModeParaId, string BearBsfiModeParaId)
        {
            List<BearFaultFrequencyDoubling> BearFaultFrequencyDoublings = new List<BearFaultFrequencyDoubling>();
            int frequencyDoublingErrorPoint = 8; // 默认 倍频误差点数
            int frequencyDoublingNumber = 0;// 倍频个数
            int envSpecFreqSpecErrorPoint = 1;//包络谱与频谱误差点数
            double intervalFrequency = FrequencyDomainData.Skip(1).First().Key; // 频率间隔
            switch (BearFaultFrequencyType)
            {
                case nameof(EnumBeaingFaultFrequency.BPFI):
                    frequencyDoublingNumber = 35;

                    // 模型配置参数赋值
                    BearingDiagnoseConfig.paraAssignment(BearBpfiModeParaId, ref frequencyDoublingErrorPoint, ref frequencyDoublingNumber, ref envSpecFreqSpecErrorPoint);

                    // 存储倍频数据
                    StorageFrequencyDoublingData(BearFaultFrequencyType, Spd, FrequencyConversion, BPFI, BPFO, BSF, FTF, FrequencyDomainData, EnvelopeSpectrumData, BearFaultFrequencyDoublings, intervalFrequency,
                        frequencyDoublingNumber, frequencyDoublingErrorPoint, envSpecFreqSpecErrorPoint);
                    break;
                case nameof(EnumBeaingFaultFrequency.BSF):
                    frequencyDoublingNumber = 20;

                    // 模型配置参数赋值
                    BearingDiagnoseConfig.paraAssignment(BearBsfiModeParaId, ref frequencyDoublingErrorPoint, ref frequencyDoublingNumber, ref envSpecFreqSpecErrorPoint);

                    // 存储倍频数据
                    StorageFrequencyDoublingData(BearFaultFrequencyType, Spd, FrequencyConversion, BSF, BPFI, BPFO, FTF, FrequencyDomainData, EnvelopeSpectrumData, BearFaultFrequencyDoublings, intervalFrequency,
                        frequencyDoublingNumber, frequencyDoublingErrorPoint, envSpecFreqSpecErrorPoint);
                    break;
                case nameof(EnumBeaingFaultFrequency.BPFO):
                    frequencyDoublingNumber = 50;

                    // 模型配置参数赋值
                    BearingDiagnoseConfig.paraAssignment(BearBpfoModeParaId, ref frequencyDoublingErrorPoint, ref frequencyDoublingNumber, ref envSpecFreqSpecErrorPoint);

                    // 存储倍频数据
                    StorageFrequencyDoublingData(BearFaultFrequencyType, Spd, FrequencyConversion, BPFO, BSF, BPFI, FTF, FrequencyDomainData, EnvelopeSpectrumData, BearFaultFrequencyDoublings, intervalFrequency,
                     frequencyDoublingNumber, frequencyDoublingErrorPoint, envSpecFreqSpecErrorPoint);
                    break;
                case nameof(EnumBeaingFaultFrequency.FTF):
                    frequencyDoublingNumber = 20;

                    // 模型配置参数赋值
                    BearingDiagnoseConfig.paraAssignment(BearFtfModeParaId, ref frequencyDoublingErrorPoint, ref frequencyDoublingNumber, ref envSpecFreqSpecErrorPoint);

                    // 存储倍频数据
                    StorageFrequencyDoublingData(BearFaultFrequencyType, Spd, FrequencyConversion, FTF, BSF, BPFI, BPFO, FrequencyDomainData, EnvelopeSpectrumData, BearFaultFrequencyDoublings, intervalFrequency,
                     frequencyDoublingNumber, frequencyDoublingErrorPoint, envSpecFreqSpecErrorPoint);
                    break;
            }
            return BearFaultFrequencyDoublings;
        }


        /// <summary>
        ///  存储倍频数据
        /// </summary>
        /// <param name="BearFaultFrequencyType">故障类型</param>
        /// <param name="Spd">转速</param>
        /// <param name="FrequencyConversion">峰值提取转频</param>
        /// <param name="PresentFaultCoefficient">当前故障系数</param>
        /// <param name="AdditionalFaultCoefficient1">其余故障系数1</param>
        /// <param name="AdditionalFaultCoefficient2">其余故障系数2</param>
        /// <param name="AdditionalFaultCoefficient3">其余故障系数3</param>
        /// <param name="FrequencyDomainData">频域数据</param>
        /// <param name="EnvelopeSpectrumData">包络谱谱数据</param>
        /// <param name="BearFaultFrequencyDoublings">倍频封装对象</param>
        /// <param name="IntervalFrequency">频域间隔</param>
        /// <param name="FrequencyDoublingNumber">倍频个数</param>
        /// <param name="FrequencyDoublingErrorPoint">倍频误差点数</param>
        /// <param name="EnvSpecFreqSpecErrorPoint"> 包络谱与频谱误差点数</param>
        /// 
        private static void StorageFrequencyDoublingData(string BearFaultFrequencyType, double? Spd, double FrequencyConversion, double PresentFaultCoefficient,
            double AdditionalFaultCoefficient1, double AdditionalFaultCoefficient2, double AdditionalFaultCoefficient3, SortedDictionary<double, double> FrequencyDomainData,
            SortedDictionary<double, double> EnvelopeSpectrumData, List<BearFaultFrequencyDoubling> BearFaultFrequencyDoublings, double IntervalFrequency,
            int FrequencyDoublingNumber, int FrequencyDoublingErrorPoint, int EnvSpecFreqSpecErrorPoint)
        {
            // 1.倍频计算
            bool acquisitionCorrection = true;//采集修正: 不是采集修正默认峰值修正
            List<PkData> frequencyDoublingData = GenBearingDiagnoseBase.BearingFrequencyDoublingCalculate(Spd.Value, FrequencyConversion, PresentFaultCoefficient, AdditionalFaultCoefficient1, AdditionalFaultCoefficient2, AdditionalFaultCoefficient3,
                FrequencyDoublingNumber, FrequencyDomainData, EnvelopeSpectrumData, IntervalFrequency, FrequencyDoublingErrorPoint, ref acquisitionCorrection, EnvSpecFreqSpecErrorPoint, null);
            // 2.倍频封装
            int count = 1;
            foreach (var item in frequencyDoublingData)
            {
                BearFaultFrequencyDoublings.Add(new BearFaultFrequencyDoubling
                {
                    MultipleLabel = BearFaultFrequencyType + "_" + count + "X",
                    FrequencyValue = double.Parse(item.X.ToString("G9")),
                });
                count = count + 1;
            }
        }

        /// <summary>
        /// 获取频谱数据
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <param name="MeasDataType">采集类型</param>
        /// <param name="Wavedata">波形数据</param>
        /// <param name="SignalType">数据类型</param>
        /// <returns></returns>
        public static SortedDictionary<double, double> GetSpectrumData(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, EnumMonitorType MeasDataType, ref double[] Wavedata, EnumSignalType SignalType)
        {
            // 1.查询波形数据   
            Wavedata = ReadWaveformData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, SignalType);

            // 2.FFT
            SortedDictionary<double, double> FrequencyDomainData = new SortedDictionary<double, double>();
            if (Wavedata != null && Wavedata.Length != 0)
            {
                FrequencyDomainData = FFTW.traditionFFT(CopyData(Wavedata), SampleRate, Wavedata.Length * 1.0);
            }

            return FrequencyDomainData;
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

        /// <summary>
        /// 获取发电机转速数据 ，根据机组加 转速特征值+时间查询
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <returns></returns>
        public static double? GetGenSpd(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime)
        {
            double? Spd = null;
            // 1.1 使用采集事件表查询发电机转速
            DateTime timeBegin = DateTime.ParseExact(AcqTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime timeEnd = timeBegin.AddMilliseconds(1);

            // 获取设备树对象
            DevMeasLocation meas = _devTreeRepository.GetMeaslocationByDeviceID(WindturbineID).First();
            List<MeasEventBase> MeasDataEvent = dbSelect.GetMeasEventByDeviceID(meas, timeBegin, timeEnd);


            // 1.3 采集事件数据是否存在
            if (MeasDataEvent != null && MeasDataEvent.Count != 0)
            {
                Spd = MeasDataEvent[0].RotSpd;
            }
            else
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "没有查询到对应转速，默认使用恒定转速");
            }
            return Spd;
        }

        /// <summary>
        ///  查询波形数据 
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <param name="MeasDataType">采集类型</param>
        /// <param name="SignalType">数据类型</param>
        /// <returns></returns>
        private static double[] ReadWaveformData(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, EnumMonitorType MeasDataType, EnumSignalType SignalType)
        {
            double[] WaveDatas = null;
            try
            {
                // 查询波形数据
                DateTime timeBegin = DateTime.ParseExact(AcqTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                // List<TWDataBase> datas = _waveReadFactory.GetDeviceWaveRead(MeasDataType).GetMeasWaveData(StationID, WindturbineID, MeasDataType, MeasLoctionID, SignalType, timeBegin, SampleRate);
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(MeasLoctionID) ?? new();
                List<TWDataBase> datas = dbSelect.GetMeasWaveData(measObj, timeBegin, SampleRate);

                if (datas != null && datas.Count != 0)
                {
                    // 3.读取波形文件树
                    if (datas[0].WaveData != null && datas[0].WaveData.Length != 0)
                    {
                        WaveDatas = datas[0].WaveData.Select(x => (double)x).ToArray();
                    }
                    else
                    {
                        ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + "波形 WaveData 数据为空");
                    }
                }
                else
                {
                    ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + "波形数据为null");
                }
                return WaveDatas;
            }

            catch (Exception ex)
            {
                ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + "查询波形数据获取失败" + ex);
                return WaveDatas;
            }
        }



        /// <summary>
        ///  查询发电机波形数据
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <param name="MeasDataType">采集类型</param>
        /// <param name="SignalType">数据类型</param>
        /// <returns></returns>
        private static double[] ReadGenWaveformData(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, ref double ReviseSampleRate, double SampleRate, EnumMonitorType MeasDataType, EnumSignalType SignalType)
        {
            double[] WaveDatas = null;
            try
            {
                // 1.时间类型转换
                DateTime timeBegin = DateTime.ParseExact(AcqTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                List<TWDataBase> datas = new List<TWDataBase>();
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(MeasLoctionID) ?? new();

                // 2.根据测点自动更新为发电机的测点
                string ReviseMeasLoctionID = MeasLoctionID;

                if (MeasLoctionID.Contains("MST"))
                {
                    // 2.1 获取测点定义表中发电机测点数据进行修正        
                    List<DevMeasLocation> SurveyStation = _devTreeRepository.GetMeaslocationByDeviceID(WindturbineID);
                    if (SurveyStation != null && SurveyStation.Count != 0)
                    {
                        // 2.2根据机组id 和发电机code,获取发电机测点id
                        List<string> GenMeasLoctionID = SurveyStation.Where(vo => (vo.MeasLoctionID.Contains("GENDEE") || vo.MeasLoctionID.Contains("GENNDE"))).Select(vo => vo.MeasLoctionID).ToList();
                        if (GenMeasLoctionID != null && GenMeasLoctionID.Count != 0)
                        {
                            GenMeasLoctionID = GenMeasLoctionID.OrderBy(m => m).ToList();
                            ReviseMeasLoctionID = GenMeasLoctionID[0];
                        }
                    }

                    // 2.3查询波形数据
                    //  datas = _waveReadFactory.GetDeviceWaveRead(MeasDataType).GetMeasWaveData(StationID, WindturbineID, MeasDataType, ReviseMeasLoctionID, SignalType, timeBegin, null);
                    datas = dbSelect.GetMeasWaveData(measObj, timeBegin, SampleRate);
                }
                else
                {   // 2.1查询波形数据
                    // datas = _waveReadFactory.GetDeviceWaveRead(MeasDataType).GetMeasWaveData(StationID, WindturbineID, MeasDataType, ReviseMeasLoctionID, SignalType, timeBegin, SampleRate);
                    datas = dbSelect.GetMeasWaveData(measObj, timeBegin, SampleRate);
                }

                if (datas != null && datas.Count != 0)
                {
                    // 3.读取波形数据
                    if (datas[0].WaveData != null && datas[0].WaveData.Length != 0)
                    {
                        WaveDatas = datas[0].WaveData.Select(x => (double)x).ToArray();
                        ReviseSampleRate = datas[0].SampleRate;
                    }
                    else
                    {
                        ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + ReviseSampleRate + "波形 WaveData 数据为空");
                    }
                }
                else
                {
                    ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + ReviseSampleRate + "波形数据为null");
                }
                return WaveDatas;
            }

            catch (Exception ex)
            {
                ALog.Information(MeasLoctionID + "_" + AcqTime + "_" + ReviseSampleRate + "查询波形数据获取失败" + ex);
                return WaveDatas;
            }
        }




        /// <summary>
        ///  获取边频
        /// </summary>
        /// <param name="WindturbineID"></param>
        /// <param name="MeasLoctionID"></param>
        /// <param name="AcqTime"></param>
        /// <param name="SampleRate"></param>
        /// <param name="FaultFrequency"></param>
        /// <param name="BearFaultFrequencyType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static IActionResult GetSideFrequency(string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, double FaultFrequency, string BearFaultFrequencyType)
        {

            // 设备树：根据测点id   找到风场id 和部件类型数据
            DevMeasLocation devMeasLocation = DevTreeRepsitory.Instance.GetMeasLocationByMeaslocID(MeasLoctionID);
            string StationID = devMeasLocation.StationID;
            EnumMonitorType MeasDataType = devMeasLocation.MeasDataType;
            EnumSignalType SignalType = devMeasLocation.SignalType;
            List<double> SideFrequency = new List<double>();

            // 1.查询发电机转速
            double? Spd = GetGenSpd(StationID, WindturbineID, MeasLoctionID, AcqTime);

            // 2.设备属性参数赋值
            double BPFI = 0;// 内圈故障频率
            double BPFO = 0;// 外圈故障频率
            double BSF = 0;// 滚动体故障频率
            double FTF = 0;// 保持架故障频率
            string BearBpfoModeParaId = null;// 外圈模型参数id
            string BearBpfiModeParaId = null;// 内圈模型参数id
            string BearFtfModeParaId = null;// 轴承保持架模型参数id
            string BearBsfiModeParaId = null;// 轴承滚动体模型参数id
            double ConstantSpeed = 0;// 恒定转速
            double SpdCompare = 0;// 变速比
            bool AttributeExist = DevicePropertieEevaluation(WindturbineID, MeasLoctionID, AcqTime, ref Spd, ref BPFI, ref BPFO, ref BSF, ref FTF, ref BearBpfoModeParaId, ref BearBpfiModeParaId, ref BearFtfModeParaId, ref BearBsfiModeParaId, ref ConstantSpeed, ref SpdCompare);
            if (!AttributeExist) // 判断属性是否存在
            {
                return _createReponse.CreateResponse("", 200, "缺少轴承故障频率信息，无法进行标注！");
            }

            Spd = Spd / 60;// 折算转频
                           // 3.获取频谱间隔
                           //double SpectrumInterval = getSpectrumInterval(StationID, WindturbineID,MeasLoctionID, AcqTime, SampleRate, MeasDataType, SignalType);
            double SpectrumInterval = 0;
            // 4.根据故障类型封装边频数据
            if (BearFaultFrequencyType == nameof(EnumBeaingFaultFrequency.BPFI))
            {
                // 4.1.1转频修正
                Spd = spdRevise(StationID, WindturbineID, MeasLoctionID, AcqTime, MeasDataType, SignalType, SampleRate, Spd, BPFI, BPFO, BSF, FTF, BearBpfiModeParaId, ConstantSpeed, nameof(EnumBeaingFaultFrequency.BPFI), ref SpectrumInterval);
                // 4.1.2 内圈边频数据
                return bPFISideFrequencyEncapsulation(MeasLoctionID, FaultFrequency, SideFrequency, ref Spd, SpdCompare, SpectrumInterval);

            }
            else if (BearFaultFrequencyType == nameof(EnumBeaingFaultFrequency.BSF))
            {
                // 4.2.1转频修正
                Spd = spdRevise(StationID, WindturbineID, MeasLoctionID, AcqTime, MeasDataType, SignalType, SampleRate, Spd, BSF, BPFI, BPFO, FTF, BearBsfiModeParaId, ConstantSpeed, nameof(EnumBeaingFaultFrequency.BSF), ref SpectrumInterval);
                // 4.2.2 滚动体边频数据
                return bSFSideFrequencyEncapsulation(FaultFrequency, SideFrequency, Spd, FTF, SpectrumInterval);

            }
            else
            {
                return _createReponse.CreateResponse("", 200, "故障类型输入不正确，请重新输入！");
            }

        }

        /// <summary>
        /// 转频修正 
        /// </summary>
        /// <param name="StationID">风场id</param>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="MeasDataType">采集类型</param>
        /// <param name="SignalType">数据类型</param>
        /// <param name="SampleRate"></param>
        /// <param name="Spd">转速</param>
        /// <param name="PresentFaultCoefficient">当前故障系数</param>
        /// <param name="AdditionalFaultCoefficient1">其余故障系数1</param>
        /// <param name="AdditionalFaultCoefficient2">其余故障系数2</param>
        /// <param name="AdditionalFaultCoefficient3">其余故障系数3</param>
        /// <param name="ModeParaId">模型参数id</param
        /// <param name="ConstantSpeed">恒定转速</param>
        /// <param name="BearFaultFrequencyType">轴承损伤类型</param>
        /// <param name="SpectrumInterval">频谱间隔</param>
        /// <returns></returns>
        private static double? spdRevise(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, EnumMonitorType MeasDataType, EnumSignalType SignalType, double SampleRate, double? Spd,
            double PresentFaultCoefficient, double AdditionalFaultCoefficient1, double AdditionalFaultCoefficient2, double AdditionalFaultCoefficient3, string ModeParaId,
            double ConstantSpeed, string BearFaultFrequencyType, ref double SpectrumInterval)
        {

            // 1.根据测点获取波形数据
            double[] Wavedata = null;
            SortedDictionary<double, double> FrequencyDomainData = GetSpectrumData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, ref Wavedata, SignalType);

            // 2.获取发电机频谱数据
            SortedDictionary<double, double> GenFrequencyDomainData = GetGenSpectrumData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, SignalType);

            // 3.峰值提取修正转频
            var result = PeakSearch.DictionaryToArray(GenFrequencyDomainData);
            double FrequencyConversion = PeakSearch.GetPeaks(result.dataX, result.dataY, (Spd.Value) * 0.7, (ConstantSpeed / 60) * 1.1).X;
            // 4.获取包络谱数据
            SortedDictionary<double, double> EnvelopeSpectrumData = HilbertTransformEnvelope.hilbertTransformEnvelopeSpectrum(new List<double>(Wavedata), SampleRate, null);

            // 5.频率间隔
            double IntervalFrequency = FrequencyDomainData.Skip(1).First().Key;
            SpectrumInterval = IntervalFrequency;
            List<PkData> PeakExtractionVOs = new List<PkData>();

            // 6.模型配置参数赋值
            int frequencyDoublingErrorPoint = 8; // 默认 倍频误差点数
            int frequencyDoublingNumber = 0;// 倍频个数
            int envSpecFreqSpecErrorPoint = 1;//包络谱与频谱误差点数
            BearingDiagnoseConfig.paraAssignment(ModeParaId, ref frequencyDoublingErrorPoint, ref frequencyDoublingNumber, ref envSpecFreqSpecErrorPoint);

            // 7. 获取频带误差范围间隔 
            double bandErrorRangeSpace = GenBearingDiagnoseBase.GetBandErrorRangeInterval(FrequencyConversion, PresentFaultCoefficient, AdditionalFaultCoefficient1, AdditionalFaultCoefficient2, AdditionalFaultCoefficient3, IntervalFrequency, frequencyDoublingErrorPoint);

            // 6.故障发电机修正基频
            bool AcquisitionCorrection = true;
            double FundamentalFrequency = GenBearingDiagnoseBase.FundamentalFrequencyCalculation(Spd.Value, FrequencyConversion, PresentFaultCoefficient, EnvelopeSpectrumData, PeakExtractionVOs, bandErrorRangeSpace, ref AcquisitionCorrection);

            // 7.获取修正转频
            if (!AcquisitionCorrection)
            {
                Spd = FrequencyConversion;
            }
            return Spd;
        }





        /// <summary>
        /// 缓存队列 该mid+采样率 为key,value 
        /// </summary>
        public static ConcurrentDictionary<string, FixedSizeConcurrentQueue<DiagnosisResult>> cacheQueue = new ConcurrentDictionary<string, FixedSizeConcurrentQueue<DiagnosisResult>>();
        /// <summary>
        /// 记录 key 的插入顺序（用于淘汰最旧的缓存）
        /// </summary>
        private static ConcurrentQueue<string> keyInsertionOrder = new ConcurrentQueue<string>();
        private static readonly object lockObj = new object(); // 用于线程安全操作

        /// <summary>
        /// 获取轴承故障诊断结果(自诊断)
        /// </summary>
        /// <param name="WindturbineID">机组id</param>
        /// <param name="MeasLoctionID">测点id</param>
        /// <param name="AcqTime">时间</param>
        /// <param name="SampleRate">采样率</param>
        /// <returns></returns>
        internal static Dictionary<string, bool> getBearingDiagnoseResult(string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate)
        {
            Dictionary<string, bool> bearingDiagnoseResult = new Dictionary<string, bool>();
            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BPFI)] = false;
            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BPFO)] = false;
            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BSF)] = false;
            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.FTF)] = false;

            // 设备树：根据测点id   找到风场id 和部件类型数据
            DevMeasLocation devMeasLocation = DevTreeRepsitory.Instance.GetMeasLocationByMeaslocID(MeasLoctionID);
            string StationID = devMeasLocation.StationID;
            EnumMonitorType MeasDataType = devMeasLocation.MeasDataType;
            EnumSignalType SignalType = devMeasLocation.SignalType;

            // 1. 判断是否为发电机测点，其他返回默认值
            if (!MeasLoctionID.Contains("GEN"))
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "该测点轴承损伤模型未开发，待续！");
                return bearingDiagnoseResult;
            }

            // 2.查询发电机转速
            double? Spd = GetGenSpd(StationID, WindturbineID, MeasLoctionID, AcqTime);
            if (Spd == null || Spd <= 60) // 判断转速是否存在
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "转速值太小，未诊断！");
                return bearingDiagnoseResult;
            }

            // 3.设备属性参数赋值
            double BPFI = 0;// 内圈故障
            double BPFO = 0;// 外圈故障
            double BSF = 0;// 滚动体故障
            double FTF = 0;// 保持架故障
            string BearBpfoModeParaId = null;// 轴承外圈模型参数id
            string BearBpfiModeParaId = null;// 轴承内圈模型参数id
            string BearFtfModeParaId = null;// 轴承保持架模型参数id
            string BearBsfiModeParaId = null;// 轴承滚动体模型参数id
            double ConstantSpeed = 0;// 恒定转速
            double SpdCompare = 0;// 变速比
            bool AttributeExist = DevicePropertieEevaluation(WindturbineID, MeasLoctionID, AcqTime, ref Spd, ref BPFI, ref BPFO, ref BSF, ref FTF, ref BearBpfoModeParaId, ref BearBpfiModeParaId, ref BearFtfModeParaId, ref BearBsfiModeParaId, ref ConstantSpeed, ref SpdCompare);
            if (!AttributeExist) // 判断属性是否存在
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "缺少轴承故障频率信息，未诊断！");
                return bearingDiagnoseResult;
            }

            try
            {

                // 4 据当前时间 key 查询缓存队列中是否有数据，没有数据,清空缓存队列，重新填充，有数据使用缓存数据
                List<DiagnosisResult> cacheData = GetCacheQueue(MeasLoctionID + "_" + SampleRate + "_" + DateTime.Parse(AcqTime));
                if (cacheData == null || cacheData.Count == 0)
                {
                    // 4.1并发获取运行数据
                    List<OperatingData> operatingData = GetRunData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, 3, MeasDataType, SignalType);
                    if (operatingData == null || operatingData.Count == 0)
                    {
                        ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "没有查询到波形数据");
                        return bearingDiagnoseResult;
                    }

                    // 4.2 对轴承进行自诊断
                    List<DiagnosisResult> diagnosisResults = BearingModeTransfer.BearingModeDiagnose(operatingData, BPFI, BPFO, BSF, FTF, BearBpfiModeParaId, BearBpfoModeParaId, BearBsfiModeParaId, BearFtfModeParaId, ConstantSpeed);

                    // 4.3.将执行结果存储到缓存队列
                    if (diagnosisResults != null && diagnosisResults.Count != 0)
                    {
                        List<DiagnosisResult> diagnoses = diagnosisResults.ToList().OrderByDescending(vo => vo.AcqTime).ToList();// 倒序
                        string key = diagnoses[0].MeasLocID + "_" + diagnoses[0].SampleRate + "_" + diagnoses[0].AcqTime;
                        foreach (var data in diagnoses)
                        {
                            //超过缓存队列设定个数，去掉最早缓存数据，
                            AddCacheQueue(key, 4, data, 10);
                        }
                    }

                    // 4.4 数据加入后再次查询缓存队列
                    cacheData = GetCacheQueue(MeasLoctionID + "_" + SampleRate + "_" + DateTime.Parse(AcqTime));
                }
                ResultEncapsulation(bearingDiagnoseResult, cacheData);
                return bearingDiagnoseResult;
            }
            catch (Exception ex)
            {
                ALog.Information(WindturbineID + "_" + MeasLoctionID + AcqTime + "_" + "轴承进行自诊断异常");
                return bearingDiagnoseResult;
            }
        }
        /// <summary>
        ///  结果封装
        /// </summary>
        /// <param name="bearingDiagnoseResult"></param>
        /// <param name="cacheData"></param>
        private static void ResultEncapsulation(Dictionary<string, bool> bearingDiagnoseResult, List<DiagnosisResult> cacheData)
        {
            if (cacheData != null && cacheData.Count != 0)
            {
                foreach (var results in cacheData)
                {
                    switch (results.FaultType)
                    {
                        case EnumBearingFaultType.OUTSIDECIRCLE:
                            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BPFO)] = results.result;
                            break;
                        case EnumBearingFaultType.INNERCIRCLE:
                            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BPFI)] = results.result;
                            break;
                        case EnumBearingFaultType.ROLLINGELEMENT:
                            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.BSF)] = results.result;
                            break;
                        case EnumBearingFaultType.CAGE:
                            bearingDiagnoseResult[nameof(EnumBeaingFaultFrequency.FTF)] = results.result;
                            break;
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
        public static List<DiagnosisResult> GetCacheQueue(string key)
        {
            List<DiagnosisResult> diagReturn = new List<DiagnosisResult>();
            if (cacheQueue.ContainsKey(key))
            {
                // 根据key查找对应的缓存数据
                FixedSizeConcurrentQueue<DiagnosisResult> cbfCache = cacheQueue[key];
                diagReturn = cbfCache.GetAllElements();
                return diagReturn;
            }
            else
            {
                return diagReturn;
            }
        }

        /// <summary>
        /// 添加缓存队列，如果超过 10 个则移除最早的
        /// </summary>
        /// <param name="key"></param>
        /// <param name="count">缓存队列个数</param>
        /// <param name="value"></param>
        /// <param name="cachelimitSize">缓存限制个数</param>
        public static void AddCacheQueue(string key, int count, DiagnosisResult value, int cachelimitSize)
        {
            // 1. 添加或更新缓存队列
            var queue = cacheQueue.GetOrAdd(key, _ =>
            {
                // 新 key 加入顺序队列
                lock (lockObj)
                {
                    keyInsertionOrder.Enqueue(key);
                }
                return new FixedSizeConcurrentQueue<DiagnosisResult>(count);
            });

            // 2. 添加新数据
            queue.Enqueue(value);

            // 3. 检查是否需要清理旧缓存
            if (cacheQueue.Count >= cachelimitSize)
            {
                lock (lockObj)
                {
                    if (keyInsertionOrder.TryDequeue(out string oldestKey))
                    {
                        cacheQueue.TryRemove(oldestKey, out _);
                    }
                }
            }
        }


        /// <summary>
        ///  获取运行数据
        /// </summary>
        /// <param name="stationID">风场id</param>
        /// <param name="windturbineID">机组id</param>
        /// <param name="measLoctionID">测点id</param>
        /// <param name="acqTime">时间</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="dataSize">数据个数</param>
        /// <param name="measDataType">采集类型</param>
        /// <param name="signalType">数据类型</param>
        /// <returns></returns>
        public static List<OperatingData> GetRunData(string stationID, string windturbineID, string measLoctionID, string acqTime, double sampleRate, int dataSize, EnumMonitorType measDataType, EnumSignalType signalType)
        {
            List<OperatingData> runDatas = new List<OperatingData>();

            // 1.获取采集事件数据         
            List<MeasEventBase> measDataEventSiftData = GetCVMRunEventData(stationID, windturbineID, acqTime);

            if (measDataEventSiftData == null || measDataEventSiftData.Count == 0)
            {
                ALog.Debug(windturbineID + "_" + measLoctionID + acqTime + "_" + "采集事件数据查询为null！");
                return runDatas;
            }

            // 采集事件事件倒序
            measDataEventSiftData = measDataEventSiftData.OrderByDescending(vo => vo.AcqTime).ToList();

            // 2.波形数据 并行查询封装
            var tasks = new List<Task>();// 任务队列
            var data = new ConcurrentBag<OperatingData>();
            if (measDataEventSiftData.Count >= dataSize)
            {
                for (int i = 0; i < dataSize; i++)
                {
                    int index = i; //关键：捕获当前索引，避免 Task 共享变量
                    tasks.Add(Task.Run(() =>
                    {
                        data.Add(GetSpectrumAndEnvelopeData(stationID, windturbineID, measLoctionID, measDataEventSiftData[index].AcqTime, sampleRate, (double)measDataEventSiftData[index].RotSpd, measDataType, signalType));
                    }));
                }
            }
            else
            {
                tasks.Add(Task.Run(() =>
                {
                    data.Add(GetSpectrumAndEnvelopeData(stationID, windturbineID, measLoctionID, measDataEventSiftData[0].AcqTime, sampleRate, (double)measDataEventSiftData[0].RotSpd, measDataType, signalType));
                }));
            }

            Task.WaitAll(tasks.ToArray());// 等待所哟任务执行完成

            // 4 数据返回
            if (data != null && data.Count != 0)
            {
                // 使用 LINQ 过滤 null，并创建新的 ConcurrentBag
                var filteredData = new ConcurrentBag<OperatingData>(data.Where(d => d != null));
                runDatas = filteredData.ToList();
            }

            return runDatas;
        }

        /// <summary>
        ///  获取数据
        /// <param name="measLoctionID">测点id</param>
        /// <param name="acqTime">时间</param>
        /// <param name="sampleRate">采样率</param>
        /// <param name="path">波形路径</param>
        /// <param name="spd">转速</param>
        /// </summary>
        /// <param name="datas"></param>
        public static OperatingData GetSpectrumAndEnvelopeData(string stationID, string windturbineID, string measLoctionID, DateTime AcqTime, double sampleRate, double spd, EnumMonitorType measDataType, EnumSignalType signalType)
        {
            OperatingData spectrumAndEnvelopeData = null;
            TWDataBase vibWave = new TWDataBase();
            try
            {
                // 读取波形数据
                // List<TWDataBase> vibWaveDatas = _waveReadFactory.GetDeviceWaveRead(measDataType).GetMeasWaveData(stationID, windturbineID, measDataType, measLoctionID, signalType, AcqTime, sampleRate);
                DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(measLoctionID) ?? new();
                List<TWDataBase> vibWaveDatas = dbSelect.GetMeasWaveData(measObj, AcqTime, sampleRate);
                if (vibWaveDatas != null && vibWaveDatas.Count != 0)
                {
                    vibWave = vibWaveDatas[0];
                    // 根据路径查询波形
                    if (vibWave.WaveData != null && vibWave.WaveData.Length != 0)
                    {
                        double[] data = vibWave.WaveData.Select(x => (double)x).ToArray();// 查询波形
                        spectrumAndEnvelopeData = new OperatingData();
                        SortedDictionary<double, double> frequencyDomainData = FFTW.traditionFFT(GenBearingDiagnoseBase.CopyData(data), vibWave.SampleRate, data.Length * 1.0); // FFT
                        spectrumAndEnvelopeData.frequencyDomainData = frequencyDomainData;
                        SortedDictionary<double, double> envelopeSpectrumData = HilbertTransformEnvelope.hilbertTransformEnvelopeSpectrum(new List<double>(data), vibWave.SampleRate, null); // 包络
                        spectrumAndEnvelopeData.envelopeSpectrumData = envelopeSpectrumData;
                        spectrumAndEnvelopeData.MeasLocID = vibWave.MeasLocID;
                        spectrumAndEnvelopeData.SampleRate = vibWave.SampleRate;
                        spectrumAndEnvelopeData.AcqTime = vibWave.AcqTime;
                        spectrumAndEnvelopeData.waveData = data;
                        spectrumAndEnvelopeData.Spd = spd;
                    }
                    else
                    {
                        ALog.Information(vibWave.MeasLocID + "_" + vibWave.AcqTime + "_" + "波形数据为null");
                    }
                }
                return spectrumAndEnvelopeData;
            }
            catch (Exception ex)
            {
                ALog.Debug(vibWave.MeasLocID + "_" + vibWave.AcqTime + "_" + ex);
                return null;
            }
        }

        /// <summary>
        /// 获取传动链运行采集事件数据(近一天)
        /// </summary>
        /// <param name="stationID">风场id</param>
        /// <param name="windturbineID"></param>
        /// <param name="acqTime"></param>
        /// <returns></returns>
        public static List<MeasEventBase> GetCVMRunEventData(string stationID, string windturbineID, string acqTime)
        {
            //1. 查询采集事件表中当前时间前一天的的数据 
            List<MeasEventBase> measDataEvent = new List<MeasEventBase>();
            DateTime timeBegin = DateTime.ParseExact(acqTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            measDataEvent = _waveReadFactory.GetMeasEventByDeviceID(stationID, windturbineID, timeBegin.AddDays(-1), timeBegin.AddMilliseconds(1));

            // 2.查询数据为null 返回
            if (measDataEvent != null && measDataEvent.Count != 0)
            {
                // 3.对查询的一天内的数据 ， 采集类型传动链  筛选转速大于60，然后从达到小排序
                measDataEvent = measDataEvent.Where(vo => vo.MeasType == EnumMonitorType.CVM && vo.RotSpd > 60).OrderByDescending(data => data.AcqTime).ToList();
            }
            return measDataEvent;
        }

        /* /// <summary>
         ///  获取频谱间隔
         /// </summary>
         /// <param name="StationID">风场id</param>
         /// <param name="WindturbineID">机组id</param>
         /// <param name="MeasLoctionID">测点id</param>
         /// <param name="AcqTime">时间</param>
         /// <param name="SampleRate">采样率</param>
         /// <param name="MeasDataType">采集类型</param>
         /// <param name="SignalType">数据类型</param>
         /// <returns></returns>
         private static double getSpectrumInterval(string StationID, string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate, EnumMonitorType MeasDataType, EnumSignalType SignalType)
         {
             double SpectrumInterval = 0;
             // 1.根据测点时间采样率查询波形数据
             DateTime timeBegin = DateTime.ParseExact(AcqTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

             // List<TWDataBase> VibWaveData = _waveReadFactory.GetDeviceWaveRead(MeasDataType).GetMeasWaveData(StationID, WindturbineID, MeasDataType, MeasLoctionID, SignalType, timeBegin, SampleRate);
             DevMeasLocation measObj = _devTreeRepository.GetMeasLocationByMeaslocID(MeasLoctionID) ?? new();
             List<TWDataBase> VibWaveData = dbSelect.GetMeasWaveData(measObj, timeBegin, SampleRate);

             if (VibWaveData != null && VibWaveData.Count != 0)
             {
                 // 2.根据波形数据 波形个数和采样率计算频谱间隔
                 SpectrumInterval = SampleRate / VibWaveData[0].SamplePoint;
             }
             else
             {
                 ALog.Information(MeasLoctionID + "_" + AcqTime + SampleRate + "_" + "没有查到该波形数据");
             }
             return SpectrumInterval;
         }*/

        /// <summary>
        /// 滚动体边频数据封装
        /// </summary>
        /// <param name="FaultFrequency"></param>
        /// <param name="SideFrequency"></param>
        /// <param name="Spd"></param>
        /// <param name="FTF"></param>
        /// <param name="SpectrumInterval"></param>
        /// <returns></returns>
        private static IActionResult bSFSideFrequencyEncapsulation(double FaultFrequency, List<double> SideFrequency, double? Spd, double FTF, double SpectrumInterval)
        {
            // 1.保持架系数校验
            if (FTF == 0)
            {
                return _createReponse.CreateResponse("", 200, "未配置保持架故障信息，边频无法进行标注！");
            }

            // 2.边频间隔
            double Interval = FTF * Spd.Value;

            // 3.边频间隔/频谱间隔 =频谱间隔倍数
            int multiple = (int)(Interval / SpectrumInterval);

            // 4.修正间隔频率=频谱间隔倍数*频谱间隔
            Interval = multiple * SpectrumInterval;

            SideFrequency.Add(double.Parse(((FaultFrequency - 3 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency - 2 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency - 1 * Interval)).ToString("G9")));
            SideFrequency.Add(FaultFrequency);
            SideFrequency.Add(double.Parse(((FaultFrequency + 1 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency + 2 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency + 3 * Interval)).ToString("G9")));
            return _createReponse.CreateResponse(SideFrequency);
        }

        /// <summary>
        ///  内圈边频数据封装
        /// </summary>
        /// <param name="MeasLoctionID"></param>
        /// <param name="FaultFrequency"></param>
        /// <param name="SideFrequency"></param>
        /// <param name="Spd"></param>
        /// <param name="SpdCompare"></param>
        /// <param name="SpectrumInterval"></param>
        /// <returns></returns>
        private static IActionResult bPFISideFrequencyEncapsulation(string MeasLoctionID, double FaultFrequency, List<double> SideFrequency, ref double? Spd, double SpdCompare, double SpectrumInterval)
        {
            // 1.如果测点包含主轴，转速根据变速比，转化
            if (MeasLoctionID.Contains("MST"))
            {
                Spd = Spd * (1 / SpdCompare);
            }

            // 2.边频间隔
            double Interval = Spd.Value;

            // 3.边频间隔/频谱间隔 =频谱间隔倍数
            int multiple = (int)(Interval / SpectrumInterval);

            // 4.修正间隔频率=频谱间隔倍数*频谱间隔
            Interval = multiple * SpectrumInterval;

            SideFrequency.Add(double.Parse(((FaultFrequency - 3 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency - 2 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency - 1 * Interval)).ToString("G9")));
            SideFrequency.Add(FaultFrequency);
            SideFrequency.Add(double.Parse(((FaultFrequency + 1 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency + 2 * Interval)).ToString("G9")));
            SideFrequency.Add(double.Parse(((FaultFrequency + 3 * Interval)).ToString("G9")));
            return _createReponse.CreateResponse(SideFrequency);
        }



        /// <summary>
        /// 获取转频
        /// </summary>
        /// <param name="WindturbineID">机组id</param> 
        /// <param name="AcqTime">时间</param>
        /// <returns></returns>
        internal static IActionResult getRotorFrequency(string WindturbineID, string MeasLoctionID, string AcqTime, double SampleRate)
        {
            // 设备树：根据测点id   找到风场id 和部件类型数据
            DevMeasLocation devMeasLocation = DevTreeRepsitory.Instance.GetMeasLocationByMeaslocID(MeasLoctionID);
            string StationID = devMeasLocation.StationID;
            EnumMonitorType MeasDataType = devMeasLocation.MeasDataType;
            EnumSignalType SignalType = devMeasLocation.SignalType;
            RotorFrequencyVO RotorFrequency = new RotorFrequencyVO();

            // 1.查询发电机转速
            double? Spd = GetGenSpd(StationID, WindturbineID, MeasLoctionID, AcqTime);
            if (Spd == null || Spd <= 60)
            {
                return _createReponse.CreateResponse("", 200, "转速值太小，无法进行标注！");
            }

            // 2.查询变速比数据
            double SpdCompare = 0;// 变速比
            double ConstantSpeed = 0;// 恒定转速
            List<DeviceParamEx> DeviceParamExes = DevTreeRepsitory.Instance.GetDeviceParamExByWindturbineID(WindturbineID);
            if (DeviceParamExes == null || DeviceParamExes.Count == 0)
            {
                DeviceParamExes = DeviceParamExes.Where(o => o.ParmKey == "spdCompare" || o.ParmKey == "constantSpeed").ToList();

                return _createReponse.CreateResponse("", 200, "未配置设备属性数据，无法进行标注！");
            }
            // 3.赋值变速比 恒定转速
            foreach (var value in DeviceParamExes)
            {
                if (value.ParmKey == "spdCompare")
                {
                    SpdCompare = double.Parse(value.ParmValue);
                }
                else if (value.ParmKey == "constantSpeed")
                {
                    ConstantSpeed = double.Parse(value.ParmValue);
                }
            }

            // 4.获取发电机频谱数据
            SortedDictionary<double, double> FrequencyDomainData = GetGenSpectrumData(StationID, WindturbineID, MeasLoctionID, AcqTime, SampleRate, MeasDataType, SignalType);

            // 5.使用峰值提取法 修正转频
            var result = PeakSearch.DictionaryToArray(FrequencyDomainData);
            Spd = PeakSearch.GetPeaks(result.dataX, result.dataY, (Spd.Value / 60) * 0.7, (ConstantSpeed / 60) * 1.1).X;

            // 6.如果测点包含主轴，转速根据变速比，转化
            if (MeasLoctionID.Contains("MST"))
            {
                Spd = Spd.Value * (1 / SpdCompare);
            }

            // 7.转频数据封装
            return _createReponse.CreateResponse(FrequencyConversionEncapsulation(out RotorFrequency, Spd));

        }
    }
}
