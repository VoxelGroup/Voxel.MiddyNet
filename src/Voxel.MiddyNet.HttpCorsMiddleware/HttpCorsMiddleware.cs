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
        private string incomingOrigin = string.Empty;
        private string httpMethod;

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
                lambdaEvent.Headers.ContainsKey("Origin") ? lambdaEvent.Headers["Origin"] : string.Empty;
            var lowerCaseOriginHeaderValue =
                lambdaEvent.Headers.ContainsKey("origin") ? lambdaEvent.Headers["origin"] : string.Empty;

            return !string.IsNullOrWhiteSpace(capitalCaseOriginHeaderValue)
                ? capitalCaseOriginHeaderValue
                : lowerCaseOriginHeaderValue;
        }

        public Task<APIGatewayProxyResponse> After(APIGatewayProxyResponse lambdaResponse, MiddyNetContext context)
        {
            if (string.IsNullOrWhiteSpace(httpMethod)) return Task.FromResult(lambdaResponse);

            if (lambdaResponse.Headers == null)
            {
                lambdaResponse.Headers = new Dictionary<string, string>();
            }

            if (!lambdaResponse.Headers.ContainsKey(AllowOriginHeader))
            {
                if (corsOptions.Origins.Contains(incomingOrigin) )
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, incomingOrigin);
                }
                else if (corsOptions.Origins.Any())
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, corsOptions.Origins[0]);
                }
                else if (!string.IsNullOrWhiteSpace(corsOptions.Origin))
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, corsOptions.Origin);
                }
                else
                {
                    lambdaResponse.Headers.Add(AllowOriginHeader, DefaultAccessControlAllowOrigin);
                }
            }

            if (!lambdaResponse.Headers.ContainsKey(AllowHeadersHeader))
            {
                if (!string.IsNullOrWhiteSpace(corsOptions.Headers))
                {
                    lambdaResponse.Headers.Add(AllowHeadersHeader, corsOptions.Headers);
                }
            }

            if (!lambdaResponse.Headers.ContainsKey(AllowCredentialsHeader))
            {
                lambdaResponse.Headers.Add(AllowCredentialsHeader, corsOptions.Credentials.ToString().ToLowerInvariant());
            }

            if (httpMethod == "OPTIONS" && !string.IsNullOrWhiteSpace(corsOptions.CacheControl) && !lambdaResponse.Headers.ContainsKey(CacheControlHeader))
            {
                lambdaResponse.Headers.Add(CacheControlHeader, corsOptions.CacheControl);
            }

            if (!string.IsNullOrWhiteSpace(corsOptions.MaxAge) && !lambdaResponse.Headers.ContainsKey(MaxAgeHeader))
            {
                lambdaResponse.Headers.Add(MaxAgeHeader, corsOptions.MaxAge);
            }
            

            return Task.FromResult(lambdaResponse);
        }
    }

    public class CorsOptions
    {
        public string Origin { get; set; }
        public string[] Origins { get; set; }
        public string Headers { get; set; }
        public bool Credentials { get; set; }
        public string CacheControl { get; set; }
        public string MaxAge { get; set; }

        public CorsOptions()
        {
            Origins = Array.Empty<string>();
        }
    }
}
