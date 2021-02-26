using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Text.Json;

namespace Voxel.MiddyNet.ProblemDetails
{
    public abstract class ProxyResponseBuilder
    {
        private static readonly Dictionary<string, string> noCacheHeaders = new Dictionary<string, string>
        {
            [HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate",
            [HeaderNames.Pragma] = "no-cache",
            [HeaderNames.Expires] = "0"
        };

        protected IDictionary<string, IList<string>> Merge(IDictionary<string, IList<string>> multiValueHeaders)
        {
            var merged = multiValueHeaders == null
                ? new Dictionary<string, IList<string>>()
                : new Dictionary<string, IList<string>>(multiValueHeaders);

            var contentTypes = merged.ContainsKey(HeaderNames.ContentType)
                ? merged[HeaderNames.ContentType]
                : new List<string>();

            if (!contentTypes.Contains("application/problem+json"))
                contentTypes.Add("application/problem+json");

            merged[HeaderNames.ContentType] = contentTypes;

            return merged;
        }

        protected IDictionary<string, string> Merge(IDictionary<string, string> responseHeaders)
        {
            var merged = responseHeaders == null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(responseHeaders);

            foreach (var kv in noCacheHeaders)
            {
                merged[kv.Key] = kv.Value;
            }

            return merged;
        }

        protected static DetailsObject BuildProblemDetailsProblemContent(int statusCode, string instance, string requestId, string statusDescription, string content) => new DetailsObject
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = statusDescription,
            Status = statusCode,
            Detail = content,
            Instance = instance,
            AwsRequestId = requestId
        };

        protected class DetailsObject
        {
            private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
            public string Type { get; set; }
            public string Title { get; set; }
            public int Status { get; set; }
            public string Detail { get; set; }
            public string Instance { get; set; }
            public string AwsRequestId { get; set; }

            public string ToJsonString() => JsonSerializer.Serialize(this, jsonSerializerOptions);
        }
    }
}