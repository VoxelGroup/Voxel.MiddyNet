using System;

namespace Voxel.MiddyNet.HttpCors
{
    public class CorsOptions
    {
        public string Origin { get; set; }
        public string[] Origins { get; set; }
        public string Headers { get; set; }
        public bool Credentials { get; set; }
        public string CacheControl { get; set; }
        public string MaxAge { get; set; }

        public CorsOptions()
        {
            Origins = Array.Empty<string>();
        }
    }
}