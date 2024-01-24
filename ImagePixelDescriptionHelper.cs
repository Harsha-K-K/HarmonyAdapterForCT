using Philips.Platform.Common;

namespace CTHarmonyAdapters
{
    internal static class ImagePixelDescriptionHelper
    {
        internal static ImagePixelDescription GetImagePixelDescription
            (PlanarConfiguration planarConfiguration, bool isImageConverted,
                ImageConversionInformation imageConversionInformation)
        {
            var imagePixelDescription = new ImagePixelDescription(
                (ushort)imageConversionInformation.Rows,
                (ushort)imageConversionInformation.Columns,
                (ushort)imageConversionInformation.BitsStored,
                (ushort)imageConversionInformation.BitsAllocated,
                (ushort)imageConversionInformation.HighBit,
                imageConversionInformation.PhotometricInterpretation,
                (ushort)imageConversionInformation.SamplesPerPixel,
                planarConfiguration,
                imageConversionInformation.PixelAspectRatio,
                imageConversionInformation.PixelSpacing,
                isImageConverted,
                imageConversionInformation.WindowCenter,
                imageConversionInformation.ImageConversionType,
                imageConversionInformation.Offset,
                imageConversionInformation.PixelRepresentation,
                imageConversionInformation.RescaleIntercept,
                imageConversionInformation.RescaleSlope,
                imageConversionInformation.TransferSyntaxUid,
                imageConversionInformation.PixelImagerSpacing
            );
            return imagePixelDescription;
        }
    }
}