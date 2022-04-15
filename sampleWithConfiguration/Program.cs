using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
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
    .ConfigureOpenApi()
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
        var configurationBuilder = SecureHeadersMiddlewareBuilder
            .CreateBuilder()
            .UseHsts((int)TimeSpan.FromDays(365).TotalSeconds, false)
            .UseXSSProtection(XssMode.oneBlock)
            // .UseContentDefaultSecurityPolicy()
            .UseContentSecurityPolicy(blockAllMixedContent: false)
            .UseXFrameOptions()
            .UseContentTypeOptions()
            .UseCacheControl(false, maxAge: (int)TimeSpan.FromHours(1).TotalSeconds)
            .UsePermittedCrossDomainPolicies(XPermittedCrossDomainOptionValue.masterOnly)
            .UseReferrerPolicy(ReferrerPolicyOptions.sameOrigin);
        configurationBuilder.ContentSecurityPolicyConfiguration.FrameAncestors.Add(new ContentSecurityPolicyElement
        {
            DirectiveOrUri = "none"
        });
        configurationBuilder.ContentSecurityPolicyConfiguration.FormAction.Add(new ContentSecurityPolicyElement
        {
            DirectiveOrUri = "self"
        });
        configurationBuilder.ContentSecurityPolicyConfiguration.UpgradeInsecureRequests = true;
        configurationBuilder.ContentSecurityPolicyConfiguration.BlockAllMixedContent = true;
        return configurationBuilder.Build();
    }
}
