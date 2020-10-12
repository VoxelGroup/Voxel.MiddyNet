using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using NSubstitute;
using Voxel.MiddyNet.Tracing.SQSMiddleware;
using Xunit;

namespace Voxel.MiddyNet.Tracing.SNSMiddleware.Tests
{
    public class SNSTracingMiddlewareShould
    {
        [Fact]
        public async Task EnrichLoggerWithTraceContext()
        {
            var logger = Substitute.For<IMiddyLogger>();
            var context = new MiddyNetContext(Substitute.For<ILambdaContext>(), _ => logger);
            
            var middleware = new SQSTracingMiddleware();

            var snsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage>()
                {
                    new SQSEvent.SQSMessage
                    {
                        MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                        {
                            {
                                "traceparent",
                                new SQSEvent.MessageAttribute {StringValue = "traceparent header value"}
                            },
                            {
                                "tracestate",
                                new SQSEvent.MessageAttribute {StringValue = "tracestate header value"}
                            }
                        }
                    }
                    
                }
            };

            await middleware.Before(snsEvent, context);
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "traceparent"));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "tracestate"));
        }
    }
}
