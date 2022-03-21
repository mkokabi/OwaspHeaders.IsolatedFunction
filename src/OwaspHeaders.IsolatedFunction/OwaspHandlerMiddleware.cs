using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using OwaspHeaders.Core;
using OwaspHeaders.Core.Extensions;
using OwaspHeaders.Core.Models;

namespace OwaspHeaders.IsolatedFunction;

public class OwaspHandlerMiddleware: IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        await next(context);
        
        ILogger<OwaspHandlerMiddleware> logger = context.GetLogger<OwaspHandlerMiddleware>();

        if (logger == null)
        {
            logger = new NullLogger<OwaspHandlerMiddleware>();
        }
        
        var config = SecureHeadersMiddlewareExtensions.BuildDefaultConfiguration();

        var response = FunctionContextExtensions.GetHttpResponseData(context, logger);
        if (response == null)
        {
            throw new InvalidOperationException("response is null");
        }

        if (config == null)
        {
            throw new ArgumentException($@"Expected an instance of the {nameof(SecureHeadersMiddlewareConfiguration)} object.");
        }

        if (config.UseHsts)
        {
            response.Headers.TryAddWithoutValidation(Constants.StrictTransportSecurityHeaderName,
                config.HstsConfiguration.BuildHeaderValue());
        }

        if (config.UseXFrameOptions)
        {
            response.Headers.TryAddWithoutValidation(Constants.XFrameOptionsHeaderName,
                config.XFrameOptionsConfiguration.BuildHeaderValue());
        }

        if (config.UseXssProtection)
        {
            response.Headers.TryAddWithoutValidation(Constants.XssProtectionHeaderName,
                config.XssConfiguration.BuildHeaderValue());
        }

        if (config.UseXContentTypeOptions)
        {
            response.Headers.TryAddWithoutValidation(Constants.XContentTypeOptionsHeaderName, "nosniff");
        }

        if (config.UseContentSecurityPolicyReportOnly)
        {
            
            response.Headers.TryAddWithoutValidation(Constants.ContentSecurityPolicyReportOnlyHeaderName,
                config.ContentSecurityPolicyReportOnlyConfiguration.BuildHeaderValue());
        }
        else if (config.UseContentSecurityPolicy)
        {
            response.Headers.TryAddWithoutValidation(Constants.ContentSecurityPolicyHeaderName,
                config.ContentSecurityPolicyConfiguration.BuildHeaderValue());
        }

        if (config.UseXContentSecurityPolicy)
        {
            response.Headers.TryAddWithoutValidation(Constants.XContentSecurityPolicyHeaderName,
            config.ContentSecurityPolicyConfiguration.BuildHeaderValue());
        }

        if (config.UsePermittedCrossDomainPolicy)
        {
            response.Headers.TryAddWithoutValidation(Constants.PermittedCrossDomainPoliciesHeaderName,
                config.PermittedCrossDomainPolicyConfiguration.BuildHeaderValue());
        }

        if (config.UseReferrerPolicy)
        {
            response.Headers.TryAddWithoutValidation(Constants.ReferrerPolicyHeaderName,
                config.ReferrerPolicy.BuildHeaderValue());
        }

        if (config.UseExpectCt)
        {
            response.Headers.TryAddWithoutValidation(Constants.ExpectCtHeaderName,
                config.ExpectCt.BuildHeaderValue());
        }

        if (config.RemoveXPoweredByHeader)
        {
            TryRemoveHeader(response.Headers, Constants.PoweredByHeaderName);
            TryRemoveHeader(response.Headers, Constants.ServerHeaderName);
        }
    }

    private bool TryRemoveHeader(HttpHeadersCollection headersCollection, string headerName)
    {
        if (!headersCollection.Contains(headerName))
            return true;
        try
        {
            headersCollection.Remove(headerName);
            return true;
        }
        catch (ArgumentException ex)
        {
            return false;
        }
    }
}