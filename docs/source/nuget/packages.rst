Existing NuGet packages
=======================

There are a bunch of NuGet packages available for you to use.

Voxel.Middy.Tracing.Core
------------------------
This is a quite independent package that you can use on its own. It has an implementation of the ``TraceContext`` as described `here <https://www.w3.org/TR/trace-context/>`_.

The actual implementation has the following logic:
* If the ``traceparent`` header is not valid, it recreates the entire header and resets the ``tracestate`` one too. We define valid as:

    * It has 4 parts separated by hyphens
    * The version part is two characters long
    * The trace-id part is 32 characters long
    * The parent-id part is 16 characters long
    * The flags part is 2 characters long

* If the ``traceparent`` header is valid, it modifies the parent-id part with a new value.
* If the ``traceparent`` header is valid, it propagate whatever there is in the ``tracestate`` header.

Voxel.MiddyNet.Tracing.SNS
--------------------------
This is another package that is quite independent. It contains extension methods to add the information of the ``TraceContext`` object from ``Voxel.MiddyNet.Tracing.Core`` to an SNS message via ``MessageAttributes``.

You can use this package inside a Lambda function or from any place, to send the ``TraceContext`` information in a way that MiddyNet will be able to read using a middleware.

Voxel.MiddyNet.Tracing.Http
---------------------------
This is another package that is quite independent. It contains extension methods to add the information of the ``TraceContext`` object from ``Voxel.MiddyNet.Tracing.Core`` to an HttpRequestMessage via ``Headers``.

You can use this package inside a Lambda function or from any place, to send the ``TraceContext`` information in a way that MiddyNet will be able to read using a middleware.

Voxel.MiddyNet
--------------
This is the main package. You need to add it to your project if you want to use MiddyNet.

Voxel.MiddyNet.Tracing.SNSMiddleware
------------------------------------
This package contains a middleware that reads the ``TraceContext`` information from the ``MessageAttributes`` of an SNS event and enriches the ``MiddyLogger`` with them, so that your logs will have it and it will be easier to correlate them.

The logs will have a property for ``traceparent``, another one for ``tracestate``, and another one for ``trace-id``.

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this::

    public class MySample : MiddyNet<SNSEvent, int>
    {
        public MySample()
        {
            Use(new SNSTracingMiddleware<int>());
        }

        protected override async Task<int> Handle(SNSEvent snsEvent, MiddyNetContext context)
        {
            context.Logger.Log(LogLevel.Info, "hello world");

            // Do stuff

            return Task.FromResult(0);
        }
    }

Voxel.MiddyNet.Tracing.SQSMiddleware
------------------------------------
This package contains a middleware that reads the ``TraceContext`` information from the ``MessageAttributes`` of an SQS event and enriches the ``MiddyLogger`` with them, so that your logs will have it and it will be easier to correlate them.

The logs will have a property for ``traceparent``, another one for ``tracestate``, and another one for ``trace-id``.

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this::

    public class MySample : MiddyNet<SQSEvent, int>
    {
        public MySample()
        {
            Use(new SQSTracingMiddleware<int>());
        }

        protected override async Task<int> Handle(SQSEvent sqsEvent, MiddyNetContext context)
        {
            context.Logger.Log(LogLevel.Info, "hello world");

            // Do stuff

            return Task.FromResult(0);
        }
    }

Voxel.MiddyNet.Tracing.ApiGatewayMiddleware
-------------------------------------------
This package contains a middleware that reads the ``TraceContext`` information from the ``traceparent`` and ``tracestate`` headers of an ``APIGatewayProxyRequest`` or an ``APIGatewayHttpApiV2ProxyRequest`` and enriches the ``MiddyLogger`` with them, so that your logs will have it and it will be easier to correlate them.

The logs will have a property for ``traceparent``, another one for ``tracestate``, and another one for ``trace-id``.

In addition, the MiddyNetContext is enriched with a new entry in the AdditionalContext collection that contains a TraceContext object.

