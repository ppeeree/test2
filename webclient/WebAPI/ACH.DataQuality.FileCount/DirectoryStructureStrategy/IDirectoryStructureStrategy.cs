using ACH.DataQuality.FileCount.FileTypeStrategy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ACH.DataQuality.FileCount.DirectoryStructureStrategy
{
    /// <summary>
    /// 不同目录结构解析策略
    /// </summary>
    public interface IDirectoryStructureStrategy
    {
        /// <summary>
        /// 判断当前策略是否能处理该目录
        /// </summary>
        bool CanHandle(DirectoryInfo directory, DateTime statisticsDate);

        /// <summary>
        /// 解析目录并获取统计结果
        /// </summary>
        List<CMSDataStatistics> Parse(DirectoryInfo directory, DateTime statisticsDate, Dictionary<string, IFileTypeStrategy> fileTypeStrategies);
    }
}
