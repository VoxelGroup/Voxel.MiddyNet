using System.Collections.Generic;

namespace Voxel.MiddyNet.SSM
{
    public class SSMOptions
    {
        public List<SSMParameterToGet> ParametersToGet { get; set; }
        public int CacheExpiryInMillis { get; set; }
    }
}