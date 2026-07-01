namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    /// 轴承诊断
    /// </summary>
    public class BearFaultFrequencyTypeVO
    {
        public string BearFaultFrequencyType { get; set; }
        public List<BearFaultFrequencyDoubling> BearFaultFrequencyDoubling { get; set; }
    }
    public class BearFaultFrequencyDoubling
    {
        public string MultipleLabel { get; set; }
        public double FrequencyValue { get; set; }
    }
}
