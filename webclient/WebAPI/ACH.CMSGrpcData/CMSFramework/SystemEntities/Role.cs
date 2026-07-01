using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 角色实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class Role
    {
        public Role()
        {
            //this.Functions = new List<Function>();
            //Modules = new List<Module>();
            //Users = new List<User>();
        }
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 角色ID
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string  RoleID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 角色名称
        /// </summary>
        [DataMember]
        public string RoleName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 是否系统角色
        /// </summary>
        //[DataMember]
        //internal string IsSystemRoleStr
        //{
        //    get;
        //    set;
        //}



        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public bool IsSystemRole
        {
            get;
            set;
        }
        
        /// <summary>
        /// 获取权限描述
        /// </summary>
        /// <returns></returns>
        public string GetFunctionDescription()
        {
            string functionDescription = "";
            if (Functions != null)
            {
                foreach (Function item in Functions)
                {
                    functionDescription += item.FunctionName;
                    functionDescription += " ";
                }
            }

            return functionDescription;
        }


        /// <summary>
        /// 获取模块描述
        /// </summary>
        /// <returns></returns>
        public string GetModuleDescription()
        {
            string moduleDescription = "";
            if (Modules != null)
            {
                foreach (Module item in Modules)
                {
                    moduleDescription += item.ModuleName;
                    moduleDescription += " ";
                }
            }

            return moduleDescription;
        }



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 权限集合
        /// </summary>
        //[IgnoreDataMember]
        public List<Function> Functions
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///模块集合
        /// </summary>
        //[IgnoreDataMember]
        public List<Module> Modules
        {
            get;
            set;
        }

        //[IgnoreDataMember]
       // public List<User> Users { get; set; }


        public override string ToString()
        {
            return RoleName;
        }
    }
}
