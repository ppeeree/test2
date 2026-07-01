using ACH.ACHLog.SeriLog;
using ACH.DataEntity.TheWeave;
using ACH.DBConn.Bladex;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using EigenValueData = ACH.DataEntity.TheWeave.EigenValueData;
using MeasWaveData = ACH.DataEntity.TheWeave.MeasWaveData;

namespace ACH.DBRepository.TheWeave
{
    public class TheWeaveRepository : ITheWeaveRepository
    {
        CommonMethods commonMethods;
        BladexDBContext bladexDBContext = new BladexDBContext();
        private static IConfiguration _configuration;
        public TheWeaveRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            commonMethods = new CommonMethods(_configuration);
        }

        /// <summary>
        /// 获取转速值范围内的特征值数据
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="evCode">特征值Code</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="minRot">最小转速</param>
        /// <param name="maxRot">最大转速</param>
        /// <returns></returns>
        public List<EigenValueData> GetEigenValueData(string deviceID, string measlocID, string[] evCode, DateTime startTime, DateTime endTime, double? minRot = null, double? maxRot = null)
        {
            // 没有转速筛选时不依赖测量事件索引表。部分历史月份只有特征值分表，
            // 如果先查 MeasDataEvent 会把已有趋势数据过滤成空。
            List<EigenValueData> evDatas = GetAllEigenValueDataByTimeRange(deviceID, measlocID, evCode, startTime, endTime);
            if (minRot == null && maxRot == null)
            {
                return FillEigenValueMeta(evDatas);
            }

            List<EigenValueData> res = new List<EigenValueData>();

            // step1：根据转速范围获取测量事件
            List<MeasDataEvent> measEvents = GetMeasDataEvent(deviceID, startTime, endTime, minRot, maxRot);
            if (measEvents == null || measEvents.Count < 1)
            {
                return new List<EigenValueData>();
            }

            foreach (MeasDataEvent evItem in measEvents)
            {
                // step3：取筛选出的测量事件表时间相同的数据
                List<EigenValueData> selectEvData = evDatas.Where(o => o.AcqTime == evItem.AcqTime).ToList();

                // 特征值趋势接口单位赋值
                if (selectEvData == null || selectEvData.Count == 0)
                    continue;

                res.AddRange(FillEigenValueMeta(selectEvData));
            }

            return res;
        }

        private List<EigenValueData> FillEigenValueMeta(List<EigenValueData> evDatas)
        {
            List<EigenValueData> res = new List<EigenValueData>();
            if (evDatas == null || evDatas.Count == 0)
            {
                return res;
            }

            foreach (EigenValueData item in evDatas)
            {
                List<EvCalcConfig> unitDatas = commonMethods.GetCVMEvRoule(item.MeasLocID);
                if (unitDatas == null || unitDatas.Count == 0)
                    continue;

                foreach (var item1 in unitDatas)
                {
                    if (item1.EigenValueCode == item.EigenValueCode)
                    {
                        item.EigenValueName = item1.EigenValueName;
                        item.SingleType = item1.SignalTypeCode;
                        item.Unit = item1.Unit;
                        break;
                    }
                }
                res.Add(item);
            }

            return res;
        }

