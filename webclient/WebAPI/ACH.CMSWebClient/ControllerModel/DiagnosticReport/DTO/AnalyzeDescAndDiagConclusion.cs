namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class AnalyzeDescAndDiagConclusion
    {
        public AnalyzeDescAndDiagConclusion()
        {
            DiagnosisConclusions = new List<SaveDiagnosisConclusionDTO>();
            AnalyzeDesc = string.Empty;
        }
        public string AnalyzeDesc { get; set; }
        public List<SaveDiagnosisConclusionDTO> DiagnosisConclusions { get; set; }
    }
}
