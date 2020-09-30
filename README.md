
# Voxel.MiddyNet

Middy .NET is a lightwave middleware library for AWS Lambda and .NET Core 3.1. It's a port of the amazing [middy](https://github.com/middyjs/middy) package for NodeJS.

It allows you to inject middlewares into your lambda functions so that your code is entirely focused on business logic. Right now, there's only one (incomplete) middleware implemented (the SSM middleware to get data from SSM). Feel free to send PRs adding more middlewares. 

Middlewares are published as separate NuGet packages (one NuGet for middleware) so that your lambda package can be as small as possible.

## Prerequisites

```
- NET Core 3.1
- Your preferred IDE
```


## Running the tests

Tests are written using xUnit and NSubstitute.
1. Clone this repository
2. Build the solution
3. Run tests using VS embedded Tests Window or with this command: `dotnet test` under the test project folder


## Usage
This library will force you to organize your lambda functions in a certain way, although we thing this way it's quite convenient. Each lambda will reside in it's own file. An example of using the library with the SSM middleware will look like this:

```
public class ForwardEmail : MiddyNet<SNSEvent, int>
{
    private static HttpClient httpClient = new HttpClient();
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

As you can see, you need to derive from MiddyNet, and specify the type of event that you get, and the type of the object that you'll return. This will force you that the method you need to specify as a handler of your class is a method called `Handler`. This `Handler` method will run the Before function of the middlewares and eventually will call the `Handle` method you'll need to override. That's where you need to put your lambda function code.

After your code is run, Middy .NET will call the after methods of the middlewares in reverse order. You can use this to, for example, alter the response in some way (adding headers, for example).

### Handling errors
Errors can happen in the `Before` method of the middleware or in the `After` method of them. Although we capture those errors, we treat those them slightly different.

#### Errors on Before
When an exception is thrown by a middleware in the `Before` method, the exception is captured and added to the `MiddlewareExceptions` List, so that the following middlewares and the function can react to it.

After our function is called, this list is cleared.

#### Errors on After
When an exception is thrown by a middleware in the `After` method, the exception is captured and added to the `MiddlewareExceptions` list, so that the following middlewares can reacto to it. When all the middlewares have run, if this list has any item, an `AggregateException` with all of them is thrown.

## How to write a middleware
To write a new Middleware, you just need to implement the interface `ILambdaMiddleware` and implement the before and after methods. If you need to store data so that the `Handle` method can see it, you can use the `AdditionalContext` dictionary inside the `MiddyContext` object.

## Maintainers

* Voxel Media S.L.

See also the list of [contributors](https://github.com/VoxelGroup/Voxel.MiddyNet/contributors) who participated in this project.

## Contributing

Please read [CONTRIBUTING.md](..) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

