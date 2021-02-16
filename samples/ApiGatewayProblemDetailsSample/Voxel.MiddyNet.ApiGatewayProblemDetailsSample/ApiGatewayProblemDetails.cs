using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System;
using System.Threading.Tasks;
using Voxel.MiddyNet.ProblemDetails;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace Voxel.MiddyNet.ApiGatewayProblemDetailsSample
{
    public class ApiGatewayProblemDetails : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public ApiGatewayProblemDetails()
        {
            Use(new ProblemDetailsMiddleware(new ProblemDetailsMiddlewareOptions().Map<NotImplementedException>(123)));
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            throw new NotImplementedException("this will be used in the problem details description");
        }
    }
}
