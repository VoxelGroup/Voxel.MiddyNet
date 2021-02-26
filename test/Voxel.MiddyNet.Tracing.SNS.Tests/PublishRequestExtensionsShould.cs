using System;
using Amazon.SimpleNotificationService.Model;
using FluentAssertions;
using Voxel.MiddyNet.Tracing.Core;
using Xunit;

namespace Voxel.MiddyNet.Tracing.SNS.Tests
{
    public class PublishRequestExtensionsShould
    {
        [Fact]
        public void EnrichMessageAttributesWithTraceContext()
        {
            var publishRequest = new PublishRequest();
            var traceContext = TraceContext.Handle("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01",
                "congo=t61rcWkgMzE");

            publishRequest.EnrichWithTraceContext(traceContext);

            publishRequest.MessageAttributes.Should().ContainKey("traceparent");
            publishRequest.MessageAttributes.Should().ContainKey("tracestate");
        }
    }
}
