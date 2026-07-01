using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:steel
    // create:2011-06-08
    /// <summary>
    /// 波形数据实体
    /// </summary>
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    [KnownType(typeof(WorkingConditionData))]
    [KnownType(typeof(SVMWaveFormData))]
    [KnownType(typeof(VibWaveFormData))]
    [ProtoBuf.ProtoInclude(20, typeof(VibWaveFormData))]
    [ProtoBuf.ProtoInclude(21, typeof(SVMWaveFormData))]
    public class WaveFormData
    {
        public WaveFormData()
        {
            this.Data_Qual_Type = EnumDataQualityType.NotAcquired;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasDefinitionID 采集定义ID
        /// </summary>
        [DataMember(Order = 1)]
        public string MeasDefinitionID
        {
            get;
            set;
        }
        

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindTurbineID 机组ID
        /// </summary>
        [DataMember(Order = 2)]
        public string WindTurbineID
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// MeasLocationID 测量位置ID
        /// </summary>
        [DataMember(Order = 3)]
        public string MeasLocationID
        {
            get;
            set;
        }       

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// AcquisitionTime 采集时间
        /// </summary>
        [DataMember(Order = 4)]
        public DateTime AcquisitionTime
        {
            get;
            set;
        }
        

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WaveDefinitionID 波形定义ID
        /// </summary>
        [DataMember(Order = 5)]
        public string WaveDefinitionID
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// OutPowerBandCode 工况级别
        /// </summary>
        //[DataMember]
        //public string OutPowerBandCode
        //{
        //    get { return outPowerBandCode; }
        //    set { outPowerBandCode = value; }
        //}



        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WindParkID 风电场ID
        /// </summary>
        [DataMember(Order = 6)]
        public string WindParkID
        {
            get;
            set;
        }

        


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// SampleRate 采样频率
        /// </summary>
        [DataMember(Order = 7)]
        public double SampleRate
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WaveLength 数据长度
        /// </summary>
        [DataMember(Order = 8)]
        public int WaveLength
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ConvertCoefficient 转换系数
        /// </summary>
        [DataMember(Order = 9)]
        public double ConvertCoefficient
        {
            get;
            set;
        }
        

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// WaveData 波形数据
        /// </summary>
        [DataMember(Order = 10)]
        public byte[] WaveData
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ComponentName 部件名称
        /// </summary>
        [DataMember(Order = 11)]
        public string ComponentName
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// LocationSection 测量位置截面
        /// </summary>
        [DataMember(Order = 12)]
        public string LocationSection
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// LocationOrientation 测量位置方向
        /// </summary>
        [DataMember(Order = 13)]
        public string LocationOrientation
        {
            get;
            set;
        }

        // add by steel @ 2015-8-20
        /// <summary>
        /// 波形定义描述
        /// </summary>
        [DataMember(Order = 14)]
        public string WaveDefDescription
        {
            get;
            set;
        }


        /// <summary>
        /// 波形路径
        /// </summary>
         [DataMember(Order = 15)]
        public string WaveDataPath
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2011-07-06
        /// <summary>
        /// 测量定义类型
        /// </summary>
        [DataMember(Order = 16)]
         public EnumWaveFormType WaveformType
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        // author:steel
        // create:2011-07-21
        /// <summary>
        /// 数据质量
        /// </summary>
        [DataMember(Order = 17)]
        public EnumDataQualityType Data_Qual_Type
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // author: GuoKaile
        // create: 2012-08-31
        /// <summary>
        /// 工况数据
        /// </summary>
        [IgnoreDataMember]
        [Obsolete("Only VibAnalyzer Use", false)]
        public List<WorkingConditionData> WorkConDataList
        {
            get;
            set;
        }



        /// <summary>
        /// EF Mapping.
        /// </summary>
        [IgnoreDataMember]
        public MeasEvent_Wave WFDataMeasEvent
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 构造波形数据对象
        /// </summary>
        /// <param name="_measDefType"></param>
        /// <returns></returns>
        //public static WaveFormData ConstructWaveFormData(EnumWaveFormType _measDefType)
        //{
        //    WaveFormData waveFormObj = null;
        //    switch (_measDefType)
        //    {
        //        case EnumWaveFormType.MDF_Time:
        //            waveFormObj = new WaveFormData_Time();
        //            break;

        //        case EnumWaveFormType.MDF_Envelope:
        //            waveFormObj = new WaveFormData_Envelope();
        //            break;

        //        case EnumWaveFormType.MDF_Order:
        //            waveFormObj = new WaveFormData_Order();
        //            break;

        //        case EnumWaveFormType.MDF_OrderEnvelope:
        //            waveFormObj = new WaveFormData_OrderEnv();
        //            break;
        //        default:
        //            break;
        //    }


        //    return waveFormObj;
        //}
    }
}
