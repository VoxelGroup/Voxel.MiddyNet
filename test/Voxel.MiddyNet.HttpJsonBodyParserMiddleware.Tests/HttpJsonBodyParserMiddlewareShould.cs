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
        private MiddyNetContext context;
        private TestObject expectation;
        private string serializedExpectation;

        public HttpJsonBodyParserMiddlewareShould()
        {
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            expectation = new TestObject("bar");
            serializedExpectation = JsonConvert.SerializeObject(expectation);
        }

        [Fact]
        public async Task ProcessTheJsonRequest()
        {
            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = serializedExpectation
            };
            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            await middleware.Before(request, context);

            context.AdditionalContext.ContainsKey("Body").Should().BeTrue();
            context.AdditionalContext["Body"].Should().BeEquivalentTo(expectation);
        }

        [Fact]
        public async Task ErrorWhenJsonNotMapsToObject()
        {
            var source = "Not Mapped object" + serializedExpectation;
            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = source
            };
            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            Action action = () => middleware.Before(request, context);

            action.Should().Throw<Exception>().WithMessage($"Error parsing \"{source}\" to type {typeof(TestObject)}");
        }

        [Fact]
        public async Task NotProcessTheBodyIfNoHeaderIsPassed()
        {
            var request = new APIGatewayProxyRequest()
            {
                Body = serializedExpectation
            };
            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            await middleware.Before(request, context);

            context.AdditionalContext.ContainsKey("Body").Should().BeTrue();
            context.AdditionalContext["Body"].Should().Be(serializedExpectation);
        }
    }
}
