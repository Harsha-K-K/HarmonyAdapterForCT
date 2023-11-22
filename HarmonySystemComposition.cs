using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using Philips.Platform.SystemIntegration.Decoupling;

namespace CTHarmonyAdapters
{
    public class HarmonySystemComposition : SystemCompositionBase

    {
        private DataModificationEventsBase incisiveEventManager = new IncisiveEventManager();
        private DeviceCapabilitiesManagerBase incisiveCapabilitiesManager = new IncisiveCapabilitiesManager();
        private AuthorizationManagerBase incisiveAuthorizationManager = new IncisiveAuthorizationManager();
        private PatientKeyProviderBase incisivePatientKeyProvider = new IncisivePatientKeyProvider();
        private DeviceConfigurationReaderBase incisiveDeviceConfigurationReader = new IncisiveDeviceConfigurationReader();
        private DeviceConfigurationWriterBase incisiveDeviceConfigurationWriter = new IncisiveDeviceConfigurationWriter();

        //private DeviceConfigurationReader deviceCongifReader;
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
            return incisiveEventManager;
        }

        protected override DeviceCapabilitiesManagerBase CreateDeviceCapabilitiesManager()
        {
            return incisiveCapabilitiesManager;
        }

        protected override DicomObjectFactoryBase CreateDicomObjectFactory()
        {
            return DicomObject.DicomObjectFactory;
        }

        protected override AuthorizationManagerBase CreateAuthorizationManager()
        {
            return incisiveAuthorizationManager;
        }

        protected override PatientKeyProviderBase CreatePatientKeyProvider()
        {
            return incisivePatientKeyProvider;
        }

        protected override DeviceConfigurationReaderBase CreateDeviceConfigurationReader()
        {
            return incisiveDeviceConfigurationReader;
        }

        protected override DeviceConfigurationWriterBase CreateDeviceConfigurationWriter()
        {
            return incisiveDeviceConfigurationWriter;
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
            /*return new HarmonyStoreManager()*/
            ;
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
