﻿using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Voxel.MiddyNet.Tracing.SNSMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.SNSTracingSample
{
    public class SNSTracing : MiddyNet<SNSEvent, int>
    {
        public SNSTracing()
        {
            Use(new SNSTracingMiddleware<int>());
        }

        protected override Task<int> Handle(SNSEvent lambdaEvent, MiddyNetContext context)
        {
            var originalTraceParentHeaderValue = string.Empty;
            if (lambdaEvent.Records[0].Sns.MessageAttributes.ContainsKey("traceparent"))
            {
                originalTraceParentHeaderValue = lambdaEvent.Records[0].Sns.MessageAttributes["traceparent"].Value;
            }

            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty("original-traceparent", originalTraceParentHeaderValue));

            return Task.FromResult(0);
        }
    }
}
