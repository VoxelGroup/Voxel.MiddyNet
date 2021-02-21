using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetails
{
    public class ProblemDetailsMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly ProblemDetailsMiddlewareOptions options;

        public ProblemDetailsMiddleware(ProblemDetailsMiddlewareOptions options = null) =>
            this.options = options ?? new ProblemDetailsMiddlewareOptions();

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context) => Task.CompletedTask;

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            if (!IsProblem(lambdaResponse?.StatusCode) && !context.HasExceptions)
                return Task.FromResult(lambdaResponse);

            var formattedResponse = new ResponseBuilder(options).BuildProblemDetailsContent(context, lambdaResponse);

            context.MiddlewareBeforeExceptions.Clear();
            context.MiddlewareAfterExceptions.Clear();
            context.HandlerException = null;

            return Task.FromResult(formattedResponse);
        }

        private bool IsProblem(int? statusCode) => statusCode == null || (statusCode >= 400 && statusCode < 600);
    }
}
