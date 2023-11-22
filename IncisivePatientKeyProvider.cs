using Philips.Platform.Common;
using System;

namespace CTHarmonyAdapters
{
    internal class IncisivePatientKeyProvider : PatientKeyProviderBase
    {
        public override PatientKey CreateDummyPatientKey()
        {
            throw new NotImplementedException();
        }

        public override PatientKey CreatePatientKeyFromDicomObject(DicomObject dicomObject)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetTypes()
        {
            throw new NotImplementedException();
        }
    }
}
