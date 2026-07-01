namespace ACH.CMSWebClient.ControllerModel.DownloadTask
{
    /// <summary>
    /// 获取诊断数据打包的文件信息
    /// </summary>
    public class ZipFileInfoDTO
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件时间
        /// </summary>
        public string FileTime { get; set; }
        /// <summary>
        /// 打包的机组个数
        /// </summary>
        public string TurbineNum { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public string FileMemory { get; set; }
    }
}
