using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{

    /// <summary>
    /// 机组的CMS数据，可以代表BVM和CMS，但是不能两个同时在一个结构里出现
    /// </summary>
    [DataContract]
    [Serializable]
    public class WindTurbineCMSData
    {
        public WindTurbineCMSData()
        {
            DAQEventDataList = new List<DAQEventData>();
        }

        /// <summary>
        /// 风电场ID
        /// </summary>
        [DataMember(Order=1)]
        public string WindParkID
        {
            get;
            set;
        }


        /// <summary>
        /// 机组ID
        /// </summary>
        [DataMember(Order = 2)]
        public string WindTurbineID
        {
            get;
            set;
        }



        /// <summary>
        /// 采集的波形数据、特征值数据、DAU监测状态
        /// </summary>
        [DataMember(Order = 3)]
        public List<DAQEventData> DAQEventDataList
        {
            get;
            set;
        }
        

        /// <summary>
        /// DAU监测数据
        /// </summary>
        [DataMember(Order = 4)]
        public RTAlarmStatus_DAU DAUMonitorData
        {
            get;
            set;
        }

        
        // 传动链报警状态
        [DataMember(Order = 5)]
        public AlarmStatus_Turbine WindTurbineAlarmStatus
        {
            get;
            set;
        }

        /// <summary>
        /// 油液数据
        /// </summary>
        [DataMember(Order = 6)]
        public List<OilData> OilDataList
        {
            get;
            set;
        }
        //[DataMember(Order = 6)]
        //public AlarmStatus_TurbineFreeze WindTurbineFreezeAlarmStatus
        //{
        //    get;
        //    set;
        //}

    }



    /// <summary>
    /// 一次同步采集得到的数据
    /// </summary>
    [DataContract]
    [Serializable]
    public class DAQEventData
    {
        public DAQEventData()
        {
            DAUDcDataList = new List<SensorDCData>();
        }

        /// <summary>
        /// 波形数据
        /// </summary>
        [DataMember(Order = 1)]
        public MeasEvent_Wave EventWaveData
        {
            get;
            set;
        }

        /// <summary>
        /// 特征值数据
        /// </summary>
        [DataMember(Order = 2)]
        public MeasEvent_EigenValue EventEVData
        {
            get;
            set;
        }

        
        /// <summary>
        /// 直流分量表
        /// </summary>
        [DataMember(Order = 3)]
        public List<SensorDCData> DAUDcDataList
        {
            get;
            set;
        }
    }
}
