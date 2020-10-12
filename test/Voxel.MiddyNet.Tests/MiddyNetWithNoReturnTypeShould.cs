using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Voxel.MiddyNet.Tests
{
    public class MiddyNetWithNoReturnTypeShould
    {
        private readonly List<string> logLines = new List<string>();
        private readonly List<string> contextLines = new List<string>();
        private const string FunctionLog = "FunctionCode";
        private const string MiddlewareBeforeLog = "MiddlewareBeforeCode";
        private const string ContextLog = "ContextLog";
        private const string ContextKeyLog = "ContextKeyLog";
        private List<Exception> middlewareExceptions = new List<Exception>();

        public class TestLambdaFunction : MiddyNet<int>
        {
            public TestLambdaFunction(List<string> logLines, List<string> contextLogLines, int numberOfMiddlewares, bool withFailingMiddleware = false, List<Exception> exceptions = null)
            {
                LogLines = logLines;
                ContextLogLines = contextLogLines;
                Exceptions = exceptions ?? new List<Exception>();
                for (var i = 0; i < numberOfMiddlewares; i++)
                {
                    Use(new TestMiddleware(logLines, i+1, withFailingMiddleware));
                }
            }

            public List<string> LogLines { get; set; }
            public List<string> ContextLogLines { get; }
            public List<Exception> Exceptions { get; set; }

            protected override Task Handle(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add(FunctionLog);
                ContextLogLines.AddRange(context.AdditionalContext.Select(kv => $"{kv.Key}-{kv.Value}"));
                Exceptions.AddRange(context.MiddlewareExceptions);

                return Task.CompletedTask;
            }
        }

        public class MiddlewareException : Exception { }

        public class TestMiddleware : IBeforeLambdaMiddleware<int>
        {
            private readonly int position;
            public List<string> LogLines { get; }
            public bool Failing { get; }

            public TestMiddleware(List<string> logLines, int position, bool failing)
            {
                this.position = position;
                LogLines = logLines;
                Failing = failing;
            }

            public Task Before(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add($"{MiddlewareBeforeLog}-{position}");
                context.AdditionalContext.Add($"{ContextKeyLog}-{position}", $"{ContextLog}-{position}");

                if(Failing) throw new MiddlewareException();

                return Task.CompletedTask;
            }
        }

        [Fact]
        public async Task RunMiddlewareAroundTheFunction()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 1);

            await lambdaFunction.Handler(1, new FakeLambdaContext());

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", FunctionLog);
        }

        [Fact]
        public async Task RunMiddlewareBeforeActionInOrder()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, new FakeLambdaContext());

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", $"{MiddlewareBeforeLog}-2", FunctionLog);
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
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, numberOfMiddlewares, true, middlewareExceptions);

            Func<Task> act = async () => await lambdaFunction.Handler(1, new FakeLambdaContext());

            middlewareExceptions.Should().HaveCount(numberOfMiddlewares);
            middlewareExceptions.Should().AllBeOfType<MiddlewareException>();
        }
    }
}
