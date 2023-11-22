using Philips.Platform.ApplicationIntegration.DataAccess;
using Philips.Platform.Common;
using System;

namespace CTHarmonyAdapters
{
    internal class PixelDataImpl : PixelData
    {
        private BulkDataReference pixelDataReference;
        private ImagePixelDescription imagePixelDescription;
        private DicomObject dcm;

        public PixelDataImpl(DicomObject dcm)
        {
            if (dcm != null)
            {
                pixelDataReference =
                    dcm.GetBulkDataReference(DicomDictionary.DicomPixelData);
            }
        }

        public override void LoadPixels()
        {
            throw new NotImplementedException();
        }

        public override void LoadAndLockPixels()
        {
            throw new NotImplementedException();
        }

        public override int Unlock()
        {
            throw new NotImplementedException();
        }

        public override void MarkForCleanup()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public override void Lock()
        {
            throw new NotImplementedException();
        }

        public override int Size { get; }

        public override ImagePixelDescription Description
        {
            get
            {
                if (imagePixelDescription == null)
                {

                    //        imageConversionInformation =
                    //            new ImageConversionInformation(imageDicomObject);


                    //PlanarConfiguration planarConfig =
                    //    GetPlanarConfiguration(imageConversionInformation.PlanarConfiguration);
                    //bool isImageConverted =
                    //    (imageConversionInformation.ImageConversionType != ImageConversions.None);
                    //imagePixelDescription = ImagePixelDescriptionHelper.GetImagePixelDescription(
                    //    planarConfig,
                    //    isImageConverted,
                    //    imageConversionInformation);
                }
                return imagePixelDescription;
            }
        }

        public override IntPtr Pixels { get; }
    }
}
