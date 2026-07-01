
using ACH.DataEntity.Common;
using ACH.DataEntity.StatusTree;
using System.Collections.Generic;

namespace ACH.RTStatusTree.HADUStatusTree
{
    public interface IHADUStatusRepository
    {
        #region 旧版本RT数据获取接口
        /// <summary>
        /// 根据风场ID获取风场->HADU状态树
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public HADUStatus GetHADUTypeLists(string stationID);


        /// <summary>
        /// 根据采集器ID获取通道状态列表
        /// </summary>
        /// <param name="deviceID"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public List<ChannelStatusItem> GetChannelStatusList(string deviceID, string deviceType);


        /// <summary>
        /// 在内存中构建RT树
        /// </summary>
        public void CreatedSensorStatusTree();


        /// <summary>
        /// 根据风场ID获取采集器状态列表
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public List<HADUStatusItem> GetTVMSTEStatusLists(string stationID);

        /// <summary>
        /// 根据风场ID获取法兰螺栓采集器列表
        /// </summary>
        /// <param name="stationID"></param>
        /// <returns></returns>
        public List<HADUStatusItem> GetBFMStatusLists(string stationID);
        #endregion
    }
}
