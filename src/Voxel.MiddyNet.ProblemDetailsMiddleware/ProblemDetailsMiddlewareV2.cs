using Amazon.Lambda.APIGatewayEvents;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware
{
    public class ProblemDetailsMiddlewareV2 : ILambdaMiddleware<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        private readonly ProblemDetailsMiddlewareOptions options;

        public ProblemDetailsMiddlewareV2(ProblemDetailsMiddlewareOptions options = null) =>
            this.options = options ?? new ProblemDetailsMiddlewareOptions();

        public bool InterruptsExecution => false;

        public Task Before(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context) => Task.CompletedTask;

        public Task<APIGatewayHttpApiV2ProxyResponse> After(APIGatewayHttpApiV2ProxyResponse lambdaResponse, MiddyNetContext context)
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
