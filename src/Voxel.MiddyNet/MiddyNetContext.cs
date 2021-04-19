using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;

namespace Voxel.MiddyNet
{
    public class MiddyNetContext
    {
        public ILambdaContext LambdaContext { get; private set; }
        public IDictionary<string, object> AdditionalContext { get; }
        public List<Exception> MiddlewareBeforeExceptions { get; }
        public List<Exception> MiddlewareAfterExceptions { get; }
        public Exception HandlerException { get; set; }
        public IMiddyLogger Logger { get; private set; }
        public Func<ILambdaLogger, IMiddyLogger> LoggerFactory { get; }

        public bool HasExceptions => HandlerException != null || MiddlewareBeforeExceptions.Count > 0 || MiddlewareAfterExceptions.Count > 0;

        public MiddyNetContext(ILambdaContext context)
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareBeforeExceptions = new List<Exception>();
            MiddlewareAfterExceptions = new List<Exception>();
            LoggerFactory = logger => new MiddyLogger(logger, context);
            AttachToLambdaContext(context);
        }

        public MiddyNetContext(ILambdaContext context, Func<ILambdaLogger, IMiddyLogger> loggerFactory )
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareBeforeExceptions = new List<Exception>();
            MiddlewareAfterExceptions = new List<Exception>();
            LoggerFactory = loggerFactory;
            AttachToLambdaContext(context);
        }

        public void AttachToLambdaContext(ILambdaContext context)
        {
            LambdaContext = context;
            Logger = LoggerFactory(context.Logger);
        }

        public List<Exception> GetAllExceptions()
        {
            var all = new List<Exception>();
            all.AddRange(MiddlewareBeforeExceptions);
            if (HandlerException != null) all.Add(HandlerException);
            all.AddRange(MiddlewareAfterExceptions);
            return all;
        }

        public void Clear()
        {
            AdditionalContext.Clear();
            MiddlewareBeforeExceptions.Clear();
            HandlerException = null;
            MiddlewareAfterExceptions.Clear();
        }
    }
}
