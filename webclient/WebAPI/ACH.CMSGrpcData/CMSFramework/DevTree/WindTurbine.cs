using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-07
    /// <summary>
    /// 机组实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class WindTurbine
    {
        public WindTurbine()
        {
            TurComponentList = new List<WindTurbineComponent>();
            ProcessMeasLocList = new List<MeasLoc_Process>();
            MeasLocSVMList = new List<MeasLoc_SVM>();
            VibMeasLocList = new List<MeasLoc_Vib>();
            DevMeasLocRotSpds = new List<MeasLoc_RotSpd>();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember]
        public string WindTurbineID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindParkID 风电场ID
        /// </summary>
        [DataMember]
        public string WindParkID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineCode 机组编号
        /// </summary>
        [DataMember]
        public string WindTurbineCode
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineName 机组名称
        /// </summary>
        [DataMember]
        public string WindTurbineName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineModel 机组型号
        /// </summary>
        [DataMember]
        public string WindTurbineModel
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// OperationalDate 投运日期
        /// </summary>
        [DataMember]
        public DateTime OperationalDate
        {
            get;
            set;
        }
        /// <summary>
        /// 并网转速，为了和之前的系统兼容
        /// </summary>
        [DataMember]
        public int MinWorkingRotSpeed
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        // author:lidan
        // create:2011-06-21
        /// <summary>
        /// 机组额定功率
        /// </summary>
        [IgnoreDataMember]
        [Obsolete("Use WTurbineModel Property.", false)]
        public float RatedPower
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:lidan
        // create:2011-06-20
        /// <summary>
        /// 机组型号
        /// </summary>
        [IgnoreDataMember]
        public WindTurbineModel WTurbineModel
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan 
        // time: 2011-09-22
        /// <summary>
        /// 部件列表
        /// </summary>
        [IgnoreDataMember]
        public List<WindTurbineComponent> TurComponentList
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan 
        // time: 2011-09-26
        /// <summary>
        /// 工况测量位置列表 No Ef Mapping
        /// </summary>
        [IgnoreDataMember]
        public List<MeasLoc_Process> ProcessMeasLocList
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan 
        // time: 2011-09-23
        /// <summary>
        /// 机组转速测量位置 No Ef Mapping
        /// </summary>
        [IgnoreDataMember]
        public MeasLoc_RotSpd RotSpdMeasLoc
        {
            get
            {
                if (DevMeasLocRotSpds.Count > 0)
                {
                    return DevMeasLocRotSpds[0];
                }
                return null;
            }
            set
            {
                DevMeasLocRotSpds.Clear();

                if (value != null)
                {
                    DevMeasLocRotSpds.Add(value);
                }
            }
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile 
        // time: 2011-08-24
        public override string ToString()
        {
            return this.WindTurbineName;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:ZhangMai
        // create:2013-05-17
        /// <summary>
        /// 晃度仪测量位置列表No Ef Mapping
        /// </summary>
        [IgnoreDataMember]
        public List<MeasLoc_SVM> MeasLocSVMList
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan 
        // time: 2011-09-22
        /// <summary>
        /// 振动测量位置列表 No Ef Mapping
        /// </summary>
        [IgnoreDataMember]
        public List<MeasLoc_Vib> VibMeasLocList
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2015-10-16
        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public WindPark DevWindPark
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2015-10-16
        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public List<MeasLoc_RotSpd> DevMeasLocRotSpds
        {
            get;
            set;
        }

    }
}
