using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Voxel.MiddyNet
{
    public class MiddyLogger : IMiddyLogger
    {
        private readonly ILambdaLogger lambdaLogger;
        private readonly ILambdaContext lambdaContext;

        private List<LogProperty> globalProperties = new List<LogProperty>();

        public MiddyLogger(ILambdaLogger lambdaLogger, ILambdaContext lambdaContext)
        {
            this.lambdaLogger = lambdaLogger;
            this.lambdaContext = lambdaContext;
        }

        public void Log(LogLevel logLevel, string message)
        {
            AddLambdaContextProperties();

            var logMessage = new LogMessage
            {
                Level = logLevel,
                Message = message,
                Properties = globalProperties.ToDictionary(p => p.Key, p => p.Value)
            };

            InternalLog(logMessage);
        }

        public void Log(LogLevel logLevel, string message, params LogProperty[] properties)
        {
            AddLambdaContextProperties();

            var logMessage = new LogMessage
            {
                Level = logLevel,
                Message = message,
                Properties = globalProperties.Concat(properties).ToDictionary(p=>p.Key, p=>p.Value)
            };

            InternalLog(logMessage);
        }

        public void EnrichWith(LogProperty logProperty)
        {
            globalProperties.Add(logProperty);
        }

        private void InternalLog(LogMessage logMessage)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true
            };
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new LogMessageJsonConverter());

            var jsonString = JsonSerializer.Serialize(logMessage, options);

            lambdaLogger.Log(jsonString);
        }

        private void AddLambdaContextProperties()
        {
            globalProperties.Add(new LogProperty("AwsRequestId", lambdaContext.AwsRequestId));
            globalProperties.Add(new LogProperty("FunctionName", lambdaContext.FunctionName));
            globalProperties.Add(new LogProperty("FunctionVersion", lambdaContext.FunctionVersion));
            globalProperties.Add(new LogProperty("MemoryLimitInMB", lambdaContext.MemoryLimitInMB));
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public class LogProperty
    {
        public string Key { get; }
        public object Value { get; }

        public LogProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    internal class LogMessage
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
