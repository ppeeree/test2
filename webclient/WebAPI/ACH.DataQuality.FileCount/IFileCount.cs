using System;
using System.Collections.Generic;
using System.Text;

namespace ACH.DataQuality.FileCount
{
    public interface IFileCount
    {
        /// <summary>
        /// 统计文件数量
        /// </summary>
        /// <param name="path"></param>
        /// <param name="statisticsDate"></param>
        /// <returns></returns>
        public List<CMSDataStatistics> GetFileCount(string path, DateTime statisticsDate);

        /// <summary>
        /// 将统计结果写入CSV文件
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileName"></param>
        /// <param name="statisticsList"></param>
        public void WriteCSVFile(string directoryPath, string fileName, List<CMSDataStatistics> statisticsList);
    }
}
