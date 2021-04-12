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
            T source;
            try
            {
                source = JsonConvert.DeserializeObject<T>(lambdaEvent.Body);
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error parsing \"{lambdaEvent.Body}\" to type {typeof(T)}");
            }
            
            context.AdditionalContext.Add("Body", source);
            return Task.CompletedTask;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context) => Task.FromResult(lambdaResponse);
    }
}