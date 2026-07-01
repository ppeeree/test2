using ACH.DataRepository.DevTree;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using CMSFramework.BusinessEntity;
using WindCMS.IAnalyzerDomain;

namespace WindCMS.GRPCWPServerImp.Domain
{
    internal class DeviceTreeDomain : IReadDevTree
    {
        private readonly IConfiguration _config;
        public DeviceTreeDomain(IConfiguration config)
        {
            _config = config;
        }
        IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;

        /// <summary>
        /// 获取用户关联设备树
        /// </summary>
        /// <param name="_userName"></param>
        /// <returns></returns>
        public DevTreeData GetDevTreeData(string _userName)
        {
            DevTreeData devTreeData = new DevTreeData();

            // 获取风场
            devTreeData.WindParkList = GetWindParks();
            // 获取机组机型
            devTreeData.WindTurModelList = GetWindTurbineModels();
            // 获取机组列表
            devTreeData.WindTurbineList = GetWindTurbines();
            // 获取部件列表
            devTreeData.WindTurbineComponentList = GetComponents();
            // 获取振动测量位置列表
            devTreeData.MeasLoc_VibList = GetMeasLocVibs();
            // 获取工况测量位置列表
            devTreeData.MeasLoc_ProcessList = GetMeasLocProcess();
            // 获取转速测量位置列表
            devTreeData.MeasLoc_RotList = GetMeasLocRots();
            // 获取晃度测量位置列表
            devTreeData.MeasLoc_SVMList = GetSVMLocs();
            // 数据对象组装
            return devTreeData;
        }
        /// <summary>
        /// 获取晃度测量位置
        /// </summary>
        /// <returns></returns>
        private List<MeasLoc_SVM> GetSVMLocs()
        {
            return new List<MeasLoc_SVM>();
        }
        /// <summary>
        /// 获取转速测量位置
        /// </summary>
        /// <returns></returns>
        private List<MeasLoc_RotSpd> GetMeasLocRots()
        {
            var rotMeasLoc = _devTreeRepository.GetAllMeasLocation().Where(o => o.MeasLoctionName == "发电机转速");
            List<MeasLoc_RotSpd> rotSpd = new List<MeasLoc_RotSpd>();
            foreach (var item in rotMeasLoc)
            {
                var measLoc = new MeasLoc_RotSpd();
                measLoc.MeasLocationID = item.MeasLoctionID;
                measLoc.WindTurbineID = item.DeviceID;
                measLoc.MeasLocName = item.MeasLoctionName;
                measLoc.OrderSeq = item.sort;
                rotSpd.Add(measLoc);
            }
            return rotSpd;
        }
        /// <summary>
        /// 获取工况测量位置
        /// </summary>
        /// <returns></returns>
        private List<MeasLoc_Process> GetMeasLocProcess()
        {
            return new List<MeasLoc_Process>();
        }

        /// <summary>
        /// 振动测量位置
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<MeasLoc_Vib> GetMeasLocVibs()
        {
            List<MeasLoc_Vib> result = new List<MeasLoc_Vib>();
            var measLoc = _devTreeRepository.GetAllMeasLocation();
            foreach (var item in measLoc)
            {
                if (item.MeasLoctionName == "发电机转速")
                    continue;
                var vibLoc = new MeasLoc_Vib();
                vibLoc.MeasLocationID = item.MeasLoctionID;
                vibLoc.WindTurbineID = item.DeviceID;
                vibLoc.MeasLocName = item.MeasLoctionName;
                vibLoc.OrderSeq = item.sort;
                vibLoc.ComponentID = item.ComponentID;
                vibLoc.SectionName = item.Section;
                vibLoc.Orientation = item.Orientation;
                result.Add(vibLoc);

            }
            return result;
        }

