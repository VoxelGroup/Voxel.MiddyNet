using System;

namespace Voxel.MiddyNet.SSMMiddleware
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}