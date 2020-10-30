using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.ApiGatewayTracingSample
{
    public class ApiGatewayTracing : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public ApiGatewayTracing()
        {
            Use(new ApiGatewayTracingMiddleware<APIGatewayProxyResponse>());
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest proxyRequest, MiddyNetContext context)
        {
            var originalTraceParentHeaderValue = string.Empty;
            if (proxyRequest.Headers.ContainsKey("traceparent"))
            {
                originalTraceParentHeaderValue = proxyRequest.Headers["traceparent"];
            }

            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty("original-traceparent", originalTraceParentHeaderValue));

            return Task.FromResult(new APIGatewayProxyResponse()
            {
                StatusCode = 200,
                Body = "Ok", 
                IsBase64Encoded = false              
            });
        }
    }
}
