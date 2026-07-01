using ACH.DataEntity.TheWeave;
using System;
using System.Collections.Generic;

namespace ACH.DBRepository.TheWeave
{
    /// <summary>
    /// TheWeave数据库相关查询接口
    /// </summary>
    public interface ITheWeaveRepository
    {
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
        public List<EigenValueData> GetEigenValueData(string deviceID, string measlocID, string[] evCode, DateTime startTime, DateTime endTime, double? minRot = null, double? maxRot = null);


        /// <summary>
        /// 获取转速范围内的全部测量事件索引
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="minRot">最小转速</param>
        /// <param name="maxRot">最大转速</param>
        /// <returns></returns>
        public List<MeasDataEvent> GetMeasDataEvent(string deviceID, DateTime startTime, DateTime endTime, double? minRot, double? maxRot);


        /// <summary>
        /// 获取波形数据，返回结果不带有详细波形信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<MeasWaveData> GetMeasWaveData(string deviceID, DateTime startTime, DateTime endTime);


        /// <summary>
        /// 获取波形数据，返回结果带有详细波形信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="measLocID">测点ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <param name="sampleRate">采样频率</param>
        /// <returns></returns>
        public List<MeasWaveData> GetMeasWaveData(string deviceID, string measLocID, DateTime acqTime, double? sampleRate = null);



        /// <summary>
        /// 获取波形数据，返回结果带有详细波形信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="acqTime">采集时间</param>
        /// <returns></returns>
        public List<PulseWaveData> GetPulseWaveData(string deviceID, DateTime acqTime);
    }
}
