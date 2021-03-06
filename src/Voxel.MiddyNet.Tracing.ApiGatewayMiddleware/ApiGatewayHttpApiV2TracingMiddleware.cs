﻿using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware
{
    public class ApiGatewayHttpApiV2TracingMiddleware : ILambdaMiddleware<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";
        private const string TraceIdHeaderName = "trace-id";

        public static string TraceContextKey = "TraceContext";

        public Task Before(APIGatewayHttpApiV2ProxyRequest apiGatewayEvent, MiddyNetContext context)
        {
            var traceParentHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceParentHeaderName))
                traceParentHeaderValue = apiGatewayEvent.Headers[TraceParentHeaderName];

            var traceStateHeaderValue = string.Empty;
            if (apiGatewayEvent.Headers.ContainsKey(TraceStateHeaderName))
                traceStateHeaderValue = apiGatewayEvent.Headers[TraceStateHeaderName];

            var traceContext = TraceContext.Handle(traceParentHeaderValue, traceStateHeaderValue);

            context.AdditionalContext.Add(TraceContextKey, traceContext);

            context.Logger.EnrichWith(new LogProperty(TraceParentHeaderName, traceContext.TraceParent));
            context.Logger.EnrichWith(new LogProperty(TraceStateHeaderName, traceContext.TraceState));
            context.Logger.EnrichWith(new LogProperty(TraceIdHeaderName, traceContext.TraceId));

            return Task.CompletedTask;
        }

        public Task<APIGatewayHttpApiV2ProxyResponse> After(APIGatewayHttpApiV2ProxyResponse lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }
    }
}
