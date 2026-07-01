using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 波形定义测量事件
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasEvent_Wave : MeasEvent
    {
        public MeasEvent_Wave()
        {
            WaveFormDataList = new List<VibWaveFormData>();
            WaveFormData_SVMList = new List<SVMWaveFormData>();
            WaveFormData_RotSpdList = new List<RotSpeedWaveData>();

            WorkingConDataList = new List<WorkingConditionData>();

            WaveFormData_WorkList = new List<WorkConditionWaveFormData>();

            AlarmDegree = EnumAlarmDegree.AlarmDeg_Unknown;
            Data_Qual_Type = EnumDataQualityType.NotAcquired;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 转速波形个数
        /// </summary>
        [DataMember]
        public int RotSpdWaveFromNum
        {
            get;
            set;
        }

        /// <summary>
        /// 晃度波形个数
        /// </summary>
        [DataMember]
        public int SVMWaveFormNum
        {
            get;
            set;
        }

        /// <summary>
        /// 波形个数
        /// </summary>
        [DataMember]
        public int WaveFormNum
         {
             get;
             set;
         }

        /// <summary>
        /// 波形路径
        /// </summary>
        [DataMember]
        public string WaveDataPath
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------
        // add by steel @ 2015-8-7
        /// <summary>
        /// 振动波形列表
        /// </summary>
        [DataMember]
        public List<VibWaveFormData> WaveFormDataList
         {
             get;
             set;
         }

        
         //-----------------------------------------------------------------------
         // add by steel @ 2015-8-7
        /// <summary>
        /// 晃度仪波形列表
        /// </summary>
        [DataMember]
        public List<SVMWaveFormData> WaveFormData_SVMList
         {
             get;
             set;
         }


        /// <summary>
        /// 转速数据
        /// </summary>
        [DataMember]
        public List<RotSpeedWaveData> WaveFormData_RotSpdList
         {
             get;
             set;
         }

        /// <summary>
        /// 工况波形
        /// </summary>
        [DataMember]
        public List<WorkConditionWaveFormData> WaveFormData_WorkList
        {
            get;
            set;
        }

        /// <summary>
        /// 兼容原来系统
        /// </summary>
        [IgnoreDataMember]
        public RotSpeedWaveData WaveFormData_RotSpd
        {
            get
            {
                if (WaveFormData_RotSpdList.Count > 0)
                {
                    return WaveFormData_RotSpdList[0];
                }
                return null;
            }
            set
            {
                WaveFormData_RotSpdList.Clear();

                if (value != null)
                {
                    WaveFormData_RotSpdList.Add(value);
                } 
            }
        }
    
    }
}
