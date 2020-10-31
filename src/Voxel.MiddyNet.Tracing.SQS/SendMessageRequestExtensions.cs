using Amazon.SQS.Model;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SQS
{
    public static class SendMessageRequestExtensions
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public static void EnrichWithTraceContext(this SendMessageRequest sendMessageRequest, TraceContext traceContext)
        {
            sendMessageRequest.AddMessageAttribute(TraceParentHeaderName, traceContext.TraceParent);
            sendMessageRequest.AddMessageAttribute(TraceStateHeaderName, traceContext.TraceState);
        }

        private static void AddMessageAttribute(this SendMessageRequest publishRequest, string key, string value)
        {
            publishRequest.MessageAttributes.Add(key, new MessageAttributeValue
            {
                DataType = "String",
                StringValue = value
            });
        }
    }
}
