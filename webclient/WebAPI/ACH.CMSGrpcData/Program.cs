using wtphm.gRPCServer.Services;

namespace ACH.CMSGrpcData;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddGrpc();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(21806);
        });
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.MapGrpcService<WindCMSAPIServer>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
    }
}
