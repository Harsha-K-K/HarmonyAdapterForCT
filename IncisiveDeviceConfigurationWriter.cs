using Philips.Platform.SystemIntegration;
using Philips.Platform.SystemIntegration.Decoupling;
using System;

namespace CTHarmonyAdapters
{
    internal class IncisiveDeviceConfigurationWriter : DeviceConfigurationWriterBase
    {
        public override event EventHandler<MediaInsertedEventArgs> MediaInserted;
        public override event EventHandler<MediaEjectedEventArgs> MediaEjected;

        public override void AddDevice(BasicDeviceInfo deviceInfo)
        {
        }

        public override void MountDevice(BasicDeviceInfo deviceInfo, string dicomDirPath)
        {

        }

        public override void RemoveDevice(string deviceId)
        {

        }

        public override void UNMountDevice(string deviceId)
        {

        }

        public override void UpdateDeviceName(BasicDeviceInfo deviceInfo, string newName)
        {

        }
    }
}
