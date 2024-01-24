// Copyright Koninklijke Philips N.V. 2011

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using Philips.Platform.Common;
using Philips.Platform.Dicom;
//using DicomDictionary = Philips.Platform.Common.CommonDicomDictionary;
//using PhilipsDictionary = Philips.Platform.Common.CommonPhilipsDictionary;

namespace CTHarmonyAdapters { 
    //TICS -6@201 cyclomatic complexity (Legacy code)
/// <summary>
/// Performs signed to unsigned pixel data conversion.
/// TODO: Handle window center as well.
/// </summary>
    internal sealed class ImageTranslator {
        
        #region Member variables

        private static HashSet<string> CompressedColorImages =
            new HashSet<string>(StringComparer.Ordinal){
                PhotometricInterpretation.YbrFull,
                PhotometricInterpretation.YbrFull422,
                PhotometricInterpretation.YbrPartial420,
                PhotometricInterpretation.YbrPartial422,
                PhotometricInterpretation.YbrIct,
                PhotometricInterpretation.YbrRct
            };
        // Some sop class uids doesn't need pixel padding conversion to zero
        // This results in some inconsistency in presentation of that image in the
        // viewer. Earlier only MR has this issue, now DXR is also having some
        // issues with converting pixel padding values for monochrome1 images.
        private IList<string> SopClassUidsToIgnoreForPixelPaddingConversion = new string[] {
            WellKnownSopClassUids.MRImageStorage,
            WellKnownSopClassUids.BreastTomosynthesisImageStorage,
            WellKnownSopClassUids.DigitalMammographyXrayImageStorageForPresentation
        };
        private double[] translationOffsetIntercept;
        private ReadOnlyArray<double> newWindowCenter;
        private ReadOnlyArray<ushort> newPlanarConfiguration;
        private readonly DicomObject dicomObject;
        private DicomObject translatedDicomObject;
        private ReadOnlyArray<double> windowCenter;
        private ReadOnlyArray<ushort> planarConfiguration;
        private IntPtr sourcePixels;
        private IntPtr outputPixels;
        private int? newPixelPaddingValue;
        private readonly int? numberOfFrames;
        private double newIntercept;
        private double newSlope;
        private readonly double intercept;
        private readonly double slope;

        private readonly string sopClassUid;
        private readonly string photometricInterpretation;


        private readonly int bitsAllocated;
        private readonly int bitsStored;
        private readonly int pixelRepresentation;
        private readonly int rows;
        private readonly int columns;
        private readonly int highBit;
        private readonly int samplesPerPixel;
        private readonly int pixelPaddingValue;
        private readonly int pixelPaddingRangeLimitValue;
        private int newRows;
        private int newColumns;
        private int newHighBit;
        private int newPixelRepresentation;
        private int myOffset;
        private ImageConversions imageConversions = ImageConversions.None;
        private ImageTranslationStatus translationStatus =
            ImageTranslationStatus.ShouldNotBeTranslated;
        private TranslationType translationType;
        private readonly bool hasPixelPaddingValue;
        private readonly bool hasPixelPaddingRangeLimitValue;
        private readonly bool clipNegativeValuesToZero;
        private readonly bool useNoOfFrames;
        private bool isDefaultSlopeSet;
        private bool isDefaultInterceptSet;
        private bool setToCanonicalCalled;
        private bool rescaleUpdated;
        private bool isSpectralBaseImage;
        #endregion Member Variables

        /// <summary>
        /// Returns the translation offset intercept
        /// </summary>
        internal double[] OriginalOffset {
            get {
                return translationOffsetIntercept;
            }
        }

