using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Voxel.MiddyNet.ProblemDetails
{
    internal class ExceptionResponseBuilder: ProxyResponseBuilder
    {
        private readonly ProblemDetailsMiddlewareOptions options;

        public ExceptionResponseBuilder(ProblemDetailsMiddlewareOptions options) => this.options = options;

        public APIGatewayProxyResponse CreateExceptionResponse(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
        {
            (int statusCode, string body) = BuildProblemDetailsExceptions(context);
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = body
            };
        }

        private (int statusCode, string body) BuildProblemDetailsExceptions(MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var instance = context.LambdaContext.InvokedFunctionArn;
            var requestId = context.LambdaContext.AwsRequestId;

            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);

            var detailsObject = options.TryMap(detailsException.GetType(), out int statusCode)
                ? BuildProblemDetailsProblemContent(statusCode, instance, requestId, ReasonPhrases.GetReasonPhrase(statusCode), ComposeDetail(new[] { detailsException }))
                : BuildDetailsObject((dynamic)detailsException, statusCode, instance, requestId);

            return (statusCode, JsonSerializer.Serialize(detailsObject, jsonSerializerOptions));
        }

        private static DetailsObject BuildDetailsObject(AggregateException exception, int statusCode, string instance, string requestId) => new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = nameof(AggregateException),
            Status = statusCode,
            Detail = ComposeDetail(exception.InnerExceptions),
            Instance = instance,
            AwsRequestId = requestId
        };

        private static DetailsObject BuildDetailsObject(Exception exception, int statusCode, string instance, string requestId) => new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = exception.GetType().Name,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance,
            AwsRequestId = requestId
        };

        private static string ComposeDetail(IEnumerable<Exception> exceptions) =>
            string.Join(", ", exceptions.Select(e => $"{e.GetType().Name}: {e.Message}"));

    }
}