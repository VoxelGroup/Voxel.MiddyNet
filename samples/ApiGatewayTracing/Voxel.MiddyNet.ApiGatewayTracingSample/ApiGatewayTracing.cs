using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;
using Voxel.MiddyNet.Tracing.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.ApiGatewayTracingSample
{
    public class ApiGatewayTracing : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public ApiGatewayTracing()
        {
            Use(new ApiGatewayTracingMiddleware());
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest proxyRequest, MiddyNetContext context)
        {
            context.Logger.Log(LogLevel.Info, "Function called. This log will have the traceparent received in the API call");

            context.TraceContext = TraceContext.ChangeParentId(context.TraceContext);

            context.Logger.Log(LogLevel.Info, "This will have a traceparent with the ParentId changed");

            return Task.FromResult(new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }
}
