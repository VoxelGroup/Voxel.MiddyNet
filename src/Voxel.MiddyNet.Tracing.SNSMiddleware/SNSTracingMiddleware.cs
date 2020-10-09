﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SNSMiddleware
{
    public class SNSTracingMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            if (!(lambdaEvent is SNSEvent))
            {
                context.MiddlewareExceptions.Add(new InvalidOperationException($"Trying to use the SNSTracingMiddleware with an event of type {typeof(TReq)}"));
                return Task.CompletedTask;
            }

            var snsEvent = lambdaEvent as SNSEvent;
            var snsMessage = snsEvent.Records.First().Sns;

            var traceParentHeaderValue = string.Empty;
            if (snsMessage.MessageAttributes.ContainsKey(TraceParentHeaderName))
                traceParentHeaderValue = snsMessage.MessageAttributes[TraceParentHeaderName].Value;

            var traceStateHeaderValue = string.Empty;
            if (snsMessage.MessageAttributes.ContainsKey(TraceStateHeaderName))
                traceParentHeaderValue = snsMessage.MessageAttributes[TraceStateHeaderName].Value;

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