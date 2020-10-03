using Amazon.Lambda.Core;
using NSubstitute;
using Xunit;

namespace Voxel.MiddyNet.Tests
{
    public class MiddyLoggerShould
    {
        [Fact]
        public void LogLevelAndMessage()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Debug, "hello world");

            lambdaLogger.Received().Log(Arg.Is(@"{
  ""Message"": ""hello world"",
  ""Level"": ""Debug""
}"));
        }

        [Fact]
        public void LogExtraProperties()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", "value"));
            lambdaLogger.Received().Log(Arg.Is(@"{
  ""Message"": ""hello world"",
  ""Level"": ""Info"",
  ""key"": ""value""
}"));
        }

        [Fact]
        public void LogExtraPropertiesWithObject()
        {
            var lambdaLogger = Substitute.For<ILambdaLogger>();
            var classToLog = new ClassToLog
            {
                Property1 = "The value of property1",
                Property2 = "The value of property2"
            };

            var logger = new MiddyLogger(lambdaLogger);
            logger.Log(LogLevel.Info, "hello world", new LogProperty("key", classToLog));
            lambdaLogger.Received().Log(Arg.Is(@"{
  ""Message"": ""hello world"",
  ""Level"": ""Info"",
  ""key"": {
    ""Property1"": ""The value of property1"",
    ""Property2"": ""The value of property2""
  }
}"));
        }

        internal class ClassToLog
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }
    }
}
