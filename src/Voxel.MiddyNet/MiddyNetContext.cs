using System;
using System.Collections.Generic;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public class MiddyNetContext
    {
        public ILambdaContext LambdaContext { get; set; }
        public IDictionary<string, object> AdditionalContext { get; }
        public List<Exception> MiddlewareExceptions { get; set; }

        public MiddyNetContext()
        {
            AdditionalContext = new Dictionary<string, object>();
            MiddlewareExceptions = new List<Exception>();
        }
    }
}
