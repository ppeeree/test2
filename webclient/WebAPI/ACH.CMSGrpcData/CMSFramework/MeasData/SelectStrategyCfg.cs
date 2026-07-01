using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    /// <summary>
    /// 功率范围
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class PowerRange
    {
        [DataMember]
        public string windParkID;

        [DataMember]
        public string rangeID;

        [DataMember]
        public float lowerLimit;

        [DataMember]
        public float upperLimit;        
    }
    /// <summary>
    /// 转速范围
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class SpeedRange
    {
        [DataMember]
        public string windParkID;

        [DataMember]
        public string rangeID;

        [DataMember]
        public float lowerLimit;

        [DataMember]
        public float upperLimit;       
    }
    /// <summary>
    /// 上传诊断中心波形数据筛选机制配置
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class SelectStrategyCfg
    {
        [DataMember]
        public string WindParkID;

        /// <summary>
        /// 服务器IP？
        /// </summary>
        [DataMember]
        public string SerIP;

        /// <summary>
        /// 上传间隔时间？
        /// </summary>
        [DataMember]
        public string UploadInterval;

        [DataMember]
        public List<PowerRange> powerRangeList;

        [DataMember]
        public List<SpeedRange> speedRangeList;        
    }
}
