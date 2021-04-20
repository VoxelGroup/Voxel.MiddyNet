using System;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware
{
    public class HttpV2JsonBodyParserMiddleware<T> : ILambdaMiddleware<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        private const string BodyContextKey = "Body";

        public Task Before(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context)
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

        private static bool HasJsonContentHeaders(APIGatewayHttpApiV2ProxyRequest lambdaEvent)
        {
            return lambdaEvent.Headers != null &&
                   (lambdaEvent.Headers.ContainsKey("Content-Type") &&
                    lambdaEvent.Headers["Content-Type"] == "application/json");
        }

        public Task<APIGatewayHttpApiV2ProxyResponse> After(APIGatewayHttpApiV2ProxyResponse lambdaResponse, MiddyNetContext context) => Task.FromResult(lambdaResponse);
    }
}