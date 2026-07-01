namespace ACH.CMSWebClient.ControllerModel.Analysis
{
    /// <summary>
    ///  转速
    /// </summary>
    public class RotorFrequencyVO
    {
        public string RotorFrequencyType { get; set; }
        public List<RotorFrequencyDoubling> RotorFrequencyDoubling { get; set; }
    }
    public class RotorFrequencyDoubling
    {
        public string MultipleLabel { get; set; }
        public double FrequencyValue { get; set; }
    }

}
