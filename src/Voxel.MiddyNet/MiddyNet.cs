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

            middlewares.ForEach(async m => await m.Before(lambdaEvent, MiddyContext));

            var response = await Handle(lambdaEvent, MiddyContext);

            foreach (var middleware in Enumerable.Reverse(middlewares))
            {
                response = await middleware.After(response, MiddyContext);
            }

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
