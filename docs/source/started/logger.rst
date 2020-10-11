Middy Logger
============

As we explained in the :doc:`Middy Context </middycontext>` section, **MiddyNet** gives you a logger to be able to log structured logs easily. You will still have access to the original logger provided by AWS, by accessing the *Logger* property of the *ILambdaContext* object.

Basic logging
-------------
If you just need to log a message, you can just do:

``
middyContext.Logger.Log(LogLevel.Debug, "hello world");
``

This will generate a log like this:
``
{
  "Message": "hello world",
  "Level": "Debug"
}
``

Logging extra properties
------------------------
If you need to add extra properties to your main message, you can add them this way:

``
middyContext.Logger.Log(LogLevel.Info, "hello world", new LogProperty("key1", "value1"), , new LogProperty("key2", "value2"));
``

This will generate a log like this:
``
{
  "Message": "hello world",
  "Level": "Info",
  "key1": "value1",
  "key2": "value2"
}
``

Enriching with properties
-------------------------
If you don't want to add the same properties again and again, you can enrich the logger with them like this:

``
middyContext.Logger.EnrichWith(new LogProperty("key", "value"));
``

Once done that, every time that you log a single message
``
middyContext.Logger.Log(LogLevel.Debug, "hello world");
``

The property will be added to the final log message:
``
{
  "Message": "hello world",
  "Level": "Info",
  "key": "value"
}
``

The same will happen if you log a message with one or more properties. The enriching properties will be added to those.

Logging a complex property
--------------------------
The property you add can be a complex object, and it will be serialised as JSON. So, for example, if you log this:

``
var classToLog = new ClassToLog
{
    Property1 = "The value of property1",
    Property2 = "The value of property2"
};

middyContext.Logger.Log(LogLevel.Info, "hello world", new LogProperty("key", classToLog));
``

you will have a log like this:

``
{
  "Message": "hello world",
  "Level": "Info",
  "key"": {
    "Property1": "The value of property1",
    "Property2": "The value of property2"
  }
}
``