using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.HttpCors;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.HttpCorsSample
{
    public class HttpCorsHeaders : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public HttpCorsHeaders()
        {
            Use(new HttpCorsMiddleware(new CorsOptions
            {
                Origin = "*",
                Headers = "x-example",
                Credentials = true,
                CacheControl = "max-age=3600, s-maxage=3600, proxy-revalidate",
                MaxAge = "3600"
            }));
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            var result = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "hello from test"
            };

            return Task.FromResult(result);    
        }
    }
}
