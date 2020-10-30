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
        [Fact]
        public void LogLevelAndMessage()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var receivedLog = string.Empty;
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Debug, "hello world");
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogExtraProperties()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var receivedLog = string.Empty;
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", "value"));
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogExtraPropertiesWithObject()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var receivedLog = string.Empty;
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));
            var classToLog = new ClassToLog
            {
                Property1 = "The value of property1",
                Property2 = "The value of property2"
            };

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", classToLog));
            
            Approvals.Verify(receivedLog);
        }

        [Fact]
        public void LogEnrichWithExtraProperties()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var receivedLog = string.Empty;
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));

            var logger = new MiddyLogger(lambdaLogger);
            logger.EnrichWith(new LogProperty("key", "value"));
            logger.Log(LogLevel.Info, "hello world");

            Approvals.Verify(receivedLog);
        }
        [Fact]
        public void LogGlobalPropertiesAndExtraProperties()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var receivedLog = string.Empty;
            lambdaLogger.Log(Arg.Do<string>(a => receivedLog = a));

            var logger = new MiddyLogger(lambdaLogger);
            logger.EnrichWith(new LogProperty("key", "value"));
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key2", "value2"));

            Approvals.Verify(receivedLog);
        }

        internal class ClassToLog
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }
    }
}
