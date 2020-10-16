Writing a Middleware
====================

The basics
----------

Writing a new middleware is quite easy. You just need to implement the ``ILambdaMiddleware`` interface and implement both methods. The minimum implementation of a middleware that doesn't do anything would be this one::

    public class DummyMiddleware<TReq, TRes> : ILambdaMiddleware<TReq, TRes>
    {
        public Task Before(TReq lambdaEvent, MiddyNetContext context)
        {
            return Task.CompletedTask;
        }

        public Task<TRes> After(TRes lambdaResponse, MiddyNetContext context)
        {
            return Task.FromResult(lambdaResponse);
        }
    }

Storing data
------------

You can add data to the context so that the next middlewares or the lambda function can access it. To do so, you just need to add a new entry into the ``AdditionalContext`` property::

    public Task Before(TReq lambdaEvent, MiddyNetContext context)
    {
        context.AdditionalContext.Add("key", "value");
        return Task.CompletedTask;
    }

The value of the property can be whatever you want, from a single type to a complex object.

Exceptions
----------
You don't need to catch exceptions in the middleware. The library will catch them for you and add them to the ``MiddlewareExceptions`` property.