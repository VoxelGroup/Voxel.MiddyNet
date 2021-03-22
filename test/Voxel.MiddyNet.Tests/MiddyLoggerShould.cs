using Amazon.Lambda.Core;
using ApprovalTests;
using ApprovalTests.Reporters;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class MiddyLoggerShould
    {
        private readonly ILambdaLogger lambdaLogger = Substitute.For<ILambdaLogger>();
        private readonly ILambdaContext lambdaContext = Substitute.For<ILambdaContext>();
        private const string AwsRequestId = "12345";
        private const string FunctionName = "FunctionName";
        private const string FunctionVersion = "1.0";
        private const int MemoryLimitInMb = 1024;

        private string receivedLog = string.Empty;
        

        public MiddyLoggerShould()
        {
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));
            lambdaContext.AwsRequestId.Returns(AwsRequestId);
            lambdaContext.FunctionName.Returns(FunctionName);
            lambdaContext.FunctionVersion.Returns(FunctionVersion);
            lambdaContext.MemoryLimitInMB.Returns(MemoryLimitInMb);
        }

        [Fact]
        public void LogLevelAndMessage()
        {
            var logger = new MiddyLogger(lambdaLogger, lambdaContext);
            logger.Log(LogLevel.Debug, "hello world");
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogExtraProperties()
        {
            var logger = new MiddyLogger(lambdaLogger, lambdaContext);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", "value"));
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogExtraPropertiesWithObject()
        {
            var classToLog = new ClassToLog
            {
                Property1 = "The value of property1",
                Property2 = "The value of property2"
            };

            var logger = new MiddyLogger(lambdaLogger, lambdaContext);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", classToLog));
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogEnrichWithExtraProperties()
        {
            var logger = new MiddyLogger(lambdaLogger, lambdaContext);
            logger.EnrichWith(new LogProperty("key", "value"));
            logger.Log(LogLevel.Info, "hello world");

            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogEnrichWithDynamicProperties()
        {
            var logger = new MiddyLogger(lambdaLogger);
            logger.EnrichWith(new object(), o => o.ToString());
            logger.Log(LogLevel.Info, "hello world");

            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogGlobalPropertiesAndExtraProperties()
        {
            var logger = new MiddyLogger(lambdaLogger, lambdaContext);
            logger.EnrichWith(new LogProperty("key", "value"));
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key2", "value2"));

            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogDynamicPropertiesFromProperty()
        {
            var someObject = new { SomeProperty = "some value" };
            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", someObject, so => so.SomeProperty);

            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogDynamicPropertiesFromMethod()
        {
            var someObject = new object();
            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", someObject, so => so.ToString());

            Approvals.Verify(receivedLog);
        }

        internal class ClassToLog
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }
    }
}
