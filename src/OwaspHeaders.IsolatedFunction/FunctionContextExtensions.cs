using System.Reflection;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace OwaspHeaders.IsolatedFunction;

public static class FunctionContextExtensions
{
    /// <summary>
    /// Returns the HttpRequestData from the function context if it exists.
    /// </summary>
    /// <param name="functionContext"></param>
    /// <returns>HttpRequestData or null</returns>
    public static HttpRequestData GetHttpRequestData(this FunctionContext functionContext,
        ILogger<OwaspHandlerMiddleware> logger)
    {
        try
        {
            object functionBindingsFeature = functionContext.GetIFunctionBindingsFeature(logger);
            Type type = functionBindingsFeature.GetType();
            var inputData =
                type?.GetProperties().Single(p => p.Name is "InputData").GetValue(functionBindingsFeature) as
                    IReadOnlyDictionary<string, object>;
            return inputData?.Values.SingleOrDefault(o => o is HttpRequestData) as HttpRequestData ?? throw new InvalidOperationException();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sets the FunctionContext IFunctionBindingsFeature InvocationResult with a HttpResponseData. 
    /// 
    /// We are using this to add exceptions to the Azure Function response...to gently handle exceptions
    /// caught by the exception handling middleware and return either a BadRequest 404 or Internal Server
    /// Error 500 HTTP Result.
    /// </summary>
    /// <param name="functionContext"></param>
    /// <param name="response"></param>
    public static void SetHttpResponseData(this FunctionContext functionContext, HttpResponseData response,
        ILogger<OwaspHandlerMiddleware> logger)
    {
        try
        {
            object functionBindingsFeature = functionContext.GetIFunctionBindingsFeature(logger);
            Type type = functionBindingsFeature.GetType();
            PropertyInfo pinfo = type?.GetProperties().Single(p => p.Name is "InvocationResult");
            pinfo?.SetValue(functionBindingsFeature, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
    }

    /// <summary>
    /// Returns the HttpRequestData from the function context if it exists. 
    /// 
    /// </summary>
    /// <param name="functionContext"></param>
    /// <param name="response"></param>
    public static HttpResponseData? GetHttpResponseData(this FunctionContext functionContext,
        ILogger<OwaspHandlerMiddleware> logger)
    {
        PropertyInfo? pinfo = null;
        try
        {
            object functionBindingsFeature = functionContext.GetIFunctionBindingsFeature(logger);
            Type type = functionBindingsFeature.GetType();
            pinfo = type?.GetProperties().Single(p => p.Name is "InvocationResult");
            var result = pinfo?.GetValue(functionBindingsFeature) as HttpResponseData;
            if (result != null)
            {
                return result;
            }
            pinfo = type?.GetProperties().Single(p => p.Name is "OutputBindingData");
            var functionBindingsFeatureDictionary = pinfo?.GetValue(functionBindingsFeature) as Dictionary<string, object>;
            object? httpResponseDataObject;
            if (functionBindingsFeatureDictionary == null)
            {
                logger.LogWarning("functionBindingsFeatureDictionary is null");
                return null;
            }
            if (functionBindingsFeatureDictionary.TryGetValue("HttpResponse", out httpResponseDataObject))
            {
                HttpResponseData? httpResponseData = httpResponseDataObject as HttpResponseData;
                if (httpResponseData != null)
                {
                    return httpResponseData;
                }
            }
            logger.LogWarning("functionBindingsFeatureDictionary does not contain HttpResponse");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "pinfo {pinfo}", pinfo);
            return null;
        }
    }

    /// <summary>
    /// Retrieves the IFunctionBindingsFeature property from the FunctionContext.
    /// </summary>
    /// <param name="functionContext"></param>
    /// <returns>IFunctionBindingsFeature or null</returns>
    private static object GetIFunctionBindingsFeature(this FunctionContext functionContext,
        ILogger<OwaspHandlerMiddleware> logger)
    {
        try
        {
            KeyValuePair<Type, object> keyValuePair =
                functionContext.Features.SingleOrDefault(f => f.Key.Name is "IFunctionBindingsFeature");
            return keyValuePair.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            return null;
        }
    }
}