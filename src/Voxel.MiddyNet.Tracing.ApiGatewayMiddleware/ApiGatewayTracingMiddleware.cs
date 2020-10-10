using System;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware
{
    public class ApiGatewayTracingMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            if (!(lambdaEvent is APIGatewayProxyRequest))
            {
                context.MiddlewareExceptions.Add(new InvalidOperationException($"Trying to use the ApiGatewayTracingMiddleware with an event of type {typeof(TReq)}"));
                return Task.CompletedTask;
            }

            var apiGatewayEvent = lambdaEvent as APIGatewayProxyRequest;

            var traceParentHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceParentHeaderName))
                traceParentHeaderValue = apiGatewayEvent.Headers[TraceParentHeaderName];

            var traceStateHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceStateHeaderName))
                traceStateHeaderValue = apiGatewayEvent.Headers[TraceStateHeaderName];

            var traceContext = TraceContext.Handle(traceParentHeaderValue, traceStateHeaderValue);

            context.Logger.EnrichWith(new LogProperty(TraceParentHeaderName, traceContext.TraceParent));
            context.Logger.EnrichWith(new LogProperty(TraceStateHeaderName, traceContext.TraceState));

            return Task.CompletedTask;
        }

        public Task<TRes> After(TRes lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }
    }
}
