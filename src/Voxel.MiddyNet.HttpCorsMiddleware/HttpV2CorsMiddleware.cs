using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using static System.String;

namespace Voxel.MiddyNet.HttpCorsMiddleware
{
    public class HttpV2CorsMiddleware : ILambdaMiddleware<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        private readonly CorsOptions corsOptions;
        private string incomingOrigin = Empty;
        private string httpMethod;

        public HttpV2CorsMiddleware() : this(new CorsOptions())
        {
        }

        public HttpV2CorsMiddleware(CorsOptions corsOptions)
        {
            this.corsOptions = corsOptions;
        }

        public bool InterruptsExecution => false;

        private const string DefaultAccessControlAllowOrigin = "*";
        private const string AllowOriginHeader = "Access-Control-Allow-Origin";
        private const string AllowHeadersHeader = "Access-Control-Allow-Headers";
        private const string AllowCredentialsHeader = "Access-Control-Allow-Credentials";
        private const string CacheControlHeader = "Cache-Control";
        private const string MaxAgeHeader = "Access-Control-Max-Age";

        public Task Before(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context)
        {
            incomingOrigin = GetOriginHeader(lambdaEvent);

            httpMethod = lambdaEvent.RequestContext.Http.Method;
            
            return Task.CompletedTask;
        }
        
        private string GetOriginHeader(APIGatewayHttpApiV2ProxyRequest lambdaEvent)
        {
            var capitalCaseOriginHeaderValue =
                lambdaEvent.Headers.ContainsKey("Origin") ? lambdaEvent.Headers["Origin"] : Empty;
            var lowerCaseOriginHeaderValue =
                lambdaEvent.Headers.ContainsKey("origin") ? lambdaEvent.Headers["origin"] : Empty;

            return !IsNullOrWhiteSpace(capitalCaseOriginHeaderValue)
                ? capitalCaseOriginHeaderValue
                : lowerCaseOriginHeaderValue;
        }

        public Task<APIGatewayHttpApiV2ProxyResponse> After(APIGatewayHttpApiV2ProxyResponse lambdaResponse, MiddyNetContext context)
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

        private void SetMaxAgeHeader(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            if (!IsNullOrWhiteSpace(corsOptions.MaxAge) && !lambdaResponse.Headers.ContainsKey(MaxAgeHeader))
            {
                lambdaResponse.Headers.Add(MaxAgeHeader, corsOptions.MaxAge);
            }
        }

        private void SetCacheControlHeader(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            if (httpMethod == "OPTIONS" && !IsNullOrWhiteSpace(corsOptions.CacheControl) &&
                !lambdaResponse.Headers.ContainsKey(CacheControlHeader))
            {
                lambdaResponse.Headers.Add(CacheControlHeader, corsOptions.CacheControl);
            }
        }

        private void SetAllowCredentialsHeader(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            if (!lambdaResponse.Headers.ContainsKey(AllowCredentialsHeader))
            {
                lambdaResponse.Headers.Add(AllowCredentialsHeader, corsOptions.Credentials.ToString().ToLowerInvariant());
            }
        }

        private void SetAllowHeadersHeader(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            if (!lambdaResponse.Headers.ContainsKey(AllowHeadersHeader) && !IsNullOrWhiteSpace(corsOptions.Headers))
            {
                lambdaResponse.Headers.Add(AllowHeadersHeader, corsOptions.Headers);
            }
        }

        private void SetAllowOriginHeader(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
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

        private static void InitialiseHeaders(APIGatewayHttpApiV2ProxyResponse lambdaResponse)
        {
            if (lambdaResponse.Headers == null)
            {
                lambdaResponse.Headers = new Dictionary<string, string>();
            }
        }
    }
}
