using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.HttpJsonBodyParserMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.HttpV2JsonBodyParserSample
{
    public class HttpV2JsonBodyParser : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public HttpV2JsonBodyParser()
        {
            Use(new HttpV2JsonBodyParserMiddleware<Person>());
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context)
        {
            var person = ((Person)context.AdditionalContext[Constants.BodyContextKey]);
            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty(Constants.BodyContextKey, person));
            var result = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = $"Person name is {person.Name} {person.Surname} and it is {person.Age} years old."
            };

            return Task.FromResult(result);
        }
    }
}