        #region Constructor

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="theImageConversionInfo">The image conversion info.</param>
        /// <param name="theDicomObject">
        /// The dicom object. should be passed when reading from file.<br/>
        /// Default value: null.
        /// </param>
        /// <param name="clipNegativePixelDataValuesToZero">
        /// If true then the negative values in the pixel data will be clipped to 0.
        /// <br/>Default value: false</param>
        /// <param name="useNumberOfFrames">
        /// Specifies whether to number of frames in calculating the pixel data size.
        /// <br/>Default value: false
        /// </param>
        /// <param name="translationType">Type of Translation required</param>
        public ImageTranslator(
            ImageConversionInformation theImageConversionInfo,
            DicomObject theDicomObject = null,
            bool clipNegativePixelDataValuesToZero = false,
            bool useNumberOfFrames = false,
            TranslationType translationType = TranslationType.Regular
        ) {
            ModifiedImageConversionInfo = theImageConversionInfo;
            this.translationType = translationType;
            clipNegativeValuesToZero = clipNegativePixelDataValuesToZero;
            if (clipNegativeValuesToZero) {
                //avoid fxcop. 
                //TODO: remove this variable
            }
            useNoOfFrames = useNumberOfFrames;
            dicomObject = theDicomObject;
            if (theImageConversionInfo == null || !theImageConversionInfo.HasPixels) {
                return;
            }
            sopClassUid = theImageConversionInfo.SopClassUid;
            photometricInterpretation = 
                CompressedColorImages.Contains(theImageConversionInfo.PhotometricInterpretation) ?
                    MemoryManagerConstants.RGB : theImageConversionInfo.PhotometricInterpretation;
            bitsAllocated = theImageConversionInfo.BitsAllocated;
            bitsStored = theImageConversionInfo.BitsStored;
            pixelRepresentation = theImageConversionInfo.PixelRepresentation;
            newPixelRepresentation = pixelRepresentation;
            numberOfFrames = theImageConversionInfo.NumberOfFrames.HasValue ?
                theImageConversionInfo.NumberOfFrames.Value : (int?)null;
            rows = theImageConversionInfo.Rows;
            columns = theImageConversionInfo.Columns;
            highBit = theImageConversionInfo.HighBit;
            samplesPerPixel = theImageConversionInfo.SamplesPerPixel;
            if (theImageConversionInfo.RescaleIntercept.HasValue) {
                intercept = theImageConversionInfo.RescaleIntercept.Value;
            } else {
                intercept = 0;
                isDefaultInterceptSet = true;
            }
            if (theImageConversionInfo.RescaleSlope.HasValue) {
                slope = theImageConversionInfo.RescaleSlope.Value;
            } else {
                slope = 1.0;
                isDefaultSlopeSet = true;
            }
            hasPixelPaddingValue = theImageConversionInfo.PixelPaddingValue.HasValue;
            hasPixelPaddingRangeLimitValue =
                theImageConversionInfo.PixelPaddingRangeLimitValue.HasValue;
            if (hasPixelPaddingValue) {
                pixelPaddingValue = theImageConversionInfo.PixelPaddingValue.Value;
            }
            if (hasPixelPaddingRangeLimitValue) {
                pixelPaddingRangeLimitValue =
                    theImageConversionInfo.PixelPaddingRangeLimitValue.Value;
            }
            windowCenter = theImageConversionInfo.WindowCenter;
            planarConfiguration = theImageConversionInfo.PlanarConfiguration;
            isSpectralBaseImage = theImageConversionInfo.IsSpectralBaseImage;
            rescaleUpdated = false;
            RequiresTranslation();
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="theImageConversionInfo">The image conversion info.</param>
        /// <param name="theDicomObject">
        /// The dicom object. should be passed when reading from file.
        /// </param>
        /// <param name="shallowCopyInputHeader">
        /// Specifies whethe to shallow copy the given <paramref name="theDicomObject"/>
        /// before modifying it.
        /// </param>
        public ImageTranslator(
            ImageConversionInformation theImageConversionInfo,
            DicomObject theDicomObject,
            bool shallowCopyInputHeader
        )
            : this(theImageConversionInfo, theDicomObject, false, false) {
            if (theDicomObject != null && shallowCopyInputHeader) {
                dicomObject = theDicomObject.ShallowCopy();
            }
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Translate an image
        /// </summary>
        /// <param name="pixels">input original pixels, output modified pixels</param>
        /// <param name="theModifiedImageConversionInfo">The modified image conversion info.</param>
        public void Translate(
            IntPtr pixels,
            out ImageConversionInformation theModifiedImageConversionInfo
        ) {
            theModifiedImageConversionInfo = ModifiedImageConversionInfo;
            try {
                if (translationStatus == ImageTranslationStatus.ShouldBeTranslated) {
                    sourcePixels = pixels;
                    outputPixels = pixels;

                    newRows = rows;
                    newColumns = columns;

                    if (highBit != (bitsStored - 1)) {
                        // MONOCHROME: shift bits to the right place in a 16 bit word
                        // (signed or unsigned)
                        ShiftToLSB();
                    } else if ((pixelRepresentation == 1) && (bitsAllocated > bitsStored)) {
                        // shift to ensure we have a proper values while operating on 
                        // actual bits stored and high bit of the pixel data
                        ShiftSignedBit();
                    }
                    TranslateOverlayPlane();

                    if (pixelRepresentation != 0) {
                        // convert MONOCHROME images from signed to unsigned
                        ConvertToUnsigned();
                    } else {
                        // unsigned MONOCHROME
                        // also do not convert padded pixels of DXR Mammo & tomo
                        // images. They have padded pixels as 65535 and making them
                        // zero makes a white line appear in the image which 
                        // appears as a artifact to DXR
                        if (
                            hasPixelPaddingValue && 
                            (sopClassUid != WellKnownSopClassUids.
                                DigitalMammographyXrayImageStorageForPresentation && 
                            sopClassUid !=  WellKnownSopClassUids.
                                BreastTomosynthesisImageStorage || 
                                photometricInterpretation != MemoryManagerConstants.Monochrome1
                            )
                        ) {
                            bool convertPaddingValues = true;
                            if (pixelPaddingValue == 0) {
                                convertPaddingValues = hasPixelPaddingRangeLimitValue;
                            }
                            if (convertPaddingValues) {
                                ReplacePaddingValueUnsigned();
                            }
                        }
                    }
                    // check if RGB, 8-bit planarInterleavedplanar interleaved; if so
                    // convert to RGB, 8 bit PixelInterleaved
                    if (planarConfiguration[0] == 1) {
                        ConvertPlanarToPixelInterleaved();
                    }

                    SetToCanonical();
                    if (dicomObject != null) {
                        translatedDicomObject = GetModifiedDicomObject();
                    }
                    theModifiedImageConversionInfo = GetModifiedImageConversionInformation();
                }
            } catch (Exception e) {
                //LogHelper.LogError(
                //    String.Format(
                //    CultureInfo.InvariantCulture,
                //    "Image translation generated an exception: {0}",
                //    e)
                //);
                pixels = sourcePixels;
                throw;
            }
        }

        /// <summary>
        /// The logic is to convert the bit stored (typically 12 bit) signed images 
        /// into 16 bit signed images
        /// Since in signed to unsigned conversion, we use short (16 bits)
        /// Hence shift the bits to get a proper 16 bit signed pixel value
        /// </summary>
        private unsafe void ShiftSignedBit() {
            if (bitsAllocated != 16) {
                return;
            }
            /*
            * Eg: BA = 16 BS = 12 HB = 11
            *  sign = 0xF000
            *  signedHighBitMSB = 0x800
            *  bitsStoredMaskedValue = 0x0FFF
            *  outputPixels = 0xFFE (i.e. -2 decimal in 12 bits system)
            *  value = FFE (since high bit is 11)
            *  0xFFE & 0xFFFF = 1
            *  change 0xFFE ==> 0xFFFE (i.e. 12 bit -2 value now became 16 bit -2)
             */
            ushort sign = (ushort)((0xffff >> bitsStored) << bitsStored);
            ushort signedHighBitMSB = (ushort)(1 << highBit);
            ushort bitsStoredMaskedValue = (ushort)(~sign);

            unchecked {
                ushort* pixels = (ushort*)outputPixels;
                int pixelSize = 
                    numberOfFrames.HasValue ? numberOfFrames.Value : 1 * newRows * newColumns;
                for (int index = 0; index < pixelSize; ++index) {
                    ushort value = (ushort)((*pixels) & bitsStoredMaskedValue);
                    if ((value & signedHighBitMSB) != 0) { // this means it is negative number
                        value |= sign; // sign extended to bits allocated
                    }
                    *pixels++ = value;
                }
            }
        }

        /// <summary>
        /// Get translated dicom object
        /// </summary>
        public DicomObject TranslatedDicomObject {
            get {
                if (translatedDicomObject != null) {
                    return translatedDicomObject;
                }
                if (TranslationStatus == ImageTranslationStatus.ShouldBeTranslated) {
                    SetToCanonical();
                    translatedDicomObject = GetModifiedDicomObject();
                } else {
                    translatedDicomObject = dicomObject;
                }
                //This is required for Portal for correct display in
                //MMV-thumbnails.
                if (ImageTranslator.IsLossyCompressedColorImage(translatedDicomObject)) {
                    translatedDicomObject.SetString(DicomDictionary.DicomPhotometricInterpretation,
                        MemoryManagerConstants.RGB);
                }
                RemoveTags(translatedDicomObject);
                return translatedDicomObject;
            }
        }

        /// <summary>
        /// Gets the modified image conversion information.
        /// </summary>
        /// <returns></returns>
        public ImageConversionInformation GetModifiedImageConversionInformation() {
            if (translationStatus == ImageTranslationStatus.ShouldBeTranslated) {
                if (myOffset == 0) {
                    SetToCanonical();
                }
                var ici = ModifiedImageConversionInfo;
                ModifiedImageConversionInfo = new ImageConversionInformation(
                    bitsAllocated,
                    bitsStored,
                    newHighBit,
                    newPixelRepresentation,
                    rows,
                    columns,
                    samplesPerPixel,
                    numberOfFrames,
                    newIntercept,
                    newSlope,
                    sopClassUid,
                    photometricInterpretation,
                    hasPixelPaddingValue ? pixelPaddingValue : (int?)null,
                    ici.PixelAspectRatio,
                    ici.PixelSpacing,
                    newWindowCenter,
                    newPlanarConfiguration,
                    hasPixelPaddingRangeLimitValue ? pixelPaddingRangeLimitValue : (int?)null,
                    WellKnownTransferSyntaxes.ExplicitVrLittleEndian.Uid
                );
                ModifiedImageConversionInfo.Offset = myOffset;
                ModifiedImageConversionInfo.ImageConversionType = imageConversions;
                ModifiedImageConversionInfo.PixelImagerSpacing = ici.PixelImagerSpacing;
            }
            return ModifiedImageConversionInfo;
        }

        /// <summary>
        /// Removes the unnecessary tags.
        /// </summary>
        /// <param name="dcm">The dicom object from which tags have to be removed.</param>
        private static void RemoveTags(DicomObject dcm) {
            if (dcm != null) { 
                dcm.Remove(DicomDictionary.DicomPixelData);
                dcm.Remove(PhilipsDictionary.TamarCompressedPixelData);
                dcm.Remove(PhilipsDictionary.TamarCompressionType);
                dcm.Remove(DicomDictionary.DicomDataSetTrailingPadding);
                // TODO: The DicomDigitalSignaturesSequence tag may not have to
                // be removed depending on Application's requirement
                dcm.Remove(DicomDictionary.DicomDigitalSignaturesSequence);
            
                dcm.Remove(DicomDictionary.DicomItem);
                dcm.Remove(DicomDictionary.DicomItemDelimitationItem);
                dcm.Remove(DicomDictionary.DicomSequenceDelimitationItem);
                dcm.Remove(DicomDictionary.DicomSpectroscopyData);
                dcm.Remove(PhilipsDictionary.PiimSpectroBaseLine);
                dcm.Remove(
                    PhilipsDictionary.PiimSpectroBaseLineCorrectedSpectrum
                );
                dcm.Remove(PhilipsDictionary.PiimSpectroFittedSpectrum);
                dcm.Remove(PhilipsDictionary.PiimSpectroPpmAxisValues);

                dcm.Remove(PhilipsDictionary.PiimUsPrivateNativeDataSequence2D);
                dcm.Remove(PhilipsDictionary.PiimUsPrivateNativeDataSequence3D);
                dcm.Remove(PhilipsDictionary.PiimUsPrivateCompressedNativeDataSequence2D);
                dcm.Remove(PhilipsDictionary.PiimUsPrivateCompressed3DNativeDataSequence);
                dcm.Remove(PhilipsDictionary.PiimUsPrivateCompressedColorNativeDataSequence);
                dcm.Remove(
                    PhilipsDictionary.PiimUsPrivateCompressedTissueDopplerNativeDataSequence
                );
                dcm.Remove(PhilipsDictionary.PiimUsPrivateCompressedElastoNativeDataSequence);
                dcm.Remove(PhilipsDictionary.PiimUsPrivateCompressed3DHvrNativeDataSequence);
                dcm.Remove(PhilipsDictionary.PiimUsPrivate3DPixelData);
                dcm.Remove(PhilipsDictionary.PiimUsPrivate3DPixelData1);
                dcm.Remove(GEDictionary.GEMS_MR_ICON_SQ);
            }
        }


        /// <summary>
        /// Gets the modified dicom object.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Assumes that SetToCanonical is already called.</remarks>
        private DicomObject GetModifiedDicomObject() {
            if (translationStatus != ImageTranslationStatus.ShouldBeTranslated) {
                return dicomObject;
            }
            if (dicomObject == null) {
                return null;
            }
            if (
                (imageConversions & ImageConversions.SignedToUnsigned) ==
                ImageConversions.SignedToUnsigned
            ) {
                DicomObject[] sfgs = dicomObject.GetDicomObject(
                    DicomDictionary.DicomSharedFunctionalGroupsSequence);
                if (sfgs != null && sfgs.Length > 0) {
                    DicomObject dcm = sfgs[0];

                    DicomObject[] sequence = dcm.GetDicomObject(
                        DicomDictionary.DicomPixelValueTransformationSequence);
                    if (sequence != null && sequence.Length > 0) {
                        sequence[0].SetDouble(DicomDictionary.DicomRescaleSlope, newSlope);
                        sequence[0].SetDouble(DicomDictionary.DicomRescaleIntercept, newIntercept);
                    }
                }
            }
            dicomObject.SetUInt16(DicomDictionary.DicomHighBit, (ushort)newHighBit);
            dicomObject.SetUInt16(
                DicomDictionary.DicomPixelRepresentation,
                (ushort)newPixelRepresentation
            );
            dicomObject.SetDouble(DicomDictionary.DicomRescaleSlope, newSlope);
            dicomObject.SetDouble(DicomDictionary.DicomRescaleIntercept, newIntercept);
            if (newPixelPaddingValue.HasValue) {

                DicomVR tagVR = DicomVR.US;
                if (newPixelRepresentation == 1) {
                    tagVR = DicomVR.SS;
                }

                DictionaryTag pixelPaddingValueTag = new DictionaryTag(
                    DicomDictionary.DicomPixelPaddingValue.Tag,
                    tagVR,
                    DicomDictionary.DicomPixelPaddingValue.ValueMultiplicity,
                    DicomDictionary.DicomPixelPaddingValue.Name,
                    DicomDictionary.DicomPixelPaddingValue.ImplementerId);

                    if (tagVR == DicomVR.US) {
                        dicomObject.SetUInt16(
                            pixelPaddingValueTag,
                            (ushort)newPixelPaddingValue.Value);
                    } else {
                        dicomObject.SetInt16(
                            pixelPaddingValueTag,
                            (short)newPixelPaddingValue.Value);
                    }
            }
            if (translationOffsetIntercept != null && translationOffsetIntercept.Length > 0) {
                dicomObject.SetDoubleArray(
                    PhilipsDictionary.ElscintTranslationOffsetIntercept, 
                    translationOffsetIntercept
                );
            }
            // set only if not null
            // TODO: see the calculation of window center
            if (windowCenter != null) {
                dicomObject.SetDoubleArray(
                    DicomDictionary.DicomWindowCenter,
                    windowCenter.ToArray()
                );
            }

            if (planarConfiguration[0] == 1) {
                dicomObject.SetUInt16(
                    DicomDictionary.DicomPlanarConfiguration,
                    newPlanarConfiguration[0]
                );
            }
            if (
                hasPixelPaddingValue && 
                !sopClassUid.Equals(
                WellKnownSopClassUids.MRImageStorage, StringComparison.OrdinalIgnoreCase)
            ) {
                dicomObject.Remove(DicomDictionary.DicomPixelPaddingValue);
            }
            if (hasPixelPaddingRangeLimitValue) {
                dicomObject.Remove(DicomDictionary.DicomPixelPaddingRangeLimit);
            }
            if (dicomObject.HasTag(DicomDictionary.DicomSmallestPixelValueInSeries)) {
                dicomObject.Remove(DicomDictionary.DicomSmallestPixelValueInSeries);
            }
            if (dicomObject.HasTag(DicomDictionary.DicomSmallestImagePixelValue)) {
                dicomObject.Remove(DicomDictionary.DicomSmallestImagePixelValue);
            }
            if (dicomObject.HasTag(DicomDictionary.DicomLargestImagePixelValue)) {
                dicomObject.Remove(DicomDictionary.DicomLargestImagePixelValue);
            }
            if (dicomObject.HasTag(DicomDictionary.DicomLargestPixelValueInSeries)) {
                dicomObject.Remove(DicomDictionary.DicomLargestPixelValueInSeries);
            }

            if (rescaleUpdated) {

                DicomObject[] sfgs = dicomObject.GetDicomObject(
                    DicomDictionary.DicomSharedFunctionalGroupsSequence);
                if (sfgs != null && sfgs.Length > 0) {
                    DicomObject dcm = sfgs[0];

                    DicomObject[] sequence = dcm.GetDicomObject(
                        DicomDictionary.DicomPixelValueTransformationSequence);
                    if (sequence != null && sequence.Length > 0) {
                        sequence[0].Remove(DicomDictionary.DicomRescaleSlope);
                        sequence[0].Remove(DicomDictionary.DicomRescaleIntercept);
                    }
                }
            }
            if (ImageTranslator.IsLossyCompressedColorImage(dicomObject)) {
                dicomObject.SetString(DicomDictionary.DicomPhotometricInterpretation,
                    MemoryManagerConstants.RGB);
            }

            return dicomObject;
        }

        /// <summary>
        /// Gets the translation status.
        /// </summary>
        public ImageTranslationStatus TranslationStatus {
            get {
                return translationStatus;
            }
        }

        /// <summary>
        /// Gets the modified image conversion info
        /// </summary>
        public ImageConversionInformation ModifiedImageConversionInfo { get; private set; }

        /// <summary>
        /// Converts pixel data inot 12-bit pixel-data blocks
        /// </summary>
        /// <param name="src"></param>
        public static unsafe void ConvertTo12BitBlocks(IntPtr src) {
            var bulkSize = ((int*)src)[0];
            var pixelOffset = ((int*)src)[1];
            int affectedPixels = 0;
            short* temp = (short*)(src.ToInt64() + pixelOffset);
            int pixelSize = bulkSize - pixelOffset;
            for (int i = 0; i < pixelSize / 2; i++) {
                if (temp[i] > MemoryManagerConstants.MaxPixelValue) {
                    temp[i] = MemoryManagerConstants.MaxPixelValue;
                    affectedPixels++;
                }
            }
            //LogHelper.LogInfo(
            //    "Affected pixels: " +
            //    affectedPixels.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Converts to custom MR Block
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="dicomHeader"></param>
        /// <param name="tobeTruncated"></param>
        /// <param name="bitsAllocated"></param>
        public static unsafe void ConvertToCustomMRBlock(
            IntPtr dest,
            DicomObject dicomHeader,
            bool tobeTruncated,
            int bitsAllocated
        ) {
            double originalIntercept = 0;
            ReadOnlyArray<double> translationOffsetInterceptTemp;

            if (dicomHeader.HasTag(PhilipsDictionary.ElscintTranslationOffsetIntercept)) {
                translationOffsetInterceptTemp = dicomHeader.GetDoubleArray(
                    PhilipsDictionary.ElscintTranslationOffsetIntercept
                );
                originalIntercept = translationOffsetInterceptTemp[0];

                ConvertToCustomTranslationType(
                    dest,
                    originalIntercept,
                    bitsAllocated,
                    tobeTruncated
                );
            }
        }

        /// <summary>
        /// Converts the data to custom translation type
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="originalOffset"></param>
        /// <param name="bitsAllocated"></param>
        /// <param name="tobeTruncated"></param>
        public static unsafe void ConvertToCustomTranslationType(
            IntPtr dest,
            double originalOffset,
            int bitsAllocated,
            bool tobeTruncated
        ) {
            var bulkSize = ((int*)dest)[0];
            var pixelOffset = ((int*)dest)[1];
            long destAddress = dest.ToInt64() + pixelOffset;
            int pixelSize = bulkSize - pixelOffset;

            ushort offset = (ushort)(originalOffset);
            if (bitsAllocated > 8) {
                int numPixels = pixelSize / 2;
                short* destPixel = (short*)(destAddress);
                int val = 0;
                if (tobeTruncated) {
                    for (int i = 0; i < numPixels; i++) {
                        val = (ushort)destPixel[i] - offset;
                        if (val < 0) {
                            val = 0;
                        }
                        destPixel[i] = (short)val;
                    }
                } else {
                    for (int i = 0; i < numPixels; i++) {
                        val = (ushort)destPixel[i] - offset;
                        destPixel[i] = (short)val;
                    }
                }
            } else {
                int numPixels = pixelSize;
                sbyte* destPixel = (sbyte*)(destAddress);
                int val = 0;
                if (tobeTruncated) {
                    for (int i = 0; i < numPixels; i++) {
                        val = (byte)destPixel[i] - offset;
                        if (val < 0) {
                            val = 0;
                        }
                        destPixel[i] = (sbyte)val;
                    }
                } else {
                    for (int i = 0; i < numPixels; i++) {
                        val = (byte)destPixel[i] - offset;
                        destPixel[i] = (sbyte)val;
                    }
                }
            }
        }

        #endregion Public Methods

        #region Helper Methods
        internal static bool IsLossyCompressedColorImage(DicomObject image) {
            if(image != null){
                string photoMetricIntepretation = 
                    (string)image.GetValue(DicomDictionary.DicomPhotometricInterpretation);
                if (CompressedColorImages.Contains(photoMetricIntepretation)) {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Convert band interleaved RGB pixels to pixel interleaved RGB pixels.
        /// Note: 3 bands assumed.
        /// </summary>
        private unsafe void ConvertPlanarToPixelInterleaved() {
            int resultLength = rows * columns * samplesPerPixel;
            // use native allocation here so that we can control the memory
            IntPtr copy = Marshal.AllocHGlobal(resultLength);
            int numFrames = 1;
            if (useNoOfFrames && numberOfFrames.HasValue && (numberOfFrames.Value > 0)) {
                numFrames = numberOfFrames.Value;
            }
            IntPtr outPutFrame = outputPixels;
            for (int i = 0; i < numFrames; i++) {
                outPutFrame = outputPixels + (i * resultLength);
                MemoryManagerNativeMethods.MemCpy(copy, outPutFrame, resultLength);
                int bandLength = resultLength / 3;
                if ((resultLength % 3) != 0) {
                    // Discard padding bytes...
                    resultLength = 3 * bandLength;
                }
                int src0 = 0;
                int src1 = bandLength;
                int src2 = 2 * bandLength;
                byte* src = (byte*)copy;
                byte* dst = (byte*)outPutFrame;
                for (int result = 0; result < 3 * bandLength; result += 3) {
                    dst[result] = src[src0++];
                    dst[result + 1] = src[src1++];
                    dst[result + 2] = src[src2++];
                }
            }
            Marshal.FreeHGlobal(copy);
        }

        /// <summary>
        /// Test if the given input should be converted using translation
        /// </summary>
        private void RequiresTranslation() {
            translationStatus = ImageTranslationStatus.ShouldNotBeTranslated;
            if (
                (rows < 2 || columns < 2) ||
                ((bitsAllocated != 8) && (bitsAllocated != 16)) ||
                ((bitsAllocated == 8) && (bitsStored != 8))
            ) {
                translationStatus = ImageTranslationStatus.NotSupported;
            } else {
                // 8 or 16 bit images
                if (
                    !photometricInterpretation.Equals(
                        MemoryManagerConstants.Monochrome1,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !photometricInterpretation.Equals(
                        MemoryManagerConstants.Monochrome2,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !photometricInterpretation.Equals(
                        MemoryManagerConstants.RGB,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    !photometricInterpretation.Equals(
                        MemoryManagerConstants.PaletteColor,
                        StringComparison.OrdinalIgnoreCase
                    )
                ) {
                    translationStatus = ImageTranslationStatus.NotSupported;
                } else if (
                    photometricInterpretation.Equals(
                        MemoryManagerConstants.RGB,
                        StringComparison.OrdinalIgnoreCase
                    )
                ) {
                    // Only 8 RGB unsigned images are supported as on today
                    if (bitsAllocated != 8 || pixelRepresentation != 0) {
                        translationStatus = ImageTranslationStatus.NotSupported;
                    } else if (
                        planarConfiguration[0] == 1 &&
                        !isSpectralBaseImage
                    ) {
                        translationStatus = ImageTranslationStatus.ShouldBeTranslated;
                    }
                } else {
                    // MONOCHROME or PALETTE COLOR
                    if (pixelRepresentation != 0) {
                        // signed
                        translationStatus =
                            // TODO: Add formal support for signed PALETTE COLOR data.
                            // Yet, it works the same as MONOCHROME, so better to do the translate
                            // and avoid that images that are known to be incorrect are provided.
                            // UADE takes care of applying the conversion offset to the RGB LUTs.
                            ImageTranslationStatus.ShouldBeTranslated;
                    } else {
                        // unsigned data; check padding values or if 16 bit data
                        if (
                            (
                                hasPixelPaddingValue &&
                                !string.Equals(sopClassUid, WellKnownSopClassUids.MRImageStorage)
                            ) ||
                            (highBit != bitsStored - 1)
                        ) {
                            // PixelPaddingRangeLimit will be present optionally only when 
                            // PixelPaddingValue exists, so no need to check here.
                            translationStatus = ImageTranslationStatus.ShouldBeTranslated;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shifts 16 bit allocated data to LSB 
        /// </summary>
        private unsafe void ShiftToLSB() {
            if (bitsAllocated != 16) {
                return;
            }
            imageConversions = ImageConversions.ShiftToLeastSignificantBit;
            ushort sign = (ushort)((0xffff >> bitsStored) << bitsStored);
            ushort sbit = (ushort)(1 << highBit);
            byte shift = (byte)(highBit - (bitsStored - 1));
            ushort mask = (ushort)(~sign);

            unchecked {
                ushort* pixels = (ushort*)outputPixels;
                int size = newRows * newColumns;

                if (useNoOfFrames && numberOfFrames.HasValue) {
                    size *= numberOfFrames.Value;
                }
                if (pixelRepresentation == 0) {
                    // zero extend
                    for (int i = 0; i < size; ++i) {
                        *pixels = (ushort)(((*pixels) >> shift) & mask);
                        pixels++;
                    }
                } else {
                    // sign extend
                    for (int i = 0; i < size; ++i) {
                        ushort v = (ushort)(((*pixels) >> shift) & mask);
                        if ((v & sbit) != 0) {
                            v |= sign;
                        }
                        *pixels++ = v;
                    }
                }
            }
        }

        /// <summary>
        /// Translates the overlay plane.
        /// </summary>
        private static void TranslateOverlayPlane() {
            // TODO: Not yet implemented, needed?
        }

        /// <summary>
        /// Converts 8 or 16 bit allocated input signed to unsigned. 
        /// Replaces padding value with 0 - can be problematic for further normalization!
        /// </summary>
        private unsafe void ConvertToUnsigned() {
            if (bitsAllocated != 8 && bitsAllocated != 16) {
                return;
            }

            if (pixelRepresentation == 0) {
                // nothing to do for unsigned
                return;
            }

            int n = newRows * newColumns * samplesPerPixel;
            if (useNoOfFrames && numberOfFrames.HasValue) {
                n *= numberOfFrames.Value;
            }
            imageConversions = ImageConversions.SignedToUnsigned;
            if (
                (sopClassUid.Equals(
                WellKnownSopClassUids.CTImageStorage, StringComparison.OrdinalIgnoreCase) ||
                sopClassUid.Equals(
                WellKnownSopClassUids.SecondaryCaptureImageStorage,
                StringComparison.OrdinalIgnoreCase)
                ) &&
                bitsAllocated == 16
            ) {
                //discard values lower than -1024 HU, and map -1024 HU to stored value 0 
                // (if it was mapped to a negative value)
                short lowestStoredValue = 0;
                if (
                    sopClassUid.Equals(
                    WellKnownSopClassUids.CTImageStorage,
                    StringComparison.Ordinal)
                    ) {
                    //discard values lower than -1024 HU, 
                    //and map -1024 HU to stored value 0 (if it was mapped to a negative value)
                    lowestStoredValue = (short)Math.Floor((-1024 - intercept) / slope);
                } else if (
                    sopClassUid.Equals(WellKnownSopClassUids.SecondaryCaptureImageStorage, 
                    StringComparison.Ordinal)) {
                    //Lowest negative value that can be stored in the pixel data, 
                    //inorder to shift it to the position 0.
                    lowestStoredValue = (short)((1 << (bitsStored - 1)) * -1);
                }
                int offset = (lowestStoredValue < 0) ? -lowestStoredValue : 0;
                int threshold = (lowestStoredValue < 0) ? lowestStoredValue : 0;
                myOffset = offset;
                short paddingValue = (short)pixelPaddingValue;
                short paddingRangeValue = (short)pixelPaddingRangeLimitValue;
                short* data = (short*)outputPixels;
                for (int i = 0; i < n; i++) {
                    if (hasPixelPaddingRangeLimitValue) {
                        if (
                            (data[i] >= paddingValue && data[i] >= paddingRangeValue) ||
                            data[i] < threshold
                        ) {
                            imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                            data[i] = 0;
                        }
                    } else if (
                        (hasPixelPaddingValue && data[i] == paddingValue) ||
                        data[i] < threshold
                    ) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else {

                        data[i] = (short)(data[i] + offset);
                    }
                }
                return;
            } else if (
                sopClassUid.Equals(
                WellKnownSopClassUids.MRImageStorage,
                StringComparison.OrdinalIgnoreCase) &&
                bitsAllocated <= 16
            ) {
                short lowestStoredValue = (short)((1 << (bitsStored - 1)) * -1);
                int offset = -lowestStoredValue;
                myOffset = offset;
                short* data = (short*)outputPixels;
                for (int i = 0; i < n; i++) {
                    data[i] = (short)(data[i] + offset);
                }
                return;
            }
            // NOT CT & MR Images
            //clip to 0
            if (bitsAllocated == 8) {
                sbyte paddingValue = (sbyte)pixelPaddingValue;
                sbyte paddingRangeLimitValue = (sbyte)pixelPaddingRangeLimitValue;
                sbyte* data = (sbyte*)outputPixels;
                for (int i = 0; i < n; i++) {
                    if (
                        hasPixelPaddingRangeLimitValue &&
                        (data[i] >= paddingValue && data[i] <= paddingRangeLimitValue)
                    ) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] == paddingValue) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] < 0) {
                        data[i] = 0;
                    }
                }
            } else {
                short paddingValue = (short)pixelPaddingValue;
                short paddingRangeLimitValue = (short)pixelPaddingRangeLimitValue;
                short* data = (short*)outputPixels;
                for (int i = 0; i < n; i++) {
                    if (
                        hasPixelPaddingRangeLimitValue &&
                        (data[i] >= paddingValue && data[i] <= paddingRangeLimitValue)
                    ) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] == paddingValue) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] < 0) {
                        data[i] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Replaces padding value with 0 in an unsigned image
        /// </summary>
        private unsafe void ReplacePaddingValueUnsigned() {
            if (
                (bitsAllocated != 8 && bitsAllocated != 16) ||
                (!hasPixelPaddingValue || pixelRepresentation != 0)
            ) {
                return;
            }
            int n = newRows * newColumns * samplesPerPixel;

            if (useNoOfFrames && numberOfFrames.HasValue) {
                n *= numberOfFrames.Value;
            }
            if (bitsAllocated == 8) {
                byte* data = (byte*)outputPixels;
                byte paddingValue = (byte)pixelPaddingValue;
                byte paddingRangeValue = (byte)pixelPaddingRangeLimitValue;

                for (int i = 0; i < n; i++) {
                    if (
                        hasPixelPaddingRangeLimitValue &&
                        (data[i] >= paddingValue && data[i] <= paddingRangeValue)
                    ) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] == paddingValue) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    }
                }
            } else {
                ushort* data = (ushort*)outputPixels;
                ushort paddingValue = (ushort)pixelPaddingValue;
                ushort paddingRangeValue = (ushort)pixelPaddingRangeLimitValue;
                for (int i = 0; i < n; i++) {
                    if (
                        hasPixelPaddingRangeLimitValue &&
                        (data[i] >= paddingValue && data[i] <= paddingRangeValue)
                    ) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    } else if (data[i] == paddingValue) {
                        imageConversions |= ImageConversions.PixelPaddingValueReplacedWithZero;
                        data[i] = 0;
                    }
                }
            }
        }


        /// <summary>
        /// Sets to canonical.
        /// </summary>
        private void SetToCanonical() {
            if (setToCanonicalCalled) {
                return;
            }
            newSlope = slope;
            newIntercept = intercept;
            if (
                (sopClassUid.Equals(
                WellKnownSopClassUids.CTImageStorage,
                StringComparison.OrdinalIgnoreCase) ||
                sopClassUid.Equals(
                WellKnownSopClassUids.SecondaryCaptureImageStorage, 
                StringComparison.OrdinalIgnoreCase)
                ) &&
                pixelRepresentation != 0 &&
                bitsAllocated == 16
            ) {
                short lowestStoredValue = 0;
                //discard values lower than -1024 HU, 
                //and map -1024 HU to stored value 0 (if it was mapped to a negative value)
                if (
                    sopClassUid.Equals(
                    WellKnownSopClassUids.CTImageStorage,
                    StringComparison.Ordinal)
                    ) {
                    lowestStoredValue = (short)Math.Floor((-1024 - intercept) / slope);
                } else if (
                    sopClassUid.Equals(
                    WellKnownSopClassUids.SecondaryCaptureImageStorage,
                    StringComparison.Ordinal)
                ) {
                    lowestStoredValue = (short)((1 << (bitsStored - 1)) * -1);
                }
                int offset = (lowestStoredValue < 0) ? -lowestStoredValue : 0;
                myOffset = offset;
                imageConversions = ImageConversions.SignedToUnsigned |
                    ImageConversions.RescaleInterceptAdjustedAsPerPixelOffset;
                // real world sequences are not yet taken care
                newIntercept -= slope * myOffset;
                newSlope = slope;
                rescaleUpdated = true;
            } else if (
                sopClassUid.Equals(
                WellKnownSopClassUids.MRImageStorage,
                StringComparison.OrdinalIgnoreCase) &&
                pixelRepresentation != 0 &&
                bitsAllocated <= 16
            ) {
                //LogHelper.LogInfo(
                //    "Original Rescale Intercept:" +
                //    intercept.ToString(CultureInfo.InvariantCulture)
                //);
                int lowestStoredValue = (short)((1 << (bitsStored - 1)) * -1);
                int offset = -lowestStoredValue;
                myOffset = offset;
                newIntercept += lowestStoredValue * slope;
                newSlope = slope;
                imageConversions = ImageConversions.SignedToUnsigned |
                    ImageConversions.RescaleInterceptAdjustedAsPerPixelOffset;
                if (hasPixelPaddingValue) {
                    newPixelPaddingValue = pixelPaddingValue + offset;
                }
                double status = 0;
                if (isDefaultSlopeSet) {
                    if (isDefaultInterceptSet) {
                        status = 3.0;
                    } else {
                        status = 1.0;
                    }
                } else if (isDefaultInterceptSet) {
                    status = 2.0;
                }
                double[] statusVals = { (double)offset, status };
                translationOffsetIntercept = statusVals;
                rescaleUpdated = true;
            }
            // For few sop classes in SopClassUidsToIgnoreForPixelPaddingConversion,
            // pixel-padding value is not replaced with zero
            if (
                hasPixelPaddingValue && 
                pixelPaddingValue != 0 && 
                !SopClassUidsToIgnoreForPixelPaddingConversion.Contains(sopClassUid)
            ) {
                imageConversions = imageConversions 
                    | ImageConversions.PixelPaddingValueReplacedWithZero;
            }
            if (highBit != bitsStored - 1) {
                imageConversions |= ImageConversions.ShiftToLeastSignificantBit;
            }
            if (
                planarConfiguration[0] == 1 &&
                (
                photometricInterpretation.Equals(
                    MemoryManagerConstants.RGB,
                    StringComparison.OrdinalIgnoreCase)
                )
            ) {
                // planar to pixel interleaved conversion
                newPlanarConfiguration = new ReadOnlyArray<ushort>(new ushort[]{ 0 });
                imageConversions |= ImageConversions.PlanarInterleavedToPixelInterleaved;
            } else {
                newPlanarConfiguration = planarConfiguration;
            }
            if (windowCenter != null) {
                newWindowCenter = windowCenter;
                // todo: enable calculation of window center
            }
            newHighBit = bitsStored - 1;
            if (this.translationType != TranslationType.None) {
                newPixelRepresentation = 0;
            }
            setToCanonicalCalled = true;
        }

        #endregion Helper Methods
    }

/// <summary>
/// Translation status
/// </summary>
internal enum ImageTranslationStatus
{
    /// <summary>
    /// Image is OK (no conversion needed)
    /// </summary>
    ShouldNotBeTranslated,

    /// <summary>
    /// Image should be converted by ImageTranlator
    /// </summary>
    ShouldBeTranslated,

    /// <summary>
    /// Image is not supported (ImageTranslator cannot convert it)
    /// Bad images (e.g.missing critical attributes) or we don't know how to convert the image
    /// </summary>
    NotSupported
}

    /// <summary>
    /// Translation Types for unsigned to signed
    /// conversion
    /// </summary>
    public enum TranslationType
    {
        /// <summary>
        /// Converts the signed to unsigned 
        /// </summary>
        Regular = 0x00,
        /// <summary>
        /// Makes the negative values 0
        /// </summary>
        Truncated = 0x01,
        /// <summary>
        /// Does not convert signed to unsgined
        /// </summary>
        None = 0x02
    };
}
