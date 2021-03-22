How it works
============

**MiddyNet** is a lightweight middleware engine. A middleware is a piece of code that can be run before or after your function is run. In the *before* section, middlewares are run in the same order that are added. In the *after* section, middlewares are run in the inverse order that are added. Usually, a middleware will only make sense to do anything in one of the sections.

Exceptions can be thrown by the middlewares and they are captured by the library. If the exception is thrown in the *before* section, the exception is made available to the following middlewares and eventually to your function via the context. If the exception is thrown in the *handler* or in the *after* section, it's eventually thrown by the library as an *AggregateException*. Non cleaned ``MiddlewareBeforeExceptions`` will also be included in the resulting AggregateException.

**MiddyNet** also adds a Logger to log structured logging and a way to capture and propagate the TraceContext as described `here <https://www.w3.org/TR/trace-context/>`_.