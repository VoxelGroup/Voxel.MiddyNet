using System;
using System.Linq;

namespace Voxel.MiddyNet.Tracing.Core
{
    public class TraceContext
    {
        private string version;
        public string TraceId { get; }
        private string parentId;
        private string traceFlags;

        private TraceContext(string version, string traceId, string parentId, string traceFlags)
        {
            this.version = version;
            TraceId = traceId;
            this.parentId = parentId;
            this.traceFlags = traceFlags;
            TraceState = string.Empty;
        }

        private TraceContext(string version, string traceId, string parentId, string traceFlags, string traceState)
        {
            this.version = version;
            TraceId = traceId;
            this.parentId = parentId;
            this.traceFlags = traceFlags;
            TraceState = traceState;
        }

        public static TraceContext Handle(string traceParent, string traceState)
        {
            if (string.IsNullOrWhiteSpace(traceParent))
            {
                return Reset();
            }

            var traceParentSplitted = traceParent.Split('-');
            if (traceParentSplitted.Length != 4)
            {
                return Reset();
            }

            var version = traceParentSplitted[0];
            var traceId = traceParentSplitted[1];
            var parentId = traceParentSplitted[2];
            var traceFlags = traceParentSplitted[3];

            if (VersionFormatIsInvalid(version) 
                || TraceIdIsInvalid(traceId) 
                || ParentIdIsInvalid(parentId) 
                || FlagsAreNotValid(traceFlags))
            {
                return Reset();
            }

            return new TraceContext(version, traceId, parentId, traceFlags, traceState);
        }

        private static bool FlagsAreNotValid(string traceFlags)
        {
            return traceFlags.Length != 2;
        }

        private static bool ParentIdIsInvalid(string parentId)
        {
            return parentId.Length != 16;
        }

        private static bool TraceIdIsInvalid(string traceId)
        {
            return traceId.Length != 32;
        }

        private static bool VersionFormatIsInvalid(string version)
        {
            return version.Length != 2;
        }

        private static TraceContext Reset()
        {
            return new TraceContext("00", RandomString(32), RandomString(16), "00");
        }

        public string TraceParent => $"{version}-{TraceId}-{parentId}-{traceFlags}";
        public string TraceState { get; }

        private static readonly Random random = new Random();
        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static TraceContext MutateParentId(string traceparent, string tracestate)
        {
            var traceContext = Handle(traceparent, tracestate);
            return new TraceContext(traceContext.version, traceContext.TraceId, RandomString(16), traceContext.traceFlags, traceContext.TraceState);
        }
    }
}
