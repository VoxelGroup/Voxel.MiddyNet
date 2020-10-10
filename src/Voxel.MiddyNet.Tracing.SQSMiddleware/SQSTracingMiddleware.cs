using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SQSMiddleware
{
    public class SQSTracingMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            if (!(lambdaEvent is SQSEvent))
            {
                context.MiddlewareExceptions.Add(new InvalidOperationException($"Trying to use the SQSTracingMiddleware with an event of type {typeof(TReq)}"));
                return Task.CompletedTask;
            }

            var sqsEvent = lambdaEvent as SQSEvent;
            var sqsMessage = sqsEvent.Records.First();

            var traceParentHeaderValue = string.Empty;
            if (sqsMessage.MessageAttributes.ContainsKey(TraceParentHeaderName))
                traceParentHeaderValue = sqsMessage.MessageAttributes[TraceParentHeaderName].StringValue;

            var traceStateHeaderValue = string.Empty;
            if (sqsMessage.MessageAttributes.ContainsKey(TraceStateHeaderName))
                traceParentHeaderValue = sqsMessage.MessageAttributes[TraceStateHeaderName].StringValue;

            var traceContext = TraceContext.Handle(traceParentHeaderValue, traceStateHeaderValue);

            context.Logger.EnrichWith( new LogProperty(TraceParentHeaderName, traceContext.TraceParent));
            context.Logger.EnrichWith(new LogProperty(TraceStateHeaderName, traceContext.TraceState));

            return Task.CompletedTask;
        }

        public Task<TRes> After(TRes lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }
    }
}
