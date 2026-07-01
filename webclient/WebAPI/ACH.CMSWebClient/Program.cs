using ACH.ACHLog.SeriLog;
using ACH.CMSWebClient.Service;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// 判断注册诊断数据下载的定时服务
if (builder.Configuration.GetValue<bool>("StartDownloadService"))
{
    builder.Services.AddHostedService<DiagDataDownLoad>();
}

// 配置跨域
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod());
});

// 监听端口，端口号从配置文件获取，默认为5150
builder.WebHost.ConfigureKestrel(options =>
{
    int port = builder.Configuration.GetValue<int>("ServerPort", 5150);
    options.ListenAnyIP(port);

    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(15);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
});

builder.Services.Configure<KestrelServerOptions>(option => option.AllowSynchronousIO = true)
                .Configure<IISServerOptions>(option => option.AllowSynchronousIO = true);


// 添加 YARP 服务
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 初始化日志
ALog.Init();
ALog.Debug("服务启动");

// 1. 基础中间件 
app.UseResponseCaching();
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});

// 2. 静态文件（优先于路由）
app.UseStaticFiles(); // wwwroot

// 2.1 三维模型文件
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".glb"] = "model/gltf-binary";
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/modelfile",
    FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modelfile")),
    ContentTypeProvider = provider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"
});

// 2.2 assets/data
var assprovider = new FileExtensionContentTypeProvider();
assprovider.Mappings[".glb"] = "model/gltf-binary";
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/assets/data",
    FileProvider = new PhysicalFileProvider(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "assets", "data")),
    ContentTypeProvider = assprovider,
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=31536000,immutable"
});

// 3. 路由和CORS
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseAuthorization();


// 4.1 API 控制器
app.MapControllers();
// 4.2 YARP 反向代理
app.MapReverseProxy();

app.Run();
ALog.Information($"web端服务关闭");
