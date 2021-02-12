using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware
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

        public async Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            int statusCode = lambdaResponse?.StatusCode ?? 500;
            if (IsProblem(statusCode)) return BuildProblemDetailsContent(statusCode, context.LambdaContext?.InvokedFunctionArn);
            if (!context.HasExceptions) return lambdaResponse;
            string detailsJson = BuildProblemDetailsContent(statusCode, context);
            return new APIGatewayProxyResponse
            {
                IsBase64Encoded = false,
                StatusCode = 500,
                Headers = noCacheHeaders,
                Body = detailsJson
            };
        }

        private bool IsProblem(int statusCode) => statusCode >= 400 && statusCode < 600;

        private APIGatewayProxyResponse BuildProblemDetailsContent(int statusCode, string instance)
        {
            var statusDescription = ReasonPhrases.GetReasonPhrase(statusCode);
            return new APIGatewayProxyResponse
            {
                IsBase64Encoded = false,
                StatusCode = statusCode,
                Headers = noCacheHeaders,
                Body = $"{{\"Type\": \"https://httpstatuses.com/{statusCode}\",\"Title\":\"{statusDescription}\",\"Status\":\"{statusCode}\",\"Instance\":\"{instance}\"}}"
            };
        }

        private static string BuildProblemDetailsContent(int statusCode, MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);
            var detailsObject = BuildDetailsObject((dynamic)detailsException, statusCode, context.LambdaContext?.InvokedFunctionArn);
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
