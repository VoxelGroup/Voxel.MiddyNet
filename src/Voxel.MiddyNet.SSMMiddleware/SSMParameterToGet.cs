﻿namespace Voxel.MiddyNet.SSMMiddleware
{
    public class SSMParameterToGet
    {
        public string Name { get; }
        public string Path { get; }

        public SSMParameterToGet(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}