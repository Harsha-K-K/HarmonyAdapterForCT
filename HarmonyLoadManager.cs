using Philips.Platform.Adapters.Services;
using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using Philips.Platform.StorageDevicesClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace CTHarmonyAdapters
{
    public class HarmonyLoadManager : Philips.Platform.ApplicationIntegration.Decoupling.LoadManagerBase
    {
        public IIncisiveAccessor.IIncisiveAccessor proxy;
        private const string Uri = "net.tcp://localhost:6565/IncisiveAccessor";
        private static HarmonyLoadManager instance = null;

        private HarmonyLoadManager() {
            var binding = new NetTcpBinding(SecurityMode.None);
            //binding.MaxReceivedMessageSize = int.MaxValue;

            var channel = new ChannelFactory<IIncisiveAccessor.IIncisiveAccessor>(binding);
            var endpoint = new EndpointAddress(Uri);

            proxy = channel.CreateChannel(endpoint);
        }

        public static HarmonyLoadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HarmonyLoadManager();
                }
                return instance;
            }
        }

        //static HarmonyLoadManager()
        //{
        //    Identifier id = Identifier.CreateImageIdentifier(Identifier.CreateDummyPatientKey(), "1.3.46.670589.61.128.0.202308181459004783629047404.0",
        //        "1.3.46.670589.61.128.1.20230818145912017000100013325131495", "1.3.46.670589.61.128.2.20230818145919608010100010532657604");
        //    StorageKey storageKey = new StorageKey("LocalDatabase", id);
        //    //StorageKeyCollection k1 = new StorageKeyCollection(k);

        //    var dcmObj1 = DicomObject.CreateInstance(@"Y:\ImageData\1.3.46.670589.61.128.0.202308181459004783629047404.0\00010101\D01010001");

        //    // to populate PixelRepresentations
        //    var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj1, null);
        //    var commonPixelData = new PixelDataImplementation(fetchResult, true);

        //    dcmObj = new PersistentDicomObjectCollection();


        //    dcmObj.Add(new PersistentDicomObject(storageKey, dcmObj1, commonPixelData, true));

            
        //    //LoadFullHeaders(k1);
        //}

        public override bool IsSeriesUnderConstruction(StorageKey seriesKey)
        {
            return false;
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
                var sopUids = proxy.GetSopInstanceUids(storageKey.Identifier.SeriesInstanceUid);

                foreach(var sopInstanceUid in sopUids)
                {
                    studyInstanceUids.Add(storageKey.Identifier.StudyInstanceUid);
                    seriesInstanceUids.Add(storageKey.Identifier.SeriesInstanceUid);
                    sopInstanceUids.Add(sopInstanceUid);
                }

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

                var p = Identifier.CreateImageIdentifier(storageKeys[0].Identifier.PatientKey,studyInstanceUids[itemIndex],
                    seriesInstanceUids[itemIndex], sopInstanceUids[itemIndex]);

                var key = new StorageKey("LocalDatabase", p);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObj, commonPixelData, true));
            }

            timer.Stop();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                 $" LoadFastHeaders:  {timer.ElapsedTicks / 10000}, WCF-GetFilePaths: {wcfTimer.ElapsedTicks / 10000}\n";

            //File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

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
      
        //public override PersistentDicomObjectCollection LoadFullHeaders(StorageKeyCollection storageKeys)
        //{
        //    var timer = Stopwatch.StartNew();

        //    //if(dcmObj == null)
        //    //{
        //        //var dcmObj1 = DicomObject.CreateInstance(@"Y:\ImageData\1.3.46.670589.61.128.0.202308181459004783629047404.0\00010101\D01010001");

        //        //// to populate PixelRepresentations
        //        //var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj1, null);
        //        //var commonPixelData = new PixelDataImplementation(fetchResult, true);

        //        //dcmObj.Add(new PersistentDicomObject(storageKeys[0], dcmObj1, commonPixelData, true));

        //    //}

        //    timer.Stop();
        //    var processInfo = Process.GetCurrentProcess();

        //    var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
        //                     $" LoadFullHeaders:  {timer.ElapsedTicks / 10000}\n";

        //    File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);


        //    return dcmObj;
        //}


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

                // to populate PixelRepresentations
                var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj, null);
                var commonPixelData = new PixelDataImplementation(fetchResult, true);

                persistentDicomObjects.Add(new PersistentDicomObject(storageKeys[itemIndex], dcmObj, commonPixelData, true));
            }
            timer.Stop();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                 $" LoadFullHeaders:  {timer.ElapsedTicks / 10000}, WCF-GetFilePaths: {wcfTimer.ElapsedTicks / 10000}\n";

            //File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

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
            return;
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
