using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.ApiGatewayHttpApiV2TracingSample
{
    public class ApiGatewayHttpApiV2Tracing : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public ApiGatewayHttpApiV2Tracing()
        {
            Use(new ApiGatewayHttpApiV2TracingMiddleware());
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest proxyRequest, MiddyNetContext context)
        {
            var originalTraceParentHeaderValue = string.Empty;
            if (proxyRequest.Headers.ContainsKey("traceparent"))
            {
                originalTraceParentHeaderValue = proxyRequest.Headers["traceparent"];
            }

            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty("original-traceparent", originalTraceParentHeaderValue));

            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }
}
