using ACH.DataEntity.DevTree;
using ACH.DevTree.Entity;
using System;
using System.Collections.Generic;

namespace ACH.DBRepository.DevTree
{
    /// <summary>
    /// 该接口类是对DevTree.dat数据库的查询的封装
    /// 也是对外部源ACH.DevTree.DataRepository的补充
    /// </summary>
    public interface IDevTreeRepository
    {
        /// <summary>
        /// 获取风场下的机组Info信息
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        public List<DeviceInfo> GetDeviceInfoByStationID(string stationID);


        /// <summary>
        /// 根据设备ID获取3D位置信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<DeviceMeaslocPositopnPlan> GetDeviceMeaslocPositopnPlan(string deviceId);


        /// <summary>
        /// 根据设备ID获取3D位置信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public (List<DeviceModelPlan> model, List<DeviceMeaslocPositopnPlan> measloc) GetDevice3DModelPlan(string deviceId);
    }
}