This TraceContext object provides a MutateParentId method that can be used to obtain a traceparent with the same Version, TraceId, and TraceFlags but with a new ParentId that can be used to call other systems, as `recommended by W3C. <https://www.w3.org/TR/trace-context/#mutating-the-traceparent-field>`_

Sample code
^^^^^^^^^^^
A typical use of the middleware for APIGateway will look like this::

    public class MySample : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public MySample()
        {
            Use(new ApiGatewayTracingMiddleware<APIGatewayProxyResponse>());
        }

        protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest apiEvent, MiddyNetContext context)
        {
            //This log is enriched with the tracing information received in the headers of the request
            context.Logger.Log(LogLevel.Info, "Function called.");

            //If you need to call another system, you need to obtain a traceparent based on the original traceparent
            //received but with the ParentId changed
            var currentTraceContext = (TraceContext)context.AdditionalContext[ApiGatewayTracingMiddleware.TraceContextKey];
            var newTraceContext = TraceContext.MutateParentId(currentTraceContext);

            //Now you can use this newTraceContext in your calls 
            var traceparentForCallingAnotherSystem = newTraceContext.TraceParent;
            var tracestateForCallingAnotherSystem = newTraceContext.TraceState;

            return Task.FromResult(new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }

and for APIGatewayHttpV2Api will look like this::

    public class ApiGatewayHttpApiV2Tracing : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public ApiGatewayHttpApiV2Tracing()
        {
            Use(new ApiGatewayHttpApiV2TracingMiddleware());
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest proxyRequest, MiddyNetContext context)
        {
            //This log is enriched with the tracing information received in the headers of the request
            context.Logger.Log(LogLevel.Info, "Function called.");

            //If you need to call another system, you need to obtain a traceparent based on the original traceparent
            //received but with the ParentId changed
            var currentTraceContext = (TraceContext)context.AdditionalContext[ApiGatewayHttpApiV2TracingMiddleware.TraceContextKey];
            var newTraceContext = TraceContext.MutateParentId(currentTraceContext);

            //Now you can use this newTraceContext in your calls 
            var traceparentForCallingAnotherSystem = newTraceContext.TraceParent;
            var tracestateForCallingAnotherSystem = newTraceContext.TraceState;

            return Task.FromResult(new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "Ok"
            });
        }
    }

Voxel.MiddyNet.SSM
------------------
This package contains a middleware that allows you to retrieve secrets from ``Parameter Store``. It also allows you to cache them to minimise the calls to ``Parameter Store``.

Configuration
^^^^^^^^^^^^^
You need to pass a ``SSMOptions`` object in the constructor with the following properties:
* CacheExpiryInMillis: number of milliseconds that the middleware will cache the parameter. During this time, it won't go again to ``ParameterStore`` to read the parameter.
* ParametersToGet: a list of ``SSMParameterToGet``. Each ``SSMParameterToGet`` has two properties:

    * Name: Name of the parameter in the lambda function. You will use this name later to access the value of the parameter inside your lambda function.
    * Path: Path of the parameter in ``ParameterStore``

The middleware will store the values of the parameters in the ``AdditionalContext`` of the ``MiddyContext``. It will add a property there for each parameter. The key of the property will be the name of the parameter.

Sample code
^^^^^^^^^^^
A typical configuration and use of the middelware will look like this::

    public class MySSMSample : MiddyNet<SNSEvent, int>
    {
        private const string Param1Name = "Param1Name";
        private const string Param2Name = "Param2Name";

        public MySSMSample()
        {
            var param1Path = System.Environment.GetEnvironmentVariable("param1Path");
            var param2Path = System.Environment.GetEnvironmentVariable("param2Path");

            var options = new SSMOptions
            {
                ParametersToGet = new List<SSMParameterToGet>
                {
                    new SSMParameterToGet(Param1Name, param1Path),
                    new SSMParameterToGet(Param2Name, param2Path)
                }
            };

            Use(new SSMMiddleware<SNSEvent, int>(options));
        }


        protected override async Task<int> Handle(SNSEvent snsEvent, MiddyNetContext context)
        {
            var param1Value = context.AdditionalContext[Param1Name].ToString();
            var param2Value = context.AdditionalContext[Param2Name].ToString();

            // Do stuff

            return Task.FromResult(0);
        }
    }

