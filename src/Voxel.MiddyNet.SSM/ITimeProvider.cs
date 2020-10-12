using System;

namespace Voxel.MiddyNet.SSM
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}