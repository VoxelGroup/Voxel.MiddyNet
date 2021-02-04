using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware.Tests
{
    public class ProblemDetailsMiddlewareShould
    {
        private readonly ProblemDetailsMiddleware middleware;
        private readonly MiddyNetContext context;

        public ProblemDetailsMiddlewareShould()
        {
            middleware = new ProblemDetailsMiddleware(null);
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
        }

        [Fact]
        public async Task BeTransparentWhenNoExceptionsOccur()
        {
            var expectedResponse = new APIGatewayProxyResponse
            {
                Body = "some body",
                Headers = new Dictionary<string, string>()
            };

            var actualResponse = await middleware.After(expectedResponse, context);

            actualResponse.Should().BeEquivalentTo(expectedResponse);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursDuringBeforeMiddlewares()
        {

        }
    }
}
