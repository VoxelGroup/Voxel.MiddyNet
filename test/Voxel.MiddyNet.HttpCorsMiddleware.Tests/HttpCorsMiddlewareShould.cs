using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.HttpCorsMiddleware.Tests
{
    public class HttpCorsMiddlewareShould
    {
        private MiddyNetContext context;
        private APIGatewayProxyRequest request;

        public HttpCorsMiddlewareShould()
        {
            context = new MiddyNetContext(Substitute.For<ILambdaContext>());
            request = new APIGatewayProxyRequest
            {
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
            };
        }

        [Fact]
        public async Task DefaultAccessControlAllowOriginHeaderToAsterisk()
        {
            var middleware = new HttpCorsMiddleware();

            var previousResponse = new APIGatewayProxyResponse();

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
        }

        [Fact]
        public async Task NotOverrideAlreadyDeclaredAccessControlAllowOriginHeader()
        {
            var middleware = new HttpCorsMiddleware();

            var previousResponse = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"Access-Control-Allow-Origin", "http://example.com" }
                }
            };

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "http://example.com");
        }

        [Fact]
        public async Task UseOriginSpecifiedInOptions()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions{Origin = "http://example.com"});

            var previousResponse = new APIGatewayProxyResponse();

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "http://example.com");
        }

        [Theory]
        [InlineData("Origin")]
        [InlineData("origin")]
        public async Task ReturnWhitelistedOrigin(string headerName)
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions { Origins = new []{ "http://example.com", "http://another-example.com"} });

            var previousResponse = new APIGatewayProxyResponse();
            request.Headers.Add(headerName, "http://another-example.com");

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "http://another-example.com");
        }

        [Theory]
        [InlineData("Origin")]
        [InlineData("origin")]
        public async Task ReturnFirstOriginAsDefaultIfNoMatch(string headerName)
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions { Origins = new[] { "http://example.com", "http://another-example.com" } });

            var previousResponse = new APIGatewayProxyResponse();
            request.Headers.Add(headerName, "http://yet-another-example.com");

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "http://example.com");
        }

        [Fact]
        public async Task UseAllowedHeadersSpecifiedInOptions()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                Headers = "x-example"
            });

            var previousResponse = new APIGatewayProxyResponse();

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            response.Headers.Should().Contain("Access-Control-Allow-Headers", "x-example");
        }

        [Fact]
        public async Task NotOverrideAlreadyDeclaredAccessControlAllowHeadersHeader()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                Headers = "x-another-example"
            });

            var previousResponse = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"Access-Control-Allow-Headers", "x-example" }
                }
            };

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            response.Headers.Should().Contain("Access-Control-Allow-Headers", "x-example");
        }

        [Theory]
        [InlineData("false", true)]
        [InlineData("true", false)]
        public async Task NotOverrideAlreadyDeclaredAccessControlAllowCredentialsHeader(string incomingHeader,
            bool configValue)
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                Credentials = configValue
            });

            var previousResponse = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    {"Access-Control-Allow-Credentials", incomingHeader }
                }
            };

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            response.Headers.Should().Contain("Access-Control-Allow-Credentials", incomingHeader);
        }

        [Theory]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        public async Task UseChangeCredentialsAsSpecifiedInOptions(bool configValue, string expectedHeader)
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                Credentials = configValue
            });

            var previousResponse = new APIGatewayProxyResponse();

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            response.Headers.Should().Contain("Access-Control-Allow-Credentials", expectedHeader);
        }

        [Fact]
        public async Task NotChangeAnythingIfHttpMethodIsNotPresentInTheRequest()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions());

            var previousResponse = new APIGatewayProxyResponse();

            request.HttpMethod = string.Empty;

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Should().Be(previousResponse);
        }

        [Theory]
        [InlineData("max-age=3600, s-maxage=3600, proxy-revalidate", "OPTIONS", "max-age=3600, s-maxage=3600, proxy-revalidate")]
        [InlineData("max-age=3600, s-maxage=3600, proxy-revalidate", "GET", "")]
        [InlineData("max-age=3600, s-maxage=3600, proxy-revalidate", "PUT", "")]
        [InlineData("max-age=3600, s-maxage=3600, proxy-revalidate", "POST", "")]
        [InlineData("max-age=3600, s-maxage=3600, proxy-revalidate", "PATCH", "")]
        public async Task SetCacheControlHeaderIfPresentInConfigAndHttpMethodIsOptions(string configValue, string httpMethod, string expectedHeader)
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                CacheControl = configValue
            });

            var previousResponse = new APIGatewayProxyResponse();

            request.HttpMethod = httpMethod;

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            if(!string.IsNullOrWhiteSpace(expectedHeader))
                response.Headers.Should().Contain("Cache-Control", expectedHeader);
        }

        [Fact]
        public async Task NotOverwriteCacheControlHeaderIfAlreadySet()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                CacheControl = "max-age=3600, s-maxage=3600, proxy-revalidate"
            });

            var previousResponse = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Cache-Control", "max-age=1200" }
                }
            };

            request.HttpMethod = "OPTIONS";

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Allow-Origin", "*");
            response.Headers.Should().Contain("Cache-Control", "max-age=1200");
        }

        [Fact]
        public async Task SetAccessControlMaxAgeHeaderIfPresentInConfig()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                MaxAge = "3600"
            });

            var previousResponse = new APIGatewayProxyResponse();

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Max-Age", "3600");
        }

        [Fact]
        public async Task NotOverwriteAccessControlMaxAgeHeaderIfAlreadySet()
        {
            var middleware = new HttpCorsMiddleware(new CorsOptions
            {
                MaxAge = "3600"
            });

            var previousResponse = new APIGatewayProxyResponse
            {
                Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Max-Age", "-1" }
                }
            };

            await middleware.Before(request, context);
            var response = await middleware.After(previousResponse, context);

            response.Headers.Should().Contain("Access-Control-Max-Age", "-1");
        }
    }
}
