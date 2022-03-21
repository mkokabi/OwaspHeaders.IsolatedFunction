using Microsoft.Extensions.Hosting;
using OwaspHeaders.IsolatedFunction;


var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
      builder.UseMiddleware<OwaspHandlerMiddleware>();
    })
    .Build();

host.Run();