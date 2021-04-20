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
            //This log is enriched with the tracing information received in the headers of the request
            context.Logger.Log(LogLevel.Info, "Function called.");

            //If you need to call another system, you need to obtain a traceparent based on the original traceparent
            //received but with the ParentId changed
            var currentTraceContext = (TraceContext)context.AdditionalContext[TraceContext.TraceContextKey];
            var newTraceContext = TraceContext.MutateParentId(currentTraceContext);

            //Now you can use this newTraceContext in your calls 
            var traceparentForCallingAnotherSystem = newTraceContext.TraceParent;
            var tracestateForCallingAnotherSystem = newTraceContext.TraceState;

            return Task.FromResult(new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }
}
