using System;
using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware
{
    public class HttpJsonBodyParserMiddleware<T> : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            if (!HasJsonContentHeaders(lambdaEvent))
            {
                context.AdditionalContext.Add("Body", lambdaEvent.Body);
                return Task.CompletedTask;
            }

            if (lambdaEvent.IsBase64Encoded)
            {
                lambdaEvent.Body = Encoding.UTF8.GetString(Convert.FromBase64String(lambdaEvent.Body));
            }
           
            T source;
            try
            {
                source = JsonConvert.DeserializeObject<T>(lambdaEvent.Body);
            }
            catch (JsonReaderException)
            {
                throw new Exception("Content type defined as JSON but an invalid JSON was provided");
            }
            
            context.AdditionalContext.Add("Body", source);
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