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

        private List<LogProperty> globalProperties = new List<LogProperty>();

        public MiddyLogger(ILambdaLogger lambdaLogger)
        {
            this.lambdaLogger = lambdaLogger;
        }

        public void Log(LogLevel logLevel, string message)
        {
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
            var logMessage = new LogMessage
            {
                Level = logLevel,
                Message = message,
                Properties = globalProperties.Concat(properties).ToDictionary(p=>p.Key, p=>p.Value)
            };

            InternalLog(logMessage);
        }

        public void Log<TInstance, TResponse>(LogLevel logLevel, string message, TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {
            var logMessage = new LogMessage
            {
                Level = logLevel,
                Message = message,
                Properties = globalProperties.Concat(GetPropertySelectorLogProperty(instance, selector)).ToDictionary(p => p.Key, p => p.Value)
            };

            InternalLog(logMessage);
        }

        private IEnumerable<LogProperty> GetPropertySelectorLogProperty<TInstance, TResponse>(TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {

            var getter = new KeyValueConverter<TInstance, TResponse>(instance, selector);
            yield return new LogProperty(getter.Key, getter.Value);
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

        public void EnrichWith(LogProperty logProperty)
        {
            globalProperties.Add(logProperty);
        }

        public void EnrichWith<TInstance, TResponse>(TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {
            globalProperties.Add(GetPropertySelectorLogProperty(instance, selector).FirstOrDefault());
        }
    }

    internal class KeyValueConverter<TInstance, TResponse>
    {
        private readonly TInstance instance;
        private readonly Expression<Func<TInstance, TResponse>> selector;

        public KeyValueConverter(TInstance instance, Expression<Func<TInstance, TResponse>> selector)
        {
            this.instance = instance;
            this.selector = selector;
        }

        public string Key => GetMemberName(selector.Body);

        public TResponse Value => selector.Compile().Invoke(instance);

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

            return string.Empty;
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
