using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public class MiddyNetContext
    {
        public ILambdaContext LambdaContext { get; private set; }
        public IDictionary<string, object> AdditionalContext { get; }
        public List<Exception> MiddlewareExceptions { get; set; }
        public MiddyLogger Logger { get; set; }

        public MiddyNetContext()
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareExceptions = new List<Exception>();
        }

        public void AttachToLambdaContext(ILambdaContext context)
        {
            LambdaContext = context;
            Logger = new MiddyLogger(context.Logger);
        }
    }
}
