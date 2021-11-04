using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;

namespace QRCopyPaste
{
    public static class QRMessageScannerHelper
    {
        private static readonly int _screenWidth = (int)SystemParameters.VirtualScreenWidth;
        private static readonly int _screenHeight = (int)SystemParameters.VirtualScreenHeight;
        private static readonly Bitmap _bitmap = new Bitmap(_screenWidth, _screenHeight);


        public static Result GetBarcodeResultFromQRBitmap(Bitmap bitmap)
        {
            var luminanceSource = new BitmapSourceLuminanceSource(CreateBitmapSourceFromBitmap(bitmap));

            var barcodeReader = new BarcodeReader();
            var barcodeResult = barcodeReader.Decode(luminanceSource);
            return barcodeResult;
        }


        public static Bitmap CreateBitmapFromScreen()
        {
            using (var g = Graphics.FromImage(_bitmap))
            {
                g.CopyFromScreen(
                    0, 0, 0, 0,
                    _bitmap.Size,
                    CopyPixelOperation.SourceCopy
                );
            }
            return _bitmap;
        }


        private static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            try
            {
                const int bytesPerPixel = 4;
                var bufferSize = (rect.Width * rect.Height) * bytesPerPixel;

                var bitmapSource = BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    bufferSize,
                    bitmapData.Stride
                );
                return bitmapSource;
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
