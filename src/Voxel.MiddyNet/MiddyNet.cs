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

        protected MiddyNet()
        {
            MiddyContext = new MiddyNetContext();
        }

        public async Task<TRes> Handler(TReq lambdaEvent, ILambdaContext context)
        {
            MiddyContext.LambdaContext = context;
            MiddyContext.AdditionalContext.Clear(); //  Given that the instance is reused, we need to clean the dictionary.

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

            var response = await Handle(lambdaEvent, MiddyContext);

            MiddyContext.MiddlewareExceptions.Clear(); // We assume that the function is Ok with the exceptions it might have received

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

            if (MiddyContext.MiddlewareExceptions.Any()) throw new AggregateException(MiddyContext.MiddlewareExceptions);

            return response;
        }

        protected MiddyNet<TReq, TRes> Use(ILambdaMiddleware<TReq, TRes> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        protected abstract Task<TRes> Handle(TReq lambdaEvent, MiddyNetContext context);
    }
}
