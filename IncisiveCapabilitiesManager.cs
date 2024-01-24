using Philips.Platform.ApplicationIntegration.Decoupling;
using Philips.Platform.Common;
using Philips.Platform.SystemIntegration.Decoupling;
using System;
using System.Collections.Generic;
using System.Reflection;

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
            StorageDeviceCapabilities deviceCapabilities = new StorageDeviceCapabilities();

            PropertyInfo propertyInfo = typeof(StorageDeviceCapabilities).GetProperty("DeviceId");
            propertyInfo.SetValue(deviceCapabilities, deviceId);
            PropertyInfo propertyInfoCanSubscribeForDataModifyEvent = typeof(StorageDeviceCapabilities).GetProperty("CanSubscribeForDataModifyEvent");
            propertyInfoCanSubscribeForDataModifyEvent.SetValue(deviceCapabilities, true);

            //var assemb = Assembly.Load("Philips.Platform.CommonTypes");
            //var type = assemb.GetType("Philips.Platform.Common.StorageDeviceCapabilities");
            //var q = type.GetProperty("DeviceId", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            //q.SetValue(this, deviceId);
            //var sc = (StorageDeviceCapabilities)Activator.CreateInstance(type);

            //StorageDeviceCapabilities deviceCapabilities = new StorageDeviceCapabilities();
            //deviceCapabilities.DeviceId = deviceId;
            return deviceCapabilities;
        }

        public override StorageDeviceCapabilitiesCollection GetStorageDeviceCapability(DeviceIdCollection deviceIds)
        {
            return new StorageDeviceCapabilitiesCollection(new List<StorageDeviceCapabilities>());
        }
    }
}
