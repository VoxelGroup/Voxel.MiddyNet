using System.Collections.Generic;
using Amazon.Lambda.Core;

namespace Voxel.MiddyNet
{
    public class MiddyNetContext
    {
        public ILambdaContext LambdaContext { get; set; }
        public IDictionary<string, object> AdditionalContext { get; }

        public MiddyNetContext()
        {
            AdditionalContext = new Dictionary<string, object>();
        }
    }
}
