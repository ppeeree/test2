using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.Analysis;
using ACH.DataEntity.Common;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.DBSelect;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using ACH.Helper.Comparer;
using ACH.Helper.Component;
using ACH.MeasData.Entity;

namespace ACH.CMSWebClient.ControllerImplement.Analysis
{
    public class MeasEventDataMethods
    {
        IDevTreeRepsitory devTreeRepsitory;
        DBSelect DBSelect;
        ComponentHelper componentHelper = new ComponentHelper();

        public MeasEventDataMethods(IConfiguration _configuration)
        {
            devTreeRepsitory = DevTreeRepsitory.Instance;
            DBSelect = new DBSelect(_configuration);
        }


        /// <summary>
        /// 获取数据测量事件
        /// </summary>
        /// <param name="bgTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="spdRange">转速范围</param>
        /// <param name="measlocId">机组id（按机组查询时使用，逗号分割）</param>
        /// <param name="measlocId">测量位置（逗号分割）</param>
        /// <param name="child">是否包含详细</param>
        /// <returns></returns>
        internal List<MeasEventDTO> GetMeasEventData(DateTime bgTime, DateTime endTime, string spdRange, string deviceID, string measlocId, bool detail)
        {
            List<MeasEventBase> measEventDatas = new List<MeasEventBase>();
            List<DevMeasLocation> measurements = new List<DevMeasLocation>();
            if (!string.IsNullOrWhiteSpace(deviceID))
            {
                // 按照机组查询
                string[] devices = deviceID.Split(',');
                foreach (string device in devices)
                {
                    var measLocs = devTreeRepsitory.GetMeaslocationByDeviceID(device);
                    measurements.AddRange(measLocs);
                    measEventDatas.AddRange(DBSelect.GetMeasEventByDeviceID(measLocs.First(), bgTime, endTime));
                }
            }
            if (!string.IsNullOrEmpty(measlocId))
            {
                List<EnumMonitorType> acquisitionComponentType = new List<EnumMonitorType>();
                // 获取采集部件类型数据
                Dictionary<List<string>, EnumMonitorType> acquisitionComponentTypeList = componentHelper.GetComponentTypeDicReverse();
                string[] measlocs = measlocId.Split(',');
                // 测量位置分组-> 找到其对应的机组
                foreach (string loc in measlocs)
                {
                    var measLoc = devTreeRepsitory.GetMeasLocationByMeaslocID(loc);
                    foreach (var codeList in acquisitionComponentTypeList)// 获取测点对应的采集部件类型
                    {
                        if (codeList.Key != null && codeList.Key.Count != 0)
                        {
                            foreach (var code in codeList.Key)
                            {
                                if (loc.Contains(code))
                                {
                                    acquisitionComponentType.Add(codeList.Value);
                                    break;
                                }
                            }

                        }
                    }
                    var meas = devTreeRepsitory.GetMeasLocationByMeaslocID(loc);
                    if (meas != null)
                    {
                        measurements.Add(meas);
                    }
                }
                var group = measurements.GroupBy(o => o.DeviceID);

                // 根据机组ID获取时间范围内测量事件
                foreach (var item in group)
                {
                    var measevents = DBSelect.GetMeasEventByDeviceID(item.First(), bgTime, endTime);
                    measEventDatas.AddRange(measevents);
                }
                // 按照采集部件类型过滤
                if (measEventDatas != null && measEventDatas.Count != 0)
                {   // 采集部件类型去重
                    if (acquisitionComponentType != null && acquisitionComponentType.Count != 0)
                    {
                        acquisitionComponentType = acquisitionComponentType.Distinct().ToList();
                    }
                    measEventDatas = measEventDatas.Where(vo => acquisitionComponentType.Contains(vo.MeasType)).ToList();
                }
            }

            List<MeasEventDTO> data = ConvertToWFDataInfo(measurements, measEventDatas);
            return data;
        }



