using ACH.DataEntity.App;
using ACH.DataEntity.Enum;
using System.Collections.Generic;

namespace ACH.DBRepository.App
{
    /// <summary>
    /// APP.dat系统相关的数据库查询
    /// </summary>
    public interface IAppDatRepository
    {
        /// <summary>
        /// 根据用户ID获取绑定的风场
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        public List<UserStationMapper> GetStationListByUserID(string userID);


        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public UserInfo GetUserInfoById(string userId);


        /// <summary>
        /// 根据用户Account获取用户信息
        /// </summary>
        /// <param name="userAccount">用户Account</param>
        /// <returns></returns>
        public UserInfo GetUserInfoByAccount(string userAccount);

        /// <summary>
        /// 根据用户ID获取用户角色信息
        /// </summary>
        public RoleInfo GetRoleInfoByUserId(string id);


        /// <summary>
        /// 根据机组ID和分布圆类型获取分布圆配置信息 
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="circleType">分布圆类型</param>
        /// <returns></returns>
        public List<MeaslocCircleModelConfig> GetMeaslocCircleModelConfig(string deviceID, EnumCircleType circleType);
    }
}
