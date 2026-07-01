using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSFramework.BusinessEntity;

namespace WindCMS.IAnalyzerDomain
{
    /// <summary>
    /// 以测量事件树为操作对象，读取测量数据
    /// </summary>
    public interface IReadMeasData
    {
        /// <summary>
        /// 根据机组id、开始时间、结束时间获取特征值报警事件
        /// </summary>
        /// <param name="_windTurbineId"></param>
        /// <param name="_beginTime"></param>
        /// <param name="_endTime"></param>
        /// <returns></returns>
        List<MeasEvent_EigenValue> GetMeasEventEVTreeList(string _windTurbineId, DateTime _beginTime, DateTime _endTime);


        /// <summary>
        /// 根据机组id、开始时间、结束时间 获取波形测量事件树
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        List<MeasEvent_Wave> GetMeasEventWFTreeList(string _windTurbineId, DateTime _beginTime, DateTime _endTime);
        

        /// <summary>
        /// 根据机组id、测量位置id、波形类型、采集时间 获取波形数组
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="_measLocID"></param>
        /// <param name="_WFType"></param>
        /// <param name="_acqTime"></param>
        /// <returns></returns>
        byte[] GetWaveFormData(string windTurbineId, string _measLocID, string _waveDefID, EnumWaveFormType _WFType, DateTime _acqTime);
        /// <summary>
        /// 根据机组id,采集时间获取波形数据
        /// </summary>
        /// <param name="windTurbineId"></param>
        /// <param name="_acqTime"></param>
        /// <returns></returns>
        byte[] GetRotSpdWaveData(string windTurbineId, System.DateTime _acqTime);

        /// <summary>
        /// 获取工况 ADD ZXT
        /// </summary>
        /// <param name="_waveDataPath">波形存储路径</param>
        /// <param name="iWknCode">工况代码</param>
        /// <returns></returns>
        byte[] GetWkWaveBytes(string _waveDataPath, int iWknCode);
    }
}
