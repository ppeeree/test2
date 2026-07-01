using ACH.DevTree.Entity;
using System;
using System.Collections.Generic;

namespace ACH.Download.WaveDataSelect
{
    public interface IWaveFileSelect
    {
        /// <summary>
        /// 下载数据筛选提供接口
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="measList">设备树列表</param>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="num">筛选个数</param>
        /// <returns></returns>
        public List<string> SelectWavePath(string deviceID, List<DevMeasLocation> measList, DateTime bgTime, DateTime endTime, int num);

    }
}
