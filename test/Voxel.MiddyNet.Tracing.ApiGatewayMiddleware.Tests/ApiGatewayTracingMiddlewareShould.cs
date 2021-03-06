using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using NSubstitute;
using Xunit;
using FluentAssertions;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware.Tests
{
    public class ApiGatewayTracingMiddlewareShould
    {
        private const string TracestateHeaderName = "tracestate";
        private const string TraceparentHeaderName = "traceparent";
        private const string TraceIdHeaderName = "trace-id";
        private const string TraceparentHeaderValue = "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01";
        private const string TracestateHeaderValue = "congo=ucfJifl5GOE";
        private const string TraceIdHeaderValue = "0af7651916cd43dd8448eb211c80319c";
        private const string TraceContextKey = "TraceContext";

        [Fact]
        public async Task EnrichLoggerWithTraceContext()
        {
            var logger = Substitute.For<IMiddyLogger>();
            var context = new MiddyNetContext(Substitute.For<ILambdaContext>(), _ => logger);
            var middleware = new ApiGatewayTracingMiddleware();
            var apiGatewayEvent = new APIGatewayProxyRequest
            {
                Headers = new Dictionary<string, string>
                {
                    { TraceparentHeaderName, TraceparentHeaderValue },
                    { TracestateHeaderName, TracestateHeaderValue }
                }
            };

            await middleware.Before(apiGatewayEvent, context);
            
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == TraceparentHeaderName && p.Value.ToString() == TraceparentHeaderValue));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == TracestateHeaderName && p.Value.ToString() == TracestateHeaderValue));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == TraceIdHeaderName && p.Value.ToString() == TraceIdHeaderValue));
        }

        [Fact]
        public async Task EnrichContextWithTraceContext()
        {
            var context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            var middleware = new ApiGatewayTracingMiddleware();
            var apiGatewayEvent = new APIGatewayProxyRequest
            {
                Headers = new Dictionary<string, string>
                {
                    { TraceparentHeaderName, TraceparentHeaderValue },
                    { TracestateHeaderName, TracestateHeaderValue }
                }
            };

            await middleware.Before(apiGatewayEvent, context);

            context.AdditionalContext.Should().ContainKey(TraceContextKey);
            var traceContext = context.AdditionalContext[TraceContextKey] as TraceContext;
            traceContext.TraceParent.Should().Be(TraceparentHeaderValue);
            traceContext.TraceState.Should().Be(TracestateHeaderValue);
            traceContext.TraceId.Should().Be(TraceIdHeaderValue);
        }
    }
}