        /// <summary>
        /// 获取部件列表
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private List<WindTurbineComponent> GetComponents()
        {
            List<WindTurbineComponent> result = new List<WindTurbineComponent>();
            var compGroup = _devTreeRepository.GetAllMeasLocation().GroupBy(o => o.ComponentID);
            foreach (var item in compGroup)
            {
                var comp = new WindTurbineComponent();
                comp.ComponentID = item.Key;
                comp.ComponentName = item.First().ComponentName;
                comp.ComponentModel = "";
                result.Add(comp);
            }
            return result;
        }

        /// <summary>
        /// 获取机组列表
        /// </summary>
        /// <returns></returns>
        private List<WindTurbine> GetWindTurbines()
        {
            List<WindTurbine> result = new List<WindTurbine>();
            var compGroup = _devTreeRepository.GetAllMeasLocation().GroupBy(o => o.DeviceID);
            foreach (var item in compGroup)
            {
                var device = new WindTurbine();
                device.WindTurbineID = item.Key;
                device.WindTurbineCode = item.First().DeviceName;
                device.WindTurbineName = item.First().DeviceName;
                device.WindParkID = item.First().StationID;
                result.Add(device);
            }
            return result;
            //return DataDbContext.Query<WindTurbine>("SELECT space_guid as 'WindTurbineID', space_code as 'WindTurbineCode' ,space_name as 'WindTurbineName', dept_code as 'WindParkID' FROM account_space  WHERE model_code = 'WINDTURBINE'").ToList();
        }
        private List<WindTurbineModel> GetWindTurbineModels()
        {
            return new List<WindTurbineModel>();
        }
        /// <summary>
        /// 获取风场列表
        /// </summary>
        /// <returns></returns>
        private List<WindPark> GetWindParks()
        {
            List<WindPark> result = new List<WindPark>();
            var compGroup = _devTreeRepository.GetAllMeasLocation().GroupBy(o => o.StationID);
            foreach (var item in compGroup)
            {
                var station = new WindPark();
                station.WindParkID = item.Key;
                station.WindParkName = item.First().StationName;
                station.WindParkCode = item.Key;
                result.Add(station);
            }
            return result;
        }

        /// <summary>
        /// 获取风场下设备树
        /// </summary>
        /// <param name="_windParkId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public DevTreeData GetWPDevTreeData(string _windParkId)
        {
            DevTreeData devTreeData = new DevTreeData();

            // 获取风场
            devTreeData.WindParkList = GetWindParks().Where(o => o.WindParkID == _windParkId).ToList();
            // 获取机组机型
            var windTurModelList = GetWindTurbineModels();
            // 获取机组列表
            var windTurbineList = GetWindTurbines();
            // 获取部件列表
            var windTurbineComponentList = GetComponents();
            // 获取振动测量位置列表
            var measLoc_VibList = GetMeasLocVibs();
            // 获取工况测量位置列表
            var measLoc_ProcessList = GetMeasLocProcess();
            // 获取转速测量位置列表
            var measLoc_RotList = GetMeasLocRots();
            // 获取晃度测量位置列表
            var measLoc_SVMList = GetSVMLocs();
            // 数据对象组装

            foreach (var park in devTreeData.WindParkList)
            {
                park.WindTurbineList = windTurbineList.Where(o => o.WindParkID == park.WindParkID).ToList();

                foreach (var tur in park.WindTurbineList)
                {
                    tur.TurComponentList = windTurbineComponentList.Where(o => o.WindTurbineID == tur.WindTurbineID).ToList();
                    tur.VibMeasLocList = measLoc_VibList.Where(o => o.WindTurbineID == tur.WindTurbineID).ToList();
                    tur.ProcessMeasLocList = measLoc_ProcessList.Where(o => o.WindTurbineID == tur.WindTurbineID).ToList();
                    tur.RotSpdMeasLoc = measLoc_RotList.FirstOrDefault(o => o.WindTurbineID == tur.WindTurbineID);
                    tur.MeasLocSVMList = measLoc_SVMList.Where(o => o.WindTurbineID == tur.WindTurbineID).ToList();
                }
            }

            return devTreeData;
        }
    }
}
