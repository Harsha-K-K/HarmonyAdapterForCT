using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace CTHarmonyAdapters
{
    internal class PixelDataImpl : PixelData
    {
        private GCHandle pinnedArray;
        private BulkDataReference pixelDataReference;
        private ImagePixelDescription imagePixelDescription;
        private DicomObject dcm;
        private static readonly HashSet<string> unConvertedPhotoMetric =
            new HashSet<string> {
                "YBR_FULL",
                "YBR_FULL_422",
                "YBR_PARTIAL_422",
                "YBR_RCT",
                "YBR_ICT"
            };
        private static readonly DictionaryTag[] mandatoryPixelModuleAttributes =
            new[] {
                DicomDictionary.DicomBitsAllocated,
                DicomDictionary.DicomBitsStored,
                DicomDictionary.DicomHighBit,
                DicomDictionary.DicomPhotometricInterpretation,
                DicomDictionary.DicomPixelRepresentation,
                DicomDictionary.DicomRows,
                DicomDictionary.DicomColumns,
                DicomDictionary.DicomSamplesPerPixel
            };
        private int bulkSize;
        private IntPtr pixels = IntPtr.Zero;
        private int addingcount;
        private readonly DictionaryTag _pixelDataTag = new DictionaryTag(DicomDictionary.DicomPixelData.Tag, DicomVR.OW,
            DicomDictionary.DicomPixelData.ValueMultiplicity, DicomDictionary.DicomPixelData.Name,
            DicomDictionary.DicomPixelData.ImplementerId);

        static readonly object syncObj = new object();

        public PixelDataImpl(DicomObject dicomObject)
        {
            if (dicomObject != null)
            {
                dcm = dicomObject;
                pixelDataReference =
                    dicomObject.GetBulkDataReference(_pixelDataTag);
            }
        }

        public override void LoadPixels()
        {
            LoadAndLockPixels();
        }

        public override void LoadAndLockPixels()
        {
            lock (syncObj)
            {
                var pixelArray = GetPixelData(dcm);
                pinnedArray = GCHandle.Alloc(pixelArray, GCHandleType.Pinned);
                pixels = pinnedArray.AddrOfPinnedObject();
            }
        }

        public override int Unlock()
        {
            addingcount--;
            return Math.Max(addingcount,0);
        }

        public override void MarkForCleanup()
        {
            //pinnedArray.Free();
        }

        protected override void Dispose(bool disposing)
        {

            pinnedArray.Free();
            pixels = IntPtr.Zero;
        }

        public override void Lock()
        {
            addingcount++;
        }

        /// <summary>
        /// Returns true if the PixelData has pixels, false otherwise
        /// </summary>
        internal bool HasPixels
        {
            get
            {
                if (dcm == null)
                {
                    return false;
                }
                foreach (DictionaryTag mandatoryTag in mandatoryPixelModuleAttributes)
                {
                    if (!dcm.HasTag(mandatoryTag))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override int Size
        {
            get
            {
                if (HasPixels && bulkSize == 0)
                {
                    bulkSize =
                        Description.Rows *
                        Description.Columns *
                        ((Description.BitsAllocated + 7) / 8) *
                        Description.SamplesPerPixel;
                }
                return bulkSize;
            }
        }

        public override ImagePixelDescription Description
        {
            get
            {
                ImageConversionInformation imageConversionInformation;
                if (imagePixelDescription == null)
                {
                    imageConversionInformation =
                                new ImageConversionInformation(dcm);
                    var imageTranslator =
                        new ImageTranslator(
                            imageConversionInformation, dcm, true);
                    imageConversionInformation =
                        imageTranslator.GetModifiedImageConversionInformation();
                    if (
                        unConvertedPhotoMetric.Contains(
                        imageConversionInformation.PhotometricInterpretation)
                    )
                    {
                        imageConversionInformation.PhotometricInterpretation = "RGB";
                    }
                    PlanarConfiguration planarConfig =
                        GetPlanarConfiguration(imageConversionInformation.PlanarConfiguration);
                    bool isImageConverted =
                        (imageConversionInformation.ImageConversionType != ImageConversions.None);
                    imagePixelDescription = ImagePixelDescriptionHelper.GetImagePixelDescription(
                        planarConfig,
                        isImageConverted,
                        imageConversionInformation);

                    StringBuilder writer = new StringBuilder();
                    PropertyInfo[] props = imagePixelDescription.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        object propValue = prop.GetValue(imagePixelDescription, null);
                        writer.AppendLine($"{prop.Name} : {propValue}");
                    }
                    writer.AppendLine($"Pixel Size = {Size}");
                    //File.AppendAllText($@"D:\MyLogs\PixelDataMine\log.txt", writer.ToString());
                }
                return imagePixelDescription;
            }
        }

        /// <summary>
        /// Gets the planar configuration.
        /// </summary>
        /// <param name="planarConfiguration">The planar configuration.</param>
        /// <returns>PlanarConfiguration</returns>
        private static PlanarConfiguration GetPlanarConfiguration(
            ReadOnlyArray<ushort> planarConfiguration
        )
        {
            PlanarConfiguration planarConfig = PlanarConfiguration.PixelInterleaved;
            if (
                planarConfiguration.Count > 0 &&
                planarConfiguration[0] == 1
            )
            {
                planarConfig = PlanarConfiguration.PlanarInterleaved;
            }
            return planarConfig;
        }

        public override IntPtr Pixels
        {
            get { return pixels; }
        }

        private byte[] GetPixelData(DicomObject dicomFileObject)
        {
            byte[] pixelDataBuffer = null;
            if (dicomFileObject != null)
            {
                using (var pixelDataStream = dicomFileObject.GetBulkData(_pixelDataTag))
                {
                    byte[] buffer = new byte[pixelDataStream.Length];
                    var read = pixelDataStream.Read(buffer, 0, buffer.Length);
                    pixelDataBuffer = buffer;
                }
            }
            return pixelDataBuffer;
        }
    }
}
