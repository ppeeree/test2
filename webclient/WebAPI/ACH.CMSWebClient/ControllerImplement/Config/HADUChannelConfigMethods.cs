using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.Config;
using ACH.DataEntity.AnylzerData;
using ACH.DataEntity.DevTreeData;
using ACH.DataRepository.DevTree;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.ApiReponse;
using SqlSugar;
using System.ComponentModel;

namespace ACH.CMSWebClient.ControllerImplement.Config
{
    public class HADUChannelConfigMethods
    {
        static IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        /// <summary>
        /// 1、GetMeasIDByStationID接口实现：根据风场ID获取该风场下的全部特征值CODE和名称
        /// </summary>
        /// <param name="stationId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static List<KeyValueModel> GetMeasCodeByStationID(string stationId)
        {
            List<KeyValueModel> res = new List<KeyValueModel>();
            List<DevMeasLocation> deviceList = _devTreeRepository.GetAllMeasLocation(stationId);

            var firstDevList = deviceList.GroupBy(d => d.DeviceID).First();

            foreach (var device in firstDevList)
            {
                string comCode = device.ComponentID.Replace(device.DeviceID, "");
                string measCode = device.MeasLoctionID.Replace(device.DeviceID, "");
                KeyValueModel obj = new KeyValueModel();
                obj.Key = $"{comCode}&&{measCode}";
                obj.Value = device.MeasLoctionName;
                res.Add(obj);
            }
            return res;
        }

        /// <summary>
        /// 2、接口实现：获取枚举的序号-描述键值列表
        /// </summary>
        internal static List<KeyValueModel> GetEnumKeyValues<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T))
                       .Cast<T>()
                       .Select((e, idx) => new KeyValueModel
                       {
                           Key = e.ToString(),
                           Value = GetDescription(e)
                       })
                       .ToList();
        }

        /// <summary>
        /// 2、接口实现：获取枚举描述
        /// </summary>
        private static string GetDescription<T>(T value) where T : Enum
        {
            var field = typeof(T).GetField(value.ToString());
            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }


        /// <summary>
        /// 4、查
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static List<HADUConfigList> GetHADUConfigList()
        {
            List<HADUConfigList> res = new List<HADUConfigList>();

            List<HADUChannelMapper> data = MapperRepository.Instance.GetAllHADUMapper();

            foreach (var item in data)
            {
                // 根据测点ID获取设备
                DevMeasLocation dev = _devTreeRepository.GetMeasLocationByMeaslocID(item.MeasLocationID);

                HADUConfigList obj = new HADUConfigList();
                obj.StationID = item.StationID;
                obj.StationName = dev.StationName;
                obj.DeviceID = item.DeviceID;
                obj.DeviceName = dev.DeviceName;
                obj.MeasLocName = dev.MeasLoctionName;
                obj.MeasLocID = item.MeasLocationID;
                obj.Chno = item.MapChno;

                res.Add(obj);
            }

            return res;
        }


        /// <summary>
        /// 5、增
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal static void AddHADUConfig(AddHADUConfigParam param)
        {

            List<HADUChannelMapper> data = new List<HADUChannelMapper>();
            foreach (var device in param.DeviceIDList)
            {
                string[] devices = device.Split(",");
                foreach (var meas in param.MeasLocList)
                {
                    string[] measList = meas.Split(",");
                    string[] codes = measList[0].Split("&&");
                    string measID = $"{devices[0]}{codes[1]}";

                    DevMeasLocation dev = _devTreeRepository.GetMeasLocationByMeaslocID(measID);
                    if (dev == null) continue;
                    HADUChannelMapper obj = new HADUChannelMapper();
                    obj.StationID = param.StationID;
                    obj.MapStationID = param.MapStationID;
                    obj.DeviceID = devices[0];
                    obj.MapDeviceID = devices[1];
                    obj.ComponentID = $"{devices[0]}{codes[0]}";
                    obj.SingalType = dev.SignalType;
                    obj.MeasLocationID = measID;
                    obj.MapChno = int.Parse(measList[1]);

                    data.Add(obj);
                }
            }

            // 向表中存储
            MapperRepository.Instance.AddHADUMapper(data);
        }


        /// <summary>
        /// 6、更新
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal static void UpdateHADUConfig(HADUConfigList param)
        {
            // 更新测点映射表
            HADUChannelMapper obj = MapperRepository.Instance.GetMeaslocMapper(param.MeasLocID);
            // int chno = int.Parse(param.Chno);
            if (obj.MapChno != param.Chno)
            {
                obj.MapChno = param.Chno;
                MapperRepository.Instance.UpdateHADUMapper(obj);
            }

            // 更新设备树表
            DevMeasLocation dev = _devTreeRepository.GetMeasLocationByMeaslocID(param.MeasLocID);
            if (param.MeasLocName != dev.MeasLoctionName)
            {
                dev.MeasLoctionName = param.MeasLocName;
                _devTreeRepository.UpdateMeasLocName(dev);// DevMeasLocationRepository.UploadMeasLocName(dev);
            }
        }


        /// <summary>
        /// 7、删除测点映射
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal static void DeleteHADUConfig(string measIDs)
        {
            List<HADUChannelMapper> data = new List<HADUChannelMapper>();
            string[] measList = measIDs.Split(',');
            foreach (var meas in measList)
            {
                HADUChannelMapper obj = MapperRepository.Instance.GetMeaslocMapper(meas);
                data.Add(obj);
            }

            MapperRepository.Instance.DeleteHADUMapper(data);
        }
    }
}
