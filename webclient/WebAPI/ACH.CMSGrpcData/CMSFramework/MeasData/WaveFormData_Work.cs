using CMSFramework.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    [DataContract(Namespace = "http://CMSFramework.BusinessEntity/")]
    public class WorkConditionWaveFormData : WaveFormData
    {
        [DataMember(Order = 18)]
        public EnumWorkCondition_ParamType Param_Type_Code
        {
            get;
            set;
        }

        [DataMember(Order = 19)]
        public int DAUChannelID
        {
            get;
            set;
        }

        [IgnoreDataMember]
        public float[] WorkConditionWaveData
        {
            get
            {
                if (WaveData == null)
                    return null;

                float[] fltData = new float[WaveData.Length / 4];
                Buffer.BlockCopy(WaveData, 0, fltData, 0, WaveData.Length);

                return fltData; 
            }
            set
            {
                byte[] data = new byte[value.Length * 4];
                Buffer.BlockCopy(value, 0, data, 0, data.Length);
                this.WaveData = data;
            }
        }
    }
}
