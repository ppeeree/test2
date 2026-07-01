using ACH.DataRepository.DevTree;
using ACH.MeasData.DB;
using ACH.MeasData.Entity;
using CMSFramework.BusinessEntity;
using WindCMS.IAnalyzerDomain;

namespace WindCMS.GRPCWPServerImp.Domain
{
    public class MeasureDataDomain : IReadMeasData
    {
        private readonly IConfiguration _config;
        IDeviceEVRead evRead;
        IDeviceWaveRead waveRead;
        public MeasureDataDomain(IConfiguration config)
        {
            _config = config;
            evRead = new CMSEVTrendDatDB(_config);
            waveRead = new ACHDeviceReadWaveDB(_config);
        }
        /// <summary>
        /// 根据机组id、开始时间、结束时间获取特征值报警事件
        /// </summary>
        /// <param name="_windTurbineId"></param>
        /// <param name="_beginTime"></param>
        /// <param name="_endTime"></param>
        /// <returns></returns>
        public List<MeasEvent_EigenValue> GetMeasEventEVTreeList(string _windTurbineId, DateTime _beginTime, DateTime _endTime)
        {
            // Step 1 获取机组事件段内特征值列表
            string stationID = DevTreeRepsitory.Instance.GetDeviceInfoByID(_windTurbineId).StationId;
            List<ACH.MeasData.Entity.EigenValueData> evlist = evRead.GetEigenValueHis(stationID, _windTurbineId, new string[] { "RMS" }, _beginTime, _endTime, "", null, null);

            var group = evlist.GroupBy(o => o.AcqTime);

            // Step 2 根据特征值时间，构建特征值测量事件树
            List<MeasEvent_EigenValue> measEvents = new List<MeasEvent_EigenValue>();
            foreach (var evs in group)
            {
                MeasEvent_EigenValue measEvent_EigenValue = new MeasEvent_EigenValue();
                measEvent_EigenValue.AcquisitionTime = evs.Key;
                measEvent_EigenValue.EigenValueNum = evs.Count();
                measEvent_EigenValue.EVDataList = ConvertEvObj(evs.ToList());
                measEvent_EigenValue.WindTurbineID = _windTurbineId;
                measEvents.Add(measEvent_EigenValue);
            }
            return measEvents;
        }

        private List<EigenValueData_Vib> ConvertEvObj(List<ACH.MeasData.Entity.EigenValueData> eigenValueDatas)
        {
            List<EigenValueData_Vib> evs = new List<EigenValueData_Vib>();
            foreach (var ev in eigenValueDatas)
            {
                EigenValueData_Vib _ev = new EigenValueData_Vib();
                _ev.AcquisitionTime = ev.AcqTime;
                _ev.MeasLocationID = ev.MeasLocID;
                _ev.WindTurbineID = ev.DeviceID;
                _ev.EigenValueCode = ev.EigenValueCode;
                _ev.EigenValueID = ev.EigenValueID;
                _ev.EigenValueCode = ev.EigenValueCode;
                evs.Add(_ev);
            }
            return evs;
        }


