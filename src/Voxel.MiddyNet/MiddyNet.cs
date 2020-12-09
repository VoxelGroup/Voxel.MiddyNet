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

            var response = await SafeHandleLambdaEvent(lambdaEvent).ConfigureAwait(false);

            response = await ExecuteAfterMiddlewares(response);

            if (MiddyContext.HasExceptions)
                throw new AggregateException(MiddyContext.GetAllExceptions());

            return response;
        }

        private async Task<TRes> SafeHandleLambdaEvent(TReq lambdaEvent)
        {
            TRes response = default(TRes);
            try
            {
                response = await Handle(lambdaEvent, MiddyContext);
            }
            catch (Exception ex)
            {
                MiddyContext.HandlerException = ex;
            }

            return response;
        }

        private async Task<TRes> ExecuteAfterMiddlewares(TRes response)
        {
            foreach (var middleware in Enumerable.Reverse(middlewares))
            {
                try
                {
                    response = await middleware.After(response, MiddyContext);
                }
                catch (Exception ex)
                {
                    MiddyContext.MiddlewareAfterExceptions.Add(ex);
                }
            }

            return response;
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
                    MiddyContext.MiddlewareBeforeExceptions.Add(ex);
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
            
            MiddyContext.Clear();
        }

        public MiddyNet<TReq, TRes> Use(ILambdaMiddleware<TReq, TRes> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        protected abstract Task<TRes> Handle(TReq lambdaEvent, MiddyNetContext context);
    }
}
