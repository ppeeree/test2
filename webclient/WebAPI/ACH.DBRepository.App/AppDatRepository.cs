using ACH.ACHLog.SeriLog;
using ACH.DataEntity.App;
using ACH.DataEntity.Enum;
using ACH.DataRepository.DevTree;
using ACH.DBConn.Dat;
using ACH.DevTree.DataRepository;
using ACH.DevTree.Entity;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ACH.DBRepository.App
{
    public class AppDatRepository : IAppDatRepository
    {
        IDevTreeRepsitory _devTreeRepository = DevTreeRepsitory.Instance;

        /// <summary>
        /// 根据用户ID获取绑定的风场
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<UserStationMapper> GetStationListByUserID(string userId)
        {
            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                return db.Queryable<UserStationMapper>().Where(o => o.UserId == userId).ToList();
            }
        }

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public UserInfo GetUserInfoById(string userId)
        {
            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                return db.Queryable<UserInfo>().Where(o => o.Id == userId).First();
            }
        }

        /// <summary>
        /// 根据用户Account获取用户信息
        /// </summary>
        /// <param name="userAccount">用户Account</param>
        /// <returns></returns>
        public UserInfo GetUserInfoByAccount(string userAccount)
        {
            try
            {
                using (SqlSugarClient db = AppDBContext.GetAppDBConn())
                {
                    return db.Queryable<UserInfo>().Where(o => o.Account == userAccount).First();
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"根据Account{userAccount}获取用户信息异常");
                return new UserInfo();
            }
        }


        /// <summary>
        /// 获取用户角色信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public RoleInfo GetRoleInfoByUserId(string userId)
        {
            try
            {
                using (SqlSugarClient db = AppDBContext.GetAppDBConn())
                {
                    var data = db.Queryable<UserRoleMapper>().First(o => o.UserID == userId);
                    return db.Queryable<RoleInfo>().First(o => o.RoleID == data.RoleID);
                }
            }
            catch (Exception ex)
            {
                ALog.Error(ex, $"GetRoleInfoByUserId-根据用户ID{userId}获取用户角色信息异常");
                return new RoleInfo();
            }
        }

        /// <summary>
        /// 根据机组ID和分布圆类型获取分布圆配置信息 
        /// </summary>
        /// <param name="deviceID">机组ID</param>
        /// <param name="circleType">分布圆类型</param>
        /// <returns></returns>
        public List<MeaslocCircleModelConfig> GetMeaslocCircleModelConfig(string deviceID, EnumCircleType circleType)
        {
            using (SqlSugarClient db = AppDBContext.GetAppDBConn())
            {
                try
                {
                    // 先根据机组ID筛选Mapper表
                    var deviceMapper = db.Queryable<MeaslocCircleConfigMapper>().Where(o => o.DeviceID == deviceID && o.CircleType == circleType).ToList();
                    if (deviceMapper != null && deviceMapper.Count > 0)
                    {
                        List<MeaslocCircleModelConfig> res = db.Queryable<MeaslocCircleModelConfig>().Where(o => o.ConfigID == deviceMapper.First().ConfigID).ToList();
                        if (res != null && res.Count > 0)
                        {
                            return res;
                        }
                    }
                    else
                    {
                        // 获取风场ID 
                        List<DevMeasLocation> devMeas = _devTreeRepository.GetMeaslocationByDeviceID(deviceID);
                        string stationID = devMeas.FirstOrDefault().StationID;

                        var stationMapper = db.Queryable<MeaslocCircleConfigMapper>().Where(o => o.StationID == stationID && o.CircleType == circleType).ToList();
                        if (stationMapper != null && stationMapper.Count > 0)
                        {
                            List<MeaslocCircleModelConfig> res = db.Queryable<MeaslocCircleModelConfig>().Where(o => o.ConfigID == stationMapper.First().ConfigID).ToList();
                            if (res != null && res.Count > 0)
                            {
                                return res;
                            }
                        }
                    }
                    return new List<MeaslocCircleModelConfig>();
                }
                catch (Exception ex)
                {
                    ALog.Error(ex, $"GetMeaslocCircleModelConfig-根据机组ID{deviceID}获取用户信息异常");
                    return new List<MeaslocCircleModelConfig>();
                }
            }
        }
    }
}
