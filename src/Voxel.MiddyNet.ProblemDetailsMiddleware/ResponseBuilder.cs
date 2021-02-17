using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Voxel.MiddyNet.ProblemDetails
{
    public class ResponseBuilder
    {
        private static readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        private static readonly Dictionary<string, string> noCacheHeaders = new Dictionary<string, string>
        {
            [HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate",
            [HeaderNames.Pragma] = "no-cache",
            [HeaderNames.Expires] = "0"
        };

        private readonly ProblemDetailsMiddlewareOptions options;

        public ResponseBuilder(ProblemDetailsMiddlewareOptions options)
        {
            this.options = options;
        }

        public APIGatewayProxyResponse BuildProblemDetailsContent(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
        {
            return context.HasExceptions
                ? CreateExceptionResponse(context, lambdaResponse)
                : CreateProblemResponse(context, lambdaResponse);
        }

        private APIGatewayProxyResponse CreateProblemResponse(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
        {
            var statusCode = lambdaResponse?.StatusCode ?? 500;
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = BuildProblemDetailsProblemContent(statusCode, context.LambdaContext.InvokedFunctionArn, context.LambdaContext.AwsRequestId, ReasonPhrases.GetReasonPhrase(statusCode), lambdaResponse?.Body)
            };
        }

        private APIGatewayProxyResponse CreateExceptionResponse(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
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

        private IDictionary<string, IList<string>> Merge(IDictionary<string, IList<string>> multiValueHeaders)
        {
            var merged = multiValueHeaders == null
                ? new Dictionary<string, IList<string>>()
                : new Dictionary<string, IList<string>>(multiValueHeaders);

            var contentTypes = merged.ContainsKey(HeaderNames.ContentType)
                ? merged[HeaderNames.ContentType]
                : new List<string>();

            if (!contentTypes.Contains("application/problem+json"))
                contentTypes.Add("application/problem+json");

            merged[HeaderNames.ContentType] = contentTypes;

            return merged;
        }

        private IDictionary<string, string> Merge(IDictionary<string, string> responseHeaders)
        {
            var merged = responseHeaders == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(responseHeaders);

            foreach (var kv in noCacheHeaders)
            {
                merged[kv.Key] = kv.Value;
            }

            return merged;
        }

        private (int statusCode, string body) BuildProblemDetailsExceptions(MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var instance = context.LambdaContext.InvokedFunctionArn;
            var requestId = context.LambdaContext.AwsRequestId;

            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);

            if (options.TryMap(detailsException.GetType(), out int statusCode))
                return (statusCode, BuildProblemDetailsProblemContent(statusCode, instance, requestId, ReasonPhrases.GetReasonPhrase(statusCode), ComposeDetail(new[] { detailsException })));

            var detailsObject = BuildDetailsObject((dynamic)detailsException, statusCode, instance, requestId);

            return (statusCode, JsonSerializer.Serialize(detailsObject, jsonSerializerOptions));
        }

        private static string BuildProblemDetailsProblemContent(int statusCode, string instance, string requestId, string statusDescription, string content) => JsonSerializer.Serialize(new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = statusDescription,
            Status = statusCode,
            Detail = content,
            Instance = instance,
            AwsRequestId = requestId
        }, jsonSerializerOptions);

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

        private class DetailsObject
        {
            public string Type { get; set; }
            public string Title { get; set; }
            public int Status { get; set; }
            public string Detail { get; set; }
            public string Instance { get; set; }
            public string AwsRequestId { get; set; }
        }

    }
}