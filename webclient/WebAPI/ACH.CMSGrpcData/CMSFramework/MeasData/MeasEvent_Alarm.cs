using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 测量事件 报警
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class MeasEvent_Alarm : MeasEvent_Wave
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AlarmTime 报警时间，并未被存储
        /// </summary>
        [DataMember]
        public DateTime AlarmTime
        {
            get;
            set;
        }
    }
}
