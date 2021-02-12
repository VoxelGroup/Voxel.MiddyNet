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
            middleware = new ProblemDetailsMiddleware();
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
        }

        [Fact]
        public async Task BeTransparentWhenNoExceptionsOccur()
        {
            var givenResponse = new APIGatewayProxyResponse
            {
                Body = "some body",
                Headers = new Dictionary<string, string>()
            };
            var actualResponse = await middleware.After(givenResponse, context);
            actualResponse.Should().BeEquivalentTo(givenResponse);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursDuringBeforeMiddlewares()
        {
            context.MiddlewareBeforeExceptions.Add(new InvalidOperationException());
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursDuringAfterMiddlewares()
        {
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException());
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursInHandler()
        {
            context.HandlerException = new InvalidOperationException();
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.StatusCode.Should().Be(500);
        }
    }
}
