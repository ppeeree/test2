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
    /// 报警处理日志
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class AlarmEventHandleLog
    {
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
        // author:lidan
        // create:2011-07-04
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
        /// AlarmTime 报警时间
        /// </summary>
        [DataMember]
        public DateTime AlarmTime
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// LogType 日志类型
        /// </summary>
        [DataMember]
        public string LogType
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// HandleUser 处理人
        /// </summary>
        [DataMember]
        public string HandleUser
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WorkDescription 处理内容
        /// </summary>
        [DataMember]
        public string WorkDescription
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// HandleTime 处理时间
        /// </summary>
        [DataMember]
#if NET40
        [System.ComponentModel.DataAnnotations.Key]
#endif
        public DateTime HandleTime
        {
            get;
            set;
        }
    }
}
