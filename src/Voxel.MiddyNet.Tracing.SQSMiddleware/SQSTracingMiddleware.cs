using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SQSMiddleware
{
    public class SQSTracingMiddleware : IBeforeLambdaMiddleware<SQSEvent>
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public Task Before(SQSEvent sqsEvent, MiddyNetContext context)
        {
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
    }
}
