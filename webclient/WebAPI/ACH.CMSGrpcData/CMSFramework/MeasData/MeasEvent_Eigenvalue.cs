using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 测量事件 特征值
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasEvent_EigenValue : MeasEvent
    {
        public MeasEvent_EigenValue()
        {
            EVDataList = new List<EigenValueData_Vib>();
            SVMEVDataList = new List<EigenValueData_SVM>();
            EV_TYLBList = new List<EigenValueData_TYLB>();
            WorkingConDataList = new List<WorkingConditionData>();
            AlarmDegree = EnumAlarmDegree.AlarmDeg_Unknown;
            Data_Qual_Type = EnumDataQualityType.NotAcquired;
        }


        /// <summary>
        /// 特征值个数
        /// </summary>
        [DataMember]
        public int EigenValueNum
        {
            get;
            set;
        }
        
        /// <summary>
        /// 晃度特征值个数
        /// </summary>
        [DataMember]
        public int SVMEigenValueNum
        {
            get;
            set;
        }


        /// <summary>
        /// 特征值数据列表
        /// </summary>
        [DataMember]
        public List<EigenValueData_Vib> EVDataList
        {
            get;
            set;
        }


        /// <summary>
        /// SVM特征值数据列表
        /// </summary>
        [DataMember]
        public List<EigenValueData_SVM> SVMEVDataList
        {
            get;
            set;
        }

        /// <summary>
        /// TYLB特征值数据列表
        /// </summary>
        [DataMember]
        public List<EigenValueData_TYLB> EV_TYLBList
        {
            get;
            set;
        }
    }
}
