using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using CMSFramework.BusinessEntity;

namespace CMSFramework.BusinessEntity
{
    //-----------------------------------------------------------------------------------------------------------------------
    // author:GuoKaile
    // create:2012-01-10
    /// <summary>
    /// 机组型号实体
    /// modified by whr 2014-1-8 “XmlInclude”——>模板对象序列化为Xml文件
    /// </summary>
    [DataContract(Namespace = "http://WTCMSLive.Manager.Entity/")]
    public class WindTurbineModel
    {
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组型号
        /// </summary>
        [DataMember]
        public string TurbineModel
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组提供商
        /// </summary>
        [DataMember]
        public string Manufactory
        {
            get;
            set;
        }


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组额定功率
        /// </summary>
        [DataMember]
        public float RatedPower
        {
            get;
            set;
        }

        

        #region Description 描述
        /// <summary>
        /// Description 描述
        /// </summary>
        [DataMember]
        public string Description
        {
            get;
            set;
        }
        #endregion 


        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 叶片个数
        /// </summary>
        [DataMember]
        public int BladeNum
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组结构图
        /// </summary>
        [DataMember]
        public byte[] StructureDiagram
        {
            get;
            set;
        }


 
        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// 机组类型
        /// </summary>
        [DataMember]
        public EnumTurModelStructureType StructureType
        {
            get;
            set;
        }

       /*
        //-----------------------------------------------------------------------------------------------------------------------
        // Author: GuoKaile
        // Time: 2012-04-11
        /// <summary>
        /// 机型部件型号列表
        /// </summary>
        [DataMember]
        public List<ComponentModel> ComponentList
        {
            get;
            set;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // Author: Yangming
        // Time: 2012-04-17
        /// <summary>
        /// 机型故障频率
        /// </summary>
        [DataMember]
        public List<WindTurbModelFaultFreq> WindTurbModelFaultFreqList 
        { 
            get; 
            set; 
        }
        */

        //-----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return TurbineModel;
        }

        //-----------------------------------------------------------------------------------------------------------------------
        // Author: ZhangMai
        // Time: 2012-05-15
        /// <summary>
        /// 版本号
        /// </summary>
        [DataMember]
        public string VersionNumber
        {
            get;
            set; 
        }
    }
}
