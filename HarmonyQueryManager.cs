using ChDefine;
using PatientDataInfo;
using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.ApplicationIntegration.Tracing;
using Philips.Platform.Common;
using Philips.Platform.Common.DataAccess;
using System;
using System.Diagnostics;
using System.ServiceModel;
using TraceLevel = Philips.Platform.Common.TraceLevel;

namespace CTHarmonyAdapters
{
    public class HarmonyQueryManager : Philips.Platform.ApplicationIntegration.Decoupling.QueryManagerBase
    {
        public IIncisiveAccessor.IIncisiveAccessor Proxy;
        private const string Uri = "net.tcp://localhost:6565/IncisiveAccessor";
        private static HarmonyQueryManager instance;
        private static object syncRoot = new object();
        private static PerformanceTracer perfTracer =
            PerformanceTracer.CreatePerformanceTracer(typeof(HarmonyQueryManager));

        public static HarmonyQueryManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new HarmonyQueryManager();
                        }
                    }
                }

                return instance;
            }
        }

        private HarmonyQueryManager()
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            //binding.MaxReceivedMessageSize = int.MaxValue;

            var channel = new ChannelFactory<IIncisiveAccessor.IIncisiveAccessor>(binding);
            var endpoint = new EndpointAddress(Uri);
            Proxy = channel.CreateChannel(endpoint);

        }

        public override TraceLevel TraceLevel
        {
            get
            {
                if (perfTracer.IsInfoOn) return TraceLevel.Info;

                if (perfTracer.IsVerboseOn) return TraceLevel.Verbose;

                return TraceLevel.None;
            }

            set {
                throw new NotImplementedException(); }
        }

        public override TimeSpan GetDeviceQueryTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public override TimeSpan GetExtractResultsTime
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanQueryFrame(string deviceToquery)
        {

            throw new NotImplementedException();
        }

        public override InstanceSopInfoCollection GetUniqueInstanceSopInformation(StorageKey key)
        {
            throw new NotImplementedException();
        }

        public override QueryTask MultipleDeviceQueryAsync(DeviceIdCollection deviceIDCollection, QueryLevel level, Identifier parentIdentifier, DicomFilter filter, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryProgressChangedEventArgs> progressCallback, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection Query(string deviceID, QueryLevel level, Identifier parentIdentifier, DicomFilter filter)
        {

            var timer = Stopwatch.StartNew();
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            if(deviceID == "DummyStudyDevice")
            {
                if (filter.Value?[0] == "PR")
                {
                    return persistentDicomObjects;
                }

                var studyID = filter.Value != null ? filter.Value[0] : parentIdentifier.StudyInstanceUid;
                var dcmObject = CreateDummyDicom(studyID, null);

                var id = Identifier.CreateStudyIdentifier(Identifier.CreateDummyPatientKey(), studyID);
                var key = new StorageKey("DummyStudyDevice", id);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

                return persistentDicomObjects;

            }

            if (filter.QueryType == QueryType.MatchAny)
            {
                if(filter.Value == null || filter.Value[0] == "")
                {
                    return persistentDicomObjects;
                }
                else
                {
                    var patientStudyInfo = Proxy.GetPatientStudyInfo(filter.Value?[0]);

                    if (patientStudyInfo.StudyInfo.StudyInstanceUID != null || patientStudyInfo.StudyInfo.StudyInstanceUID == "")
                    {
                        return persistentDicomObjects;
                    }
                    var dcmObject = CreateDicom(patientStudyInfo.StudyInfo, patientStudyInfo.PatientInfo);

                    var id = Identifier.CreateStudyIdentifier(Identifier.CreatePatientKeyFromDicomObject(dcmObject), patientStudyInfo.StudyInfo.StudyInstanceUID);
                    var key = new StorageKey("LocalDatabase", id);

                    persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));
                    return persistentDicomObjects;

                }
            }

            if (filter.QueryType == QueryType.MatchAll)
            {
                if (parentIdentifier == null)
                {
                    var patientStudyInfoList = Proxy.GetAllPatientStudyInfo();

                    foreach (var patientStudyInfo in patientStudyInfoList)
                    {
                        var dcmObject = CreateDicom(patientStudyInfo.StudyInfo, patientStudyInfo.PatientInfo);

                        var id = Identifier.CreateStudyIdentifier(Identifier.CreatePatientKeyFromDicomObject(dcmObject), patientStudyInfo.StudyInfo.StudyInstanceUID);
                        var key = new StorageKey(deviceID, id);

                        persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

                    }

                }
                else
                {
                    //Debugger.Launch();

                    var dataInfo = Proxy.GetPatientStudyImageInfo(parentIdentifier.StudyInstanceUid);
                    var dcmObject = CreateDicom(dataInfo.studyInfo, dataInfo.patientInfo, dataInfo.imageSeriesInfo);

                    var id = Identifier.CreateSeriesIdentifier(Identifier.CreatePatientKeyFromDicomObject(dcmObject), parentIdentifier.StudyInstanceUid, dataInfo.imageSeriesInfo.SeriesInstanceUID);
                    var key = new StorageKey(deviceID, id);

                    persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

                }

            }

            else if( filter.QueryType == QueryType.And)
            {
                var patID = filter.Expression[0].Value[0];

                var patientInfo = Proxy.GetPatientInfoFromID(patID);

                var patientStudyInfo = Proxy.GetPatientStudyInfo(patientInfo.StudyInstanceUID);

                var dcmObject = CreateDicom(patientStudyInfo.StudyInfo, patientStudyInfo.PatientInfo);

                var id = Identifier.CreateStudyIdentifier(Identifier.CreatePatientKeyFromDicomObject(dcmObject), patientStudyInfo.StudyInfo.StudyInstanceUID);
                var key = new StorageKey("LocalDatabase", id);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));
            }

            else if (filter.QueryType == QueryType.MatchExactString)
            {
                if (filter.Tag.Name == "DicomModality" && filter.Value?[0] == "PR")
                {
                    return persistentDicomObjects; //TODO: Presentation state check

                }
                else
                {
                    var patientStudyInfo = Proxy.GetPatientStudyInfo(filter.Value?[0]);

                    var dcmObject = CreateDicom(patientStudyInfo.StudyInfo, patientStudyInfo.PatientInfo);

                    var id = Identifier.CreateStudyIdentifier(Identifier.CreatePatientKeyFromDicomObject(dcmObject), patientStudyInfo.StudyInfo.StudyInstanceUID);
                    var key = new StorageKey("LocalDatabase", id);

                    persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

                }
            }

            timer.Stop();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now:hh.mm.ss.ffffff} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                 $" Query {filter.QueryType} : {filter.ToString()} :  {timer.ElapsedTicks / 10000} \n";

            //File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

            return persistentDicomObjects;

        }

        private DicomObject CreateDummyDicom(string studyId, string seriesId)
        {
            var dicomObject = DicomObject.CreateInstance();

            dicomObject.SetString(DicomDictionary.DicomSeriesTime, DateTime.Now.Date.ToString());
            dicomObject.SetString(DicomDictionary.DicomContentTime, null);
            dicomObject.SetString(DicomDictionary.DicomModality, "CT");
            dicomObject.SetString(DicomDictionary.DicomManufacturer, "Philips");
            dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
            dicomObject.SetString(DicomDictionary.DicomStationName, null);
            dicomObject.SetString(DicomDictionary.DicomSeriesDescription, "");
            dicomObject.SetString(DicomDictionary.DicomPatientName, "DummyPat");
            dicomObject.SetString(DicomDictionary.DicomPatientId, "101");
            dicomObject.SetString(DicomDictionary.DicomPatientBirthDate, "14/02/1990");
            dicomObject.SetString(DicomDictionary.DicomBodyPartExamined, null);
            dicomObject.SetString(DicomDictionary.DicomSoftwareVersions, "INCISIVE5_1");
            dicomObject.SetString(DicomDictionary.DicomStudyTime, DateTime.Now.Date.ToString());
            dicomObject.SetString(DicomDictionary.DicomAccessionNumber, "111");
            dicomObject.SetString(DicomDictionary.DicomModalitiesInStudy, "CT");
            dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
            dicomObject.SetString(DicomDictionary.DicomStudyInstanceUid, studyId);
            dicomObject.SetString(DicomDictionary.DicomStudyId, studyId);
            if(seriesId != null)
            {
                dicomObject.SetString(DicomDictionary.DicomSeriesInstanceUid, seriesId);
            }
            dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepDescription, "");
            dicomObject.SetString(DicomDictionary.DicomDateTime, null);

            return dicomObject;
        }

        public override QueryTask QueryAsync(string deviceID, QueryLevel level, Identifier parentIdentifier, DicomFilter filter, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryProgressChangedEventArgs> progressCallback, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection QueryChildren(StorageKey storagekey)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection QueryChildren(StorageKey storagekey, DicomFilter filter)
        {
            //Debugger.Launch();

            var timer = Stopwatch.StartNew();
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            if (storagekey.SourceDevice == "DummyStudyDevice")
            {
                var dcmObject1 = CreateDummyDicom(storagekey.Identifier.StudyInstanceUid, "1.3.46.670589.88.2269738417469835076.264587346879930907");

                var id1 = Identifier.CreateSeriesIdentifier(Identifier.CreateDummyPatientKey(), storagekey.Identifier.StudyInstanceUid, "1.3.46.670589.88.2269738417469835076.264587346879930907");
                var key1 = new StorageKey("DummyStudyDevice", id1);

                persistentDicomObjects.Add(new PersistentDicomObject(key1, dcmObject1, null, false));

                return persistentDicomObjects;
                
            }

            var dataInfo = Proxy.GetPatientStudyImageInfo(storagekey.Identifier.StudyInstanceUid);

            var dcmObject = CreateDicom(dataInfo.studyInfo, dataInfo.patientInfo, dataInfo.imageSeriesInfo);

            var id = Identifier.CreateSeriesIdentifier(storagekey.Identifier.PatientKey,
                storagekey.Identifier.StudyInstanceUid, dataInfo.imageSeriesInfo.SeriesInstanceUID);
            var key = new StorageKey("LocalDatabase", id);


            persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

            timer.Stop();

            //var processInfo = Process.GetCurrentProcess();

            //var logMessage = $"{DateTime.Now:hh.mm.ss.ffffff} ProcessID: {processInfo.Id}, ProcessName: {processInfo.ProcessName}," +
            //                 $" QueryChildren {filter.QueryType} : {filter.ToString()}   {timer.ElapsedTicks / 10000} \n";

            //File.AppendAllText(@"D:\MyLogs\HarmonyIncisiveLogs.txt", logMessage);

            return persistentDicomObjects;
        }

        public override QueryTask QueryChildrenAsync(StorageKey storageKey, DicomFilter filter, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryProgressChangedEventArgs> progressCallback, EventHandler<Philips.Platform.ApplicationIntegration.DataAccess.QueryCompletedEventArgs> completedCallback)
        {
            throw new NotImplementedException();
        }

        public override ImageDicomObjectList QueryImagesBySeries(string deviceId, Identifier parentIdentifier, DicomFilter filter)
        {
            throw new NotImplementedException();
        }

        public override PersistentDicomObjectCollection QueryStudy(string deviceToQuery, DicomFilter filter)
        {
            var persistentDicomObjects = new PersistentDicomObjectCollection();

            if (deviceToQuery == "DummyStudyDevice" && filter.QueryType == QueryType.MatchAll)
            {
                var dcmObject = CreateDummyDicom("1.3.46.670589.88.74503675686557505.243252870275214482", null);

                var id = Identifier.CreateStudyIdentifier(Identifier.CreateDummyPatientKey(), "1.3.46.670589.88.74503675686557505.243252870275214482");
                var key = new StorageKey("DummyStudyDevice", id);

                persistentDicomObjects.Add(new PersistentDicomObject(key, dcmObject, null, false));

            }
             
            return persistentDicomObjects;

        }

        public override PersistentDicomObjectCollection QueryStudy(string deviceToQuery, DicomFilter filter, DictionaryTagsCollection sortCriteria, QuerySortOrder sortOrder, int maxRecords)
        {
            throw new NotImplementedException();
        }

        private DicomObject CreateDicom(StudyInfo studyInfo, PatientInfo patientInfo, ImageSeriesInfo imageSeriesInfo = null)
        {
            var dicomObject = DicomObject.CreateInstance();

            dicomObject.SetString(DicomDictionary.DicomSpecificCharacterSet, studyInfo.SpecificCharacterSet);
            dicomObject.SetString(DicomDictionary.DicomSeriesTime, studyInfo.StudyTime.ToString());
            dicomObject.SetString(DicomDictionary.DicomContentTime, null);
            dicomObject.SetString(DicomDictionary.DicomModality, "CT");
            dicomObject.SetString(DicomDictionary.DicomManufacturer, "Philips");
            dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
            dicomObject.SetString(DicomDictionary.DicomStationName, null);
            dicomObject.SetString(DicomDictionary.DicomSeriesDescription, studyInfo.ProcedureDescription);
            dicomObject.SetString(DicomDictionary.DicomPatientName, patientInfo.PatientName);
            dicomObject.SetString(DicomDictionary.DicomPatientId, patientInfo.PatientID);
            dicomObject.SetString(DicomDictionary.DicomPatientBirthDate, patientInfo.PatientsBirthDate.ToString());
            dicomObject.SetString(DicomDictionary.DicomBodyPartExamined, null);
            dicomObject.SetString(DicomDictionary.DicomSoftwareVersions, "INCISIVE5_1");
            dicomObject.SetString(DicomDictionary.DicomStudyTime, studyInfo.StudyTime.ToString());
            dicomObject.SetString(DicomDictionary.DicomAccessionNumber, studyInfo.AccessionNumber);
            dicomObject.SetString(DicomDictionary.DicomModalitiesInStudy, "CT");
            dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
            dicomObject.SetString(DicomDictionary.DicomReferringPhysicianName, studyInfo.ReferringPhysiciansName);
            dicomObject.SetString(DicomDictionary.DicomStudyDescription, studyInfo.StudyDescription);
            dicomObject.SetString(DicomDictionary.DicomOperatorsName, studyInfo.OperatorsName);
            dicomObject.SetString(DicomDictionary.DicomPatientSex, patientInfo.PatientsSex.ToString());
            dicomObject.SetString(DicomDictionary.DicomPatientAge, patientInfo.PatientsAge.ToString());
            dicomObject.SetString(DicomDictionary.DicomPatientSize, patientInfo.PatientsSize);
            dicomObject.SetString(DicomDictionary.DicomPatientWeight, patientInfo.PatientsWeight);
            dicomObject.SetString(DicomDictionary.DicomStudyInstanceUid, patientInfo.StudyInstanceUID);
            dicomObject.SetString(DicomDictionary.DicomStudyId, studyInfo.StudyID);
            dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepDescription, studyInfo.PerformedProcedureStepDescription);
            dicomObject.SetString(DicomDictionary.DicomDateTime, null);

            if (imageSeriesInfo != null)
            {
                var modifiedDcmObject = AddImageSeriesInfo(imageSeriesInfo, dicomObject);

                return modifiedDcmObject;
            }

            return dicomObject;
        }

        private DicomObject AddImageSeriesInfo(ImageSeriesInfo imageSeriesInfo, DicomObject dicomObject)
        {
            dicomObject.SetString(DicomDictionary.DicomProtocolName, imageSeriesInfo.ProtocolName);
            dicomObject.SetString(DicomDictionary.DicomPatientPosition, imageSeriesInfo.PatientPosition.ToString());
            dicomObject.SetString(DicomDictionary.DicomSeriesInstanceUid, imageSeriesInfo.SeriesInstanceUID);
            dicomObject.SetString(DicomDictionary.DicomSeriesNumber, imageSeriesInfo.SeriesNumber.ToString());
            dicomObject.SetString(DicomDictionary.DicomFrameOfReferenceUid, imageSeriesInfo.FrameReferenceInstanceUID);
            dicomObject.SetString(DicomDictionary.DicomRows, imageSeriesInfo.Rows.ToString());
            dicomObject.SetString(DicomDictionary.DicomColumns, imageSeriesInfo.Columns.ToString());
            dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepStartDate, null);
            dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepStartTime, null);

            return dicomObject;
        }

        //private IncisiveModel GetValues(string studyInstanceUid)
        //{
        //    var patientInfo = Proxy.GetPatientInfo(studyInstanceUid);

        //    var studyInfo = Proxy.GetStudyInfo(studyInstanceUid);

        //    var seriesInstanceUid = Proxy.GetSeriesInstanceUids(studyInstanceUid);
        //    var instanceUid = seriesInstanceUid as string[] ?? seriesInstanceUid.ToArray();
        //    var imageSeriesInfo = Proxy.GetImageSeriesInfo(instanceUid.FirstOrDefault());

        //    var incisiveModel = new IncisiveModel(imageSeriesInfo, patientInfo, studyInfo);

        //    return incisiveModel;
        //}

        //private DicomObject CreateDicom(ImageSeriesInfo imageSeriesInfo, StudyInfo studyinfo, PatientInfo patientInfo)
        //{
        //    DicomObject dicomObject = DicomObject.CreateInstance();

        //    dicomObject.SetString(DicomDictionary.DicomSpecificCharacterSet, studyinfo.SpecificCharacterSet);
        //    dicomObject.SetString(DicomDictionary.DicomSeriesTime, studyinfo.StudyTime.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomContentTime, null);
        //    dicomObject.SetString(DicomDictionary.DicomModality, imageSeriesInfo.Modality);
        //    dicomObject.SetString(DicomDictionary.DicomManufacturer, "Philips");
        //    dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
        //    dicomObject.SetString(DicomDictionary.DicomStationName, null);
        //    dicomObject.SetString(DicomDictionary.DicomSeriesDescription, studyinfo.ProcedureDescription);
        //    dicomObject.SetString(DicomDictionary.DicomPatientName, patientInfo.PatientName);

        //    dicomObject.SetString(DicomDictionary.DicomPatientId, patientInfo.PatientID);
        //    dicomObject.SetString(DicomDictionary.DicomPatientBirthDate, patientInfo.PatientsBirthDate.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomBodyPartExamined, null);
        //    dicomObject.SetString(DicomDictionary.DicomSoftwareVersions, "INCISIVE5_1");
        //    dicomObject.SetString(DicomDictionary.DicomProtocolName, imageSeriesInfo.ProtocolName);
        //    dicomObject.SetString(DicomDictionary.DicomPatientPosition, imageSeriesInfo.PatientPosition.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomStudyInstanceUid, patientInfo.StudyInstanceUID);
        //    dicomObject.SetString(DicomDictionary.DicomSeriesInstanceUid, imageSeriesInfo.SeriesInstanceUID);
        //    dicomObject.SetString(DicomDictionary.DicomSeriesNumber, imageSeriesInfo.SeriesNumber.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomFrameOfReferenceUid, imageSeriesInfo.FrameReferenceInstanceUID);
        //    dicomObject.SetString(DicomDictionary.DicomRows, imageSeriesInfo.Rows.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomColumns, imageSeriesInfo.Columns.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepStartDate, null);
        //    dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepStartTime, null);
        //    return dicomObject;
        //}

        //private DicomObject CreateDicom(PatientStudyInfo patientStudyInfo)
        //{
        //    DicomObject dicomObject = DicomObject.CreateInstance();

        //    dicomObject.SetString(DicomDictionary.DicomSpecificCharacterSet, patientStudyInfo.StudyInfo.SpecificCharacterSet);
        //    dicomObject.SetString(DicomDictionary.DicomStudyTime, patientStudyInfo.StudyInfo.StudyTime.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomAccessionNumber, patientStudyInfo.StudyInfo.AccessionNumber);
        //    dicomObject.SetString(DicomDictionary.DicomModalitiesInStudy, "CT");
        //    dicomObject.SetString(DicomDictionary.DicomInstitutionName, null);
        //    dicomObject.SetString(DicomDictionary.DicomReferringPhysicianName, patientStudyInfo.StudyInfo.ReferringPhysiciansName);
        //    dicomObject.SetString(DicomDictionary.DicomStudyDescription, patientStudyInfo.StudyInfo.StudyDescription);
        //    dicomObject.SetString(DicomDictionary.DicomOperatorsName, patientStudyInfo.StudyInfo.OperatorsName);

        //    dicomObject.SetString(DicomDictionary.DicomPatientName, patientStudyInfo.PatientInfo.PatientName);
        //    dicomObject.SetString(DicomDictionary.DicomPatientId, patientStudyInfo.PatientInfo.PatientID);
        //    dicomObject.SetString(DicomDictionary.DicomPatientBirthDate, patientStudyInfo.PatientInfo.PatientsBirthDate.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomPatientSex, patientStudyInfo.PatientInfo.PatientsSex.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomPatientAge, patientStudyInfo.PatientInfo.PatientsAge.ToString());
        //    dicomObject.SetString(DicomDictionary.DicomPatientSize, patientStudyInfo.PatientInfo.PatientsSize);
        //    dicomObject.SetString(DicomDictionary.DicomPatientWeight, patientStudyInfo.PatientInfo.PatientsWeight);


        //    dicomObject.SetString(DicomDictionary.DicomStudyInstanceUid, patientStudyInfo.PatientInfo.StudyInstanceUID);
        //    dicomObject.SetString(DicomDictionary.DicomStudyId, patientStudyInfo.StudyInfo.StudyID);
        //    dicomObject.SetString(DicomDictionary.DicomPerformedProcedureStepDescription, patientStudyInfo.StudyInfo.PerformedProcedureStepDescription);
        //    dicomObject.SetString(DicomDictionary.DicomDateTime, null);
        //    return dicomObject;
        //}

    }
}