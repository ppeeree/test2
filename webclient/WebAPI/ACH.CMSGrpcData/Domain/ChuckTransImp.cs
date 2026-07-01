using Grpc.Core;
using WindCMSAPI;

namespace wtphm.gRPCServer
{
    public class ChuckTransImp: CMSAPI.CMSAPIBase
    {
        public override async Task chuckDevTree(ChuckRequest request, IServerStreamWriter<BlockData> responseStream, ServerCallContext context)
        {
            int totalSize = 409600;
            int blockNum = totalSize / request.Blocksize;
            for (int inx = 0; inx < blockNum; inx++)
            {
                await responseStream.WriteAsync(new BlockData
                {
                    ChuckData = Google.Protobuf.ByteString.CopyFrom(new byte[request.Blocksize]),
                });
            }
        }



        /// <summary>
        /// 分块发送数据
        /// </summary>
        /// <param name="blockSize"></param>
        /// <param name="responseStream"></param>
        /// <param name="serdata"></param>
        /// <returns></returns>
        public static async Task SendData(int blockSize, Grpc.Core.IAsyncStreamWriter<WindCMSAPI.BlockData> responseStream, byte[] serdata)
        {
            int totalSize = serdata.Length;

            int blockNum = totalSize / blockSize;

            if (totalSize % blockSize != 0)
            {
                blockNum += 1;
            }

            System.Diagnostics.Debug.Write("Start Chuck Trans, TotalSize:");
            System.Diagnostics.Debug.Write(totalSize);
            System.Diagnostics.Debug.Write(", BlkNum:");
            System.Diagnostics.Debug.WriteLine(blockNum);


            int sentSize = 0;
            Random rand = new Random();

            for (int inx = 0; inx < blockNum; inx++)
            {
                int currBlkSize = blockSize;

                // 不够一个完整的块？
                if ((totalSize - sentSize) < blockSize)
                {
                    currBlkSize = totalSize - sentSize;
                }

                await responseStream.WriteAsync(new WindCMSAPI.BlockData
                {
                    ChuckData = Google.Protobuf.ByteString.CopyFrom(serdata, sentSize, currBlkSize),
                });

                sentSize += blockSize;

                // A bit of delay before sending the next one.
                await Task.Delay(rand.Next(1000) + 500);
            }

            System.Diagnostics.Debug.WriteLine("End Trans.");

        }
    }
}
