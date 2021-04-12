using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware.Tests
{
    public class HttpJsonBodyParserMiddlewareShould
    {
        [Fact]
        public async Task ProcessTheJsonRequest()
        {
            var context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            var expectation = new TestObject("bar");
            var source = JsonConvert.SerializeObject(expectation);
            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = source
            };
            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            await middleware.Before(request, context);

            context.AdditionalContext.ContainsKey("Body").Should().BeTrue();
            context.AdditionalContext["Body"].Should().BeEquivalentTo(expectation);
        }
    }
}
