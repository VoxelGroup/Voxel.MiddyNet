using System;
using System.Collections.Generic;

namespace Voxel.MiddyNet.ProblemDetailsMiddleware
{
    public class ProblemDetailsMiddlewareOptions
    {
        private readonly Dictionary<Type, int> mappings;

        public ProblemDetailsMiddlewareOptions()
        {
            mappings = new Dictionary<Type, int>();
        }

        public ProblemDetailsMiddlewareOptions Map<T>(int statusCode) where T: Exception
        {
            mappings[typeof(T)] = statusCode;
            return this;
        }

        public bool TryMap(Type type, out int statusCode)
        {
            foreach(var map in mappings)
            {
                if (map.Key.IsAssignableFrom(type))
                {
                    statusCode = map.Value;
                    return true;
                }
            }
            statusCode = 500;
            return false;
        }
    }
}
