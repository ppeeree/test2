using ACH.DataEntity.Alarm;
using System;
using System.Collections.Generic;

namespace ACH.RTStatusTree.DevStatusTree
{
    public interface IStatusRepository
    {
        /// <summary>
        /// 1、获取全部风场列表
        /// </summary>
        /// <returns></returns>
        List<StationItem> GetStations();

        /// <summary>
        /// 2、根据风场ID获取设备状态树
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        StationItem GetStationsById(string stationId);

        /// <summary>
        /// 3、根据机组ID获取设备状态树
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        DeviceItem GetDeviceId(string deviceID);

        /*/// <summary>
        /// 获取风场状态趋势
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByStationID(string stationID, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取机组状态趋势
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<DeviceItem> GetDeviceStateTrend(string deviceID, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取部件状态趋势
        /// </summary>
        /// <param name="stationID"></param>
        /// <param name="componentId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByCompID(string stationID, string componentId, DateTime startTime, DateTime endTime);*/

    }
}
