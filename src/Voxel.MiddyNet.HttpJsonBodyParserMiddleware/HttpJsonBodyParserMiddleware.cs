using System;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware
{
    public abstract class HttpJsonBodyParserMiddleware
    {
        public const string BodyContextKey = "Body";
    }
    public class HttpJsonBodyParserMiddleware<T> : HttpJsonBodyParserMiddleware, ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse> where T : new()
    {
        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            if (!HasJsonContentHeaders(lambdaEvent))
            {
                context.AdditionalContext.Add(BodyContextKey, lambdaEvent.Body);
                return Task.CompletedTask;
            }

            if (lambdaEvent.IsBase64Encoded)
            {
                lambdaEvent.Body = Encoding.UTF8.GetString(Convert.FromBase64String(lambdaEvent.Body));
            }

            var source = JsonSerializer.Deserialize<T>(lambdaEvent.Body);


            context.AdditionalContext.Add(BodyContextKey, source);
            return Task.CompletedTask;
        }

        private static bool HasJsonContentHeaders(APIGatewayProxyRequest lambdaEvent)
        {
            return lambdaEvent.Headers != null &&
                   (lambdaEvent.Headers.ContainsKey("Content-Type") &&
                    lambdaEvent.Headers["Content-Type"] == "application/json");
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context) => Task.FromResult(lambdaResponse);
    }
}