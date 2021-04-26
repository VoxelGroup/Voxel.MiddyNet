using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using FluentAssertions;
using Xunit;

namespace Voxel.MiddyNet.Tests
{
    public class MiddyNetShould
    {
        private readonly List<string> logLines = new List<string>();
        private readonly List<string> contextLines = new List<string>();
        private const string FunctionLog = "FunctionCode";
        private const string MiddlewareBeforeLog = "MiddlewareBeforeCode";
        private const string MiddlewareAfterLog = "MiddlewareAfterCode";
        private const string ContextLog = "ContextLog";
        private const string ContextKeyLog = "ContextKeyLog";
        private List<Exception> middlewareExceptions = new List<Exception>();

        public class TestLambdaFunction : MiddyNet<int, int>
        {
            private readonly bool withFailingHandler;

            public TestLambdaFunction(List<string> logLines, List<string> contextLogLines, int numberOfMiddlewares, bool withFailingMiddleware = false, bool withFailingHandler = false, bool withInterruptingMiddleware = false)
                : this(logLines, contextLogLines, numberOfMiddlewares, withFailingMiddleware, withFailingMiddleware, withFailingHandler, withInterruptingMiddleware) { }

            public TestLambdaFunction(List<string> logLines, List<string> contextLogLines, int numberOfMiddlewares, bool withFailingBeforeMiddleware, bool withFailingAfterMiddleware, bool withFailingHandler, bool withInterruptingMiddleware)

            {
                LogLines = logLines;
                ContextLogLines = contextLogLines;
                for (var i = 0; i < numberOfMiddlewares; i++)
                {
                    Use(new TestBeforeMiddleware(logLines, i + 1, withFailingBeforeMiddleware, withInterruptingMiddleware));
                    Use(new TestAfterMiddleware(logLines, i + 1, withFailingAfterMiddleware));
                }

                this.withFailingHandler = withFailingHandler;
            }

            public List<string> LogLines { get; set; }
            public List<string> ContextLogLines { get; }

            public MiddyNetContext MiddyContext { get; set; }

            protected override Task<int> Handle(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add(FunctionLog);
                ContextLogLines.AddRange(context.AdditionalContext.Select(kv => $"{kv.Key}-{kv.Value}"));
                MiddyContext = context;

                if (withFailingHandler) throw new MiddlewareException();

                return Task.FromResult(0);
            }
        }

        public class MiddlewareException : Exception { }

        public class TestBeforeMiddleware : ILambdaMiddleware<int, int>
        {
            private readonly int position;
            private readonly bool interrupts = false;
            public List<string> LogLines { get; }
            public bool Failing { get; }

            public bool InterruptsExecution => interrupts;

            public TestBeforeMiddleware(List<string> logLines, int position, bool failing, bool interrupts)
            {
                this.position = position;
                this.interrupts = interrupts;
                LogLines = logLines;
                Failing = failing;
            }

            public Task Before(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add($"{MiddlewareBeforeLog}-{position}");
                context.AdditionalContext.Add($"{ContextKeyLog}-{position}", $"{ContextLog}-{position}");

                if (Failing) throw new MiddlewareException();

                return Task.CompletedTask;
            }

            public Task<int> After(int lambdaResponse, MiddyNetContext context)
            {
                return Task.FromResult(lambdaResponse);
            }
        }

        public class TestAfterMiddleware : ILambdaMiddleware<int, int>
        {
            private readonly int position;
            public List<string> LogLines { get; }
            public bool Failing { get; }

            public bool InterruptsExecution => false;

            public TestAfterMiddleware(List<string> logLines, int position, bool failing)
            {
                this.position = position;
                LogLines = logLines;
                Failing = failing;
            }

            public Task Before(int lambdaEvent, MiddyNetContext context)
            {
                return Task.CompletedTask;
            }

            public Task<int> After(int lambdaResponse, MiddyNetContext context)
            {
                LogLines.Add($"{MiddlewareAfterLog}-{position}");

                if (Failing) throw new MiddlewareException();
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task RunMiddlewareAroundTheFunction()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 1);

            await lambdaFunction.Handler(1, new FakeLambdaContext());

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", FunctionLog, $"{MiddlewareAfterLog}-1");
        }