        /// <summary>
        ///  获取时间范围内特征值Code列表中的全部特征值
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="evCode">特征值Code</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<EigenValueData> GetAllEigenValueDataByTimeRange(string deviceID, string measlocID, string[] evCode, DateTime startTime, DateTime endTime)
        {
            List<EigenValueData> res = new List<EigenValueData>();

            // 获取开始结束时间切割的自然月
            List<DataQueryTimeDTO> month = commonMethods.SplitQueryTime(startTime, endTime);

            using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
            {
                foreach (DataQueryTimeDTO dt in month)
                {
                    string tableName = db.SplitHelper<EigenValueData>().GetTableName(dt.StartTime);
                    // 表如果不存在新建分表
                    if (!db.DbMaintenance.IsAnyTable(tableName, false))
                        continue;
                    try
                    {
                        if (measlocID == "")
                        {
                            var data = db.Queryable<EigenValueData>()
                                .AS(tableName)
                                .Where(o => o.DeviceID == deviceID)
                                .Where(o => o.AcqTime >= startTime && o.AcqTime <= endTime)
                                .Where(o => evCode.Contains(o.EigenValueCode))
                                .ToList();
                            res.AddRange(data);
                        }
                        else
                        {
                            foreach (string evcode in evCode)
                            {
                                string evid = $"{measlocID}&&{evcode}";

                                var data = db.Queryable<EigenValueData>()
                                    .AS(tableName)
                                    .Where(o => o.EigenValueID == evid)
                                    .Where(o => o.AcqTime >= startTime && o.AcqTime <= endTime)
                                    .ToList();
                                res.AddRange(data);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, $"GetAllEigenValueDataByTimeRange-获取{dt.StartTime}-{dt.EndTime}内的特征值数据异常");
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// 获取转速范围内的全部测量事件索引
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="minRot">最小转速</param>
        /// <param name="maxRot">最大转速</param>
        /// <returns></returns>
        public List<MeasDataEvent> GetMeasDataEvent(string deviceID, DateTime startTime, DateTime endTime, double? minRot, double? maxRot)
        {
            List<MeasDataEvent> res = new List<MeasDataEvent>();

            // 获取开始结束时间切割的自然月
            List<DataQueryTimeDTO> month = commonMethods.SplitQueryTime(startTime, endTime);

            // 获取转速值范围
            double newMinRot = minRot == null ? double.MinValue : minRot.Value;
            double newMaxRot = maxRot == null ? double.MaxValue : maxRot.Value;

            using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
            {
                foreach (DataQueryTimeDTO dt in month)
                {
                    string tableName = db.SplitHelper<MeasDataEvent>().GetTableName(dt.StartTime);
                    try
                    {
                        var data = db.Queryable<MeasDataEvent>()
                            .AS<MeasDataEvent>(tableName)
                            .Where(o => o.AcqTime <= endTime && o.AcqTime >= startTime)
                            .Where(o => o.DeviceID == deviceID)
                            .Where(o => o.RotSpd >= newMinRot && o.RotSpd <= newMaxRot).ToList();
                        res.AddRange(data);
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, $"GetMeasDataEvent-获取{dt.StartTime}-{dt.EndTime}内的测量事件索引异常");
                    }
                }
            }
            return res;
        }


        /// <summary>
        /// 获取波形数据，返回结果不带有详细波形信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<MeasWaveData> GetMeasWaveData(string deviceID, DateTime startTime, DateTime endTime)
        {
            try
            {
                List<MeasWaveData> res = new List<MeasWaveData>();

                // 切割自然月
                List<DataQueryTimeDTO> splitMonth = commonMethods.SplitQueryTime(startTime, endTime);


                using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
                {
                    foreach (var month in splitMonth)
                    {
                        // 查询 EIWaveData
                        string EItableName = db.SplitHelper<EIWaveData>().GetTableName(month.StartTime);
                        List<EIWaveData> ei = db.Queryable<EIWaveData>().AS(EItableName).Where(o => o.DeviceID == deviceID && o.AcqTime >= startTime && o.AcqTime <= endTime).ToList();
                        if (ei == null || ei.Count == 0)
                        {
                            ALog.Debug($"GetMeasWaveData-EIWaveData表中没查询到{startTime}~{endTime}，{deviceID}测点波形");
                            return new List<MeasWaveData>();
                        }
                        res.AddRange(ConvertEIWaveData(ei, null, ""));



                        // 查询 PulseWaveData 转速数据
                        string pulsetableName = db.SplitHelper<PulseWaveData>().GetTableName(month.StartTime);
                        List<PulseWaveData> pulse = db.Queryable<PulseWaveData>().AS(pulsetableName).Where(o => o.DeviceID == deviceID && o.AcqTime >= startTime && o.AcqTime <= endTime).ToList();
                        if (pulse == null || pulse.Count == 0)
                        {
                            ALog.Debug($"GetMeasWaveData-PulseWaveData表中没查询到{startTime}~{endTime}，{deviceID}测点波形");
                            return new List<MeasWaveData>();
                        }
                        res.AddRange(ConvertPulseWaveData(pulse, ""));


                        // 查询 VibWaveData 振动数据
                        string vibtableName = db.SplitHelper<VibWaveData>().GetTableName(month.StartTime);
                        List<VibWaveData> vib = db.Queryable<VibWaveData>().AS(vibtableName).Where(o => o.DeviceID == deviceID && o.AcqTime >= startTime && o.AcqTime <= endTime).ToList();
                        if (vib == null || vib.Count == 0)
                        {
                            ALog.Debug($"GetMeasWaveData-VibWaveData表中没查询到{startTime}~{endTime}，{deviceID}测点波形");
                            return new List<MeasWaveData>();
                        }
                        res.AddRange(ConvertVibWaveData(vib, null, ""));
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasCMSVibData-获取{deviceID}，{startTime}~{endTime}时间的波形异常");
                return new List<MeasWaveData>();
            }
        }


        /// <summary>
        /// 获取波形数据，返回结果带有详细波形信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="measlocId">测点ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="sampleRate">采样频率</param>
        /// <returns></returns>
        public List<MeasWaveData> GetMeasWaveData(string deviceID, string measlocId, DateTime dateTime, double? sampleRate = null)
        {
            try
            {
                using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
                {
                    List<PulseWaveData> spd = new List<PulseWaveData>();
                    try
                    {
                        //根据时间获取转速数据
                        string tableName = db.SplitHelper<PulseWaveData>().GetTableName(dateTime);
                        spd = db.Queryable<PulseWaveData>().AS(tableName).Where(o => o.AcqTime == dateTime).ToList();
                    }
                    catch (Exception ex)
                    {
                        ALog.Error(ex, $"GetMeasWaveData-获取转速值异常");
                    }


                    if (measlocId.Contains(ConstStr.TOW))
                    {
                        // 查询 EIWaveData
                        string tableName = db.SplitHelper<EIWaveData>().GetTableName(dateTime); // 确定对应的数据库表
                        List<EIWaveData> datas = new List<EIWaveData>();

                        datas = db.Queryable<EIWaveData>()
                            .AS(tableName)
                            .Where(o => measlocId == o.MeasLocID && o.AcqTime == dateTime)
                            .WhereIF(sampleRate != null, o => o.SampleRate == sampleRate)
                            .ToList(); // 在数据库表中筛选波形数据


                        if (datas == null || datas.Count == 0)
                        {
                            ALog.Debug($"GetMeasWaveData-EIWaveData表中没查询到{measlocId}测点波形");
                            return new List<DataEntity.TheWeave.MeasWaveData>();
                        }

                        return ConvertEIWaveData(datas, spd, measlocId);
                    }
                    else if (measlocId.Contains(ConstStr.GENRSPD))
                    {
                        try
                        {
                            // 查询 PulseWaveData 转速数据
                            string tableName = db.SplitHelper<PulseWaveData>().GetTableName(dateTime); // 确定对应的数据库表
                            List<PulseWaveData> datas = db.Queryable<PulseWaveData>().AS(tableName).Where(o => measlocId == o.MeasLocID && o.AcqTime == dateTime).ToList(); // 在数据库表中筛选波形数据

                            if (datas == null || datas.Count == 0)
                            {
                                ALog.Debug($"GetMeasWaveData-PulseWaveData表中没查询到{measlocId}测点波形");
                                return new List<DataEntity.TheWeave.MeasWaveData>();
                            }

                            return ConvertPulseWaveData(datas, measlocId);
                        }
                        catch (Exception ex)
                        {
                            ALog.Error(ex, $"GetMeasCMSVibData:{measlocId},{dateTime}");
                            return new List<DataEntity.TheWeave.MeasWaveData>();
                        }
                    }
                    else
                    {
                        // 查询 VibWaveData 振动数据
                        string tableName = db.SplitHelper<VibWaveData>().GetTableName(dateTime); // 确定对应的数据库表
                        List<VibWaveData> datas = new List<VibWaveData>();
                        if (sampleRate == null)
                        {
                            datas = db.Queryable<VibWaveData>().AS(tableName).Where(o => o.MeasLocID == measlocId && o.AcqTime == dateTime).ToList();
                        }
                        else
                        {
                            datas = db.Queryable<VibWaveData>().AS(tableName).Where(o => o.MeasLocID == measlocId && o.AcqTime == dateTime && o.SampleRate == sampleRate).ToList();
                        }
                        if (datas == null || datas.Count == 0)
                        {
                            ALog.Debug($"GetMeasWaveData-{tableName}表中没查询到{measlocId}测点波形");
                            return new List<MeasWaveData>();
                        }

                        return ConvertVibWaveData(datas, spd, measlocId);
                    }
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasCMSVibData-获取{measlocId}，{dateTime}时间的波形异常");
                return new List<MeasWaveData>();
            }
        }


        #region  将ACH对象转化为MeasWaveData 
        // EIWaveData转化为MeasWaveData
        private List<MeasWaveData> ConvertEIWaveData(List<EIWaveData> vibs, List<PulseWaveData> spd, string measlocId)
        {
            List<MeasWaveData> resultData = new List<MeasWaveData>();
            foreach (var vib in vibs)
            {
                MeasWaveData waveData = new MeasWaveData()
                {
                    WindParkName = "",
                    CompName = "",
                    MeaslocName = "",
                    DeviceName = "",
                    WaveLength = vib.SamplePoint,
                    AcqTime = vib.AcqTime,
                    CompId = vib.ComponentID,
                    DeviceID = vib.DeviceID,
                    SampleRate = (int)vib.SampleRate,
                    RMS = vib.AVG,
                    WaveType = "1",
                    MeaslocId = vib.MeasLocID,
                    WavePath = vib.WavePath,
                    RoteSpd = 0
                };
                if (spd != null && spd.Count != 0)
                {
                    List<PulseWaveData> spdDatas = spd.Where(o => o.DeviceID == waveData.DeviceID && o.AcqTime == waveData.AcqTime).ToList();
                    if (spdDatas != null && spdDatas.Count > 0)
                    {
                        waveData.RoteSpd = spdDatas[0].AVG;
                    }
                }
                resultData.Add(waveData);
            }
            return resultData;
        }

        // PulseWaveData转化为MeasWaveData
        private List<MeasWaveData> ConvertPulseWaveData(List<PulseWaveData> vibs, string measlocId)
        {
            List<MeasWaveData> resultData = new List<MeasWaveData>();
            foreach (var vib in vibs)
            {
                MeasWaveData waveData = new MeasWaveData()
                {
                    WindParkName = "",
                    CompName = "",
                    MeaslocName = "",
                    DeviceName = "",
                    WaveLength = vib.SamplePoint,
                    AcqTime = vib.AcqTime,
                    CompId = vib.ComponentID,
                    DeviceID = vib.DeviceID,
                    SampleRate = (int)vib.AVG,
                    RMS = vib.AVG,
                    WaveType = "1",
                    MeaslocId = vib.MeasLocID,
                    WavePath = vib.WavePath,
                    RoteSpd = vib.AVG
                };
                resultData.Add(waveData);
            }
            return resultData;
        }

        // VibWaveData转化为MeasWaveData
        private List<MeasWaveData> ConvertVibWaveData(List<VibWaveData> vibs, List<PulseWaveData> spd, string measlocId)
        {
            List<MeasWaveData> resultData = new List<MeasWaveData>();
            foreach (var vib in vibs)
            {

                MeasWaveData waveData = new MeasWaveData()
                {
                    WindParkName = "",
                    CompName = "",
                    MeaslocName = "",
                    DeviceName = "",
                    WaveLength = vib.SamplePoint,
                    AcqTime = vib.AcqTime,
                    CompId = vib.ComponentID,
                    DeviceID = vib.DeviceID,
                    SampleRate = (int)vib.SampleRate,
                    RMS = vib.RMS,
                    WaveType = "1",
                    MeaslocId = vib.MeasLocID,
                    WavePath = vib.WavePath,
                    RoteSpd = 0
                };

                if (spd != null && spd.Count != 0)
                {
                    List<PulseWaveData> spdDatas = spd.Where(o => o.DeviceID == waveData.DeviceID && o.AcqTime == waveData.AcqTime).ToList();
                    if (spdDatas != null && spdDatas.Count > 0)
                    {
                        waveData.RoteSpd = spdDatas[0].AVG;
                    }
                }
                resultData.Add(waveData);
            }
            return resultData;
        }


        public List<PulseWaveData> GetPulseWaveData(string deviceID, DateTime acqTime)
        {
            List<PulseWaveData> res = new List<PulseWaveData>();
            try
            {
                using (SqlSugarClient db = bladexDBContext.GetTheWeaveDBConn())
                {
                    string tableName = db.SplitHelper<PulseWaveData>().GetTableName(acqTime);
                    res = db.Queryable<PulseWaveData>().AS(tableName).Where(o => o.AcqTime == acqTime).ToList();
                    /*foreach (PulseWaveData item in data)
                    {
                        // 根据地址获取波形
                        PulseWaveData obj = commonMethods.GetMeasWaveData(item);
                        res.Add(obj);
                    }*/
                }

                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"在TheWeave数据库中查询PulseWaveData异常");
                return res;
            }
        }

        #endregion
    }
}
