using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using System.Collections.Generic;

namespace CTHarmonyAdapters
{
    internal class IncisiveCapabilitiesManager : DeviceCapabilitiesManagerBase
    {
        public override PrinterDeviceCapabilities GetPrinterDeviceCapability(string deviceId)
        {
            return null;
        }

        public override PrinterDeviceCapabilitiesCollection GetPrinterDeviceCapability(DeviceIdCollection deviceIds)
        {
            return new PrinterDeviceCapabilitiesCollection(new List<PrinterDeviceCapabilities>());
        }

        public override StorageDeviceCapabilities GetStorageDeviceCapability(string deviceId)
        {
            return null;
        }

        public override StorageDeviceCapabilitiesCollection GetStorageDeviceCapability(DeviceIdCollection deviceIds)
        {
            return new StorageDeviceCapabilitiesCollection(new List<StorageDeviceCapabilities>());
        }
    }
}
