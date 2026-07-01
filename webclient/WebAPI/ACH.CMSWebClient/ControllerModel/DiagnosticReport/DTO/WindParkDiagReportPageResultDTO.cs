namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO
{
    public class WindParkDiagReportPageResultDTO
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalCount { get; set; }
        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }
        /// <summary>
        /// 子项集合（分页数据）
        /// </summary>
        public List<WindParkDiagReportDTO> Children { get; set; }
    }

}
