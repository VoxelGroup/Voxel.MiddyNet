using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;

namespace Voxel.MiddyNet.ProblemDetails
{
    internal class ContentResponseBuilder : ProxyResponseBuilder
    {
        public APIGatewayProxyResponse CreateProblemResponse(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
        {
            var statusCode = lambdaResponse?.StatusCode ?? 500;
            return new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = JsonSerializer.Serialize(BuildProblemDetailsProblemContent(statusCode, context.LambdaContext.InvokedFunctionArn, context.LambdaContext.AwsRequestId, ReasonPhrases.GetReasonPhrase(statusCode), lambdaResponse?.Body), jsonSerializerOptions)
            };
        }
    }
}