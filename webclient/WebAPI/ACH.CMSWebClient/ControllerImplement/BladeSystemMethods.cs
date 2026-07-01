
using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.ControllerModel.BladeSystem;
using ACH.CMSWebClient.ControllerModel.Component;
using ACH.DataEntity.App;
using ACH.DataEntity.DevTree;
using ACH.DataRepository.DevTree;
using ACH.DBRepository.App;
using ACH.DBRepository.DevTree;
using ACH.DevTree.DataRepository;
using ACH.Helper.Component;
using System.Security.Cryptography;
using System.Text;

namespace ACH.CMSWebClient.ControllerImplement
{
    public class BladeSystemMethods
    {
        IAppDatRepository systemRepository = new AppDatRepository();
        IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;
        ComponentHelper componentHelper = new ComponentHelper();
        IDevTreeRepository webDevTreeRepository = new DevTreeRepository();


        /// <summary>
        /// 使用MD5对密码加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string GetMd5Hash(string input)
        {
            using var md5 = MD5.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        internal void GetSystemToken(UserInfo userInfo, RoleInfo roleInfo, SystemToken res)
        {
            // 根据登录用户的account获取用户信息
            res.account = userInfo.Account;
            res.user_id = userInfo.Id;
            res.post_id = userInfo.Id;
            res.user_name = userInfo.Name;
            res.nick_name = userInfo.Name;
            res.role_id = roleInfo.RoleID;
            res.role_name = roleInfo.RoleCode;

            // 获取该用户绑定的风场
            UserStationMapper stationMap = systemRepository.GetStationListByUserID(userInfo.Id).First();
            if (stationMap == null)
            {
                ALog.Information($"{userInfo.Name}用户没有绑定的风场");
                return;
            }

            var stationInfo = _devTreeRepository.GetStationInfoByID(stationMap.StationID);
            if (stationInfo == null)
            {
                ALog.Information($"{stationMap.StationID}风场没有风场信息");
                return;
            }
            res.dept_name = stationInfo.StationName;
            res.deptCode = stationInfo.StationId;
            res.dept_id = stationInfo.StationId;
            res.longitude = stationInfo.Longitude.ToString();
            res.latitude = stationInfo.Latitude.ToString();
            res.elevation = stationInfo.Elevation.ToString();
        }



        /// <summary>
        /// 根据机组ID获取三维模型信息
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal DeviceMeaslocModelPlanDTO Get3DModelInfo(string deviceID)
        {
            var data = webDevTreeRepository.GetDevice3DModelPlan(deviceID);

            DeviceMeaslocModelPlanDTO res = new DeviceMeaslocModelPlanDTO();
            res.model = data.model;
            res.measloc = data.measloc.First() == null ? new List<PlanCompModel>() : data.measloc.First().children;

            return res;
        }

        /// <summary>
        /// 获取聚合部件和实体部件树
        /// </summary>
        /// <returns></returns>
        public List<ComponentTreeDTO> GetComponentTree()
        {
            List<ComponentTreeDTO> res = new List<ComponentTreeDTO>();
            try
            {
                // 获取聚合部件和实体部件关系的字典
                var pageCompDir = componentHelper.GetComponentDirc();

                // 将字典中的数据按照父部件名称聚合
                var groups = pageCompDir.Values.GroupBy(p => p.PageCompType);
                foreach (var pageComp in groups)
                {
                    var firstComp = pageComp.First();

                    ComponentTreeDTO obj = new ComponentTreeDTO(
                        firstComp.PageCompType,
                        firstComp.PageCompName,
                        firstComp.Sort.ToString()
                    );

                    foreach (var comp in pageComp)
                    {
                        obj.AddChild(comp.PageCompName, comp.PageCompType, comp.CompName, comp.CompType, comp.Sort);
                    }
                    res.Add(obj);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetComponentTree-构建聚合状态树异常");
            }
            return res;
        }

    }

}
