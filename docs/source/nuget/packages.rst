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

Voxel.MiddyNet
--------------
This is the main package. You need to add it to your project if you want to use MiddyNet.

Voxel.MiddyNet.Tracing.SNSMiddleware
------------------------------------
This package contains a middleware that reads the ``TraceContext`` information from the ``MessageAttributes`` of an SNS event and enriches the ``MiddyLogger`` with them, so that your logs will have it and it will be easier to correlate them.

The logs will have a property for ``traceparent``, another one for ``tracestate``, and another one for ``trace-id``.

It adds and Exception to the ``MiddlewareExceptions`` list if the event is not of type ``SNSEvent``.

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this::

    public class MySample : MiddyNet<SNSEvent, int>
    {
        public MySample()
        {
            Use(new SNSTracingMiddleware<SNSEvent, int>());
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

It adds and Exception to the ``MiddlewareExceptions`` list if the event is not of type ``SQSEvent``.

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this::

    public class MySample : MiddyNet<SQSEvent, int>
    {
        public MySample()
        {
            Use(new SQSTracingMiddleware<SQSEvent, int>());
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
This package contains a middleware that reads the ``TraceContext`` information from the ``traceparent`` and ``tracestate`` headers of an ``APIGatewayProxyRequest`` and enriches the ``MiddyLogger`` with them, so that your logs will have it and it will be easier to correlate them.

The logs will have a property for ``traceparent``, another one for ``tracestate``, and another one for ``trace-id``.

It adds and Exception to the ``MiddlewareExceptions`` list if the event is not of type ``APIGatewayProxyRequest``.

Sample code
^^^^^^^^^^^
A typical use of the middelware will look like this::

    public class MySample : MiddyNet<APIGatewayProxyRequest, int>
    {
        public MySample()
        {
            Use(new ApiGatewayTracingMiddleware<APIGatewayProxyRequest, int>());
        }

        protected override async Task<int> Handle(APIGatewayProxyRequest apiEvent, MiddyNetContext context)
        {
            context.Logger.Log(LogLevel.Info, "hello world");

            // Do stuff

            return Task.FromResult(0);
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
