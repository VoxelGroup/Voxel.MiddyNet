using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using ApprovalTests;
using ApprovalTests.Reporters;
using FluentAssertions;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ProblemDetailsMiddlewareShould
    {
        private readonly ProblemDetailsMiddleware middleware;
        private readonly MiddyNetContext context;

        public ProblemDetailsMiddlewareShould()
        {
            middleware = new ProblemDetailsMiddleware();
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            context.LambdaContext.AwsRequestId.Returns("some-request-id");
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
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursDuringAfterMiddlewares()
        {
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException());
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.StatusCode.Should().Be(500);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursInHandler()
        {
            context.HandlerException = new InvalidOperationException();
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.StatusCode.Should().Be(500);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task MaintainOriginalHeadersThatAreNotCacheRelated()
        {
            Dictionary<string, string> givenHeaders = new Dictionary<string, string>()
            {
                ["a-header"] = "a-value",
                ["another-header"] = "another-value"
            };
            var givenResponse = new APIGatewayProxyResponse
            {
                Body = "some body",
                Headers = givenHeaders
            };
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException());

            var actualResponse = await middleware.After(givenResponse, context);
            actualResponse.Headers.Should().Contain(givenHeaders);
        }

        [Fact]
        public async Task IncludeNoCacheHeadersInProblemDetails()
        {
            var noCacheHeaders = new Dictionary<string, string>
            {
                [HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate",
                [HeaderNames.Pragma] = "no-cache",
                [HeaderNames.Expires] = "0"
            };
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException());
            var response = await middleware.After(new APIGatewayProxyResponse(), context);
            response.Headers.Should().Contain(noCacheHeaders);
        }

        [Fact]
        public async Task KeepOriginalMultivaluedHeadersInProblemDetails()
        {
            var givenHeaders = new Dictionary<string, IList<string>>
            {
                ["some-header"] = new List<string> { "a-value", "another-value" }
            };
            var response = await middleware.After(new APIGatewayProxyResponse { MultiValueHeaders = givenHeaders }, context);
            response.MultiValueHeaders.Should().Contain(givenHeaders);
        }

        [Fact]
        public async Task AlwaysIncludeProblemDetailsContentType()
        {
            var expectedHeaders = new Dictionary<string, IList<string>>
            {
                [HeaderNames.ContentType] = new List<string> { "application/problem+json" }
            };
            var response = await middleware.After(new APIGatewayProxyResponse { StatusCode = 500 }, context);
            response.MultiValueHeaders.Should().BeEquivalentTo(expectedHeaders);
        }

        [Fact]
        public async Task FormatProblemsThatAreNotCausedByExceptions()
        {
            var givenResponse = new APIGatewayProxyResponse
            {
                Body = "some body",
                Headers = new Dictionary<string, string>(),
                StatusCode = 503
            };
            var response = await middleware.After(givenResponse, context);
            Approvals.Verify(response.Body);
        }
    }
}
