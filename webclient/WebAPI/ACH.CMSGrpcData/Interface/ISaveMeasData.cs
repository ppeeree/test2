using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMSFramework.BusinessEntity;

namespace WindCMS.IAnalyzerDomain
{
    /// <summary>
    /// 以测量事件树为操作对象，保存测量数据 
    /// </summary>
    public interface ISaveMeasData
    {
          
        /// <summary>
        /// 移动到历史
        /// </summary>
        /// <param name="_measEvtList"></param>
        void MoveMeasEventWFTreeInHisDB(string _windParkId, List<MeasEvent_Wave> _measEvtList);


        /// <summary>
        /// 保存测量事件树结构
        /// </summary>
        /// <param name="_measEvtTree"></param>
        /// <param name="_db"></param>
        void SaveMeasEventWFTree(MeasEvent_Wave _measEvtTree, EnumDataSource _db);


    }
}
