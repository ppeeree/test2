using ACH.CMSWebClient.ControllerImplement;
using ACH.CMSWebClient.ControllerModel.BladeSystem;
using ACH.CMSWebClient.ControllerModel.Component;
using ACH.DataEntity.App;
using ACH.DBRepository.App;
using ACH.Helper.ApiReponse;
using ACH.Helper.Component;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// Bladx系统相关接口
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class BladeSystemController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        BladeSystemMethods bladeSyatemMethods = new BladeSystemMethods();
        IAppDatRepository systemRepository = new AppDatRepository();
        ComponentHelper componentHelper = new ComponentHelper();
        private IConfiguration _configuration;
        public BladeSystemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 获取系统版本等信息
        /// </summary>
        /// <param name="domain">域名</param>
        /// <returns></returns>
        [HttpGet("info")]
        public IActionResult GetSystemInfoByDomain(string domain)
        {
            SystemInfoDTO res = new SystemInfoDTO();
            res.WebsiteTitleName = _configuration["xnTitle"] ?? "风机混凝土塔筒在线状态监测系统";
            res.DeptType = "control";
            res.WTPHMServiceVersion = _configuration["xnVersion"] ?? "1.6.7.2";

            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        ///  登录
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="username"></param>
        /// <param name="password">前端使用MD5进行密码加密</param>
        /// <param name="grant_type"></param>
        /// <param name="scope"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost("token")]
        public IActionResult GetSystemToken(string tenantId, string? username, string? password, string? refresh_token, string grant_type, string scope, string? type)
        {
            SystemToken res = new SystemToken();
            string filePath = "userToken.json";
            string jsonString = System.IO.File.ReadAllText(filePath);
            res = JsonConvert.DeserializeObject<SystemToken>(jsonString);

            try
            {
                // 开始登录时的传参
                if (grant_type == "password")
                {
                    UserInfo userInfo = systemRepository.GetUserInfoByAccount(username);
                    // 验证用户是否存在
                    if (userInfo == null)
                    {
                        SystemErrorToken obj = new SystemErrorToken() { error_code = 400, error_description = "不存在该用户" };
                        return Ok(obj);
                    }

                    RoleInfo roleInfo = systemRepository.GetRoleInfoByUserId(userInfo.Id);


                    // 验证密码是否正确
                    string dbPassword = bladeSyatemMethods.GetMd5Hash(userInfo.Password);
                    bool ok = string.Equals(password, dbPassword, StringComparison.OrdinalIgnoreCase);
                    if (!ok)
                    {
                        SystemErrorToken obj = new SystemErrorToken() { error_code = 400, error_description = "用户名或密码不正确" };
                        return Ok(obj);
                    }

                    // 根据用户名修改接口返回信息
                    bladeSyatemMethods.GetSystemToken(userInfo, roleInfo, res);

                    // 将res结果重新更新json文件中的风场
                    return Ok(res);
                }
                else
                {
                    // token验证资格时返回值
                    // TODO：资格验证
                    // bladeSyatemMethods.ValidateToken(refresh_token, res.refresh_token);
                    return Ok(res);
                }

            }
            catch (Exception ex)
            {
                return _createReponse.CreateResponse(new String(""), 500, ex.Message);
            }
        }


        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            return _createReponse.CreateResponse("");
        }


        /// <summary>
        /// 获取顶部菜单
        /// </summary>
        /// <returns></returns>
        [HttpGet("top-menu")]
        public IActionResult GetSystemTopMenu()
        {
            List<SystemTopMenuDTO> res = new List<SystemTopMenuDTO>();
            string filePath = "topMenu.json";

            string jsonString = System.IO.File.ReadAllText(filePath);

            res = JsonConvert.DeserializeObject<List<SystemTopMenuDTO>>(jsonString);

            // List<SystemTopMenuDTO> res = bladeSyatemMethods.GetSystemTopMenu();

            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        /// 根据顶部菜单ID获取二级菜单
        /// </summary>
        /// <param name="topMenuId"></param>
        /// <returns></returns>
        [HttpGet("routes")]
        public IActionResult GetSystemSecondpMenu(string topMenuId)
        {
            string filePath = "topMenu.json";
            string jsonString = System.IO.File.ReadAllText(filePath);
            List<SystemTopMenuDTO> allData = JsonConvert.DeserializeObject<List<SystemTopMenuDTO>>(jsonString);

            List<SystemSecondMenuDTO> res = new List<SystemSecondMenuDTO>();

            foreach (var item in allData)
            {
                if (item.id == topMenuId)
                {
                    res.AddRange(item.children);
                }
                else
                {
                    res.AddRange(item.children.Where(o => o.id == topMenuId));
                }
            }
            return _createReponse.CreateResponse(res);
        }

        /// <summary>
        /// 获取页面的按钮权限
        /// </summary>
        /// <returns></returns>
        [HttpGet("buttons")]
        public IActionResult GetSystemButtons()
        {
            List<SystemButtonDTO> res = new List<SystemButtonDTO>();
            string filePath = "button.json";

            string jsonString = System.IO.File.ReadAllText(filePath);

            res = JsonConvert.DeserializeObject<List<SystemButtonDTO>>(jsonString);

            return _createReponse.CreateResponse(res);
        }

        /* /// <summary>
         /// 获取包含省份信息的风场列表
         /// </summary>
         /// <param name="type"></param>
         /// <returns></returns>
         [HttpGet("getStationList")]
         public IActionResult GetStationProvinceList(string type)
         {
             *//*List<ContaryStation> res = new List<ContaryStation>();
             string filePath = "stationList.json";

             string jsonString = System.IO.FileSystem.ReadAllText(filePath);

             res = JsonConvert.DeserializeObject<List<ContaryStation>>(jsonString);*//*

             return _createReponse.CreateResponse(new List<string>());
         }*/


        /// <summary>
        /// 聚合部件树
        /// </summary>
        /// <returns></returns>
        [HttpGet("getPagecompAll")]
        public IActionResult GetPagecompAll()
        {
            List<ComponentTreeDTO> res = bladeSyatemMethods.GetComponentTree();

            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        /// 根据机组ID获取三维模型信息
        /// </summary>
        /// <param name="windturbineId">机组ID</param>
        /// <returns></returns>
        [HttpGet("getFanMeasureConfigInfo")]
        public IActionResult Get3DModelInfo(string windturbineId)
        {
            DeviceMeaslocModelPlanDTO res = bladeSyatemMethods.Get3DModelInfo(windturbineId);

            return _createReponse.CreateResponse(res);

        }

        /// <summary>
        /// 根据机组ID和聚合部件类型获取属性
        /// </summary>
        /// <param name="windTurbineId">机组ID</param>
        /// <param name="entityType">聚合部件类型</param>
        /// <returns></returns>
        [HttpGet("getEntityBasicInfo")]
        public IActionResult GetEntityBasicInfo(string windTurbineId, string entityType)
        {
            List<CompPropertiesDTO> res = new List<CompPropertiesDTO>();
            return _createReponse.CreateResponse(res);
        }


        /// <summary>
        /// 根据用户ID获取页面布局
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        [HttpGet("getPageLayout")]
        public IActionResult getPageLayout(string userName, string layoutName)
        {
            return _createReponse.CreateResponse(new List<string>());
        }


        [HttpGet("DefaultRoter")]
        public IActionResult DefaultRoter()
        {
            // 如果没有配置默认展示全景监视页面
            string router = _configuration["defaultRouter"] ?? "/screen";

            return _createReponse.CreateResponse(router);
        }
    }
}
