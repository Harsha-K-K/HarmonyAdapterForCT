using Philips.Platform.Common;
using System;

namespace CTHarmonyAdapters
{
    internal class HarmonyTracer : CommonTracingBase
    {
        public override event EventHandler<EventArgs> TraceLevelsChanged;

        public override TraceLevel GetTraceLevel(string namespaceOrModuleName, string className)
        {
            return TraceLevel.None;
        }

        public override TraceLevel GetTraceLevel(string category, string namespaceOrModuleName, string className)
        {
            return TraceLevel.None;
        }

        public override void SetDefaultTraceLevel(TraceLevel defaultTraceLevel)
        {
            throw new NotImplementedException();
        }

        public override void SetTraceLevel(string fullyQualifiedClassName, TraceLevel traceLevel)
        {
            throw new NotImplementedException();
        }

        public override void TraceInfo(string namespaceOrModuleName, string className, string message, AdditionalLogData data)
        {
            throw new NotImplementedException();
        }

        public override void TraceVerbose(string namespaceOrModuleName, string className, string message, AdditionalLogData data)
        {
            throw new NotImplementedException();
        }
    }
}
