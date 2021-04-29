using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Voxel.MiddyNet.Tracing.ApiGatewayMiddleware;
using Voxel.MiddyNet.Tracing.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.HttpApiV2TracingSample
{
    public class ApiGatewayHttpApiV2Tracing : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public ApiGatewayHttpApiV2Tracing()
        {
            Use(new ApiGatewayHttpApiV2TracingMiddleware());
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest proxyRequest, MiddyNetContext context)
        {
            //This log is enriched with the tracing information received in the headers of the request
            context.Logger.Log(LogLevel.Info, "Function called.");

            //If you need to call another system, you need to obtain a traceparent based on the original traceparent
            //received but with the ParentId changed
            var currentTraceContext = (TraceContext)context.AdditionalContext[ApiGatewayHttpApiV2TracingMiddleware.TraceContextKey];
            var newTraceContext = TraceContext.MutateParentId(currentTraceContext);

            //Now you can use this newTraceContext in your calls 
            var traceparentForCallingAnotherSystem = newTraceContext.TraceParent;
            var tracestateForCallingAnotherSystem = newTraceContext.TraceState;

            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }
}
