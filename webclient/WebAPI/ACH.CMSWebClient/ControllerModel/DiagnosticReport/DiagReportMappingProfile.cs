using ACH.CMSWebClient.ControllerModel.DiagnosticReport.DTO;
using ACH.DataEntity.ReportData;
using AutoMapper;

namespace ACH.CMSWebClient.ControllerModel.DiagnosticReport
{
    public class DiagReportMappingProfile : Profile
    {
        /// <summary>
        /// 
        /// </summary>
        public DiagReportMappingProfile()
        {
            CreateMap<DefaultDiagnosisConclusion, DefaultDiagnosisConclusionDTO>().ReverseMap();
            CreateMap<DefaultRuningAdvice, DefaultRuningAdviceDTO>().ReverseMap();
            CreateMap<DeviceReportAnalyzerRecord, DiagnosisReportAnalyzerRecordDTO>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<DiagnosisReportAnalyzerRecordDTO, DeviceReportAnalyzerRecord>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<DeviceReportDiagnosisConclusion, DiagnosisReportConclusionDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.WarningLevel))
                .ReverseMap();
            CreateMap<DiagnosisReportConclusionDTO, DeviceReportDiagnosisConclusion>()
                .ForMember(dest => dest.WarningLevel, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
            CreateMap<DeviceDiagnosisReport, DiagnosisReportDTO>().ReverseMap();
            CreateMap<DeviceDiagnosisReport, SimpleDiagnosisReportDTO>().ReverseMap();
            CreateMap<DeviceDiagnosisAnalyzerRecord, SaveAnalyzerRecordDTO>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.AcqTime, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<SaveAnalyzerRecordDTO, DeviceDiagnosisAnalyzerRecord>()
                .ForMember(dest => dest.Image, opt => opt.Ignore())
                .ForMember(dest => dest.AcqTime, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<WindParkDiagnosisReport, WindParkDiagReportDTO>().ReverseMap();
            CreateMap<WindParkInfoDTO, WindParkDiagReportSummaryDTO>().ReverseMap();
            CreateMap<WindParkInfoDTO, WindParkDiagReportDetailDTO>().ReverseMap();
        }
    }
}
