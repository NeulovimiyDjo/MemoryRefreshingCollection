       

        public static BitmapSource CreateBitmapSourceFromPixelValuesArray(byte[] pixelValuesArray, int width, int height)
        {
            const int bytesPerPixel = 4;

            if (pixelValuesArray.Count() != width * height * bytesPerPixel)
                throw new Exception($"{nameof(pixelValuesArray)} size doesn't match width * height.");


            float dpiX, dpiY;
            using (Graphics graphics = Graphics.FromHwnd(IntPtr.Zero))
            {
                dpiX = graphics.DpiX;
                dpiY = graphics.DpiY;
            }


            var bitmapSource = BitmapSource.Create(
                width,
                height,
                dpiX,
                dpiY,
                PixelFormats.Bgra32,
                null,
                pixelValuesArray,
                width * bytesPerPixel// + width * bytesPerPixel % 4 == 0 ? 0 : (4 - width * bytesPerPixel % 4)
            );
            return bitmapSource;
        }
		
		
		
		

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
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





        private static Bitmap WritableBitmapToBitmap(WriteableBitmap writableBitmap)
        {
            Bitmap bitmap;
            using (MemoryStream ms = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();

                var frame = BitmapFrame.Create(writableBitmap);
                encoder.Frames.Add(frame);
                encoder.Save(ms);

                bitmap = new Bitmap(ms);
            }
            return bitmap;
        }



        private static readonly int _screenWidth = (int)SystemParameters.VirtualScreenWidth;
        private static readonly int _screenHeight = (int)SystemParameters.VirtualScreenHeight;
        private static readonly Bitmap _bitmap = new Bitmap(_screenWidth, _screenHeight);

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
		
		