using Microsoft.Extensions.Configuration;
using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using Philips.Platform.CommonUtilities.Configuration.Toolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace CTHarmonyAdapters
{
    internal class IncisivePatientKeyProvider : PatientKeyProviderBase
    {
        #region PrivateMembers

        /// <summary>
        /// static instance of IPatientKeyManager
        /// </summary>
        private static readonly IncisivePatientKeyProvider patientKeyProvider = new IncisivePatientKeyProvider();

        /// <summary>
        /// The Instance
        /// </summary>
        public static IncisivePatientKeyProvider Instance
        {
            get
            {
                return patientKeyProvider;
            }
        }

        #endregion

        #region IPatientKeyProvider Members

        /// <summary>
        /// Creates a dummy patient key
        /// </summary>
        /// <returns>dummy patient key</returns>
        public override PatientKey CreateDummyPatientKey()
        {
            return new PatientKeyImplementation(null, null);
        }

        /// <summary>
        /// Creates and returns a Patient key which is constructed using 
        /// the patient information provided in the Dicom object
        /// </summary>
        /// <param name="dicomObject">
        /// DicomObject which should contain the
        /// attributes like PatientName, PatientId and PatientBirthDate
        /// </param> 
        /// <exception cref="ArgumentNullException">When dicomObject is null</exception>
        /// <returns>Patient key</returns>
        public override PatientKey CreatePatientKeyFromDicomObject(DicomObject dicomObject)
        {
            return CreatePatientKey(dicomObject);
        }

        /// <summary>
        /// For KnownType of DataContractSerializer
        /// </summary>
        /// <returns></returns>
        public override Type[] GetTypes()
        {
            return new[] { typeof(PatientKeyImplementation) };
        }

        #endregion

        //#region Internal Methods

        ///// <summary>
        ///// Creates Patient Key from given composite file
        ///// </summary>
        ///// <param name="compositeFileName">the composite file full path</param>
        ///// <returns>Patient Key</returns>
        //internal static PatientKeyImplementation CreatePatientKey(
        //    string compositeFileName
        //)
        //{
        //    try
        //    {
        //        var deserializedData =
        //            DicomSerializer.Deserialize(compositeFileName, true, true);
        //        var patientKey = CreatePatientKey(deserializedData);
        //        return patientKey;
        //    }
        //    catch (IOException ex)
        //    {
        //        return null;
        //    }
        //}

        internal static PatientKeyImplementation CreatePatientKey(DicomObject dicomObject)
        {
            if (dicomObject == null)
            {
                throw new ArgumentNullException("dicomObject");
            }
            return new PatientKeyImplementation(dicomObject);
        }

        //#endregion
    }
    public sealed class PatientKeyImplementation : PatientKey
    {
        private bool isDummy;

        //
        // Summary:
        //     Check whether the patient key is dummy
        public override bool IsDummy => isDummy;

        //
        // Summary:
        //     Constructs patient key from dicom object.
        //
        // Parameters:
        //   dicomObject:
        public PatientKeyImplementation(DicomObject dicomObject)
        {
            PatientId = dicomObject.GetString(DicomDictionary.DicomPatientId);
            PatientName = PatientNameUtility.RemoveTrailingSuffixesFromAllGroups(dicomObject.GetString(DicomDictionary.DicomPatientName));
            isDummy = string.IsNullOrWhiteSpace(PatientName) && string.IsNullOrWhiteSpace(PatientId);
            AdditionalIdentifyingAttributes = new SortedDictionary<DictionaryTag, object>();
        }

        //
        // Summary:
        //     Constructs a PatientKey which represents a patient
        //
        // Parameters:
        //   patientId:
        //     Unique Id for the patient
        //
        //   patientName:
        //     Name of the patient
        public PatientKeyImplementation(string patientId, string patientName)
        {
            PatientId = patientId;
            PatientName = PatientNameUtility.RemoveTrailingSuffixesFromAllGroups(patientName);
            isDummy = string.IsNullOrWhiteSpace(PatientName) && string.IsNullOrWhiteSpace(PatientId);
        }

        //
        // Summary:
        //     Constructs a PatientKey which represents a patient
        //
        // Parameters:
        //   patientId:
        //     Unique Id for the patient
        //
        //   patientName:
        //     Name of the patient
        //
        //   isDummy:
        //     Indicates if the patient is a dummy patient
        public PatientKeyImplementation(string patientId, string patientName, bool isDummy)
        {
            PatientId = patientId;
            PatientName = PatientNameUtility.RemoveTrailingSuffixesFromAllGroups(patientName);
            this.isDummy = isDummy;
        }

        //
        // Summary:
        //     Compares the unique patient attributes identified as part of patient key.
        //
        // Parameters:
        //   other:
        public override bool Equals(PatientKey other)
        {
            bool flag = (object)this == other || IsDummy || (other != null && other.IsDummy) || IsAttributesValueEmpty(other) || ComparePatientIdAndPatientname(other);
            if (!AreAdditionalAttributesPresent())
            {
                return flag;
            }

            return flag;
        }

        private bool IsNullOrEmptyOrUnknown(string PatientId)
        {
            return string.IsNullOrWhiteSpace(PatientId) || PatientId == "UNKNOWN";
        }

        private bool IsAttributesValueEmpty(PatientKey other)
        {
            return other != null && IsNullOrEmptyOrUnknown(PatientId) && IsNullOrEmptyOrUnknown(other.PatientId) && string.CompareOrdinal(PatientName, other.PatientName) == 0;
        }

        private bool ComparePatientIdAndPatientname(PatientKey other)
        {
            return other != null && string.CompareOrdinal(PatientId, other.PatientId) == 0 && string.CompareOrdinal(PatientName, other.PatientName) == 0;
        }

        //
        // Summary:
        //     Compute the hash code of the PatientKey.
        //
        // Returns:
        //     the hash code of the PatientKey
        public override int GetHashCode()
        {
            int num = 17;
            return num * 23 + ((!string.IsNullOrEmpty(PatientId)) ? PatientId.GetHashCode() : 0);
        }

        //
        // Summary:
        //     Returns a System.String that represents this instance. eg. PatientId: ABC; PatientName:
        //     XYZ; IsDummy: False;
        //
        // Returns:
        //     A System.String that represents this instance.
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(512);
            stringBuilder.Append(PatientKeyUtility.GetPatientUidFromPatientKey(this));
            stringBuilder.Append("; IsDummy: " + IsDummy);
            return stringBuilder.ToString();
        }

        //
        // Summary:
        //     Check if any additional attributes are present
        private bool AreAdditionalAttributesPresent()
        {
            return AdditionalIdentifyingAttributes != null && AdditionalIdentifyingAttributes.Count > 0;
        }
    }

    //
    // Summary:
    //     Provides Utility methods to handle patient name
    internal static class PatientNameUtility
    {
        //
        // Summary:
        //     Removes trailing caret(^) and equals(=) signs from the string value passed. Also
        //     removes any spaces just before or after a caret or an equal sign.
        //
        // Parameters:
        //   valueOne:
        //     String
        //
        // Returns:
        //     String with trailing caret(^) and equals(=) signs removed and with spaces just
        //     before or after a caret or an equal sign removed.
        //
        // Remarks:
        //     Few examples of what the method does -
        //     1. aaa^bbb^ccc^^=ssa^bgf^^^= becomes aaa^bbb^ccc=ssa^bgf
        //     2. aaa^^bbb^ccc^^=ssa^bgf^^^= becomes aaa^^bbb^ccc=ssa^bgf
        //     3. aaa ^ ^ ^ b bb ^ c = s becomes aaa^^^b bb^c=s
        internal static string RemoveTrailingSuffixesFromAllGroups(string valueOne)
        {
            if (string.IsNullOrEmpty(valueOne))
            {
                return valueOne;
            }

            valueOne = valueOne.Trim();
            StringBuilder stringBuilder = new StringBuilder(valueOne.Length);
            string[] array = ((valueOne.IndexOf('=') < 0) ? new string[1] { valueOne } : valueOne.Split('='));
            int num;
            for (int i = 0; i < array.Length; i++)
            {
                string text = array[i];
                if (text.Length > 0)
                {
                    text = text.Trim();
                    string[] array2 = ((text.IndexOf('^') < 0) ? new string[1] { text } : text.Split('^'));
                    for (int j = 0; j < array2.Length; j++)
                    {
                        string text2 = array2[j];
                        if (text2.Length > 0)
                        {
                            text2 = text2.Trim();
                            stringBuilder.Append(text2);
                        }

                        stringBuilder.Append("^");
                    }

                    num = stringBuilder.Length - 1;
                    while (num >= 0 && stringBuilder[num] == '^')
                    {
                        stringBuilder.Remove(num, 1);
                        num--;
                    }
                }

                stringBuilder.Append("=");
            }

            num = stringBuilder.Length - 1;
            while (num >= 0 && stringBuilder[num] == '=')
            {
                stringBuilder.Remove(num, 1);
                num--;
            }

            return stringBuilder.ToString();
        }

        internal static bool PatientNameMatch(string patientName1, string patientName2)
        {
            if (patientName1 == null || patientName2 == null)
            {
                return false;
            }

            if (patientName1.Equals(patientName2))
            {
                return true;
            }

            return string.CompareOrdinal(RemoveTrailingSuffixesFromAllGroups(patientName1), RemoveTrailingSuffixesFromAllGroups(patientName2)) == 0;
        }
    }

    //
    // Summary:
    //     Utilities for Patient key
    internal static class PatientKeyUtility
    {
        //
        // Summary:
        //     Gets patient uid from patient key object
        //
        // Parameters:
        //   patientKey:
        internal static string GetPatientUidFromPatientKey(PatientKey patientKey)
        {
            if (patientKey == null)
            {
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder(512);
            stringBuilder.Append("PatientId: " + (string.IsNullOrWhiteSpace(patientKey.PatientId) ? "UNKNOWN" : patientKey.PatientId));
            stringBuilder.Append("; PatientName: " + (string.IsNullOrWhiteSpace(patientKey.PatientName) ? "UNKNOWN" : patientKey.PatientName));
            if (patientKey.AdditionalIdentifyingAttributes == null)
            {
                return stringBuilder.ToString();
            }

            foreach (DictionaryTag key in patientKey.AdditionalIdentifyingAttributes.Keys)
            {
                string text = patientKey.AdditionalIdentifyingAttributes[key]?.ToString();
                stringBuilder.Append("; " + key.Name + ": " + (string.IsNullOrWhiteSpace(text) ? "UNKNOWN" : text));
            }

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// Provides BIU injected additional identifying attributes
    /// </summary>
    // @AdapterType: Disk 
    internal static class PatientKeyAdditionalIdentifyingAttributesProvider
    {

        private static readonly List<DictionaryTag> supportedTagsLookup =
            new List<DictionaryTag> { DicomDictionary.DicomPatientSex, DicomDictionary.DicomPatientBirthDate };

        private static HashSet<DictionaryTag> injectedAdditionalAttributes = ReadAdditionalAttributes();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicomObject"></param>
        internal static SortedDictionary<DictionaryTag, object> GetAdditionalAttributes(DicomObject dicomObject)
        {
            if (injectedAdditionalAttributes == null)
            {
                return null;
            }
            var additionalIdentifyingAttributes = new SortedDictionary<DictionaryTag, object>();
            foreach (var tag in injectedAdditionalAttributes)
            {
                additionalIdentifyingAttributes.Add(tag, dicomObject.GetString(tag));
            }
            return additionalIdentifyingAttributes;
        }

        /// <summary>
        /// Read additional attributes to be used for PatientKey
        /// </summary>
        /// <returns></returns>
        private static HashSet<DictionaryTag> ReadAdditionalAttributes()
        {
            var root = ConfigurationAccess.Root;
            var section = root.GetSection("PatientKeyConfiguration");
            //var additionalInjectedTagNames = section.Get<PatientKeyConfiguration>()?.AdditionalAttributes;
            HashSet<string> additionalInjectedTagNames = null;

            var additionalAttributes = new HashSet<DictionaryTag>();
            if (additionalInjectedTagNames == null)
            {
                return null;
            }
            foreach (var attributeString in additionalInjectedTagNames)
            {
                var tag = DictionaryBase.GetDictionaryTag(attributeString);
                if (supportedTagsLookup.Contains(tag))
                {
                    additionalAttributes.Add(DictionaryBase.GetDictionaryTag(attributeString));
                }
                else
                {
                    throw new ConfigurationException(
                        attributeString + ", passed as part of patient key configuration is not supported");
                }
            }
            return additionalAttributes.Count > 0 ? additionalAttributes : null;
        }
        internal static void ReloadConfig()
        {
            injectedAdditionalAttributes = ReadAdditionalAttributes();
        }
        internal static bool AreEqual(SortedDictionary<DictionaryTag, object> other,
            SortedDictionary<DictionaryTag, object> additionalIdentifyingAttributes)
        {
            return other != null &&
                other.SequenceEqual(additionalIdentifyingAttributes);
        }
    }


}
