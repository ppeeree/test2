using ACH.Helper.Comparer;

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    /// <summary>
    /// 分析记录树结构DTO
    /// </summary>
    [Serializable]
    public class AnalyzerRecordTreeDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type
        {
            get
            {
                return "Windturbine";
            }
        }
        /// <summary>
        /// 部件信息列表
        /// </summary>
        public List<CompTreeDTO> Children { get; set; }
    }
    /// <summary>
    /// 部件信息DTO
    /// </summary>
    [Serializable]
    public class CompTreeDTO : ISortable
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Type { get { return "MeaslocName"; } }
        /// <summary>
        /// 测量点信息列表
        /// </summary>
        public List<MeasurementTreeDTO> Children { get; set; }

        public string GetSortableName() => Name ?? string.Empty;
    }
    /// <summary>
    /// 测量点信息DTO
    /// </summary>
    [Serializable]
    public class MeasurementTreeDTO : ISortable
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string Type { get { return "Measurement"; } }
        /// <summary>
        /// 分析记录信息列表
        /// </summary>
        public List<AnalyzerImageTreeDTO> Children { get; set; }

        public string GetSortableName() => Name ?? string.Empty;
    }
    /// <summary>
    /// 分析记录信息DTO
    /// </summary>
    [Serializable]
    public class AnalyzerImageTreeDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get { return "AnalyzerRecord"; } }
        /// <summary>
        /// 分析记录信息列表
        /// </summary>
        public List<RecordTreeDTO> Children { get; set; }

    }
    /// <summary>
    /// 分析记录信息DTO
    /// </summary>
    [Serializable]
    public class RecordTreeDTO
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EigenValueId { get; set; }
        public double SampleDataSpeed { get; set; }
        public string Image { get; set; }
    }

}
