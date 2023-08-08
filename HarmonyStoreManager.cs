using System;
using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;

namespace CTHarmonyAdapters
{
    internal class HarmonyStoreManager : Philips.Platform.ApplicationIntegration.Decoupling.StoreManagerBase
    {
        public override void StoreComposite(string deviceId, DicomObject compositeDicomObject, IntPtr pixelDataReference)
        {
            throw new NotImplementedException();
        }

        public override void StoreComposite(string deviceId, DicomObject compositeDicomObject)
        {
            throw new NotImplementedException();
        }

        public override void DelayedStoreComposite(string deviceId, DicomObject compositeDicomObject)
        {
            throw new NotImplementedException();
        }

        public override StoreSessionBase CreateStoreSession(DeviceIdCollection deviceIds)
        {
            throw new NotImplementedException();
        }

        public override StoreSessionBase CreateStoreSession(string deviceId)
        {
            throw new NotImplementedException();
        }

        public override MultiFrameStoreSessionBase CreateMultiFrameStoreSession(DeviceIdCollection deviceIds, DicomObject commonHeader)
        {
            throw new NotImplementedException();
        }

        public override MultiFrameStoreSessionBase CreateMultiFrameStoreSession(string deviceId, DicomObject commonHeader)
        {
            throw new NotImplementedException();
        }
    }
}