        /// <summary>
        /// 根据机组id、开始时间、结束时间 获取波形测量事件树
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>5
        public List<MeasEvent_Wave> GetMeasEventWFTreeList(string _windTurbineId, DateTime _beginTime, DateTime _endTime)
        {
            List<MeasEvent_Wave> measEvent_Waves = new List<MeasEvent_Wave>();

            // 根据机组，获取波形数据
            List<VibWaveFormData> waves = GetVibWaveFormData(_windTurbineId, _beginTime, _endTime);
            foreach (var wave in waves)
            {
                if (string.IsNullOrWhiteSpace(wave.SignalType))
                {
                    wave.SignalType = "1";
                    wave.WaveformType = EnumWaveFormType.WDF_Time;
                }
            }
            var vibGroup = waves.GroupBy(o => o.AcquisitionTime).ToList();
            foreach (var vibs in vibGroup)
            {
                MeasEvent_Wave measEvent = new MeasEvent_Wave();
                measEvent.AcquisitionTime = vibs.Key;
                measEvent.WaveFormNum = vibs.Count();
                measEvent.WaveFormDataList = vibs.ToList();
                measEvent.WindTurbineID = _windTurbineId;
                measEvent_Waves.Add(measEvent);
            }
            // 根据波形数据，分组构建波形测量事件
            return measEvent_Waves;
        }
        /// <summary>
        /// 获取波形索引
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private List<VibWaveFormData> GetVibWaveFormData(string windTurbineId, DateTime beginTime, DateTime endTime)
        {
            // Step 1 获取机组事件段内特征值列表
            string stationID = DevTreeRepsitory.Instance.GetDeviceInfoByID(windTurbineId).StationId;
            var wavedatas = waveRead.GetMeasWaveData(stationID, windTurbineId, ACH.DataEntity.Common.EnumMonitorType.CVM, beginTime, endTime);

            List<VibWaveFormData> vibWaveFormDataList = new List<VibWaveFormData>();
            foreach (var item in wavedatas)
            {
                var measLoc = DevTreeRepsitory.Instance.GetMeasLocationByMeaslocID(item.MeasLocID);
                VibWaveFormData vibWaveFormData = new VibWaveFormData();
                vibWaveFormData.AcquisitionTime = item.AcqTime;
                vibWaveFormData.WindTurbineID = item.DeviceID;
                vibWaveFormData.MeasLocationID = item.MeasLocID;
                vibWaveFormData.WaveDefinitionID = item.SampleRate.ToString();
                vibWaveFormData.WaveLength = item.SamplePoint;
                vibWaveFormData.SampleRate = item.SampleRate;
                //vibWaveFormData.SignalType = item.SingleType.ToString();
                vibWaveFormData.LocationSection = measLoc.Section;
                vibWaveFormData.ComponentName = measLoc.ComponentName;
                vibWaveFormData.LocationOrientation = measLoc.Orientation;
                vibWaveFormDataList.Add(vibWaveFormData);
            }
            //Log.Logger.Debug($"GetVibWaveFormData  sql {sql}");
            return vibWaveFormDataList;
        }


        /// <summary>
        /// 根据机组id、测量位置id、波形类型、采集时间 获取波形数组
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="_measLocID"></param>
        /// <param name="_WFType"></param>
        /// <param name="_acqTime"></param>
        /// <returns></returns>
        public byte[] GetWaveFormData(string windTurbineId, string _measLocID, string _waveDefID, EnumWaveFormType _WFType, DateTime _acqTime)
        {
            var measLoc = DevTreeRepsitory.Instance.GetMeasLocationByMeasID(_measLocID);

            List<TWDataBase> waveData = waveRead.GetMeasWaveData(measLoc.StationID, measLoc.DeviceID, measLoc.MeasDataType, measLoc.MeasLoctionID, ACH.DataEntity.Common.EnumSignalType.A, _acqTime, Convert.ToDouble(_waveDefID));
            if (waveData.Count > 0)
            {
                return DataConvertHepler.FloatArrayToByteArray(waveData[0].WaveData);
            }
            return null;
        }

        /// <summary>
        /// 根据机组id,采集时间获取转速波形数据
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="_acqTime"></param>
        /// <returns></returns>
        public byte[] GetRotSpdWaveData(string windTurbineId, System.DateTime _acqTime)
        {
            //string sql = $"SELECT wave_path FROM wtphm_wave_data WHERE windturbine_id = '{windTurbineId}' AND measloc_id = '{windTurbineId}SPD' AND plan_id = '{1}'  AND acq_time = '{_acqTime:yyyy-MM-dd HH:mm:ss}'";
            //string path = DataDbContext.QueryFirstOrDefault<string>(sql);
            //if (!string.IsNullOrWhiteSpace(path))
            //{
            //    if (File.Exists(path))
            //    {
            //        return File.ReadAllBytes(path);
            //    }
            //}
            return null;
        }

        /// <summary>
        /// 获取工况
        /// </summary>
        /// <param name="_waveDataPath">波形存储路径</param>
        /// <param name="iWknCode">工况代码</param>
        /// <returns></returns>
        public byte[] GetWkWaveBytes(string _waveDataPath, int iWknCode)
        {
            throw new NotImplementedException();
        }
    }
}
