using ACH.Alarm.Entity;
using ACH.DataEntity.Alarm;
using ACH.DataEntity.Common;
using ACH.DataEntity.StatusTree;
using System;
using System.Collections.Generic;

namespace ACH.DBRepository.SD
{
    /// <summary>
    /// 针对SD数据库中实时状态数据的读取
    /// </summary>
    public interface IAlarmStatusRepository
    {
        #region 系统自检

        /// <summary>
        /// 获取风场下的全部通道状态
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorType">采集事件类型</param>
        /// <returns></returns>
        public List<ChannelStatusAlarm> GetChannelStatusByStationId(string stationID, EnumMonitorType monitorType);

        /// <summary>
        /// 获取采集器ID下所有通道状态列表 
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="monitorID">采集器ID</param>
        /// <returns></returns>
        public List<ChannelStatusAlarm> GetChannelStatusByHADUId(string stationID, string monitorID);

        #endregion

        #region 设备预警
        /// <summary>
        /// 1、获取全部风场列表
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<DTStationInfo> GetStations(string userID);


        /// <summary>
        /// 根据风场ID或用户ID获取设备树对象
        /// </summary>
        /// <param name="stationId">风场ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<DTStationInfo> GetStationsByUserIDOrStationID(string? stationId, string? userID);

        /// <summary>
        /// 2、根据风场ID获取设备状态树
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        public DTStationInfo GetStationById(string stationId);

        /// <summary>
        /// 3、根据机组ID获取设备状态树
        /// </summary>
        /// <param name="deviceID"></param>
        /// <returns></returns>
        public DTDeviceStatus GetDeviceId(string deviceID);

        /// <summary>
        /// 获取风场下全部部件状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByStationID(string stationID, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取风场下全部机组状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<DeviceItem> GetDeviceStateTrend(string stationID, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取某个部件状态趋势
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <param name="componentId">部件ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public List<CompItem> GetCompStateTrendByCompID(string stationID, string componentId, DateTime startTime, DateTime endTime);
        #endregion
    }
}
