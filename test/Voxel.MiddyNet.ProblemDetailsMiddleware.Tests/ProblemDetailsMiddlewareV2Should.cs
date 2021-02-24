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

namespace Voxel.MiddyNet.ProblemDetails.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class ProblemDetailsMiddlewareV2Should
    {
        private readonly ProblemDetailsMiddlewareV2 middleware;
        private readonly MiddyNetContext context;

        public ProblemDetailsMiddlewareV2Should()
        {
            middleware = new ProblemDetailsMiddlewareV2(null);
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            context.LambdaContext.InvokedFunctionArn.Returns("some-function-arn");
            context.LambdaContext.AwsRequestId.Returns("some-request-id");
        }

        [Fact]
        public async Task BeTransparentWhenNoExceptionsOccur()
        {
            var givenResponse = new APIGatewayHttpApiV2ProxyResponse
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
            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            response.StatusCode.Should().Be(500);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursDuringAfterMiddlewares()
        {
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException());
            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            response.StatusCode.Should().Be(500);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task OverrideResponseWhenAnExceptionOccursInHandler()
        {
            context.HandlerException = new InvalidOperationException();
            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            response.StatusCode.Should().Be(500);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task AggregateMultipleExceptionsInDetails()
        {
            context.MiddlewareBeforeExceptions.Add(new InvalidOperationException("middleware Before exception"));
            context.HandlerException = new InvalidOperationException("Handler exception");
            context.MiddlewareAfterExceptions.Add(new InvalidOperationException("middleware After exception"));

            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);

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
            var givenResponse = new APIGatewayHttpApiV2ProxyResponse
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
            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            response.Headers.Should().Contain(noCacheHeaders);
        }

        [Fact]
        public async Task KeepOriginalCookiesInResponse()
        {
            var givenCookies = new[] { "cookie-stuff" };
            var response = await middleware.After(new APIGatewayHttpApiV2ProxyResponse { Cookies = givenCookies }, context);
            response.Cookies.Should().BeEquivalentTo(givenCookies);
        }

        [Fact]
        public async Task FormatProblemsThatAreNotCausedByExceptions()
        {
            var givenResponse = new APIGatewayHttpApiV2ProxyResponse
            {
                Body = "some body",
                Headers = new Dictionary<string, string>(),
                StatusCode = 503
            };
            var response = await middleware.After(givenResponse, context);
            Approvals.Verify(response.Body);
        }

        [Fact]
        public async Task CleanupExceptionsFromContext()
        {
            context.HandlerException = new InvalidOperationException();
            _ = await middleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            context.MiddlewareBeforeExceptions.Should().BeEmpty();
            context.HandlerException.Should().BeNull();
            context.MiddlewareAfterExceptions.Should().BeEmpty();
        }

        [Fact]
        public async Task MapExceptionToStatusCode()
        {
            context.HandlerException = new InvalidOperationException("suppose this is a duplicate key exception");

            var options = new ProblemDetailsMiddlewareOptions();
            options.Map<InvalidOperationException>(409);

            var optionsMiddleware = new ProblemDetailsMiddlewareV2(options);

            var response = await optionsMiddleware.After(new APIGatewayHttpApiV2ProxyResponse(), context);
            response.StatusCode.Should().Be(409);
            Approvals.Verify(response.Body);
        }
    }


}
