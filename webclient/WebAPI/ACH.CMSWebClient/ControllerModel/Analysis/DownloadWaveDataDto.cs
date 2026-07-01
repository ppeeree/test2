namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 原始波形下载
    /// </summary>
    public class DownloadWaveDataDTO
    {
        public List<OriginalWaveformDataRequestDTO> originalWaveformDataRequestList { get; set; }
    }

    public class OriginalWaveformDataRequestDTO
    {
        public string acqTime { get; set; }
        public string? startAcqTime { get; set; }
        public string? endAcqTime { get; set; }
        public string measlocId { get; set; }
        public string waveDefId { get; set; }
        public string waveCategory { get; set; }
        public string? measName { get; set; }
        public string? filePath { get; set; }
    }
}
