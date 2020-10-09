using System;
using Amazon.SimpleNotificationService.Model;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SNS
{
    public static class PublishRequestExtensions
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public static void EnrichWithTraceContext(this PublishRequest publishRequest, TraceContext traceContext)
        {
            publishRequest.AddMessageAttribute(TraceParentHeaderName, traceContext.TraceParent);
            publishRequest.AddMessageAttribute(TraceStateHeaderName, traceContext.TraceState);
        }

        private static void AddMessageAttribute(this PublishRequest publishRequest, string key, string value)
        {
            publishRequest.MessageAttributes.Add(key, new MessageAttributeValue
            {
                DataType = "String",
                StringValue = value
            });
        }
    }
}
