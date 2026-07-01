using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMSFramework.BusinessEntity;

namespace WindCMS.IAnalyzerDomain
{
    /// <summary>
    /// 读取设备树
    /// </summary>
    public interface IReadDevTree
    {
        /// <summary>
        /// 获取用户管理的所有风电场的设备树数据
        /// </summary>
        /// <param name="_userName"></param>
        /// <returns></returns>
        DevTreeData GetDevTreeData(string _userName);
        /// <summary>
        /// 获取风电场的设备树数据
        /// </summary>
        /// <param name="_windParkId"></param>
        /// <returns></returns>
        DevTreeData GetWPDevTreeData(string _windParkId);
        
    }
}
