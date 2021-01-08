using System.Net.Http;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.Http
{
    public static class HttpRequestMessageExtensions
    {
        private const string TraceParentHeaderName = "traceparent";
        private const string TraceStateHeaderName = "tracestate";

        public static void EnrichWithTraceContext(this HttpRequestMessage httpRequestMessage, TraceContext traceContext)
        {
            httpRequestMessage.ReplaceHeader(TraceParentHeaderName, traceContext.TraceParent);
            httpRequestMessage.ReplaceHeader(TraceStateHeaderName, traceContext.TraceState);
        }

        private static void ReplaceHeader(this HttpRequestMessage httpRequestMessage, string headerName, string headerValue)
        {
            httpRequestMessage.Headers.Remove(headerName);
            httpRequestMessage.Headers.Add(headerName, headerValue);
        }
    }
}
