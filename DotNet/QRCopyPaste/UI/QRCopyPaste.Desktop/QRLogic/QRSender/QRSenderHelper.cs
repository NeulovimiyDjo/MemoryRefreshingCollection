using System.Windows.Media.Imaging;
using ZXing;
using ZXing.Presentation;
using ZXing.QrCode.Internal;

namespace QRCopyPaste
{
    public static class QRSenderHelper
    {
        public static WriteableBitmap CreateQRWritableBitampFromString(string data)
        {
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ErrorCorrectionLevel.L);

            var writableBitmap = barcodeWriter.Write(data);
            return writableBitmap;
        }
    }
}
