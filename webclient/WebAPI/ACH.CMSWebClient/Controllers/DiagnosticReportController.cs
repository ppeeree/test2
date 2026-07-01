using ACH.CMSWebClient.Common;
using ACH.CMSWebClient.ControllerImplement.DiagnosticReport;
using ACH.CMSWebClient.ControllerModel;
using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DBRepository.App;
using ACH.Helper.ApiReponse;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;

namespace ACH.CMSWebClient.Controllers
{
    /// <summary>
    /// 诊断报告接口
    /// </summary>
    [Route("NetApi/[controller]")]
    [ApiController]
    [ExceptionFilterAttribute]
    public class DiagnosticReportController : ControllerBase
    {
        private readonly CreateReponse _createReponse = new CreateReponse();
        private DiagnosticAnalyzerRecord diagnosticAnalyzerRecord;
        private DiagnosticConclusion diagnosticConclusion;
        private DiagnosticReport diagnosticReport;
        private IReportRepository _diagnosticReportRepository = new ReportRepository();
        private DiagnosticReportExport diagnosticReportExport;
        private DiagnosticTurbine diagnosticTurbine;

        public DiagnosticReportController(IConfiguration _configuration)
        {
            diagnosticAnalyzerRecord = new DiagnosticAnalyzerRecord(_configuration);
            diagnosticConclusion = new DiagnosticConclusion(_configuration);
            diagnosticReport = new DiagnosticReport(_configuration);
            diagnosticReportExport = new DiagnosticReportExport(_configuration);
            diagnosticTurbine = new DiagnosticTurbine(_configuration);
        }

        /// <summary>
        /// 获取默认诊断报告数据模型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDefaultDiagResult")]
        public IActionResult GetDefaultDiagResult()
        {
            return _createReponse.CreateResponse(diagnosticConclusion.GetDefaultDiagResult());
        }
        /// <summary>
        /// 获取默认诊断报告运行建议数据模型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDefaultRuningAdvice")]
        public IActionResult GetDefaultRuningAdvice()
        {
            return _createReponse.CreateResponse(diagnosticConclusion.GetDefaultRuningAdvice());
        }
        /// <summary>
        /// 保存分析仪记录和诊断结论
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        [HttpPost("SaveAnalyzerRecordAndDiagnosisConclusion")]
        public IActionResult SaveAnalyzerRecordAndDiagnosisConclusion([FromBody] SaveAnalyzerRecordDTO record)
        {
            diagnosticAnalyzerRecord.SaveAnalyzerRecord(record);
            return _createReponse.CreateResponse(true);
        }
        /// <summary>
        /// 获取分析仪记录树数据模型
        /// </summary>
        /// <param name="windID">机组ID</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        [HttpGet("GetAnalyzerRecordTree")]
        public IActionResult GetAnalyzerRecordTree([FromQuery] string windturbineId, DateTime startTime, DateTime endTime)
        {
            return _createReponse.CreateResponse(diagnosticAnalyzerRecord.GetAnalyzerRecordTree(windturbineId, startTime, endTime));
        }
        /// <summary>
        /// 获取分析记录图谱数据 - 暂不使用
        /// </summary>
        /// <param name="recordID"></param>
        /// <returns></returns>
        [HttpGet("GetAnalyzerRecordImage")]
        public IActionResult GetAnalyzerRecordImage([FromQuery] int recordID)
        {
            return _createReponse.CreateResponse(diagnosticAnalyzerRecord.GetAnalyzerRecordImage(recordID));
        }
        /// <summary>
        /// 获取诊断结论数据模型
        /// </summary>
        /// <param name="windturbineId"></param>
        /// <returns></returns>
        [HttpGet("GetDiagnosisConclusion")]
        public IActionResult GetDiagnosisConclusion([FromQuery] string windturbineId)
        {
            return _createReponse.CreateResponse(diagnosticConclusion.GetWindturbineDiagnosisConclusion(windturbineId));
        }
        /// <summary>
        /// 获取最后一次分析仪记录描述和诊断结论数据模型
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLastAnalyzerDescAndDiagnosisConclusion")]
        public IActionResult GetLastAnalyzerDescAndDiagnosisConclusion([FromQuery] string windturbineId, string ImageType, string? eigenValueId, string? measlocId)
        {
            return _createReponse.CreateResponse(diagnosticAnalyzerRecord.GetLastAnalyzerDescription(windturbineId, ImageType, eigenValueId, measlocId));
        }
        /// <summary>
        /// 获取诊断报告数据模型
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        [HttpGet("GetDiagnosisReport")]
        public IActionResult GetDiagnosisReport([FromQuery] string reportGuid)
        {
            return _createReponse.CreateResponse(diagnosticReport.GetDiagnosisReport(reportGuid));
        }
        /// <summary>
        /// 删除诊断报告数据模型
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        [HttpPost("DeleteDiagnosisReport")]
        public IActionResult DeleteDiagnosisReport([FromBody] DeleteReportDTO reportDTO)
        {
            _diagnosticReportRepository.DeleteDiagnosisReport(reportDTO.ReportGuid);
            return _createReponse.CreateResponse(true);
        }
        /// <summary>
        /// 保存诊断报告数据模型
        /// </summary>
        /// <param name="diagnosis"></param>
        /// <returns></returns>
        [HttpPost("SaveDiagnosisReport")]
        public IActionResult SaveDiagnosisReport([FromBody] SaveDiagnosisReportDTO diagnosis)
        {
            return _createReponse.CreateResponse(diagnosticReport.SaveDiagnosisReport(diagnosis));
        }
        /// <summary>
        /// 获取风电场机组树数据模型
        /// </summary>
        /// <param name="windParkID"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet("GetWindParkTurbineTree")]
        public IActionResult GetWindParkTurbineTree([FromQuery] string windParkID, DateTime startTime, DateTime endTime)
        {
            return _createReponse.CreateResponse(diagnosticTurbine.GetWindParkTurbineTree(windParkID, startTime, endTime));
        }
        /// <summary>
        /// 获取风电场诊断报告汇总信息数据模型
        /// </summary>
        /// <param name="windParkID"></param>
        /// <returns></returns>
        [HttpGet("GetWindParkReportSummaryInfo")]
        public IActionResult GetWindParkReportSummaryInfo([FromQuery] string windParkID, DateTime startTime, DateTime endTime)
        {
            return _createReponse.CreateResponse(diagnosticReport.GetWindParkReportSummaryInfo(windParkID, startTime, endTime));
        }

