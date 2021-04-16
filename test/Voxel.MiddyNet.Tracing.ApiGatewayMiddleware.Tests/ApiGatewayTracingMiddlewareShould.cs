using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.Tracing.ApiGatewayMiddleware.Tests
{
    public class ApiGatewayTracingMiddlewareShould
    {
        private const string TraceparentHeaderValue = "00-0af7651916cd43dd8448eb211c80319c-b9c7c989f97918e1-01";
        private const string TracestateHeaderValue = "congo=ucfJifl5GOE";
        private const string TraceIdHeaderValue = "0af7651916cd43dd8448eb211c80319c";
        
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
                    { "traceparent", TraceparentHeaderValue },
                    {"tracestate", TracestateHeaderValue }
                }
            };

            await middleware.Before(apiGatewayEvent, context);
            
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "traceparent" && p.Value.ToString() == TraceparentHeaderValue));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "tracestate" && p.Value.ToString() == TracestateHeaderValue));
            logger.Received().EnrichWith(Arg.Is<LogProperty>(p => p.Key == "trace-id" && p.Value.ToString() == TraceIdHeaderValue));
        }
    }
}
