using Philips.Platform.Adapters.Authorization;
using Philips.Platform.Adapters.Services;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using Philips.Platform.Dicom;
using Philips.Platform.SystemIntegration.Decoupling;

namespace CTHarmonyAdapters
{
    public class HarmonySystemComposition : SystemCompositionBase

    {
        private DeviceConfigurationReader deviceCongifReader;
        //private SystemCompositionBase sc;
        //private DicomObjectFactoryBase factory;
        public HarmonySystemComposition()
        {
            //    Debugger.Launch();
            //DicomBootstrap.Execute();
            //    //File.AppendAllText(@"D:\Temp\CTHarmonyAdapter.txt", DateTime.Now + "\n" + Environment.StackTrace);
            //    var assemb = Assembly.Load("Philips.Platform.System");
            //    var t = assemb.GetType("Philips.Platform.SystemComponents.SystemComposition");
            //    sc = (SystemCompositionBase)Activator.CreateInstance(t);

            //    var assem = Assembly.Load("Philips.Platform.Dicom");
            //    var typ = assem.GetType("Philips.Platform.Dicom.DicomObjectFactory");
            //    factory = (DicomObjectFactoryBase)Activator.CreateInstance(typ);

            //    //var m = t.GetProperty("DicomObjectFactory",  BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //    var dcmObjFactoryval = factory;

            //    var p = this.GetType().GetField("dicomObjectFactory", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //    p.SetValue(this, dcmObjFactoryval);

            //    var q = this.GetType().GetField("dicomObjectFactoryIsInitialized", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //    p.SetValue(this, true);

        }

        protected override DataModificationEventsBase CreateDataModificationEvents()
        {
            //dummy return
            return EventManager.Instance;
        }

        protected override DeviceCapabilitiesManagerBase CreateDeviceCapabilitiesManager()
        {
            return Philips.Platform.Adapters.Services.DeviceCapabilitiesManager.Instance;
        }

        protected override DicomObjectFactoryBase CreateDicomObjectFactory()
        {
            return DicomObject.DicomObjectFactory;
        }

        protected override AuthorizationManagerBase CreateAuthorizationManager()
        {
            return AipAuthorizationManager.Instance;
        }

        protected override PatientKeyProviderBase CreatePatientKeyProvider()
        {
            return Philips.Platform.StorageDevices.PatientKeyProvider.Instance;
        }

        protected override DeviceConfigurationReaderBase CreateDeviceConfigurationReader()
        {
            return new DeviceConfigurationReader();
        }

        protected override DeviceConfigurationWriterBase CreateDeviceConfigurationWriter()
        {
            return new DeviceConfigurationWriter();
        }

        protected override LoadManagerBase CreateLoadManager()
        {
            return HarmonyLoadManager.Instance;
            //return base.CreateLoadManager();
        }

        protected override QueryManagerBase CreateQueryManager()
        {
            DicomObjectFactoryBase dcm = DicomObject.DicomObjectFactory;

            return HarmonyQueryManager.Instance;
            //return base.CreateQueryManager();
        }

        protected override StoreManagerBase CreateStoreManager()
        {
            return base.CreateStoreManager();
            /*return new HarmonyStoreManager()*/;
        }

        protected override CommonTracingBase CreateTracing()
        {
            return base.CreateTracing();
        }

        protected override CommonLoggingBase CreateLogging()
        {
            return base.CreateLogging();
        }
    }
}