        /// <summary>
        /// 保存风电场诊断报告数据模型
        /// </summary>
        /// <param name="report"></param>
        /// <returns></returns>
        [HttpPost("SaveWindParkDiagReport")]
        public IActionResult SaveWindParkDiagReport([FromBody] SaveWindParkDiagReportDTO report)
        {
            return _createReponse.CreateResponse(diagnosticReport.SaveWindParkDiagnosisReport(report));
        }

        /// <summary>
        /// 获取风电场诊断报告树数据模型
        /// </summary>
        /// <param name="windParkId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet("GetWindParkDiagnosisReportTree")]
        public IActionResult GetWindParkDiagnosisReportTree([FromQuery] string windParkId, DateTime startTime, DateTime endTime)
        {
            return _createReponse.CreateResponse(diagnosticReport.GetWindParkDiagnosisReportTree(windParkId, startTime, endTime));
        }
        /// <summary>
        /// 获取风电场诊断报告数据模型
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        [HttpGet("GetWindParkDiagReportDetail")]
        public IActionResult GetWindParkDiagReportDetail([FromQuery] string reportGuid)
        {
            return _createReponse.CreateResponse(diagnosticReport.GetWindParkDiagReportDetailDTO(reportGuid));
        }
        /// <summary>
        /// 删除风电场诊断报告数据模型
        /// </summary>
        /// <param name="reportDTO"></param>
        /// <returns></returns>
        [HttpPost("DeleteWindParkDiagnosisReport")]
        public IActionResult DeleteWindParkDiagnosisReport([FromBody] DeleteReportDTO reportDTO)
        {
            diagnosticReport.DeleteWindParkDiagnosisReport(reportDTO.ReportGuid);
            return _createReponse.CreateResponse(true);
        }
        /// <summary>
        /// 获取风电场诊断报告列表数据模型
        /// </summary>
        /// <param name="windParkId"></param>
        /// <param name="reportName"></param>
        /// <param name="reportTime"></param>
        /// <param name="offset"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet("GetWindParkReportList")]
        public IActionResult GetWindParkReportList([FromQuery] string? windParkId, string? reportName, string? reportTime, int offset, int pageSize)
        {
            return _createReponse.CreateResponse(diagnosticReport.GetWindParkReportList(windParkId, reportName, reportTime, offset, pageSize));
        }
        /// <summary>
        /// 导出风电场诊断报告数据模型-暂不使用
        /// </summary>
        /// <param name="reportGuid"></param>
        /// <returns></returns>
        [HttpGet("DownloadWindParkReport")]
        public IActionResult DownloadWindParkReport([FromQuery] string reportGuid)
        {
            var mimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            // 
            return File(diagnosticReportExport.GetStationReportStream(reportGuid, out string filename), mimeType, filename);
            // return FileSystem(diagnosticReportExport.DownloadWindParkDiagReport(reportGuid, out string filename), mimeType, filename);
        }

        /// <summary>
        /// 验证生成文档格式是否正确，能否用word打开
        /// </summary>
        /// <returns></returns>
        [HttpGet("TestOpenXMLDoc")]
        public IActionResult TestOpenXMLDoc()
        {
            using var doc = WordprocessingDocument.Open("D:\\donwload\\高本山风电场振动分析报告.docx", false);
            var validator = new DocumentFormat.OpenXml.Validation.OpenXmlValidator();
            var errors = validator.Validate(doc);
            foreach (var e in errors)
                Console.WriteLine($"路径:{e.Path}  错误:{e.Description}");

            return _createReponse.CreateResponse(errors);
        }
    }
}
