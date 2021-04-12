using System;
using System.Collections.Generic;
using System.Text;
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
            var source = "Make it broken" + serializedExpectation;
            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                Body = source
            };
            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            Action action = () => middleware.Before(request, context);

            action.Should().Throw<Exception>().WithMessage("Content type defined as JSON but an invalid JSON was provided");
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

        [Fact]
        public async Task HandleABase64Body()
        {
            string base64Serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(serializedExpectation));
            
            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                IsBase64Encoded = true,
                Body = base64Serialized
            };

            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            await middleware.Before(request, context);

            context.AdditionalContext.ContainsKey("Body").Should().BeTrue();
            context.AdditionalContext["Body"].Should().BeEquivalentTo(expectation);
        }

        [Fact]
        public async Task HandleInvalidBase64Body()
        {
            var source = "Make it broken" + serializedExpectation;
            string base64Serialized = Convert.ToBase64String(Encoding.UTF8.GetBytes(source));

            var request = new APIGatewayProxyRequest()
            {
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } },
                IsBase64Encoded = true,
                Body = base64Serialized
            };

            var middleware = new HttpJsonBodyParserMiddleware<TestObject>();
            Action action = () => middleware.Before(request, context);

            action.Should().Throw<Exception>().WithMessage("Content type defined as JSON but an invalid JSON was provided");
        }
    }
}
