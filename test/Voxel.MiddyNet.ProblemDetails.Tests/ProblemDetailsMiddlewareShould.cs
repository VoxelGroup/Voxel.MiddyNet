using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FluentAssertions;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Voxel.MiddyNet.ProblemDetails.Tests
{
    public class ProblemDetailsMiddlewareShould
    {
        [Fact]
        public async Task ReturnThrownExceptionsInProblemDetailsFormat()
        {
            var function = GivenAFunctionThatThrowsExceptions();
            var lambdaContext = GivenSomeLambdaContext();

            var response = await function.Handler(new APIGatewayProxyRequest(), lambdaContext);

            response.StatusCode.Should().Be(500);
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.Body);
            problemDetails.MiddlewareBeforeExceptions.Length.Should().Be(1);
            problemDetails.MiddlewareAfterExceptions.Length.Should().Be(1);
            problemDetails.HandlerException.Should().NotBeNull();
        }

        [Fact]
        public async Task IncludeStackTraceInProblemDetails()
        {
            var function = GivenAFunctionThatThrowsExceptions(true);
            var lambdaContext = GivenSomeLambdaContext();

            var response = await function.Handler(new APIGatewayProxyRequest(), lambdaContext);

            response.StatusCode.Should().Be(500);
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.Body);
            problemDetails.MiddlewareBeforeExceptions.Single().StackTrace.Should().NotBeNullOrEmpty();
            problemDetails.MiddlewareAfterExceptions.Single().StackTrace.Should().NotBeNullOrEmpty();
            problemDetails.HandlerException.StackTrace.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task NotIncludeStackTraceInProblemDetails()
        {
            var function = GivenAFunctionThatThrowsExceptions(false);
            var lambdaContext = GivenSomeLambdaContext();

            var response = await function.Handler(new APIGatewayProxyRequest(), lambdaContext);

            response.StatusCode.Should().Be(500);
            var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.Body);
            problemDetails.MiddlewareBeforeExceptions.Single().StackTrace.Should().BeNullOrEmpty();
            problemDetails.MiddlewareAfterExceptions.Single().StackTrace.Should().BeNullOrEmpty();
            problemDetails.HandlerException.StackTrace.Should().BeNullOrEmpty();
        }

        private static TestFunction GivenAFunctionThatThrowsExceptions(bool includeException = false)
        {
            var function = new TestFunction();
            function.Use(new ProblemDetailsMiddleware(includeException));
            function.Use(new ThrowsExceptionMiddleware());
            return function;
        }

        private static ILambdaContext GivenSomeLambdaContext()
        {
            var lambdaContext = Substitute.For<ILambdaContext>();
            lambdaContext.AwsRequestId.Returns("some-request-id");
            lambdaContext.InvokedFunctionArn.Returns("the-function-arn");
            return lambdaContext;
        }


        private class TestFunction : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
        {
            protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
            {
                throw new NotImplementedException();
            }
        }

        private class ThrowsExceptionMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
        {
            public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
            {
                throw new NotImplementedException();
            }

            public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
