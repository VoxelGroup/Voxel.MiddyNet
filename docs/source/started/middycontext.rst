Middy Context
=============

**MiddyNet** offers you a context with additional information you can use in your lambda function. This context has the following properties.

LambdaContext
-------------

An object implementing *ILambdaContext* with additional information provided by AWS. You can read about it `here <https://docs.aws.amazon.com/lambda/latest/dg/csharp-context.html>`_.

AdditionalContext
-----------------

A *Dictionary<string, object>* filled with the information that the different middlewares want to add to it. For example, the *SSMMiddleware* will add there the secrets gotten from SSM.

MiddlewareExceptions
--------------------

A *List<Exception>*, with the exceptions thrown by the different middlewares. 

Logger
------
An object of type MiddyLogger to allow you to log structured logs. It uses the `ILambdaLogger` provided by AWS internally. See more information about the :doc:`/logger`.