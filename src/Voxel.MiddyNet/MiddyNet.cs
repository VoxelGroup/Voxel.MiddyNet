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
        private readonly List<ILambdaMiddleware<TReq, TRes>> middlewares = new List<ILambdaMiddleware<TReq, TRes>>();
        
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
            foreach (var middleware in Enumerable.Reverse(middlewares))
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

        public MiddyNet<TReq, TRes> Use(ILambdaMiddleware<TReq, TRes> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        protected abstract Task<TRes> Handle(TReq lambdaEvent, MiddyNetContext context);
    }
}
