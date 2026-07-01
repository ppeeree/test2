using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 用户实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class User
    {
        public User()
        {
            //Roles = new List<Role>();
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///用户ID
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public string UserID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///用户名称
        /// </summary>
        [DataMember]
        public string UserName
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///密码
        /// </summary>
        [DataMember]
        public string PassWord
        {
            get;
            set;
        }
        /// <summary>
        /// 邮箱
        /// </summary>
        [DataMember]
        public string Email
        { 
            get; 
            set;
        }
        /// <summary>
        /// 电话
        /// </summary>
        [DataMember]
        public string Phone 
        { 
            get;
            set; 
        }
        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///模块集合
        /// </summary>
        [DataMember]
        public List<string> ModuleIDs
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///权限编号集合
        /// </summary>
        [DataMember]
        public List<string> FunctionIDs
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        ///风电场编号集合
        /// </summary>
        [DataMember]
        public List<string> WindParkIDs
        {
            get;
            set;
        }
        
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 角色
        /// </summary>
        //[IgnoreDataMember]
        public Role UserRole
        {
            get;
            set;
        }


        //[IgnoreDataMember]
        public List<Role> Roles
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 用户状态：正常/失效
        /// </summary>
        [DataMember]
        public string UserState
        {
            get;
            set;
        }

    }
}
