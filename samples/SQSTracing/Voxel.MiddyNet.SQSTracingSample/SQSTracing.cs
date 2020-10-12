using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Voxel.MiddyNet.Tracing.SQSMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.SQSTracingSample
{
    public class SQSTracing : MiddyNet<SQSEvent>
    {
        public SQSTracing()
        {
            Use(new SQSTracingMiddleware());
        }

        protected override Task Handle(SQSEvent lambdaEvent, MiddyNetContext context)
        {
            var originalTraceParentHeaderValue = string.Empty;

            if (lambdaEvent.Records.First().MessageAttributes.ContainsKey("traceparent"))
            {
                originalTraceParentHeaderValue = lambdaEvent.Records.First().MessageAttributes["traceparent"].StringValue;
            }

            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty("original-traceparent", originalTraceParentHeaderValue));

            return Task.CompletedTask;
        }
    }
}
