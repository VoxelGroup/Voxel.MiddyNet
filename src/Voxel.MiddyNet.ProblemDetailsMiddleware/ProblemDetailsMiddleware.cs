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
            var statusCode = lambdaResponse?.StatusCode ?? 500;
            return (IsProblem(statusCode) || context.HasExceptions) 
                ? Task.FromResult(BuildProblemDetailsContent(statusCode, context, lambdaResponse))
                : Task.FromResult(lambdaResponse);
        }

        private bool IsProblem(int statusCode) => statusCode >= 400 && statusCode < 600;

        private APIGatewayProxyResponse BuildProblemDetailsContent(int statusCode, MiddyNetContext context, APIGatewayProxyResponse lambdaResponse) => context.HasExceptions
            ? new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = Merge(lambdaResponse.Headers),
                MultiValueHeaders = Merge(lambdaResponse.MultiValueHeaders),
                Body = BuildProblemDetailsExceptionsContent(statusCode, context.GetAllExceptions(), context.LambdaContext.AwsRequestId)
            }
            : new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = Merge(lambdaResponse.Headers),
                MultiValueHeaders = Merge(lambdaResponse.MultiValueHeaders),
                Body = BuildProblemDetailsProblemContent(statusCode, context.LambdaContext.AwsRequestId, ReasonPhrases.GetReasonPhrase(statusCode), lambdaResponse.Body)
            };

        private IDictionary<string, IList<string>> Merge(IDictionary<string, IList<string>> multiValueHeaders)
        {
            var merged = multiValueHeaders == null 
                ? new Dictionary<string, IList<string>>() 
                : new Dictionary<string, IList<string>>(multiValueHeaders);
            var contentTypes = merged.ContainsKey(HeaderNames.ContentType) ? merged[HeaderNames.ContentType] : new List<string>();
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

        private static string BuildProblemDetailsProblemContent(int statusCode, string instance, string statusDescription, string content) => 
            $"{{\"Type\": \"https://httpstatuses.com/{statusCode}\",\"Title\":\"{statusDescription}\",\"Status\":\"{statusCode}\",\"Instance\":\"{instance}\",\"Details\":\"{content}\"}}";

        private static string BuildProblemDetailsExceptionsContent(int statusCode, List<Exception> exceptions, string instance)
        {
            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);
            var detailsObject = BuildDetailsObject((dynamic)detailsException, statusCode, instance);
            return JsonSerializer.Serialize(detailsObject, new JsonSerializerOptions { WriteIndented = true });
        }

        private static object BuildDetailsObject(AggregateException exception, int statusCode, string instance) => new
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = nameof(AggregateException),
            Status = statusCode,
            Detail = exception.InnerExceptions.Select(ex => BuildDetailsObject((dynamic)ex, statusCode, instance)).ToArray(),
            Instance = instance
        };

        private static object BuildDetailsObject(Exception exception, int statusCode, string instance) => new
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = exception.GetType().Name,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance
        };
    }
}
