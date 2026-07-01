using Grpc.Core;
using System.Diagnostics;
using WindCMS.GRPCWPServerImp.Domain;
using WindCMS.IAnalyzerDomain;
using WTPHM.CMSFramework;

namespace wtphm.gRPCServer.Services
{
    public partial class WindCMSAPIServer
    {
        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<WindCMSAPI.ReportSummary> chuckUploadTurbineReport(Grpc.Core.IAsyncStreamReader<WindCMSAPI.BlockData> requestStream, Grpc.Core.ServerCallContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<byte> bytesData = new List<byte>();

            while (await requestStream.MoveNext())
            {
                var blockData = requestStream.Current;

                bytesData.AddRange(blockData.ChuckData.ToByteArray());
            }

            var retport = SerializationUtility.Deserialize<CMSFramework.BusinessEntity.WTDiagnosisReport>(bytesData.ToArray());

            // 加入RDS数据处理队列
            ITurbineReportManagement reportSvc = new DiagReportDomain();

            reportSvc.UploadDiagnosisReportHVersion(retport);

            stopwatch.Stop();

            return new WindCMSAPI.ReportSummary
            {
                ElapsedTime = (int)(stopwatch.ElapsedMilliseconds / 1000)
            };
        }
    }
}
