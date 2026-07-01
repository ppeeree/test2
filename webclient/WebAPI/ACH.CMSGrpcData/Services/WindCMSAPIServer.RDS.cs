using Grpc.Core;
using System.Diagnostics;
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
        public override async Task<WindCMSAPI.CMSDataSummary> chuckUploadCMSData(Grpc.Core.IAsyncStreamReader<WindCMSAPI.BlockData> requestStream, Grpc.Core.ServerCallContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            List<byte> bytesData = new List<byte>();

            while (await requestStream.MoveNext())
            {
                var blockData = requestStream.Current;

                bytesData.AddRange(blockData.ChuckData.ToByteArray());
            }

            var cMSData = SerializationUtility.Deserialize<CMSFramework.BusinessEntity.WindTurbineCMSData>(bytesData.ToArray());

            // 加入RDS数据处理队列

            stopwatch.Stop();

            return new WindCMSAPI.CMSDataSummary
            {
                ElapsedTime = (int)(stopwatch.ElapsedMilliseconds / 1000)
            };
        }
    }
}
