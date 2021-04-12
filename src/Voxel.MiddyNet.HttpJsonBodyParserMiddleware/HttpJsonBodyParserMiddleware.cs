using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.HttpJsonBodyParserMiddleware
{
    public class HttpJsonBodyParserMiddleware<T> : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            var source = JsonConvert.DeserializeObject<T>(lambdaEvent.Body);
            context.AdditionalContext.Add("Body", source);
            return Task.CompletedTask;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context) => Task.FromResult(lambdaResponse);
    }
}