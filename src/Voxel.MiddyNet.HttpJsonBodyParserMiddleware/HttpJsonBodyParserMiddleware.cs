using System;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware
{
    public class HttpJsonBodyParserMiddleware<T> : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            if (!HasJSONContentHeaders(lambdaEvent))
            {
                context.AdditionalContext.Add("Body", lambdaEvent.Body);
                return Task.CompletedTask;
            }

            T source;
            try
            {
                source = JsonConvert.DeserializeObject<T>(lambdaEvent.Body);
            }
            catch (JsonReaderException)
            {
                throw new Exception($"Error parsing \"{lambdaEvent.Body}\" to type {typeof(T)}");
            }
            
            context.AdditionalContext.Add("Body", source);
            return Task.CompletedTask;
        }

        private static bool HasJSONContentHeaders(APIGatewayProxyRequest lambdaEvent)
        {
            return lambdaEvent.Headers != null &&
                   (lambdaEvent.Headers.ContainsKey("Content-Type") &&
                    lambdaEvent.Headers["Content-Type"] == "application/json");
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context) => Task.FromResult(lambdaResponse);
    }
}