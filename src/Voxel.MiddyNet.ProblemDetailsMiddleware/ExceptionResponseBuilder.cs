﻿using Amazon.Lambda.APIGatewayEvents;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware
{
    public class ExceptionResponseBuilder: ProxyResponseBuilder
    {
        private readonly ProblemDetailsMiddlewareOptions options;

        public ExceptionResponseBuilder(ProblemDetailsMiddlewareOptions options) => this.options = options;

        public APIGatewayProxyResponse CreateExceptionResponse(MiddyNetContext context, APIGatewayProxyResponse lambdaResponse)
        {
            var detailsObject = BuildProblemDetailsExceptions(context);
            return new APIGatewayProxyResponse
            {
                StatusCode = detailsObject.Status,
                Headers = Merge(lambdaResponse?.Headers),
                MultiValueHeaders = Merge(lambdaResponse?.MultiValueHeaders),
                Body = detailsObject.ToJsonString()
            };
        }

        public APIGatewayHttpApiV2ProxyResponse CreateExceptionResponse(MiddyNetContext context, APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            var detailsObject = BuildProblemDetailsExceptions(context);
            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = detailsObject.Status,
                Headers = Merge(lambdaResponse?.Headers),
                Cookies = lambdaResponse?.Cookies,
                Body = detailsObject.ToJsonString()
            };
        }

        private DetailsObject BuildProblemDetailsExceptions(MiddyNetContext context)
        {
            var exceptions = context.GetAllExceptions();
            var instance = context.LambdaContext.InvokedFunctionArn;
            var requestId = context.LambdaContext.AwsRequestId;

            var detailsException = exceptions.Count == 1
                ? exceptions[0]
                : new AggregateException(exceptions);

            return options.TryMap(detailsException.GetType(), out int statusCode)
                ? BuildProblemDetailsProblemContent(statusCode, instance, requestId, ReasonPhrases.GetReasonPhrase(statusCode), ComposeDetail(new[] { detailsException }))
                : BuildDetailsObject((dynamic)detailsException, statusCode, instance, requestId);
        }

        private static DetailsObject BuildDetailsObject(AggregateException exception, int statusCode, string instance, string requestId) => new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = nameof(AggregateException),
            Status = statusCode,
            Detail = ComposeDetail(exception.InnerExceptions),
            Instance = instance,
            AwsRequestId = requestId
        };

        private static DetailsObject BuildDetailsObject(Exception exception, int statusCode, string instance, string requestId) => new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = exception.GetType().Name,
            Status = statusCode,
            Detail = exception.Message,
            Instance = instance,
            AwsRequestId = requestId
        };

        private static string ComposeDetail(IEnumerable<Exception> exceptions) =>
            string.Join(", ", exceptions.Select(e => $"{e.GetType().Name}: {e.Message}"));

    }
}