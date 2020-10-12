using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public abstract class MiddyNet<TReq, TRes>
    {
        private MiddyNetContext MiddyContext { get; set; }
        private readonly List<IBeforeLambdaMiddleware<TReq>> beforeMiddlewares = new List<IBeforeLambdaMiddleware<TReq>>();
        private readonly List<IAfterLambdaMiddleware<TRes>> afterMiddlewares = new List<IAfterLambdaMiddleware<TRes>>();
        
        public async Task<TRes> Handler(TReq lambdaEvent, ILambdaContext context)
        {
            InitialiseMiddyContext(context);

            await ExecuteBeforeMiddlewares(lambdaEvent);

            var response = await Handle(lambdaEvent, MiddyContext);

            MiddyContext.MiddlewareExceptions.Clear(); // We assume that the function is Ok with the exceptions it might have received

            await ExecuteAfterMiddlewares(response);

            if (MiddyContext.MiddlewareExceptions.Any()) throw new AggregateException(MiddyContext.MiddlewareExceptions);

            return response;
        }

        private async Task ExecuteAfterMiddlewares(TRes response)
        {
            foreach (var middleware in Enumerable.Reverse(afterMiddlewares))
            {
                try
                {
                    response = await middleware.After(response, MiddyContext);
                }
                catch (Exception ex)
                {
                    MiddyContext.MiddlewareExceptions.Add(ex);
                }
            }
        }

        private async Task ExecuteBeforeMiddlewares(TReq lambdaEvent)
        {
            foreach (var middleware in beforeMiddlewares)
            {
                try
                {
                    await middleware.Before(lambdaEvent, MiddyContext);
                }
                catch (Exception ex)
                {
                    MiddyContext.MiddlewareExceptions.Add(ex);
                }
            }
        }

        private void InitialiseMiddyContext(ILambdaContext context)
        {
            if (MiddyContext == null)
            {
                MiddyContext = new MiddyNetContext(context);
            }
            else
            {
                MiddyContext.AttachToLambdaContext(context);
            }

            MiddyContext.AdditionalContext.Clear(); //  Given that the instance is reused, we need to clean the dictionary.
        }

        public MiddyNet<TReq, TRes> Use(IBeforeLambdaMiddleware<TReq> middleware)
        {
            beforeMiddlewares.Add(middleware);
            return this;
        }

        public MiddyNet<TReq, TRes> Use(IAfterLambdaMiddleware<TRes> middleware)
        {
            afterMiddlewares.Add(middleware);
            return this;
        }

        protected abstract Task<TRes> Handle(TReq lambdaEvent, MiddyNetContext context);
    }

    public abstract class MiddyNet<TReq>
    {
        private MiddyNetContext MiddyContext { get; set; }
        private readonly List<IBeforeLambdaMiddleware<TReq>> middlewares = new List<IBeforeLambdaMiddleware<TReq>>();

        public async Task Handler(TReq lambdaEvent, ILambdaContext context)
        {
            InitialiseMiddyContext(context);

            await ExecuteBeforeMiddlewares(lambdaEvent);

            await Handle(lambdaEvent, MiddyContext);
        }

        private async Task ExecuteBeforeMiddlewares(TReq lambdaEvent)
        {
            foreach (var middleware in middlewares)
            {
                try
                {
                    await middleware.Before(lambdaEvent, MiddyContext);
                }
                catch (Exception ex)
                {
                    MiddyContext.MiddlewareExceptions.Add(ex);
                }
            }
        }

        private void InitialiseMiddyContext(ILambdaContext context)
        {
            if (MiddyContext == null)
            {
                MiddyContext = new MiddyNetContext(context);
            }
            else
            {
                MiddyContext.AttachToLambdaContext(context);
            }

            MiddyContext.AdditionalContext.Clear(); //  Given that the instance is reused, we need to clean the dictionary.
        }

        protected MiddyNet<TReq> Use(IBeforeLambdaMiddleware<TReq> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        protected abstract Task Handle(TReq lambdaEvent, MiddyNetContext context);
    }
}
