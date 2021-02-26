using System;

namespace Voxel.MiddyNet.SSMMiddleware
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}