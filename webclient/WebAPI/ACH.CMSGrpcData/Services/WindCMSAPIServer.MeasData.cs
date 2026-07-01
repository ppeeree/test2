using CMSFramework.BusinessEntity;
using WindCMS.GRPCWPServerImp.Domain;
using WTPHM.CMSFramework;

namespace wtphm.gRPCServer.Services
{
    public partial class WindCMSAPIServer
    {
        public override async Task chuckEvEventTreeList(WindCMSAPI.MeasEventRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                _logger.LogDebug(string.Format(@"reques==null ({0}), responseStream=null({1}) context=null({2})", request == null, responseStream == null, context == null));
                //WindCMS.AnalyzerDomainImp.MeasDataDomain Isvc = new WindCMS.AnalyzerDomainImp.MeasDataDomain();
                MeasureDataDomain Isvc = new MeasureDataDomain(_config);
                // verify user name and password.
                List<MeasEvent_EigenValue> evDataList = Isvc.GetMeasEventEVTreeList(
                    request.WindTurbineId, DateTime.FromBinary(request.BeginTime), DateTime.FromBinary(request.Endtime));
                _logger.LogDebug(string.Format(@"evDataList==null ({0})", evDataList == null));
                byte[] serdata = SerializationUtility.Serialize<List<MeasEvent_EigenValue>>(evDataList);
                _logger.LogDebug(string.Format(@"serdata==null ({0})", serdata == null));
                _logger.LogDebug(string.Format(@"request.BaseRequest==null ({0}) request.BaseRequest.Blocksize", request.BaseRequest == null, request.BaseRequest.Blocksize == null));
                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, serdata);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckEvEventTreeList");

                throw new Grpc.Core.RpcException(Grpc.Core.Status.DefaultCancelled, ex.Message);
            }
        }

        public override async Task chuckWfEventTreeList(WindCMSAPI.MeasEventRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                //WindCMS.AnalyzerDomainImp.MeasDataDomain Isvc = new WindCMS.AnalyzerDomainImp.MeasDataDomain();
                MeasureDataDomain Isvc = new MeasureDataDomain(_config);

                // verify user name and password.

                List<MeasEvent_Wave> wfDataList = Isvc.GetMeasEventWFTreeList(
                    request.WindTurbineId, DateTime.FromBinary(request.BeginTime), DateTime.FromBinary(request.Endtime));

                byte[] serdata = SerializationUtility.Serialize<List<MeasEvent_Wave>>(wfDataList);

                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, serdata);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex,"chuckEvEventTreeList");

                throw new Grpc.Core.RpcException(Grpc.Core.Status.DefaultCancelled, ex.Message);
            }
        }


        public override async Task chuckWaveFormData(WindCMSAPI.WaveFormRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                //WindCMS.AnalyzerDomainImp.MeasDataDomain Isvc = new WindCMS.AnalyzerDomainImp.MeasDataDomain();
                MeasureDataDomain Isvc = new MeasureDataDomain(_config);

                byte[] data = Isvc.GetWaveFormData(request.WindTurbineId,
                    request.MeasLocId, request.WaveDefId, (EnumWaveFormType)request.WaveFormType, DateTime.FromBinary(request.AcqTime));

                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, data);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex,"chuckWaveFormData");

                throw ex;
            }
        }

        public override async Task chuckRotWaveData(WindCMSAPI.RotWaveRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                //WindCMS.AnalyzerDomainImp.MeasDataDomain Isvc = new WindCMS.AnalyzerDomainImp.MeasDataDomain();
                MeasureDataDomain Isvc = new MeasureDataDomain(_config);

                byte[] data = Isvc.GetRotSpdWaveData(request.WindTurbineId, DateTime.FromBinary(request.AcqTime));

                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, data);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckRotWaveData");

                throw ex;
            }
        }

        public override async Task chuckWorkWaveData(WindCMSAPI.WaveFormRequest request, Grpc.Core.IServerStreamWriter<WindCMSAPI.BlockData> responseStream, Grpc.Core.ServerCallContext context)
        {
            try
            {
                //WindCMS.AnalyzerDomainImp.MeasDataDomain Isvc = new WindCMS.AnalyzerDomainImp.MeasDataDomain();
                MeasureDataDomain Isvc = new MeasureDataDomain(_config);

                byte[] data = Isvc.GetWkWaveBytes(request.WindTurbineId, request.WaveFormType);

                await ChuckTransImp.SendData(request.BaseRequest.Blocksize, responseStream, data);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "chuckWorkWaveData");

                throw ex;
            }
        }
    }
}
