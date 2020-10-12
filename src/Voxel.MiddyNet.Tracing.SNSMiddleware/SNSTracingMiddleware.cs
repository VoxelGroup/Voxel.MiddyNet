using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SNSEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SNSMiddleware
{
    public class SNSTracingMiddleware : IBeforeLambdaMiddleware<SNSEvent>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";
        private const string TraceIdHeaderName = "trace-id";

        public Task Before(SNSEvent snsEvent, MiddyNetContext context)
        {
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
            context.Logger.EnrichWith(new LogProperty(TraceIdHeaderName, traceContext.TraceId));

            return Task.CompletedTask;
        }
    }
}
