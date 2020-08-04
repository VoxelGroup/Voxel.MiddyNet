using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public abstract class MiddyNet<Req, Res>
    {
        private MiddyNetContext MiddyContext { get; set; }
        private readonly List<ILambdaMiddleware<Req, Res>> middlewares = new List<ILambdaMiddleware<Req, Res>>();

        protected MiddyNet()
        {
            MiddyContext = new MiddyNetContext();
        }

        public async Task<Res> Handler(Req lambdaEvent, ILambdaContext context)
        {
            MiddyContext.LambdaContext = context;
            MiddyContext.AdditionalContext.Clear(); //  Given that the instance is reused, we need to clean the dictionary.

            foreach (var middleware in middlewares)
            {
                await middleware.Before(lambdaEvent, MiddyContext);
            }

            var response = await Handle(lambdaEvent, MiddyContext);

            foreach (var middleware in Enumerable.Reverse(middlewares))
            {
                response = await middleware.After(response, MiddyContext);
            }

            return response;
        }

        protected MiddyNet<Req, Res> Use(ILambdaMiddleware<Req, Res> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        protected abstract Task<Res> Handle(Req lambdaEvent, MiddyNetContext context);
    }
}
