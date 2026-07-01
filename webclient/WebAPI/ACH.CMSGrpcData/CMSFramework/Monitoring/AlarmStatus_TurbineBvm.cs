using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    public class AlarmStatus_TurbineBvm
    {
        /// <summary>
        /// 机组ID
        /// </summary>
        public string WindTurbineID
        {
            get;
            set;
        }

        /// <summary>
        /// 最新报警时间
        /// </summary>
        public DateTime AlarmUpdateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 报警等级
        /// </summary>
        public EnumAlarmDegree AlarmDegree
        {
            get 
            {
                return (EnumAlarmDegree)StatusCode;
            }
            set
            {
                StatusCode = (short)value;
            }
        }

        /// <summary>
        /// 状态码，存储数据库改用此信息
        /// </summary>
        public Int16 StatusCode
        {
            get;
            private set;
        }
    }
}
