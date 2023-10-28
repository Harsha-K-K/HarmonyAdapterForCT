using System;
using System.Reflection;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using Philips.Platform.SystemIntegration.Decoupling;

namespace CTHarmonyAdapters
{
    internal class HarmonyStoreManager : Philips.Platform.ApplicationIntegration.Decoupling.StoreManagerBase
    {
        private StoreManagerBase actualStoreManager;

        public HarmonyStoreManager()
        {
            var assemb = Assembly.Load(@"D:\Repo\cp\System\SystemComponents\Output\Bin\Philips.Platform.System.dll");
            var t = assemb.GetType("Philips.Platform.SystemComponents.SystemComposition");
            //var c = t.GetConstructor(new object { });
            var sc = (SystemCompositionBase)Activator.CreateInstance(t);
            var m = t.GetProperty("StoreManager", BindingFlags.NonPublic | BindingFlags.Instance);
            
            actualStoreManager = (StoreManagerBase)m.GetValue(sc);
        }

        public override void StoreComposite(string deviceId, DicomObject compositeDicomObject, IntPtr pixelDataReference)
        {
            actualStoreManager.StoreComposite(deviceId, compositeDicomObject, pixelDataReference);
        }

        public override void StoreComposite(string deviceId, DicomObject compositeDicomObject)
        {
            actualStoreManager.StoreComposite(deviceId, compositeDicomObject);
        }

        public override void DelayedStoreComposite(string deviceId, DicomObject compositeDicomObject)
        {
            actualStoreManager.DelayedStoreComposite(deviceId, compositeDicomObject);
        }

        public override StoreSessionBase CreateStoreSession(DeviceIdCollection deviceIds)
        {
            var session = actualStoreManager.CreateStoreSession(deviceIds);
            return session;
        }

        public override StoreSessionBase CreateStoreSession(string deviceId)
        {
            var session = actualStoreManager.CreateStoreSession(deviceId);
            return session;
        }

        public override MultiFrameStoreSessionBase CreateMultiFrameStoreSession(DeviceIdCollection deviceIds, DicomObject commonHeader)
        {
            var session = actualStoreManager.CreateMultiFrameStoreSession(deviceIds, commonHeader);
            return session;
        }

        public override MultiFrameStoreSessionBase CreateMultiFrameStoreSession(string deviceId, DicomObject commonHeader)
        {
            var session = actualStoreManager.CreateMultiFrameStoreSession(deviceId, commonHeader);
            return session;
        }
    }
}