        private List<MeasEventDTO> ConvertToWFDataInfo(List<DevMeasLocation> measurements, List<MeasEventBase> measEventDatas)
        {
            List<MeasEventDTO> res = new List<MeasEventDTO>();
            foreach (var item in measEventDatas)
            {
                var meas = measurements.FirstOrDefault(o => o.DeviceID == item.DeviceID);
                if (meas != null)
                {
                    double spd = 0.0;
                    if (item.RotSpd != null && item.RotSpd != 0)
                    {
                        spd = (double)item.RotSpd;
                    }
                    MeasEventDTO info = new()
                    {
                        Children = new List<MeasEventWaveDTO>(),
                        MeaslocSummary = item.Count + "",
                        AcqTime = item.AcqTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        CompName = DataEntity.Common.EnumHelper.GetDescription(item.MeasType),
                        RotSpeed = spd,
                        WindParkId = item.StationID,
                        WindturbineId = item.DeviceID,
                        WindParkName = meas.StationName,
                        WindturbineName = meas.DeviceName
                    };
                    res.Add(info);
                }
            }

            // 如果数据不为null倒顺排序
            if (res != null && res.Count != 0)
            {
                res = res.OrderByDescending(item => item.AcqTime).ToList();
            }
            return res;
        }


        /// <summary>
        /// 获取指定时间的测量详细信息
        /// </summary>
        /// <param name="dateTime">时间</param>
        /// <param name="deviceID">机组ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal MeasEventDTO GetMeasEventDetails(DateTime dateTime, string measlocId, string deviceID)
        {
            try
            {
                // 获取选中测点的所有波形数据
                List<string> measLocIDs = measlocId.Split(",").ToList();
                DevMeasLocation? measLoc = null;
                List<MeasEventWaveDTO> children = new List<MeasEventWaveDTO>();
                foreach (var _measLocID in measLocIDs)
                {
                    measLoc = devTreeRepsitory.GetMeasLocationByMeaslocID(_measLocID);

                    if (measLoc != null)
                    {
                        List<TWDataBase> measData = DBSelect.GetMeasWaveData(measLoc, dateTime, null);
                        children.AddRange(ConvertChildren(measLoc, measData));
                    }
                }


                // 将数据转化为接口范围对象
                if (measLoc != null && children.Any())
                {
                    var first = children.First();

                    MeasEventDTO res = new MeasEventDTO
                    {
                        MeaslocSummary = "",
                        WindturbineName = measLoc.DeviceName,
                        WindParkName = first.WindParkName,
                        AcqTime = first.AcqTime,
                        CompName = first.CompName,
                        WindParkId = measLoc.StationID,
                        WindturbineId = measLoc.DeviceID,
                        RotSpeed = 0,
                        Children = children.SortByName(ascending: true, dictType: EnumSortType.MeaslocName),
                    };
                    return res;
                }
                else
                {
                    ALog.Debug($"GetMeasEventDetails-该测点下该时间点暂无波形，返回空");
                    return new MeasEventDTO();
                }

            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetMeasEventDetails-获取波形列表详情异常");
                return new MeasEventDTO();
            }
        }


        private List<MeasEventWaveDTO> ConvertChildren(DevMeasLocation obj, List<TWDataBase> measData)
        {
            List<MeasEventWaveDTO> res = new List<MeasEventWaveDTO>();

            foreach (var item in measData)
            {
                if (item == null) continue;

                MeasEventWaveDTO ele = new MeasEventWaveDTO();
                ele.AcqTime = item.AcqTime.ToString("yyyy-MM-dd HH:mm:ss");
                ele.CompId = obj.ComponentID;
                ele.CompName = obj.ComponentName;
                ele.MeaslocId = obj.MeasLoctionID;
                ele.MeaslocName = obj.MeasLoctionName;
                ele.Rms = item.EV + "";
                ele.SampleRate = item.SampleRate;
                ele.WaveLength = item.WaveData != null ? item.WaveData.Length : 0;
                ele.WindturbineId = item.DeviceID;
                ele.WindturbineName = obj.DeviceName;
                ele.WindParkName = obj.StationName;

                res.Add(ele);
            }

            return res;
        }
    }
}
