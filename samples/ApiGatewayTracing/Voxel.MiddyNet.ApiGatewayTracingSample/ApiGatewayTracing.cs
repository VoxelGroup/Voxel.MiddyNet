using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;

namespace Voxel.MiddyNet.ApiGatewayTracingSample
{
    public class ApiGatewayTracing : MiddyNet<APIGatewayProxyRequest, int>
    {
        public ApiGatewayTracing()
        {
            Use(new ApiGatewayTracingMiddleware<int>());
        }

        protected override Task<int> Handle(APIGatewayProxyRequest proxyRequest, MiddyNetContext context)
        {
            var originalTraceParentHeaderValue = string.Empty;
            if (proxyRequest.Headers.ContainsKey("traceparent"))
            {
                originalTraceParentHeaderValue = proxyRequest.Headers["traceparent"];
            }

            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty("original-traceparent", originalTraceParentHeaderValue));

            return Task.FromResult(0);
        }
    }
}
