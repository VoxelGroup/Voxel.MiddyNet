using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;

namespace Voxel.MiddyNet.HttpCorsMiddleware
{
    public class HttpCorsMiddleware : ILambdaMiddleware<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        private readonly CorsOptions corsOptions;
        private string incomingOrigin = Empty;
        private string httpMethod;

        public HttpCorsMiddleware() : this(new CorsOptions())
        {
        }

        public HttpCorsMiddleware(CorsOptions corsOptions)
        {
            this.corsOptions = corsOptions;
        }

        private const string DefaultAccessControlAllowOrigin = "*";
        private const string AllowOriginHeader = "Access-Control-Allow-Origin";
        private const string AllowHeadersHeader = "Access-Control-Allow-Headers";
        private const string AllowCredentialsHeader = "Access-Control-Allow-Credentials";
        private const string CacheControlHeader = "Cache-Control";
        private const string MaxAgeHeader = "Access-Control-Max-Age";

        public Task Before(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            incomingOrigin = GetOriginHeader(lambdaEvent);

            httpMethod = lambdaEvent.HttpMethod;

            return Task.CompletedTask;
        }

        private string GetOriginHeader(APIGatewayProxyRequest lambdaEvent)
        {
            var capitalCaseOriginHeaderValue =
                lambdaEvent.Headers.ContainsKey("Origin") ? lambdaEvent.Headers["Origin"] : String.Empty;
            var lowerCaseOriginHeaderValue =
                lambdaEvent.Headers.ContainsKey("origin") ? lambdaEvent.Headers["origin"] : String.Empty;

            return !IsNullOrWhiteSpace(capitalCaseOriginHeaderValue)
                ? capitalCaseOriginHeaderValue
                : lowerCaseOriginHeaderValue;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            if (IsNullOrWhiteSpace(httpMethod)) return Task.FromResult(lambdaResponse);

            InitialiseHeaders(lambdaResponse);

            SetAllowOriginHeader(lambdaResponse);

            SetAllowHeadersHeader(lambdaResponse);

            SetAllowCredentialsHeader(lambdaResponse);

            SetCacheControlHeader(lambdaResponse);

            SetMaxAgeHeader(lambdaResponse);
            
            return Task.FromResult(lambdaResponse);
        }

        private void SetMaxAgeHeader(APIGatewayProxyResponse lambdaResponse)
        {
            if (!IsNullOrWhiteSpace(corsOptions.MaxAge) && !lambdaResponse.Headers.ContainsKey(MaxAgeHeader))
            {
                lambdaResponse.Headers.Add(MaxAgeHeader, corsOptions.MaxAge);
            }
        }

        private void SetCacheControlHeader(APIGatewayProxyResponse lambdaResponse)
        {
            if (httpMethod == "OPTIONS" && !IsNullOrWhiteSpace(corsOptions.CacheControl) &&
                !lambdaResponse.Headers.ContainsKey(CacheControlHeader))
            {
                lambdaResponse.Headers.Add(CacheControlHeader, corsOptions.CacheControl);
            }
        }

        private void SetAllowCredentialsHeader(APIGatewayProxyResponse lambdaResponse)
        {
            if (!lambdaResponse.Headers.ContainsKey(AllowCredentialsHeader))
            {
                lambdaResponse.Headers.Add(AllowCredentialsHeader, corsOptions.Credentials.ToString().ToLowerInvariant());
            }
        }

        private void SetAllowHeadersHeader(APIGatewayProxyResponse lambdaResponse)
        {
            if (!lambdaResponse.Headers.ContainsKey(AllowHeadersHeader) && !IsNullOrWhiteSpace(corsOptions.Headers))
            {
                lambdaResponse.Headers.Add(AllowHeadersHeader, corsOptions.Headers);
            }
        }

        private void SetAllowOriginHeader(APIGatewayProxyResponse lambdaResponse)
        {
            if (!lambdaResponse.Headers.ContainsKey(AllowOriginHeader))
            {
                if (corsOptions.Origins.Contains(incomingOrigin))
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, incomingOrigin);
                }
                else if (corsOptions.Origins.Any())
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, corsOptions.Origins[0]);
                }
                else if (!IsNullOrWhiteSpace(corsOptions.Origin))
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, corsOptions.Origin);
                }
                else
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, DefaultAccessControlAllowOrigin);
                }
            }
        }

        private static void InitialiseHeaders(APIGatewayProxyResponse lambdaResponse)
        {
            if (lambdaResponse.Headers == null)
            {
                lambdaResponse.Headers = new Dictionary<string, string>();
            }
        }
    }
}
