using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philips.Platform.Common;

namespace CTHarmonyAdapters
{
    //NOTE from Ashutosh. UADE/Harmony may not be using logging. So, implement only when you hit any exception.
    internal class HarmonyLogger : CommonLoggingBase
    {
        public override void LogMessage(int moduleId, string humanReadableModuleId, int eventId, string humanReadableEventId, DateTime dateTime,
            LogType logType, string eventType, Severity severity, string description, int threadId, string threadName,
            int processId, string processName, string machineName, string contextInfo, string additionalInfo,
            Exception exceptionInfo, StackTrace stackTrace)
        {
        }

        public override void LogMessage(int moduleId, int eventId, DateTime dateTime, LogType logType, string eventType, Severity severity,
            string[] descriptionParameters, int threadId, string threadName, int processId, string processName,
            string machineName, string contextInfo, string additionalInfo, Exception exceptionInfo, StackTrace stackTrace)
        {
        }
    }
}
