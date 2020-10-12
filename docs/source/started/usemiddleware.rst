Using a Middleware
==================

The basics
----------

To use a middleware in your lambda function, you need to add it using the function use in the constructor. For example, to add the *SQSTracingMiddleware* you need to follow the following steps:

1. Add the corresponding package::

    dotnet package add Voxel.MiddyNet.Tracing.SQSMiddleware


2. Add the corresponding using in your lambda function::

    using Voxel.MiddyNet.Tracing.SQSTracingMiddleware;

3. Add it to the engine in your constructor::

    public MySQSLambdaFunction()
    {
        // Your own initializations 
        Use(new SQSTracingMiddleware());
    }

And that's all. **MiddyNet** will execute this middleware before or after your function runs, depending wether if it implements ``IBeforeLambdaMiddleware`` or ``IAfterLambdaMiddleware``.

Middleware configurations
-------------------------

Some middlewares can have configurations you might want to take advantage of. The configuration will usually be a parameter of the constructor, like in the SSM middleware::

    public class MySQSLambdaFunction: MiddyNet<SQSEvent,int>
    {
        private const string ClientIdName = "clientIdName";
        private const string ClientSecretName = "clientSecretName";

        public MySQSLambdaFunction()
        {
            var clientIdPath = Environment.GetEnvironmentVariable("clientIdPath");
            var clientSecretPath = Environment.GetEnvironmentVariable("clientSecretPath");

            Use(new SSMMiddleware<SNSEvent, int>(new SSMOptions
            {
                CacheExpiryInMillis = 60000,
                ParametersToGet = new List<SSMParameterToGet>
                {
                    new SSMParameterToGet(ClientIdName, clientIdPath),
                    new SSMParameterToGet(ClientSecretName, clientSecretPath)
                }
            }));

            Use(new SQSTracingMiddleware<SNSEvent, int>());
        }

        protected override async Task<int> Handle(SNSEvent lambdaEvent, MiddyNetContext context)
        {
            // Your business logic

            return await Task.FromResult(0);
        }
    }
