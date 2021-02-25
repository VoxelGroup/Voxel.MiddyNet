using System;

namespace Voxel.MiddyNet.SSMMiddleware
{
    public class CachedParameter
    {
        public DateTimeOffset InsertDateTime { get; }
        public string Value { get; }

        public CachedParameter(DateTimeOffset insertDateTime, string value)
        {
            InsertDateTime = insertDateTime;
            Value = value;
        }
    }
}