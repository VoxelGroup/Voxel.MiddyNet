using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq.Expressions;
using System;

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

        public void EnrichWith<TInstance, TResponse>(TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {
            globalProperties.Add(GetPropertySelectorLogProperty(instance, selector));
        }

        private LogProperty GetPropertySelectorLogProperty<TInstance, TResponse>(TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {
            var getter = new LogPropertyKeyCalculator<TInstance, TResponse>(selector);
            return new LogDelayedProperty<TInstance, TResponse>(getter.Key, instance, selector);
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

    internal class LogPropertyKeyCalculator<TInstance, TResponse>
    {
        private readonly Expression<Func<TInstance, TResponse>> selector;
        
        public LogPropertyKeyCalculator(Expression<Func<TInstance, TResponse>> selector)
        {
            this.selector = selector;
        }

        public string Key => GetMemberName(selector.Body);

        private static string GetMemberName(Expression expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }
            if (expression is MethodCallExpression methodCallExpression)
            {
                return methodCallExpression.Method.Name;
            }
            if (expression is UnaryExpression unaryExpression)
            {
                return GetMemberName(unaryExpression);
            }

            return null;
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression methodExpression)
            {
                return methodExpression.Method.Name;
            }
            return ((MemberExpression)unaryExpression.Operand).Member.Name;
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
        public virtual object Value { get; }

        public LogProperty(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    public class LogDelayedProperty<TInstance, TResponse> : LogProperty
    {

        private readonly Expression<Func<TInstance, TResponse>> valueExpression;
        private readonly TInstance instance;

        public override object Value 
        {
            get
            {
                return valueExpression.Compile().Invoke(instance);
            }
        }

        public LogDelayedProperty(string key, TInstance instance, Expression<Func<TInstance, TResponse>> valueExpression): base(key, null)
        {
            this.valueExpression = valueExpression;
            this.instance = instance;
        }
    }

    internal class LogMessage
    {
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
