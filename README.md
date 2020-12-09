![MiddyNet Continuous Integration](https://github.com/VoxelGroup/Voxel.MiddyNet/workflows/MiddyNet%20Continuous%20Integration/badge.svg)

[![Documentation Status](https://readthedocs.org/projects/voxelmiddynet/badge/?version=latest)](https://voxelmiddynet.readthedocs.io/en/latest/?badge=latest)

# Voxel.MiddyNet

Middy .NET is a lightwave middleware library for AWS Lambda and .NET Core 3.1. It's a port of the amazing [middy](https://github.com/middyjs/middy) package for NodeJS.

It allows you to inject middlewares into your lambda functions so that your code is entirely focused on business logic. 

Middlewares are published as separate NuGet packages (one NuGet per middleware) so that your lambda package can be as small as possible.

For project documentation, please visit [readthedocs](https://voxelmiddynet.readthedocs.io).

## Prerequisites

```
- NET Core 3.1
- Your preferred IDE
```

## Running the tests

Tests are written using xUnit and NSubstitute. There are some integration tests that need an AWS profile called MiddyNetDev configured in your local machine.
1. Clone this repository
2. Build the solution
3. Run tests using VS embedded Tests Window or with this command: `dotnet test` under the test project folder

## Usage
This library will force you to organize your lambda functions in a certain way, although we think this way is quite convenient. Each lambda will reside in it's own file and the exposed function will always be called `Handler`. All functions will have an input event and an output, although your system might ignore the output. An example of using the library with the SSM middleware will look like this:

```
public class ForwardEmail : MiddyNet<SNSEvent, int>
{
    private const string Param1Name = "Param1Name";
    private const string Param2Name = "Param2Name";

    public ForwardEmail()
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

        this.Use(new SSMMiddleware<SNSEvent, int>(options));
    }


    protected override async Task<int> Handle(SNSEvent snsEvent, MiddyNetContext context)
    {
        var param1Value = context.AdditionalContext[Param1Name].ToString();
        var param2Value = context.AdditionalContext[Param2Name].ToString();

        // Do stuff

        return Task.FromResult(0);
    }
}
```

As you can see, you need to derive from MiddyNet, and specify the type of event that you get, and the type of the object that you'll return. This will force you that the method you need to specify as a handler of your class is a method called `Handler`, which is internal to the library. This `Handler` method will run the Before function of the middlewares and eventually will call the `Handle` method you'll need to override. That's where you need to put your lambda function code.

After your code is executed, Middy .NET will call the after methods of the middlewares in reverse order. You can use this to, for example, alter the response in some way (adding headers, for example).

### Handling errors
Errors can happen in the `Before` method of the middleware or in the `After` method of them. Although we capture those errors, we treat those them slightly different.

#### Errors on Before
When an exception is thrown by a middleware in the `Before` method, the exception is captured and added to the `MiddlewareBeforeExceptions` list, so that the following middlewares and the function can react to it.

#### Errors on Handler
When an exception is thrown by the function in its `Handle` method and before the `After` middlewares are called, the exception is captured in the `HandlerException` property of the context, so that the following middlewares can react to it.

#### Errors on After
When an exception is thrown by a middleware in the `After` method, the exception is captured and added to the `MiddlewareAfterExceptions` list, so that the following middlewares can react to it.

When all the middlewares have run, if these lists or property have any items, an `AggregateException` with all of them is thrown.

## How to write a middleware
To write a new Middleware, you just need to implement the interface `ILambdaMiddleware` and implement the `Before` and `After` methods, although normally you will only implement one of them. If you need to store data so that the `Handle` method can use it, you can use the `AdditionalContext` dictionary inside the `MiddyContext` object.

## Logger
There's a simple logger implemented in the package. This logger is a wrapper of the `ILambdaLogger` provided by the AWS runtime, which you can still access inside the `LambdaContext` property inside the `MiddyContext` object. Our logger, logs a JSON message so the logs are easily readable by your preferred log aggregation platform. We can also add additional properties apart from the message and LogLevel.

```
protected override async Task<int> Handle(SNSEvent lambdaEvent, MiddyNetContext context)
{
    context.Logger.Log(LogLevel.Debug, $"There's been {context.MiddlewareExceptions.Count} exceptions", new LogProperty("key", "value"));

    return await Task.FromResult(0);
}
```

Right now, there's a middleware to extract `traceparent` and `tracestate` headers from an `SNSEvent` (from the `MessageAttributes` of the first record), from an `SQSEvent` (from the `MessageAttributes` of the first record), and from an `ApiGatewayProxyRequest` (from the headers). The headers should follow the format described [here](https://www.w3.org/TR/trace-context/). The current implementation doesn't mutate `tracestate` and changes the `parent-id` section of the `traceparent` in case a valid `traceparent` header is provided. If the `traceparent` header provided is not valid, it creates a new one.

## Maintainers

* Voxel Media S.L.

See also the list of [contributors](https://github.com/VoxelGroup/Voxel.MiddyNet/contributors) who participated in this project.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

