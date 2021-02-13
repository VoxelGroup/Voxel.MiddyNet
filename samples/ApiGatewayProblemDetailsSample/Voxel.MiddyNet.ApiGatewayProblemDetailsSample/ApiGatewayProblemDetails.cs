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
            Use(new ProblemDetailsMiddleware());
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            throw new NotImplementedException("this will be used as the problem details description");
        }
    }
}
