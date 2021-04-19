using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware
{
    public class ApiGatewayTracingMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";
        private const string TraceIdHeaderName = "trace-id";

        public Task Before(APIGatewayProxyRequest apiGatewayEvent, MiddyNetContext context)
        {
            var traceParentHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceParentHeaderName))
                traceParentHeaderValue = apiGatewayEvent.Headers[TraceParentHeaderName];

            var traceStateHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceStateHeaderName))
                traceStateHeaderValue = apiGatewayEvent.Headers[TraceStateHeaderName];

            var traceContext = TraceContext.Handle(traceParentHeaderValue, traceStateHeaderValue);
            
            context.Logger.EnrichWith(new LogProperty(TraceParentHeaderName, traceContext.TraceParent));
            context.Logger.EnrichWith(new LogProperty(TraceStateHeaderName, traceContext.TraceState));
            context.Logger.EnrichWith(new LogProperty(TraceIdHeaderName, traceContext.TraceId));

            context.AdditionalContext.Add(TraceParentHeaderName, traceContext.TraceParent);
            context.AdditionalContext.Add(TraceStateHeaderName, traceContext.TraceState);
            context.AdditionalContext.Add(TraceIdHeaderName, traceContext.TraceId);

            return Task.CompletedTask;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }
    }
}
