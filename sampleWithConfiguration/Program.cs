using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OwaspHeaders.Core.Enums;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;
using OwaspHeaders.IsolatedFunction;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.UseMiddleware<OwaspHandlerMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IOwaspMiddlewareConfigurationProvider, CustomConfigurationProvider>();
    })
    .Build();

host.Run();

public class CustomConfigurationProvider : IOwaspMiddlewareConfigurationProvider
{
    public SecureHeadersMiddlewareConfiguration CustomConfiguration()
    {
        return SecureHeadersMiddlewareBuilder
            .CreateBuilder()
            .UseHsts(1200, false)
            .UseXSSProtection(XssMode.oneReport, "https://reporturi.com/some-report-url")
            // .UseContentDefaultSecurityPolicy()
            .UseContentSecurityPolicy(blockAllMixedContent: false)
            .UseCacheControl(false, maxAge: (int)TimeSpan.FromHours(1).TotalSeconds)
            .UsePermittedCrossDomainPolicies(XPermittedCrossDomainOptionValue.masterOnly)
            .UseReferrerPolicy(ReferrerPolicyOptions.sameOrigin)
            .Build();
    }
}
