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

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            var statusCode = lambdaResponse?.StatusCode ?? 500;
            return (IsProblem(statusCode) || context.HasExceptions) 
                ? Task.FromResult(BuildProblemDetailsContent(statusCode, context)) 
                : Task.FromResult(lambdaResponse);
        }

        private bool IsProblem(int statusCode) => statusCode >= 400 && statusCode < 600;

        private APIGatewayProxyResponse BuildProblemDetailsContent(int statusCode, MiddyNetContext context) => context.HasExceptions
            ? new APIGatewayProxyResponse
            {
                StatusCode = 500,
                Headers = noCacheHeaders,
                Body = BuildProblemDetailsExceptionsContent(statusCode, context.GetAllExceptions(), context.LambdaContext.InvokedFunctionArn)
            }
            : new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = noCacheHeaders,
                Body = BuildProblemDetailsProblemContent(statusCode, context.LambdaContext.InvokedFunctionArn, ReasonPhrases.GetReasonPhrase(statusCode))
            };

        private static string BuildProblemDetailsProblemContent(int statusCode, string instance, string statusDescription) => $"{{\"Type\": \"https://httpstatuses.com/{statusCode}\",\"Title\":\"{statusDescription}\",\"Status\":\"{statusCode}\",\"Instance\":\"{instance}\"}}";

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
