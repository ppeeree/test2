using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2011-06-08
    /// <summary>
    /// 模块实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class Module
    {
        public Module()
        {
            //Roles = new List<Role>();

            //ModuleFunctions = new List<Function>();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///模块ID
        /// </summary>
        [DataMember]
        public string ModuleID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///模块名称
        /// </summary>
        [DataMember]
        public string ModuleName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 模块内的功能列表
        /// </summary>
        //[IgnoreDataMember]
        //public List<Function> ModuleFunctions
        //{
        //    get;
        //    set;
        //}

        //[IgnoreDataMember]
        //public List<Role> Roles { get; set; }

    }
}
