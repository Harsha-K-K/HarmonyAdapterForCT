using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common.DataAccess;
using Philips.Platform.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

namespace CTHarmonyAdapters
{
    /// <summary>
    /// Holds various image related attributes that are of interest for the memory manager.
    /// </summary>
    [Serializable]
    public sealed class ImageConversionInformation
    {
        /// <summary>
        ///  Pixel data represent a color image described by red, green, and blue image planes.
        /// </summary>
        public static readonly string RGB = "RGB";

        #region Private Members

        private static Rule noValidationRule =
            RulesDictionary.GetRule(DicomTolerance.NoValidation);
        private ReadOnlyArray<double> myPixelSpacing;
        private ReadOnlyArray<double> myPixelImagerSpacing;
        private ReadOnlyArray<double> myWindowCenter;
        private ReadOnlyArray<int> myPixelAspectRatio;
        private ReadOnlyArray<ushort> myPlanarConfiguration;
        private double? myRescaleSlope;
        private double? myRescaleIntercept;
        private int? myPixelPaddingValue;
        private int? myNumberOfFrames;
        private int? myPixelPaddingRangeLimitValue;
        private string mySopClassUid;
        private string myPhotometricInterpretation;
        private string myTransferSyntaxUid;
        private int myBitsAllocated;
        private int myBitsStored;
        private int myHighBit;
        private int myRows;
        private int myColumns;
        private int myPixelRepresentation;
        private int mySamplesPerPixel;
        private int myOffset;
        private ImageConversions myImageConversionType;
        private bool myHasPixels;
        private bool isSpectralBaseImage;

        #endregion

        #region Constructors

