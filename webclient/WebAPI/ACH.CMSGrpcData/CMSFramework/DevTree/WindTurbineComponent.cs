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
    /// 机组部件实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class WindTurbineComponent
    {
        public WindTurbineComponent()
        {
            MeasLocSVMList = new List<MeasLoc_SVM>();
            VibMeasLocList = new List<MeasLoc_Vib>();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentID 部件ID
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string ComponentID
        {
            get;
            set;
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
        /// name 部件类型
        /// </summary>
        [DataMember]
        public string ComponentName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentModel 部件型号
        /// </summary>
        [DataMember]
        public string ComponentModel
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// CompManufacturer 生产厂商
        /// </summary>
        [DataMember]
        public string CompManufacturer
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan
        // create: 2011-07-12
        /// <summary>
        /// OrderSeq 顺序号
        /// </summary>
        [DataMember]
        public int OrderSeq
        {
            get;
            set;
        }
        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile 
        // time: 2011-08-24
        public override string ToString()
        {
            return this.ComponentName;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        // author: lidan 
        // time: 2011-09-22
        /// <summary>
        /// 振动测量位置列表
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
        /// 晃度仪测量位置列表
        /// </summary>
        [IgnoreDataMember]
        public List<MeasLoc_SVM> MeasLocSVMList
        {
            get;
            set;
        }


        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public WindTurbine DevWindTurbine
        {
            get;
            set;
        }
    }
}
