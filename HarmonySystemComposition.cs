using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;

namespace CTHarmonyAdapters
{
    internal class HarmonySystemComposition : AiiSystemCompositionBase
    {
        protected override LoadManagerBase CreateLoadManager()
        {
            return HarmonyLoadManager.Instance;
        }

        protected override QueryManagerBase CreateQueryManager()
        {
            return HarmonyQueryManager.Instance;
        }

        protected override StoreManagerBase CreateStoreManager()
        {
            return new HarmonyStoreManager();
        }

        protected override CommonTracingBase CreateTracing()
        {
            return new HarmonyTracer();
        }

        protected override CommonLoggingBase CreateLogging()
        {
            return new HarmonyLogger();
        }
    }
}
