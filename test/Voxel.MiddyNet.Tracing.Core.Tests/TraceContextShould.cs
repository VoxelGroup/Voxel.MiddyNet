using FluentAssertions;
using Xunit;
using Voxel.MiddyNet.Tracing.Core;

namespace Voxel.MiddyNet.Tracing.Core.Tests
{
    public class TraceContextShould
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "something")]
        [InlineData("", null)]
        [InlineData("", "something")]
        public void CreateANewOneIfTraceParentIsNotReceived(string traceParent, string traceState)
        {
            var traceContext = TraceContext.Handle(traceParent, traceState);

            traceContext.TraceParent.Should().NotBeNull();
            traceContext.TraceState.Should().BeEmpty();
        }

        [Fact]
        public void CreateADifferentTraceContextEachTime()
        {
            var traceContext1 = TraceContext.Handle(null, null);
            var traceContext2 = TraceContext.Handle(null, null);

            traceContext1.TraceParent.Should().NotBe(traceContext2.TraceParent);
        }

        [Fact]
        public void CreateANewTraceparentAndResetTraceStateIfTheVersionCanNotBeParsed()
        {
            var traceContext = TraceContext.Handle("0-0-0-0", "a trace state");

            traceContext.TraceParent.Should().NotBe("0-0-0-0");
            traceContext.TraceState.Should().BeEmpty();
        }

        [Fact]
        public void CreateANewTraceparentAndResetTraceStateIfTheTraceIdIsNotValid()
        {
            var traceContext = TraceContext.Handle("00-0-0-0", "a trace state");

            traceContext.TraceParent.Should().NotBe("00-0-0-0");
            traceContext.TraceState.Should().BeEmpty();
        }

        [Fact]
        public void CreateANewTraceparentAndResetTraceStateIfTheParentIdIsNotValid()
        {
            var traceContext = TraceContext.Handle("00-12345678901234567890123456789012-0-0", "a trace state");

            traceContext.TraceParent.Should().NotBe("00-12345678901234567890123456789012-0-0");
            traceContext.TraceState.Should().BeEmpty();
        }

        [Fact]
        public void CreateANewTraceparentAndResetTraceStateIfFlagsAreNotValid()
        {
            var traceContext = TraceContext.Handle("00-12345678901234567890123456789012-1234567890123456-0", "a trace state");

            traceContext.TraceParent.Should().NotBe("00-12345678901234567890123456789012-1234567890123456-0");
            traceContext.TraceState.Should().BeEmpty();
        }

        [Fact]
        public void UpdateParentIdIfTraceParentIsCorrect()
        {
            var traceContext = TraceContext.Handle("00-12345678901234567890123456789012-1234567890123456-00", "a trace state");
            traceContext.TraceParent.Should().StartWith("00-12345678901234567890123456789012-")
                .And.EndWith("-00")
                .And.NotEndWith("-1234567890123456-00");
        }

        [Fact]
        public void PropagateTraceStateIfTraceParentIsCorrect()
        {
            var traceContext = TraceContext.Handle("00-12345678901234567890123456789012-1234567890123456-00", "a trace state");
            traceContext.TraceState.Should().Be("a trace state");
        }
    }
}
