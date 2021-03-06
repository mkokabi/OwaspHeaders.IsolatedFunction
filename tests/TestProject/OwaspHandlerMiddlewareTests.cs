using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OwaspHeaders.Core;
using OwaspHeaders.IsolatedFunction;
using Xunit;

namespace TestProject;

public class OwaspHandlerMiddlewareTests
{
    [Fact]
    public void OwaspHandlerMiddleware_Invoke_should_add_headers()
    {
        var mockLogger = new Mock<ILogger<OwaspHandlerMiddleware>>();
        ILogger<OwaspHandlerMiddleware> logger = mockLogger.Object;

        var mockFunctionContext = new Mock<FunctionContext>();

        var mockLoggerFactory = new Mock<ILoggerFactory>();
        mockLoggerFactory.Setup(m => m.CreateLogger(It.IsAny<string>()))
            .Returns(new NullLogger<OwaspHandlerMiddleware>());

        var mockServiceProvide = new Mock<IServiceProvider>();
        mockServiceProvide.Setup(m => m.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        mockFunctionContext.Setup(ctx => ctx.InstanceServices)
            .Returns(mockServiceProvide.Object);

        var httpHeadersCollection = new HttpHeadersCollection();
        var value = new
        {
            InvocationResult = new FakeHttpResponseData(mockFunctionContext.Object)
            {
                Headers = httpHeadersCollection
            }
        };
        var list = new List<KeyValuePair<Type, object>>();
        list.Add(new KeyValuePair<Type, Object>(typeof(IFunctionBindingsFeature), value));

        var mockInvocationFeatures = new Mock<IInvocationFeatures>();
        mockInvocationFeatures.Setup(x => x.GetEnumerator())
            .Returns(list.GetEnumerator());

        mockFunctionContext.Setup(ctx => ctx.Features)
            .Returns(mockInvocationFeatures.Object);
        var owaspHandlerMiddleware = new OwaspHandlerMiddleware();

        var mockFunctionExecutionDelegate = new Mock<FunctionExecutionDelegate>();
        var functionExecutionDelegate = new FunctionExecutionDelegate(mockFunctionExecutionDelegate.Object);

        owaspHandlerMiddleware.Invoke(mockFunctionContext.Object, functionExecutionDelegate);

        Assert.Equal(8, httpHeadersCollection.Count());
        var expectedHeaders = new List<string>(new[]
        {
            Constants.StrictTransportSecurityHeaderName,
            Constants.XFrameOptionsHeaderName,
            Constants.XContentTypeOptionsHeaderName,
            Constants.ContentSecurityPolicyHeaderName,
            Constants.PermittedCrossDomainPoliciesHeaderName,
            Constants.ReferrerPolicyHeaderName,
            Constants.CacheControlHeaderName,
            Constants.ExpectCtHeaderName
        });
        Assert.True(expectedHeaders.TrueForAll(header =>
            httpHeadersCollection.Contains(header)));
    }
}