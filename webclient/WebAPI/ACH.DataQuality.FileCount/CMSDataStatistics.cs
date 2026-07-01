using System;
using System.Collections.Generic;
using System.Text;

namespace ACH.DataQuality.FileCount
{
    /// <summary>
    /// 统计文件数类
    /// </summary>
    public class CMSDataStatistics
    {
        /// <summary>
        /// 设备ID
        /// </summary>
        public string DeviceID { get; set; }
        /// <summary>
        /// 统计时间（年月日）
        /// </summary>
        public DateTime Stime { get; set; }
        /// <summary>
        /// CMS短波形数量
        public int CMSWaveCount { get; set; }
        /// <summary>
        /// CMS长波形数量
        public int CMSLWaveCount { get; set; }
        /// <summary>
        /// CMS不含波形文件数量
        /// </summary>
        public int CMSFCount { get; set; }
        /// <summary>
        /// BMS短波形数量
        /// </summary>
        public int BMSWaveCount { get; set; }
        /// <summary>
        /// BMS仅特征值文件数量
        /// </summary>
        public int BMSFCount { get; set; }
        /// <summary>
        /// TMS短波形数量
        /// </summary>
        public int TMSWaveCount { get; set; }
        /// <summary>
        /// TMS长波形数量
        /// </summary>
        public int TMSFWaveCount { get; set; }
    }
}
