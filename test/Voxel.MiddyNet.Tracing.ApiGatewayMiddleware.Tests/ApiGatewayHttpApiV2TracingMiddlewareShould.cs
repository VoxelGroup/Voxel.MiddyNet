using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware.Tests
{
    public class ApiGatewayHttpApiV2TracingMiddlewareShould
    {
        [Fact]
        public async Task EnrichLoggerWithTraceContext()
        {
            var logger = Substitute.For<IMiddyLogger>();
            var context = new MiddyNetContext(Substitute.For<ILambdaContext>(), _ => logger);

            var middleware = new ApiGatewayHttpApiV2TracingMiddleware();

            var apiGatewayHttpApiV2Event = new APIGatewayHttpApiV2ProxyRequest
            {
                Headers = new Dictionary<string, string>
                {
                    { "traceparent", "traceparent header value" },
                    {"tracestate", "tracestate header value" }
                }
            };

            await middleware.Before(apiGatewayHttpApiV2Event, context);
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "traceparent"));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "tracestate"));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "trace-id"));
        }
    }
}
