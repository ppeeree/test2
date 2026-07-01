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
    /// 采集位置实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasLoc_Vib : MeasLocation
    {        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentID 部件ID
        /// </summary>
        [DataMember]
        public string ComponentID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentName 部件名称
        /// </summary>
        //[DataMember]
        //public string ComponentName
        //{
        //    get;
        //    set;
        //}
        

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SectionName 截面
        /// </summary>
        [DataMember]
        public string SectionName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Orientation 方向
        /// </summary>
        [DataMember]
        public string Orientation
        {
            get;
            set;
        }
                

        //-----------------------------------------------------------------------------------------------------------------------
        // author:lidan
        // create:2011-06-17
        /// <summary>
        /// TurnRatio 变速比
        /// </summary>
        [DataMember]
        public float GearRatio
        {
            get;
            set;
        }


        ////-----------------------------------------------------------------------------------------------------------------------
        //// author:lidan
        //// create:2011-06-17
        ///// <summary>
        ///// Speed 转速
        ///// </summary>
        //[DataMember]
        //[Obsolete("Has No use...", false)]
        //public string RotSpdMeasLocID
        //{
        //    get;
        //    set;
        //}


        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2015-10-16
        /// <summary>
        /// EF Mapping
        /// </summary>
        [IgnoreDataMember]
        public WindTurbineComponent DevTurComponent
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author: steel
        // create: 2011-09-09
        /// <summary>
        /// Dau通道
        /// </summary>
        //public DAUChannelV2 DauChannel
        //{
        //    get;
        //    set;
        //}
               

    }
}
