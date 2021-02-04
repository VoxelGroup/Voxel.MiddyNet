using Amazon.Lambda.APIGatewayEvents;
using System;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware
{
    public class ProblemDetailsMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly ProblemDetailsMiddlewareOptions options;
        public ProblemDetailsMiddleware(ProblemDetailsMiddlewareOptions options)
        {
            this.options = options;
        }

        public async Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            //if (!context.HasExceptions) 
                return lambdaResponse;
            //var exceptions = context.GetAllExceptions();

        }

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            return Task.CompletedTask;
        }
    }
}
