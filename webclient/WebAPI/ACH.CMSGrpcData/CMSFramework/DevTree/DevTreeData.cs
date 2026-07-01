using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 获取变更的设备数据
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class DevTreeData
    {
        public DevTreeData()
        {
            WindTurModelList = new List<WindTurbineModel>();
            WindParkList = new List<WindPark>();
            WindTurbineList = new List<WindTurbine>();
            WindTurbineComponentList = new List<WindTurbineComponent>();
            MeasLoc_VibList = new List<MeasLoc_Vib>();
            MeasLoc_RotList = new List<MeasLoc_RotSpd>();
            MeasLoc_SVMList = new List<MeasLoc_SVM>();
            MeasLoc_ProcessList = new List<MeasLoc_Process>();
        }
        /// <summary>
        /// 机组型号列表
        /// </summary>
        [DataMember]
        public List<WindTurbineModel> WindTurModelList
        {
            get;
            set;
        }


        /// <summary>
        /// 风电场对象列表
        /// </summary>
        [DataMember]
        public List<WindPark> WindParkList
        {
            get;
            set;
        }


        /// <summary>
        /// 所有机组对象列表
        /// </summary>
        [DataMember]
        public List<WindTurbine> WindTurbineList
        {
            get;
            set;
        }

        /// <summary>
        /// 所有部件对象列表
        /// </summary>
        [DataMember]
        public List<WindTurbineComponent> WindTurbineComponentList
        {
            get;
            set;
        }

        /// <summary>
        /// 所有振动测量位置对象列表
        /// </summary>
        [DataMember]
        public List<MeasLoc_Vib> MeasLoc_VibList
        {
            get;
            set;
        }

        /// <summary>
        /// 所有转速测量位置对象列表
        /// </summary>
        [DataMember]
        public List<MeasLoc_RotSpd> MeasLoc_RotList
        {
            get;
            set;
        }


        /// <summary>
        /// 所有SVM测量位置对象列表
        /// </summary>
        [DataMember]
        public List<MeasLoc_SVM> MeasLoc_SVMList
        {
            get;
            set;
        }

        /// <summary>
        /// 所有过程量测量位置对象列表
        /// </summary>
        [DataMember]
        public List<MeasLoc_Process> MeasLoc_ProcessList
        {
            get;
            set;
        }
    }

}
