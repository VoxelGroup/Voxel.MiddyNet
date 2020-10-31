using System;
using System.Collections.Generic;
using System.Text;
using Amazon.SQS.Model;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.SQS
{
    public static class SendMessageRequestExtensions
    {
        public static void EnrichWithTraceContext(this SendMessageRequest sendMessageRequest, TraceContext traceContext)
        {
           
        }
    }
}
