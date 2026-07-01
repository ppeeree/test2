using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [ProtoBuf.ProtoContract]
    public class EigenValueData_Vib : EigenValueData
    {
        /// <summary>
        /// 特征值Code 
        /// </summary>
        [DataMember(Order = 10)]
        [ProtoBuf.ProtoMember(10)]  
        public string EigenValueCode
        {
            get;
            set;
        }


        /// <summary>
        /// SamplingTime 采样时长
        /// </summary>
        [DataMember(Order = 11)]
        [ProtoBuf.ProtoMember(11)]  
        public int SamplingTime
        {
            get;
            set;
        }
    }
}
