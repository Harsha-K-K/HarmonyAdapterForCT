using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Philips.Pcc.CT.Console.Foundation.Common.Function;
using System.Timers;
using Philips.Platform.Adapters.Services;
using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using Philips.Platform.StorageDevicesClient;

namespace CTHarmonyAdapters
{
    public class HarmonyLoadManager : Philips.Platform.ApplicationIntegration.Decoupling.LoadManagerBase
    {
        public IIncisiveAccessor.IIncisiveAccessor proxy;
        private const string Uri = "net.tcp://localhost:6565/IncisiveAccessor";


        public HarmonyLoadManager()
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var channel = new ChannelFactory<IIncisiveAccessor.IIncisiveAccessor>(binding);
            var endpoint = new EndpointAddress(Uri);

            proxy = channel.CreateChannel(endpoint);

        }

        public override PersistentDicomObjectCollection LoadFastHeaders(StorageKeyCollection storageKeys)
        {
            var timer = Stopwatch.StartNew();
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            var studyInstanceUids = new List<string>();
            var seriesInstanceUids = new List<string>();
            var sopInstanceUids = new List<string>();

            

            foreach (var storageKey in storageKeys)
            {
                studyInstanceUids.Add(storageKey.Identifier.StudyInstanceUid);
                seriesInstanceUids.Add(storageKey.Identifier.SeriesInstanceUid);

                var sopInstanceUid = proxy.GetSopInstanceUids(storageKey.Identifier.SeriesInstanceUid);
                sopInstanceUids.Add(sopInstanceUid.FirstOrDefault()); // optimise: GetAt(0) ??
            }

            var wcfTimer = Stopwatch.StartNew();
            var filepaths = proxy.GetImageFilePaths(studyInstanceUids, seriesInstanceUids, sopInstanceUids); //should get from DB
            wcfTimer.Stop();

            var filepathArray = filepaths as string[] ?? filepaths.ToArray();
            for(var itemIndex = 0; itemIndex<filepathArray.Count(); itemIndex++)
            {
                var dcmObj = DicomObject.CreateInstance(filepathArray.ElementAt(itemIndex));

                var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj, null);
                var commonPixelData = new PixelDataImplementation(fetchResult, true);

                var p = Identifier.CreateImageIdentifier(storageKeys[itemIndex].Identifier.PatientKey,studyInstanceUids[itemIndex],
                    seriesInstanceUids[itemIndex], sopInstanceUids[itemIndex]);

                var key = new StorageKey("LocalDatabase", p);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObj, commonPixelData, true));
            }

            timer.Stop();

            var processInfo = Process.GetCurrentProcess();

            var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
                             $" LoadFastHeaders:  {timer.ElapsedTicks / 10000}, WCF-GetFilePaths: {wcfTimer.ElapsedTicks / 10000}\n";

            File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

            return persistentDicomObjects;


        }

        public override PersistentDicomObjectCollection LoadFastHeaders(StorageKeyCollection storageKeys, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadFastHeadersAsync(StorageKeyCollection storageKeys)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadFastHeadersAsync(StorageKeyCollection storageKeys, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            EventHandler<LoadCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection LoadFullHeaders(StorageKeyCollection storageKeys)
        {
            var timer = Stopwatch.StartNew();
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            var studyInstanceUidCollections = new List<string>();
            var seriesInstanceUidCollections = new List<string>();
            var sopInstanceUidCollections = new List<string>();


            foreach (var storageKey in storageKeys)
            {
                studyInstanceUidCollections.Add(storageKey.Identifier.StudyInstanceUid);
                seriesInstanceUidCollections.Add(storageKey.Identifier.SeriesInstanceUid);
                sopInstanceUidCollections.Add(storageKey.Identifier.SopInstanceUid);
            }

            var wcfTimer = Stopwatch.StartNew();
            var imageFilePaths = proxy.GetImageFilePaths(studyInstanceUidCollections, seriesInstanceUidCollections, sopInstanceUidCollections);
            wcfTimer.Stop();

            for (var itemIndex = 0; itemIndex < imageFilePaths.Count(); itemIndex++)
            {
                var dcmObj = DicomObject.CreateInstance(imageFilePaths.ElementAt(itemIndex));

                // to populate PixelData
                var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj, null);
                var commonPixelData = new PixelDataImplementation(fetchResult, true);

                persistentDicomObjects.Add(new PersistentDicomObject(storageKeys[itemIndex], dcmObj, commonPixelData, true));
            }
            timer.Stop();

            var processInfo = Process.GetCurrentProcess();

            var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
                             $" LoadFullHeaders:  {timer.ElapsedTicks / 10000}, WCF-GetFilePaths: {wcfTimer.ElapsedTicks / 10000}\n";

            File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

            return persistentDicomObjects;
        }

        public override void LoadFullHeaders(PersistentDicomObjectCollection persistentDicomObjectCollection)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection LoadFullHeaders(StorageKeyCollection storageKeys, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadFullHeadersAsync(StorageKeyCollection storageKeys)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadFullHeadersAsync(StorageKeyCollection storageKeys, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            EventHandler<LoadCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override void LoadPixelModules(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }

        public override void LoadPixelModules(PersistentDicomObjectCollection headers, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadPixelModulesAsync(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadPixelModulesAsync(PersistentDicomObjectCollection headers, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            EventHandler<LoadCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override void LoadPixels(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }

        public override void LoadAndLockPixels(PersistentDicomObjectCollection imageHeaders)
        {
            throw new NotImplementedException();
        }

        public override void LoadAndLockPixels(PersistentDicomObjectCollection imageHeaders, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override void UnlockPixels(PersistentDicomObjectCollection images)
        {
            throw new NotImplementedException();
        }

        public override void LoadPixels(PersistentDicomObjectCollection headers, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadPixelsAsync(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadAndLockPixelsAsync(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadAndLockPixelsAsync(PersistentDicomObjectCollection headers, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            EventHandler<LoadCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override LoadTask LoadPixelsAsync(PersistentDicomObjectCollection headers, EventHandler<LoadProgressChangedEventArgs> progressCallback,
            EventHandler<LoadCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override long[] LoadRawPixels(PersistentDicomObjectCollection headers, IntPtr[] targetMemoryLocations)
        {
            throw new NotImplementedException();
        }

        public override Stream[] LoadRawPixels(PersistentDicomObjectCollection headers)
        {
            throw new NotImplementedException();
        }
    }
}
