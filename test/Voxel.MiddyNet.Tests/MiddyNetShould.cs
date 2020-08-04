using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public class TestLambdaFunction : MiddyNet<int, int>
        {
            public TestLambdaFunction(List<string> logLines, List<string> contextLogLines, int numberOfMiddlewares)
            {
                LogLines = logLines;
                ContextLogLines = contextLogLines;
                for (var i = 0; i < numberOfMiddlewares; i++)
                {
                    Use(new TestMiddleware(logLines, i+1));
                }
            }

            public List<string> LogLines { get; set; }
            public List<string> ContextLogLines { get; }

            protected override Task<int> Handle(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add(FunctionLog);
                ContextLogLines.AddRange(context.AdditionalContext.Select(kv => $"{kv.Key}-{kv.Value}"));

                return Task.FromResult(0);
            }
        }

        public class TestMiddleware : ILambdaMiddleware<int, int>
        {
            private readonly int position;
            public List<string> LogLines { get; }

            public TestMiddleware(List<string> logLines, int position)
            {
                this.position = position;
                LogLines = logLines;
            }

            public Task Before(int lambdaEvent, MiddyNetContext context)
            {
                LogLines.Add($"{MiddlewareBeforeLog}-{position}");
                context.AdditionalContext.Add($"{ContextKeyLog}-{position}", $"{ContextLog}-{position}");
                return Task.CompletedTask;
            }

            public Task<int> After(int lambdaResponse, MiddyNetContext context)
            {
                LogLines.Add($"{MiddlewareAfterLog}-{position}");
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task RunMiddlewareAroundTheFunction()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 1);

            await lambdaFunction.Handler(1, null);

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", FunctionLog, $"{MiddlewareAfterLog}-1");
        }

        [Fact]
        public async Task RunMiddlewareBeforeActionInOrder()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, null);

            logLines.Should().ContainInOrder($"{MiddlewareBeforeLog}-1", $"{MiddlewareBeforeLog}-2", FunctionLog);
        }

        [Fact]
        public async Task RunMiddlewareAfterActionInInverseOrder()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, null);

            logLines.Should().ContainInOrder(FunctionLog, $"{MiddlewareAfterLog}-2", $"{MiddlewareAfterLog}-1");
        }

        [Fact]
        public async Task CleanTheContextBetweenCalls()
        {
            var lambdaFunction = new TestLambdaFunction(logLines, contextLines, 2);

            await lambdaFunction.Handler(1, null);
            contextLines.Should().HaveCount(2);
            await lambdaFunction.Handler(1, null);
            contextLines.Should().HaveCount(4);
        }
    }
}