Voxel.MiddyNet.HttpCors
-----------------------
This package contains a middleware that allows you to set the CORS headers in the response. There are two versions avalaible: 

* One for REST Api (APIGatewayProxyRequest and APIGatewayProxyResponse).
* And another for Http Api (APIGatewayHttpApiV2ProxyRequest and APIGatewayHttpApiV2ProxyResponse).

Configuration
^^^^^^^^^^^^^
You can pass a ``CorsOptions`` object in the constructor with the following properties (all of them optional):

* Origin: origin to put in the header (default: "*")
* Origins: an array of allowed origins. The incoming origin is matched against the list and is returned if present.
* Headers: value to put in ``Access-Control-Allow-Headers`` (default: null)
* Credentials: if true, sets the ``Access-Control-Allow-Origin`` as request header Origin, if present (default ``false``)
* MaxAge: value to put in ``Access-Control-Max-Age`` header (default: null)
* CacheControl: value to put in ``Cache-Control`` header on pre-flight (OPTIONS) requests (default: null)

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this for Rest API::

    public class MySample : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public MySample()
        {
            Use(new HttpCorsMiddleware(new CorsOptions{Origin = "http://example.com"}));
        }

        protected override async Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest apiEvent, MiddyNetContext context)
        {
            // Do stuff

            var result = new APIGatewayProxyResponse
            {
                StatusCode = 200,
                Body = "hello from test"
            };

            return Task.FromResult(result);
        }
    }

And like this for Http API::

    public class MySample : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public MySample()
        {
            Use(new HttpV2CorsMiddleware(new CorsOptions{Origin = "http://example.com"}));
        }

        protected override async Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyResponse apiEvent, MiddyNetContext context)
        {
            // Do stuff

            var result = new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "hello from test"
            };

            return Task.FromResult(result);
        }
    }

Voxel.MiddyNet.ProblemDetailsMiddleware
---------------------------------------
The middleware contained in this package formats Api exceptions as ProblemDetails following [RFC7807](https://tools.ietf.org/html/rfc7807).

There are two versions avalaible: 

* One for REST Api (APIGatewayProxyRequest and APIGatewayProxyResponse).
* And another for Http Api (APIGatewayHttpApiV2ProxyRequest and APIGatewayHttpApiV2ProxyResponse).

Configuration
^^^^^^^^^^^^^
It can receive a ``ProblemDetailsMiddlewareOptions`` to specify mappings from a particular exception type to an Http status code. E.g: 

    var options = new ProblemDetailsMiddlewareOptions();
    options.Map<NotImplementedException>(501)));
    
When a ``NotImplementedException`` is thrown, ProblemDetailsMiddleware will return the exception message with a 501 Http status code.

Sample code
^^^^^^^^^^^
A typical usage of the ProblemDetailsMiddleware for REST Api whould look something like:

    public class ApiGatewayProblemDetails : MiddyNet<APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        public ApiGatewayProblemDetails()
        {
            Use(new ProblemDetailsMiddleware.ProblemDetailsMiddleware(new ProblemDetailsMiddlewareOptions().Map<NotImplementedException>(501)));
        }

        protected override Task<APIGatewayProxyResponse> Handle(APIGatewayProxyRequest lambdaEvent, MiddyNetContext context)
        {
            throw new NotImplementedException("this will be used in the problem details description");
        }
    }

And for Http Api:

    public class ApiGatewayProblemDetails : MiddyNet<APIGatewayHttpApiV2ProxyRequest, APIGatewayHttpApiV2ProxyResponse>
    {
        public ApiGatewayProblemDetails()
        {
            Use(new ProblemDetailsMiddleware.ProblemDetailsMiddleware(new ProblemDetailsMiddlewareOptions().Map<NotImplementedException>(501)));
        }

        protected override Task<APIGatewayHttpApiV2ProxyResponse> Handle(APIGatewayHttpApiV2ProxyRequest lambdaEvent, MiddyNetContext context)
        {
            throw new NotImplementedException("this will be used in the problem details description");
        }
    }

