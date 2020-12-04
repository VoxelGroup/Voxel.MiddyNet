using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public class MiddyNetContext
    {
        public ILambdaContext LambdaContext { get; private set; }
        public IDictionary<string, object> AdditionalContext { get; }
        public List<Exception> MiddlewareBeforeExceptions { get; internal set; }
        public List<Exception> MiddlewareAfterExceptions { get; internal set; }
        public Exception HandlerException { get; internal set; }
        public IMiddyLogger Logger { get; private set; }

        public MiddyNetContext(ILambdaContext context)
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareBeforeExceptions = new List<Exception>();
            LoggerFactory = logger => new MiddyLogger(logger);
            AttachToLambdaContext(context);
        }

        public MiddyNetContext(ILambdaContext context, Func<ILambdaLogger, IMiddyLogger> loggerFactory )
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareBeforeExceptions = new List<Exception>();
            LoggerFactory = loggerFactory;

            AttachToLambdaContext(context);
        }

        public void AttachToLambdaContext(ILambdaContext context)
        {
            LambdaContext = context;
            Logger = LoggerFactory(context.Logger);
        }

        public Func<ILambdaLogger, IMiddyLogger> LoggerFactory { get; }

        internal void FinishedBeforeMiddlewares()
        {
            MiddlewareAfterExceptions = new List<Exception>();
            MiddlewareBeforeExceptions = null; // We assume that the function has handled the exceptions Before middlewares might have thrown
        }
    }
}
