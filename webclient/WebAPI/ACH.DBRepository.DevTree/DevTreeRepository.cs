using ACH.ACHLog.SeriLog;
using ACH.DataEntity.DevTree;
using ACH.DataRepository.DevTree;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACH.DBRepository.DevTree
{
    public class DevTreeRepository : IDevTreeRepository
    {
        IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;

        /// <summary>
        /// 获取风场下的机组Info信息
        /// </summary>
        /// <param name="stationID">风场ID</param>
        /// <returns></returns>
        public List<DeviceInfo> GetDeviceInfoByStationID(string stationID)
        {
            List<DeviceInfo> turbineList = new List<DeviceInfo>();
            List<DevMeasLocation> measLocationList = _devTreeRepository.GetAllMeasLocation(stationID);

            foreach (var item in measLocationList.GroupBy(o => o.DeviceID))
            {
                // 获取该机组的所有属性
                DeviceInfo deviceInfo = _devTreeRepository.GetDeviceInfoByID(item.Key);
                turbineList.Add(deviceInfo);
            }

            return turbineList;
        }


        /// <summary>
        /// 根据设备ID获取3D位置信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public List<DeviceMeaslocPositopnPlan> GetDeviceMeaslocPositopnPlan(string deviceId)
        {
            try
            {
                DeviceInfo info = _devTreeRepository.GetDeviceInfoByID(deviceId);
                if (info == null) return new List<DeviceMeaslocPositopnPlan>();

                var measlocPlan = info.MeaslocPlan.Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");

                List<DeviceMeaslocPositopnPlan> res = JsonConvert.DeserializeObject<List<DeviceMeaslocPositopnPlan>>(measlocPlan);

                return res;
            }
            catch (Exception ex)
            {
                ALog.Error(ex.Message);
                return new List<DeviceMeaslocPositopnPlan>();
            }
        }


        /// <summary>
        /// 根据设备ID获取3D位置信息
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public (List<DeviceModelPlan> model, List<DeviceMeaslocPositopnPlan> measloc) GetDevice3DModelPlan(string deviceId)
        {
            DeviceInfo info = _devTreeRepository.GetDeviceInfoByID(deviceId);

            var measlocPlanJson = info.MeaslocPlan.Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");
            List<DeviceMeaslocPositopnPlan> measloc = JsonConvert.DeserializeObject<List<DeviceMeaslocPositopnPlan>>(measlocPlanJson);

            var modelPlanJson = info.DeviceModelPlan.Trim('"').Replace("\\\"", "\"").Replace("\\\\", "\\");
            List<DeviceModelPlan> model = JsonConvert.DeserializeObject<List<DeviceModelPlan>>(modelPlanJson);

            return (model, measloc);
        }
    }
}
