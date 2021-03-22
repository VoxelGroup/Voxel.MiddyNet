Middy Context
=============

**MiddyNet** offers you a context with additional information you can use in your lambda function. This context has the following properties.

LambdaContext
-------------

An object implementing ``ILambdaContext`` with additional information provided by AWS. You can read about it `here <https://docs.aws.amazon.com/lambda/latest/dg/csharp-context.html>`_.

AdditionalContext
-----------------

A ``Dictionary<string, object>`` filled with the information that the different middlewares want to add to it. For example, the *SSMMiddleware* will add there the secrets gotten from SSM.

MiddlewareExceptions
--------------------
There are 4 properties and a method related to exceptions:

* ``List<Exception> MiddlewareBeforeExceptions { get; }``, *Middleare with the exceptions thrown by the middlewares' *Before* method.
* ``Exception HandlerException { get; set; }``, with the exception thrown by the custom handler's *Handle* method.
* ``List<Exception> MiddlewareAfterExceptions { get; }``, with the exceptions thrown by the middlewares' *After* method. 
* ``bool HasExceptions { get; }``, which tells if there are any exceptions  in the aforementioned properties
* ``List<Exception> GetAllExceptions()``, which returns the exceptions in the three properties as a single list, in the same order as they happened.

You may want to check whether any exceptions occured before proceeding with your custom logic in the handler like so:

	if (context.HasExceptions)
    {
        // do stuff...
        context.MiddlewareBeforeExceptions.Clear();
    }

This will prevent those exceptions to be returned at the end of the pipeline processing.

Logger
------
An object of type MiddyLogger to allow you to log structured logs. It uses the ``ILambdaLogger`` provided by AWS internally. See more information about the :doc:`/logger`.

