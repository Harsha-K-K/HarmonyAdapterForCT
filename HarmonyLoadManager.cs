﻿using Philips.Platform.Adapters.Services;
using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using Philips.Platform.StorageDevicesClient;
using PixelDataImpl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;

namespace CTHarmonyAdapters
{
    public class HarmonyLoadManager : Philips.Platform.ApplicationIntegration.Decoupling.LoadManagerBase
    {
        public IIncisiveAccessor.IIncisiveAccessor proxy;
        private const string Uri = "net.tcp://localhost:6565/IncisiveAccessor";
        private static HarmonyLoadManager instance = null;

        private HarmonyLoadManager()
        {
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

        public override bool IsSeriesUnderConstruction(StorageKey seriesKey)
        {
            return false;
        }

        public override PersistentDicomObjectCollection LoadFastHeaders(StorageKeyCollection storageKeys)
        {
            var timer = Stopwatch.StartNew();
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            if (storageKeys.FirstOrDefault().SourceDevice == "DummyStudyDevice")
            {
                var dcmObj = DicomObject.CreateInstance(@"C:\PortalPms\Demonstrator\IDS\DummyStudy\S211501\S102300\00001\CT_324.dcm");

                var commonPixelData = CreatePixelData(dcmObj);
                var p = Identifier.CreateImageIdentifier(storageKeys[0].Identifier.PatientKey, storageKeys[0].Identifier.StudyInstanceUid,
                    storageKeys[0].Identifier.SeriesInstanceUid, dcmObj.GetString(DicomDictionary.DicomSopInstanceUid));

                var key = new StorageKey("DummyStudyDevice", p);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObj, commonPixelData, true));

                return persistentDicomObjects;
            }

            var studyInstanceUids = new List<string>();
            var seriesInstanceUids = new List<string>();
            var sopInstanceUids = new List<string>();



            foreach (var storageKey in storageKeys)
            {
                var sopUids = proxy.GetSopInstanceUids(storageKey.Identifier.SeriesInstanceUid);

                foreach (var sopInstanceUid in sopUids)
                {
                    studyInstanceUids.Add(storageKey.Identifier.StudyInstanceUid);
                    seriesInstanceUids.Add(storageKey.Identifier.SeriesInstanceUid);
                    sopInstanceUids.Add(sopInstanceUid);
                }

            }

            var wcfTimer = Stopwatch.StartNew();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                $" LoadFastHeaders:{studyInstanceUids}, {seriesInstanceUids}, {sopInstanceUids}\n" + Environment.StackTrace + "\n ------------------------------";

            //File.AppendAllText($@"D:\MyLogs\CTHarmonyAdapters_{processInfo.Id}.txt", logMessage);
            var filepaths = proxy.GetImageFilePaths(studyInstanceUids, seriesInstanceUids, sopInstanceUids); //should get from DB
            wcfTimer.Stop();

            var filepathArray = filepaths as string[] ?? filepaths.ToArray();
            for (var itemIndex = 0; itemIndex < filepathArray.Count(); itemIndex++)
            {
                var dcmObj = DicomObject.CreateInstance(filepathArray.ElementAt(itemIndex));

                var commonPixelData = CreatePixelData(dcmObj);

                var p = Identifier.CreateImageIdentifier(storageKeys[0].Identifier.PatientKey, studyInstanceUids[itemIndex],
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

            if (storageKeys.FirstOrDefault().SourceDevice == "DummyStudyDevice")
            {
                var dcmObj = DicomObject.CreateInstance(@"C:\PortalPms\Demonstrator\IDS\DummyStudy\S211501\S102300\00001\CT_324.dcm");

                var commonPixelData = CreatePixelData(dcmObj);

                var p = Identifier.CreateImageIdentifier(storageKeys[0].Identifier.PatientKey, storageKeys[0].Identifier.StudyInstanceUid,
                    storageKeys[0].Identifier.SeriesInstanceUid, dcmObj.GetString(DicomDictionary.DicomSopInstanceUid));

                var key = new StorageKey("DummyStudyDevice", p);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObj, commonPixelData, true));

                return persistentDicomObjects;
            }

            var studyInstanceUidCollections = new List<string>();
            var seriesInstanceUidCollections = new List<string>();
            var sopInstanceUidCollections = new List<string>();


            foreach (var storageKey in storageKeys)
            {
                studyInstanceUidCollections.Add(storageKey.Identifier.StudyInstanceUid);
                seriesInstanceUidCollections.Add(storageKey.Identifier.SeriesInstanceUid);
                if (storageKey.Identifier.SopInstanceUid == null)
                {
                    var sopUids = proxy.GetSopInstanceUids(storageKey.Identifier.SeriesInstanceUid);
                    foreach (var id in sopUids)
                        sopInstanceUidCollections.Add(id);

                }
                else
                {
                    sopInstanceUidCollections.Add(storageKey.Identifier.SopInstanceUid);
                }
            }

            var wcfTimer = Stopwatch.StartNew();
            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                $" LoadFullHeaders: {studyInstanceUidCollections[0]}, {seriesInstanceUidCollections[0]}, {sopInstanceUidCollections[0]}\n"
            //                + Environment.StackTrace + "\n ------------------------------";

            //File.AppendAllText($@"D:\MyLogs\CTHarmonyAdapters_{processInfo.Id}.txt", logMessage);

            var imageFilePaths = proxy.GetImageFilePaths(studyInstanceUidCollections, seriesInstanceUidCollections, sopInstanceUidCollections);
            wcfTimer.Stop();

            for (var itemIndex = 0; itemIndex < imageFilePaths.Count(); itemIndex++)
            {
                var dcmObj = DicomObject.CreateInstance(imageFilePaths.ElementAt(itemIndex));

                // to populate PixelRepresentations
                var commonPixelData = CreatePixelData(dcmObj);

                persistentDicomObjects.Add(new PersistentDicomObject(storageKeys[itemIndex], dcmObj, commonPixelData, true));
            }
            timer.Stop();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now.ToString("hh.mm.ss.ffffff")} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                 $" LoadFullHeaders:  {timer.ElapsedTicks / 10000}, WCF-GetFilePaths: {wcfTimer.ElapsedTicks / 10000}\n";

            //File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

            return persistentDicomObjects;
        }

        private PixelDataImplCopy CreatePixelData(DicomObject dcmObj)
        {

            //var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj, null);
            //var commonPixelData = new PixelDataImplementation(fetchResult, true);

            //var fetchResult = new FetchResult(PixelDataType.DicomFile, dcmObj, null);
            var commonPixelData = new PixelDataImplCopy(dcmObj);

            //var commonPixelData = new PixelDataImpl(dcmObj);

            return commonPixelData;
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
            return;
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
