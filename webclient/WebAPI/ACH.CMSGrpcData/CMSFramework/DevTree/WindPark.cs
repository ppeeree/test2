using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2011-06-07
    /// <summary>
    /// 风电场实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class WindPark
    {
        public WindPark()
        {
            WindTurbineList = new List<WindTurbine>();
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///风电场唯一Id
        /// </summary>
        [DataMember]
        public string WindParkID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///风电场名称
        /// </summary>
        [DataMember]
        public string WindParkName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///风电场编号
        [DataMember]
        public string WindParkCode
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///联系电话
        /// </summary>
        [DataMember]
        public string ContactTel
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///联系人
        /// </summary>
        [DataMember]
        public string ContactMan
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///投运日期
        /// </summary>
        [DataMember]
        public DateTime OperationalDate
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///概况
        /// </summary>
        [DataMember]
        public string Description
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 风电场地址
        /// </summary>
        [DataMember]
        public string Address
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 风电场邮编
        /// </summary>
        [DataMember]
        public string PostCode
        {
            get;
            set;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///机组总数
        /// </summary>
        //[DataMember]
        //[Obsolete("no need this.", false)]
        //public int WTTotalNumber
        //{
        //    get;
        //    set;
        //}

        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile 
        // time: 2013-02-20
        /// <summary>
        /// 机组列表
        /// </summary>
        [IgnoreDataMember]
        public List<WindTurbine> WindTurbineList
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile 
        // time: 2011-08-24
        public override string ToString()
        {
            return this.WindParkName;
        }

    }
}