        /// <summary>
        /// constructs the image conversion information from the given dicom object.
        /// </summary>
        /// <param name="theDicomObject">
        /// Dicom Object from which image conversion information has to be built.
        /// </param>
        /// <remarks>
        /// When using this constructor to construct the ImageConversionInformation and 
        /// then to set the resultant ImageConversionInformation in FileInformation, 
        /// make sure to check ImageConversionInformation.HasPixels. e.g:
        /// <code>
        /// FileInformation fi = new FileInformation(...);
        /// DicomObject dicomObject = ...;
        /// ImageConversionInformation ici = new ImageConversionInformation(dicomObject);
        /// fi.ImageConversionInformation = (ici.HasPixels) ? ici : null;
        /// </code>
        /// </remarks>
        public ImageConversionInformation(DicomObject theDicomObject)
        {
            GetImageInformation(theDicomObject);
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="theBitsAllocated">The bits allocated.</param>
        /// <param name="theBitsStored">The bits stored.</param>
        /// <param name="theHighBit">The high bit.</param>
        /// <param name="thePixelRepresentation">The pixel representation.</param>
        /// <param name="theRows">The rows.</param>
        /// <param name="theColumns">The columns.</param>
        /// <param name="theSamplesPerPixel">The samples per pixel.</param>
        /// <param name="theNumberOfFrames">The number of frames.</param>
        /// <param name="theRescaleIntercept">The rescale intercept.</param>
        /// <param name="theRescaleSlope">The rescale slope.</param>
        /// <param name="theSopClassUid">The SOP class UID.</param>
        /// <param name="thePhotometricInterpretation">The photometric interpretation.</param>
        /// <param name="thePixelPaddingValue">The pixel padding value.</param>
        /// <param name="thePixelAspectRatio">The pixel aspect ratio.</param>
        /// <param name="thePixelSpacing">The pixel spacing.</param>
        /// <param name="theWindowCenter">
        /// The window center. Default value = null. This was made optional parameter for this
        /// delivery so as not to break PII Adapters build. Once AII integrates this will be made
        /// non-optional. 
        /// </param>
        /// <param name="thePlanarConfiguration">The planar configuration.</param>
        /// <param name="thePixelPaddingRangeLimitValue">
        /// The pixel padding range limit value. 
        /// </param>
        /// <param name="transferSyntax">The transferSyntax in which image is stored </param>
        public ImageConversionInformation(
            int theBitsAllocated,
            int theBitsStored,
            int theHighBit,
            int thePixelRepresentation,
            int theRows,
            int theColumns,
            int theSamplesPerPixel,
            int? theNumberOfFrames,
            double? theRescaleIntercept,
            double? theRescaleSlope,
            string theSopClassUid,
            string thePhotometricInterpretation,
            int? thePixelPaddingValue,
            ReadOnlyArray<int> thePixelAspectRatio,
            ReadOnlyArray<double> thePixelSpacing,
            ReadOnlyArray<double> theWindowCenter,
            ReadOnlyArray<ushort> thePlanarConfiguration,
            int? thePixelPaddingRangeLimitValue,
            string transferSyntax
        )
        {
            BitsAllocated = theBitsAllocated;
            BitsStored = theBitsStored;
            HighBit = theHighBit;
            PixelRepresentation = thePixelRepresentation;
            Rows = theRows;
            Columns = theColumns;
            SamplesPerPixel = theSamplesPerPixel;
            NumberOfFrames = theNumberOfFrames;
            RescaleIntercept = theRescaleIntercept;
            RescaleSlope = theRescaleSlope;
            PhotometricInterpretation = thePhotometricInterpretation;
            PixelPaddingValue = thePixelPaddingValue;
            SopClassUid = theSopClassUid;
            PixelAspectRatio = thePixelAspectRatio;
            PixelSpacing = thePixelSpacing;
            WindowCenter = theWindowCenter;
            PlanarConfiguration = thePlanarConfiguration;
            PixelPaddingRangeLimitValue = thePixelPaddingRangeLimitValue;
            HasPixels = true;
            TransferSyntaxUid = transferSyntax;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the pixel spacing for the image
        /// </summary>
        public ReadOnlyArray<double> PixelSpacing
        {
            get { return myPixelSpacing; }
            internal set { myPixelSpacing = value; }
        }

        /// <summary>
        /// Gets the pixel imager spacing for the image
        /// </summary>
        public ReadOnlyArray<double> PixelImagerSpacing
        {
            get { return myPixelImagerSpacing; }
            internal set { myPixelImagerSpacing = value; }
        }

        /// <summary>
        /// Gets the window center of the image
        /// </summary>
        public ReadOnlyArray<double> WindowCenter
        {
            get { return myWindowCenter; }
            internal set { myWindowCenter = value; }
        }

        /// <summary>
        /// Gets the pixel aspect ratio for the image
        /// </summary>
        public ReadOnlyArray<int> PixelAspectRatio
        {
            get { return myPixelAspectRatio; }
            internal set { myPixelAspectRatio = value; }
        }

        /// <summary>
        /// Gets the Planar configuration.
        /// </summary>
        public ReadOnlyArray<ushort> PlanarConfiguration
        {
            get { return myPlanarConfiguration; }
            internal set { myPlanarConfiguration = value; }
        }

        /// <summary>
        /// Gets the Rescale Slope of the image
        /// </summary>
        public double? RescaleSlope
        {
            get { return myRescaleSlope; }
            internal set { myRescaleSlope = value; }
        }

        /// <summary>
        /// Gets the Rescale Intercept of the image
        /// </summary>
        public double? RescaleIntercept
        {
            get { return myRescaleIntercept; }
            internal set { myRescaleIntercept = value; }
        }

        /// <summary>
        /// Gets the PixelPaddingValue of the image
        /// </summary>
        public int? PixelPaddingValue
        {
            get { return myPixelPaddingValue; }
            internal set { myPixelPaddingValue = value; }
        }

        /// <summary>
        /// Gets the PixelPaddingRangeLimitValue of the image
        /// </summary>
        public int? PixelPaddingRangeLimitValue
        {
            get { return myPixelPaddingRangeLimitValue; }
            internal set { myPixelPaddingRangeLimitValue = value; }
        }

        /// <summary>
        /// Gets the NumberOfFrames of the image
        /// </summary>
        public int? NumberOfFrames
        {
            get { return myNumberOfFrames; }
            internal set { myNumberOfFrames = value; }
        }

        /// <summary>
        /// Gets the SOPClassUID of the image
        /// </summary>
        public string SopClassUid
        {
            get { return mySopClassUid; }
            internal set { mySopClassUid = value; }
        }

        /// <summary>
        /// Gets the PhotometricInterpretation of the image
        /// </summary>
        public string PhotometricInterpretation
        {
            get { return myPhotometricInterpretation; }
            internal set { myPhotometricInterpretation = value; }
        }

        /// <summary>
        /// Gets the PixelPaddingRangeLimitValue of the image
        /// </summary>
        public string TransferSyntaxUid
        {
            get { return myTransferSyntaxUid; }
            internal set { myTransferSyntaxUid = value; }
        }

        /// <summary>
        /// Gets the bits allocated of the image
        /// </summary>
        public int BitsAllocated
        {
            get { return myBitsAllocated; }
            internal set { myBitsAllocated = value; }
        }

        /// <summary>
        /// Gets the bits stored of the image
        /// </summary>
        public int BitsStored
        {
            get { return myBitsStored; }
            internal set { myBitsStored = value; }
        }

        /// <summary>
        /// Gets the HighBit of the image
        /// </summary>
       public int HighBit
        {
            get { return myHighBit; }
            internal set { myHighBit = value; }
        }

        /// <summary>
        /// Gets the PixelRepresentation of the image
        /// </summary>
        public int PixelRepresentation
        {
            get { return myPixelRepresentation; }
            internal set { myPixelRepresentation = value; }
        }

        /// <summary>
        /// Gets the Rows of the image
        /// </summary>
        public int Rows
        {
            get { return myRows; }
            internal set { myRows = value; }
        }

        /// <summary>
        /// Gets the Columns of the image
        /// </summary>
        public int Columns
        {
            get { return myColumns; }
            internal set { myColumns = value; }
        }

        /// <summary>
        /// Gets the SamplesPerPixel of the image
        /// </summary>
        public int SamplesPerPixel
        {
            get { return mySamplesPerPixel; }
            internal set { mySamplesPerPixel = value; }
        }

        /// <summary>
        /// Gets the offset applied for image conversions.
        /// </summary>
        public int Offset
        {
            get { return myOffset; }
            internal set { myOffset = value; }
        }

        /// <summary>
        /// Represents the type of Image Conversions happened.
        /// </summary>
       public ImageConversions ImageConversionType
        {
            get { return myImageConversionType; }
            internal set { myImageConversionType = value; }
        }

        /// <summary>
        /// Gets whether the data has pixels, i.e. whether the data is of type image.
        /// </summary>
        public bool HasPixels
        {
            get { return myHasPixels; }
            internal set { myHasPixels = value; }
        }

        /// <summary>
        /// Gets whether the image is Spectral Base Image
        /// </summary>
        internal bool IsSpectralBaseImage
        {
            get { return isSpectralBaseImage; }
            set { isSpectralBaseImage = value; }
        }

        #endregion

        #region Helper Methods
        //TICS -6@201 cyclomatic complexity (Legacy code)
        /// <summary>
        /// Gets the image conversion info.
        /// </summary>
        /// <param name="dicomObject">The dicom object.</param>
        /// <returns></returns>
        private void GetImageInformation(DicomObject dicomObject)
        {
            ushort? ba = dicomObject.GetUInt16(DicomDictionary.DicomBitsAllocated);
            ushort? bs = dicomObject.GetUInt16(DicomDictionary.DicomBitsStored);
            TransferSyntaxUid = dicomObject.GetString(DicomDictionary.DicomTransferSyntaxUid);
            if (ba == null || bs == null)
            {
                // for non-image data these two will be null
                HasPixels = false;
                return;
            }
            var rows = dicomObject.GetUInt16(DicomDictionary.DicomRows);
            if (rows == null)
            {
                // default
                rows = 512;
            }
            var cols = dicomObject.GetUInt16(DicomDictionary.DicomColumns);
            if (cols == null)
            {
                // default
                cols = 512;
            }
            Rows = rows.Value;
            Columns = cols.Value;
            BitsAllocated = ba.Value;
            BitsStored = bs.Value;
            HighBit = dicomObject.GetUInt16(DicomDictionary.DicomHighBit).Value;
            PhotometricInterpretation =
                dicomObject.GetString(DicomDictionary.DicomPhotometricInterpretation);
            PixelRepresentation = dicomObject.GetUInt16(
                DicomDictionary.DicomPixelRepresentation).Value;
            Rows = dicomObject.GetUInt16(DicomDictionary.DicomRows).Value;
            Columns = dicomObject.GetUInt16(DicomDictionary.DicomColumns).Value;
            SamplesPerPixel = dicomObject.GetUInt16(DicomDictionary.DicomSamplesPerPixel).Value;
            NumberOfFrames = dicomObject.GetInt32(DicomDictionary.DicomNumberOfFrames);
            // check if the dicomObject contains shared functional group, MF Image?
            DicomObject[] sfgs = dicomObject.GetDicomObject(
                DicomDictionary.DicomSharedFunctionalGroupsSequence);
            if (sfgs != null && sfgs.Length > 0)
            {
                DicomObject dcm = sfgs[0];

                DicomObject[] sequence = dcm.GetDicomObject(
                    DicomDictionary.DicomPixelValueTransformationSequence);
                if (sequence != null && sequence.Length > 0)
                {
                    RescaleIntercept = sequence[0].GetDouble(DicomDictionary.DicomRescaleIntercept);
                    RescaleSlope = sequence[0].GetDouble(DicomDictionary.DicomRescaleSlope);
                }
            }
            else
            {
                // try to get directly from the image object
                RescaleIntercept = dicomObject.GetDouble(DicomDictionary.DicomRescaleIntercept);
                RescaleSlope = dicomObject.GetDouble(DicomDictionary.DicomRescaleSlope);
            }
            SopClassUid = dicomObject.GetString(DicomDictionary.DicomSopClassUid);

            IsSpectralBaseImage = DetermineSpectralBaseImage(
                SopClassUid,
                PhotometricInterpretation,
                dicomObject
            );
            // PixelPaddingValue is US or SS. Almost it will in SS, so try for it first.
            DicomVR tagVR = dicomObject.GetTagVR(DicomDictionary.DicomPixelPaddingValue);
            if (tagVR != DicomVR.None)
            {
                DictionaryTag dictTag = new DictionaryTag(
                    DicomDictionary.DicomPixelPaddingValue.Tag,
                    tagVR,
                    DicomDictionary.DicomPixelPaddingValue.ValueMultiplicity,
                    DicomDictionary.DicomPixelPaddingValue.Name,
                    DicomDictionary.DicomPixelPaddingValue.ImplementerId
                );

                PixelPaddingValue = (tagVR == DicomVR.US)
                    ? (short?)dicomObject.GetUInt16(dictTag)
                    : dicomObject.GetInt16(dictTag);
            }
            DicomObject wcDicomObject = dicomObject;

            if (sfgs != null && sfgs.Length > 0)
            {
                // MF Image, get window center from DicomFrameVoiLutSequence
                DicomObject dcm = sfgs[0];

                DicomObject[] sequence = dcm.GetDicomObject(
                    DicomDictionary.DicomFrameVoiLutSequence);
                if (sequence != null && sequence.Length > 0)
                {
                    wcDicomObject = sequence[0];
                }
            }
            ReadOnlyArray<double> wc = ReadOnlyArray<double>.Empty;
            double? wcd = null;
            // as per DICOM, the window center is double array
            try
            {
                //This function is used for the getting values without validation.
                wc = GetDoubleArrayWithoutValidation(
                    wcDicomObject,
                    DicomDictionary.DicomWindowCenter);
                //TICS -8@110 Do not silently ignore exceptions
            }
            catch (ArgumentException)
            {
                // in most of the cases, WC is stored as doubleArray, but in some datasets i 
                // observed it to be double, so handle that as well
                wcd = wcDicomObject.GetDouble(DicomDictionary.DicomWindowCenter);
            }
            //TICS +8@110 Do not silently ignore exceptions
            if (wc.HasValue)
            {
                WindowCenter = wc;
            }
            else if (wcd.HasValue)
            {
                WindowCenter = new ReadOnlyArray<double>(new[] { wcd.Value });
            }

            ReadOnlyArray<int> pixelAspectRatio =
                dicomObject.GetInt32Array(DicomDictionary.DicomPixelAspectRatio);
            if (pixelAspectRatio.HasValue)
            {
                PixelAspectRatio = pixelAspectRatio;
            }


            ReadOnlyArray<double> pixelSpacing = ReadOnlyArray<double>.Empty;
            ReadOnlyArray<double> pixelImagerSpacing = ReadOnlyArray<double>.Empty;

            //This function is used for the getting values without validation.
            pixelSpacing = GetDoubleArrayWithoutValidation(
                dicomObject,
                DicomDictionary.DicomPixelSpacing);
            pixelImagerSpacing = GetDoubleArrayWithoutValidation(
                dicomObject,
                DicomDictionary.DicomImagerPixelSpacing);

            if (pixelSpacing.HasValue)
            {
                PixelSpacing = pixelSpacing;
            }
            if (pixelImagerSpacing.HasValue)
            {
                PixelImagerSpacing = pixelImagerSpacing;
            }

            if (SamplesPerPixel == 1)
            {
                PlanarConfiguration = new ReadOnlyArray<ushort>(new ushort[] { 0 });
            }
            else
            {
                ushort? planarConfig = dicomObject.GetUInt16(
                    DicomDictionary.DicomPlanarConfiguration);
                if (!planarConfig.HasValue)
                {
                    string error = "The given dicom object doesnot contain mandatory planar" +
                        " configuration attribute.Defaulting to 0";
                    planarConfig = 0;
                }
                PlanarConfiguration = new ReadOnlyArray<ushort>(new[] { planarConfig.Value });
            }
            tagVR = dicomObject.GetTagVR(DicomDictionary.DicomPixelPaddingRangeLimit);
            if (tagVR != DicomVR.None)
            {
                DictionaryTag dictTag = new DictionaryTag(
                    DicomDictionary.DicomPixelPaddingRangeLimit.Tag,
                    tagVR,
                    DicomDictionary.DicomPixelPaddingRangeLimit.ValueMultiplicity,
                    DicomDictionary.DicomPixelPaddingRangeLimit.Name,
                    DicomDictionary.DicomPixelPaddingRangeLimit.ImplementerId
                );

                PixelPaddingRangeLimitValue = (tagVR == DicomVR.US)
                    ? (short?)dicomObject.GetUInt16(dictTag)
                    : dicomObject.GetInt16(dictTag);
            }
            HasPixels = true;
        }
        //TICS +6@201 cyclomatic complexity (Legacy code)

        /// <summary>
        /// This methods retuns the value without performing validations.
        /// This should be used only when the validation of tha tag value is not required.
        /// This should be used for tags which has double as their value type.
        /// </summary>
        /// <param name="record">Object from which value has to be fetched.</param>
        /// <param name="doubleArrayTag">Tag for which the value is required</param>
        /// <returns>Returns the value for the tag</returns>
        /// <remarks>This is done here because the validation of Double Array took considerable
        /// time. Ideally this should be handled by the Dicom library.
        /// This is a stop gap measure.</remarks>
        private static ReadOnlyArray<double> GetDoubleArrayWithoutValidation(
            DicomObject record,
            DictionaryTag doubleArrayTag)
        {

            ReadOnlyArray<double> result;
                result = record.GetDoubleArray(doubleArrayTag);
            return result;
        }


        private static bool DetermineSpectralBaseImage(
            string sopClassUid,
            string photometricInterpretation,
            DicomObject dicomObject
        )
        {
            //if (
            //    photometricInterpretation.Equals(
            //        RGB,
            //        StringComparison.OrdinalIgnoreCase
            //    ) &&
            //    sopClassUid.Equals(
            //        Philips.Platform.Dicom.WellKnownSopClassUids.SecondaryCaptureImageStorage,
            //        StringComparison.Ordinal) &&
            //    IsImageTypeSBI(dicomObject)
            //    )
            //{
            //    return true;
            //}
            return false;
        }

        #endregion
    }


}