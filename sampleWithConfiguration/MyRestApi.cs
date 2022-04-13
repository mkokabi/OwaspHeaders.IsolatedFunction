using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace OwaspHeader.Sample
{
    public class MyRestApi
    {
        private readonly ILogger _logger;

        public MyRestApi(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MyRestApi>();
        }

        [Function("MyRestApi")]
        [OpenApiOperation(operationId: "Run", tags: new[] {"Sample"})]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.Created, contentType: "application/json",
            bodyType: typeof(string),
            Description = "The OK response")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
