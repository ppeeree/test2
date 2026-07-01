using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMSFramework.BusinessEntity
{
    public static class EvpWaveDefDescHelper
    {
        /// <summary>
        /// 编码滤波器为描述
        /// </summary>
        /// <param name="filerFreq"></param>
        /// <returns></returns>
        public static string EncodeEvpWaveDefDesc(float filerFreq)
        {
            return string.Format("HP={0:F0}", filerFreq);
        }


        /// <summary>
        /// 转换为用户友好型描述
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static string GetUserFriendlyEvpWaveDefDesc(string desc)
        {
            return desc.Replace("HP=", "高通：");
        }


        /// <summary>
        /// 解码出滤波器的数值
        /// </summary>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static int DecodeEvpFilter(string desc)
        {
            return int.Parse(desc.Replace("HP=", ""));
        }
    }
}