        [Fact]
        public async Task RunMiddlewareBeforeActionInOrder()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, new FakeLambdaContext());

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", $"{MiddlewareBeforeLog}-2", FunctionLog);
        }

        [Fact]
        public async Task RunMiddlewareAfterActionInInverseOrder()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, new FakeLambdaContext());

            logLines.Should().ContainInOrder(FunctionLog, $"{MiddlewareAfterLog}-2", $"{MiddlewareAfterLog}-1");
        }

        [Fact]
        public async Task CleanTheContextBetweenCalls()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, new FakeLambdaContext());
            contextLines.Should().HaveCount(2);
            await lambdaFunction.Handler(1, new FakeLambdaContext());
            contextLines.Should().HaveCount(4);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void NotifyErrorOnBefore(int numberOfMiddlewares)
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, numberOfMiddlewares, true);

            Func<Task> act = async () => await lambdaFunction.Handler(1, new FakeLambdaContext());
            
            act.Should().Throw<AggregateException>();
            lambdaFunction.MiddyContext.MiddlewareBeforeExceptions.Should().HaveCount(numberOfMiddlewares);
            lambdaFunction.MiddyContext.MiddlewareBeforeExceptions.Should().AllBeOfType<MiddlewareException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void NotifyErrorOnAfter(int numberOfMiddlewares)
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, numberOfMiddlewares, true);
            
            Func<Task> act = async () => await lambdaFunction.Handler(1, new FakeLambdaContext());

            act.Should().Throw<AggregateException>();
            lambdaFunction.MiddyContext.MiddlewareAfterExceptions.Should().HaveCount(numberOfMiddlewares);
            lambdaFunction.MiddyContext.MiddlewareAfterExceptions.Should().AllBeOfType<MiddlewareException>();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void IncludeHandlerExceptionOnAfterErrorNotifications(int numberOfMiddlewares)
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, numberOfMiddlewares, true, true);
            
            Func<Task> act = async () => await lambdaFunction.Handler(0, new FakeLambdaContext());

            var exceptionAssertions = act.Should().Throw<AggregateException>()
                .Where(a => a.InnerExceptions.Count == numberOfMiddlewares * 2 + 1)
                .Where(a => a.InnerExceptions.Take(numberOfMiddlewares).All(e => e is MiddlewareException))
                .Where(a => a.InnerExceptions.Skip(numberOfMiddlewares + 1).All(e => e is MiddlewareException));
        }

        [Theory]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        public void ThrowSpecificExceptionWhenOnlyOnePresent(bool throwBeforeException, bool throwAfterException, bool throwHandlerException)
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 1, throwBeforeException, throwAfterException, throwHandlerException, false);
            Func<Task> act = async () => await lambdaFunction.Handler(0, new FakeLambdaContext());

            act.Should().NotThrow<AggregateException>();
            act.Should().Throw<MiddlewareException>();
        }

        [Fact]
        public async Task LetMiddlewaresChangeTheFunctionResult()
        {
            var function = new AddsTwo();
            function.Use(new SquareIt());

            var result = await function.Handler(1, new FakeLambdaContext());

            result.Should().Be(9);
        }

        [Fact]
        public void StopEvaluatingBeforeMiddlewaresIfInterruptExecutionSetToTrueAndExceptionHappens()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2, true, false, true);
            Func<Task> act = async () => await lambdaFunction.Handler(0, new FakeLambdaContext());

            act.Should().Throw<MiddlewareException>();

            logLines.Should().Contain($"{MiddlewareBeforeLog}-1");
            logLines.Should().NotContain($"{MiddlewareBeforeLog}-2");

        }

        [Fact]
        public void NotExecuteLambdaIfInterruptExecutionSetToTrueAndExceptionHappens()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2, true, false, true);
            Func<Task> act = async () => await lambdaFunction.Handler(0, new FakeLambdaContext());

            act.Should().Throw<MiddlewareException>();
            logLines.Should().NotContain(FunctionLog);

        }

        private class AddsTwo : MiddyNet<int, int>
        {
            protected override Task<int> Handle(int lambdaEvent, MiddyNetContext context)
            {
                return Task.FromResult(lambdaEvent + 2);
            }
        }

        private class SquareIt : ILambdaMiddleware<int, int>
        {
            public bool InterruptsExecution => false;

            public Task<int> After(int lambdaResponse, MiddyNetContext context)
            {
                return Task.FromResult(lambdaResponse * lambdaResponse);
            }

            public Task Before(int lambdaEvent, MiddyNetContext context)
            {
                return Task.CompletedTask;
            }
        }
    }

    public class FakeLambdaContext : ILambdaContext
    {
        public string AwsRequestId { get; }
        public IClientContext ClientContext { get; }
        public string FunctionName { get; }
        public string FunctionVersion { get; }
        public ICognitoIdentity Identity { get; }
        public string InvokedFunctionArn { get; }
        public ILambdaLogger Logger { get; }
        public string LogGroupName { get; }
        public string LogStreamName { get; }
        public int MemoryLimitInMB { get; }
        public TimeSpan RemainingTime { get; }
    }
}
