using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ACH.DataQuality.FileCount.FileTypeStrategy
{
    /// <summary>
    /// 金风格式文件计数策略
    /// </summary>
    public class GWFileTypeStrategy
    {

        /// <summary>
        /// CMS文件类型策略
        /// </summary>
        public class CMSFileTypeStrategy : IFileTypeStrategy
        {
            public void UpdateStatistics(CMSDataStatistics statistics, FileInfo file)
            {
                string fileName = file.Name;
                if (fileName.EndsWith("L.cms"))
                {
                    statistics.CMSLWaveCount++;
                }
                else if (fileName.EndsWith("F.cms"))
                {
                    statistics.CMSFCount++;
                }
                else
                {
                    statistics.CMSWaveCount++;
                }
            }
        }

        /// <summary>
        /// BMS文件类型策略
        /// </summary>
        public class BMSFileTypeStrategy : IFileTypeStrategy
        {
            public void UpdateStatistics(CMSDataStatistics statistics, FileInfo file)
            {
                string fileName = file.Name;
                if (fileName.EndsWith("F.bms"))
                {
                    statistics.BMSFCount++;
                }
                else
                {
                    statistics.BMSWaveCount++;
                }
            }
        }

        /// <summary>
        /// TMS文件类型策略
        /// </summary>
        public class TMSFileTypeStrategy : IFileTypeStrategy
        {
            public void UpdateStatistics(CMSDataStatistics statistics, FileInfo file)
            {
                string fileName = file.Name;
                if (fileName.EndsWith("F.tms"))
                {
                    statistics.TMSFWaveCount++;
                }
                else
                {
                    statistics.TMSWaveCount++;
                }
            }
        }
    }
}
