using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voxel.MiddyNet.ProblemDetails
{
    public class ProblemDetailsMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly bool includeFullException;

        public ProblemDetailsMiddleware(bool includeFullException = false)
        {
            this.includeFullException = includeFullException;
        }

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            return Task.CompletedTask;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            if (context.HasExceptions)
            {
                var details = ProblemDetails(context);

                lambdaResponse ??= new APIGatewayProxyResponse();
                lambdaResponse.IsBase64Encoded = false;
                lambdaResponse.StatusCode = 500;
                lambdaResponse.Body = JsonConvert.SerializeObject(details);

                context.Clear();
            }

            return Task.FromResult(lambdaResponse);
        }

        private ProblemDetails ProblemDetails(MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var details = new ProblemDetails()
            {
                Detail = $"There have been {exceptions.Count} exceptions during the execution of this function",
                Status = 500,
                Title = "Problem Details",
                Type = "arn://" + context.LambdaContext.InvokedFunctionArn,
                Instance = context.LambdaContext.AwsRequestId
            };

            if (context.MiddlewareBeforeExceptions.Count > 0)
                details.MiddlewareBeforeExceptions = context.MiddlewareBeforeExceptions.Select(ex => new ExceptionDescrition
                {
                    Message = ex.Message,
                    StackTrace = includeFullException ? ex.StackTrace : null
                }).ToArray();

            if (context.HandlerException != null)
                details.HandlerException = new ExceptionDescrition
                {
                    Message = context.HandlerException.Message,
                    StackTrace = includeFullException ? context.HandlerException.StackTrace : null
                };

            if (context.MiddlewareAfterExceptions.Count > 0)
                details.MiddlewareAfterExceptions = context.MiddlewareAfterExceptions.Select(ex => new ExceptionDescrition
                {
                    Message = ex.Message,
                    StackTrace = includeFullException ? ex.StackTrace : null
                }).ToArray();

            return details;
        }
    }


    /// <summary>
    /// https://tools.ietf.org/html/rfc7807.
    /// </summary>
    public class ProblemDetails
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "status")]
        public int? Status { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "detail")]
        public string Detail { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "instance")]
        public string Instance { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "middlewareBeforeExceptions")]
        public ExceptionDescrition[] MiddlewareBeforeExceptions { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "middlewareAfterExceptions")]
        public ExceptionDescrition[] MiddlewareAfterExceptions { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "handlerException")]
        public ExceptionDescrition HandlerException { get; set; }
    }

    public class ExceptionDescrition
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }

}
