using Philips.Platform.SystemIntegration;
using Philips.Platform.SystemIntegration.Decoupling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace CTHarmonyAdapters
{
    internal class IncisiveDeviceConfigurationReader : DeviceConfigurationReaderBase
    {
        public override string PrimaryDeviceId => throw new NotImplementedException();

        public override bool ClearDataFromDevice(string deviceId)
        {
            return true;
        }

        public override Stream GetDeviceConfiguration(string deviceId)
        {
            return null;
        }

        public override ReadOnlyCollection<BasicDeviceInfo> GetDeviceInfo(DeviceTypes deviceType)
        {
            return new ReadOnlyCollection<BasicDeviceInfo>(new List<BasicDeviceInfo>());
        }

        public override BasicDeviceInfo GetDeviceInfo(string deviceId)
        {
            return null;
        }

        public override FileRepositoryConfiguration GetFileRepositoryConfiguration(string deviceId)
        {
            return null;
        }

        public override LocalFolderConfiguration GetLocalFolderConfiguration(string deviceId)
        {
            return null;
        }

        public override MediaRepositoryConfiguration GetMediaDeviceConfiguration(string deviceId)
        {
            return null;
        }

        public override NetworkRepositoryConfiguration GetNetworkDeviceConfiguration(string deviceId)
        {
            return null;
        }

        public override bool ValidateImageDatabase(out string errorMessage)
        {
            errorMessage = null;
            return true;
        }
    }
}
