using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace ACH.DBRepository.TheWeave
{
    public class TheWeaveEVRead : IDeviceEVRead
    {
        ITheWeaveRepository theweaveRepository;

        private static IConfiguration _configuration;
        public TheWeaveEVRead(IConfiguration configuration)
        {
            _configuration = configuration;
            theweaveRepository = new TheWeaveRepository(_configuration);
        }


        /// <summary>
        /// 获取特征值趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="deviceID">机组ID</param>
        /// <param name="evCode">特征值Code</param>
        /// <param name="timeBegin">开始时间</param>
        /// <param name="timeEnd">结束时间</param>
        /// <param name="measLocID">测点ID</param>
        /// <param name="minRot">最小转速</param>
        /// <param name="maxRot">最大转速</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<EigenValueData> GetEigenValueHis(string stationID, string deviceID, string[] evCode, DateTime timeBegin, DateTime timeEnd, string measLocID = "", double? minRot = null, double? maxRot = null)
        {
            List<EigenValueData> res = new List<EigenValueData>();
            List<ACH.DataEntity.TheWeave.EigenValueData> evDatas = theweaveRepository.GetEigenValueData(deviceID, measLocID, evCode, timeBegin, timeEnd, minRot, maxRot);

            foreach (var item in evDatas)
            {
                EigenValueData obj = new EigenValueData();
                obj.Id = item.Id;
                obj.DeviceID = item.DeviceID;
                obj.ComponentID = item.ComponentID;
                obj.MeasLocID = item.MeasLocID;
                obj.EigenValueID = item.EigenValueID;
                obj.EigenValueCode = item.EigenValueCode;
                obj.EigenValueName = item.EigenValueName;
                obj.Unit = item.Unit;
                obj.SingleType = Enum.Parse<DataEntity.Common.EnumSignalType>(item.SingleType);
                obj.SampleRate = item.SampleRate;
                obj.AcqTime = item.AcqTime;
                obj.EigenValue = item.EigenValue;
                obj.DataQuality = item.DataQuality;
                obj.EigenValueStatus = DataEntity.Common.EnumAlarmStatus.Nostate;//
                res.Add(obj);
            }
            return res;
        }
    }
}
