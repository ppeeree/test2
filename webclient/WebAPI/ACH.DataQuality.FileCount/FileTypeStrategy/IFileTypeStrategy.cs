using System.IO;

namespace ACH.DataQuality.FileCount.FileTypeStrategy
{
    /// <summary>
    /// 不同文件后缀的文件统计策略
    /// </summary>
    public interface IFileTypeStrategy
    {
        /// <summary>
        /// 更新统计信息
        /// </summary>
        /// <param name="statistics"></param>
        /// <param name="file"></param>
        void UpdateStatistics(CMSDataStatistics statistics, FileInfo file);
    }
}
