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
    /// 权限实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class Function
    {
        public Function()
        {
           // this.Roles = new List<Role>(); 
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///权限ID
        /// </summary>
        [DataMember]
        public string FunctionID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///权限名称
        /// </summary>
        [DataMember]
        public string FunctionName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///模块编号
        /// </summary>
        [DataMember]
        public string ModuleID
        {
            get;
            set;
        }


        //[IgnoreDataMember]
        //public Module SysModule { get; set; }
        //[IgnoreDataMember]
        //public ICollection<Role> Roles { get; set; }

    }
}
