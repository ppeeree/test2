using CMSFramework.BusinessEntity;
using Grpc.Core;
using WindCMS.GRPCWPServerImp.Domain;
using WindCMSAPI;
using wtphm.gRPCServer.Domain;
using WTPHM.CMSFramework;

namespace wtphm.gRPCServer.Services
{
    public partial class WindCMSAPIServer: CMSAPI.CMSAPIBase
    {
        private readonly ILogger<WindCMSAPIServer> _logger;
        private readonly IConfiguration _config;
        public WindCMSAPIServer(ILogger<WindCMSAPIServer> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public override async Task chuckLogin(WindCMSAPI.ChuckRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            _logger.LogDebug("[WindCMSAPIServer]begin chuckLogin");
            try
            {
                SysuserDomain sysuserDomain = new SysuserDomain();
                var u = sysuserDomain.VibAnalyer_Login(request.TokenMsg.UserName, request.TokenMsg.Password);

                _logger.LogDebug(string.Format("User:{0}", u));

                byte[] serdata = SerializationUtility.Serialize(u);

                await ChuckTransImp.SendData(request.Blocksize, responseStream, serdata);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckLogin");
            }
            _logger.LogDebug("[WindCMSAPIServer]end chuckLogin");

        }

        public override async Task chuckDevTree(WindCMSAPI.ChuckRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                DeviceTreeDomain deviceTreeDomain = new DeviceTreeDomain(_config);
                var devTree = deviceTreeDomain.GetDevTreeData(request.TokenMsg.UserName);

                byte[] serdata = SerializationUtility.Serialize<CMSFramework.BusinessEntity.DevTreeData>(devTree);

                await ChuckTransImp.SendData(request.Blocksize, responseStream, serdata);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckDevTree");

                throw ex;
            }
        }


        public override async Task chuckWPDevTree(WindCMSAPI.WPDevTreeRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                //DevTreeDomain isvc = new DevTreeDomain();

                //var devTree = isvc.GetWPDevTreeData(request.WindParkId);
                DeviceTreeDomain deviceTreeDomain = new DeviceTreeDomain(_config);
                var devTree = deviceTreeDomain.GetDevTreeData(request.WindParkId);

                byte[] serdata = SerializationUtility.Serialize<CMSFramework.BusinessEntity.DevTreeData>(devTree);

                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, serdata);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckWPDevTree");

                throw new Grpc.Core.RpcException(Grpc.Core.Status.DefaultCancelled, ex.Message);
            }
        }
    }
}
