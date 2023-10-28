using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using Philips.Platform.SystemIntegration.Decoupling;

namespace CTHarmonyAdapters
{
    public class HarmonySystemComposition : SystemCompositionBase
    {
        private SystemCompositionBase sc;
        private DicomObjectFactoryBase factory;

        public HarmonySystemComposition()
        {
            Debugger.Launch();
            //File.AppendAllText(@"D:\Temp\CTHarmonyAdapter.txt", DateTime.Now + "\n" + Environment.StackTrace);
            var assemb = Assembly.Load("Philips.Platform.System");
            var t = assemb.GetType("Philips.Platform.SystemComponents.SystemComposition");
            sc = (SystemCompositionBase)Activator.CreateInstance(t);

            var assem = Assembly.Load("Philips.Platform.Dicom");
            var typ = assem.GetType("Philips.Platform.Dicom.DicomObjectFactory");
            factory = (DicomObjectFactoryBase)Activator.CreateInstance(typ);

            //var m = t.GetProperty("DicomObjectFactory",  BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            var dcmObjFactoryval = factory;
            
            var p = this.GetType().GetField("dicomObjectFactory", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            p.SetValue(this, dcmObjFactoryval);

            var q = this.GetType().GetField("dicomObjectFactoryIsInitialized", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            p.SetValue(this, true);

        }



        protected override DeviceConfigurationReaderBase CreateDeviceConfigurationReader()
        {
            return sc.DeviceConfigurationReader;
        }

        protected override DeviceConfigurationWriterBase CreateDeviceConfigurationWriter()
        {
            return sc.DeviceConfigurationWriter;
        }
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
