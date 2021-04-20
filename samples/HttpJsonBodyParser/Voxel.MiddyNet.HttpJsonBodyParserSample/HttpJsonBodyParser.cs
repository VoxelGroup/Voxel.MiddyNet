using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.HttpJsonBodyParserMiddleware;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.HttpJsonBodyParserSample
{
    public class HttpJsonBodyParser : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public HttpJsonBodyParser()
        {
            Use(new HttpJsonBodyParserMiddleware<Person>());
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            var person = ((Person) context.AdditionalContext[Constants.BodyContextKey]);
            context.Logger.Log(LogLevel.Info, "Function called", new LogProperty(Constants.BodyContextKey, person));
            var result = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = $"Person name is {person.Name} {person.Surname} and it is {person.Age} years old."
            };

            return Task.FromResult(result);
        }
    }
}
