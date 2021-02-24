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

            var problemDetailsResponse = context.HasExceptions
                ? new ExceptionResponseBuilder(options).CreateExceptionResponse(context, lambdaResponse)
                : new ContentResponseBuilder().CreateProblemResponse(context, lambdaResponse);

            context.MiddlewareBeforeExceptions.Clear();
            context.MiddlewareAfterExceptions.Clear();
            context.HandlerException = null;

            return Task.FromResult(problemDetailsResponse);
        }

        private static bool IsProblem(int? statusCode) => statusCode == null || (statusCode >= 400 && statusCode < 600);
    }
}
