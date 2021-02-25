using System.Collections.Generic;

namespace Voxel.MiddyNet.SSMMiddleware
{
    public class SSMOptions
    {
        public List<SSMParameterToGet> ParametersToGet { get; set; }
        public int CacheExpiryInMillis { get; set; }
    }
}