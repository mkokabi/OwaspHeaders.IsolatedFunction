using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using OwaspHeaders.IsolatedFunction;
using Xunit;

namespace TestProject;

public class FunctionContextExtenstionTests
{
    [Fact]
    public void GetHttpResponseData_Should_return_response_data()
    {
        var mockLogger = new Mock<ILogger<OwaspHandlerMiddleware>>();
        ILogger<OwaspHandlerMiddleware> logger = mockLogger.Object;

        var mockInvocationFeatures = new Mock<IInvocationFeatures>();
        var mockFunctionContext = new Mock<FunctionContext>();
        
        var value = new
        {
            InvocationResult = new FakeHttpResponseData(mockFunctionContext.Object)
            {
                StatusCode = HttpStatusCode.OK
            }
        };
        var list = new List<KeyValuePair<Type, object>>();
        list.Add(new KeyValuePair<Type, Object>(typeof(IFunctionBindingsFeature), value));
        mockInvocationFeatures.Setup(x => x.GetEnumerator())
            .Returns(list.GetEnumerator());
        mockFunctionContext.Setup(ctx => ctx.Features)
            .Returns(mockInvocationFeatures.Object);
        var response = FunctionContextExtensions.GetHttpResponseData(mockFunctionContext.Object, logger);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}