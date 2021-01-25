using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Voxel.MiddyNet.HttpCors;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.HttpV2CorsSample
{
    public class HttpV2CorsHeaders : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public HttpV2CorsHeaders()
        {
            Use(new HttpV2CorsMiddleware(new CorsOptions
            {
                Origin = "*",
                Headers = "x-example",
                Credentials = true,
                CacheControl = "max-age=3600, s-maxage=3600, proxy-revalidate",
                MaxAge = "3600"
            }));
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context)
        {
            var result = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "hello from test"
            };

            return Task.FromResult(result);    
        }
    }
}
