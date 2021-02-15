using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetails
{
    public class ProblemDetailsMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly Dictionary<string, string> noCacheHeaders = new Dictionary<string, string>
        {
            [HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate",
            [HeaderNames.Pragma] = "no-cache",
            [HeaderNames.Expires] = "0"
        };

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context) => Task.CompletedTask;

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            var statusCode = lambdaResponse?.StatusCode;
            
            if (!IsProblem(statusCode) && !context.HasExceptions) 
                return Task.FromResult(lambdaResponse);

            var formattedResponse = Task.FromResult(BuildProblemDetailsContent(statusCode, context, lambdaResponse));

            context.MiddlewareBeforeExceptions.Clear();
            context.MiddlewareAfterExceptions.Clear();
            context.HandlerException = null;

            return formattedResponse;
        }

        private bool IsProblem(int? statusCode) => statusCode == null || (statusCode >= 400 && statusCode < 600);

        private APIGatewayProxyResponse BuildProblemDetailsContent(int? statusCode, MiddyNetContext context, APIGatewayProxyResponse lambdaResponse) => context.HasExceptions
            ? new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = BuildProblemDetailsExceptionsContent(500, context)
            }
            : new APIGatewayProxyResponse
            {
                StatusCode = statusCode ?? 500,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = BuildProblemDetailsProblemContent(statusCode ?? 500, context.LambdaContext.InvokedFunctionArn, context.LambdaContext.AwsRequestId, ReasonPhrases.GetReasonPhrase(statusCode??500), lambdaResponse?.Body)
            };

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
            foreach(var kv in noCacheHeaders)
            {
                merged[kv.Key] = kv.Value;
            }
            return merged;
        }

        private static string BuildProblemDetailsProblemContent(int statusCode, string instance, string requestId, string statusDescription, string content) => 
            $"{{\"Type\": \"https://httpstatuses.com/{statusCode}\",\"Title\":\"{statusDescription}\",\"Status\":\"{statusCode}\",\"Details\":\"{content}\",\"Instance\":\"{instance}\",\"AwsRequestId\":\"{requestId}\"}}";

        private static string BuildProblemDetailsExceptionsContent(int statusCode, MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var instance = context.LambdaContext.InvokedFunctionArn;
            var requestId = context.LambdaContext.AwsRequestId;

            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);

            var detailsObject = BuildDetailsObject((dynamic)detailsException, statusCode, instance, requestId);

            return JsonSerializer.Serialize(detailsObject, new JsonSerializerOptions { WriteIndented = true });
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

        private static string ComposeDetail(IEnumerable<Exception> exceptions) => string.Join(", ", exceptions.Select(e => $"{e.GetType().Name}: {e.Message}"));

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
