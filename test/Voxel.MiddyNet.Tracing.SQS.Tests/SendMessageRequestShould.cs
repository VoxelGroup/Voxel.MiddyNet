using Amazon.SQS.Model;
using FluentAssertions;
using Voxel.MiddyNet.Tracing.Core;
using Xunit;

namespace Voxel.MiddyNet.Tracing.SQS.Tests
{
    public class SendMessageRequestShould
    {
        [Fact]
        public void EnrichMessageAttributesWithTraceContext()
        {
            var sendMessageRequest = new SendMessageRequest();
            var traceContext = TraceContext.Handle("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
                "congo=t61rcWkgMzE");

            sendMessageRequest.EnrichWithTraceContext(traceContext);

            sendMessageRequest.MessageAttributes.Should().ContainKey("traceparent");
            sendMessageRequest.MessageAttributes.Should().ContainKey("tracestate");
        }
    }
}
