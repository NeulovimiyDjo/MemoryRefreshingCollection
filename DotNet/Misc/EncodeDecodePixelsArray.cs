
        public static readonly byte[] pattern = new byte[] { 1, 2, 3, 4 };
        // Gets the index after the first occurance of a pattern in array.
        private static int GetDataStartIndex(byte[] valuesArray)
        {
            int matchedCount = 0;
            for (int i = 0; i < valuesArray.Length; i++)
            {
                byte val = valuesArray[i];

                // Successfully matched values in a row count is also the index of next value to match in a pattern.
                byte nextValueToMatchInAPattern = pattern[matchedCount];

                if (val == nextValueToMatchInAPattern)
                    matchedCount++; // Increase successfully matched values in a row count.
                else
                    matchedCount = 0; // Reset successfully matched values in a row count.

                if (matchedCount == pattern.Length)
                    return i; // Data starts with pattern. Return actual data start index without pattern.
            }

            return -1;
        }

        public static Result DecodeDataFromScreenPixelsBitmap(Bitmap bitmap)
        {
            var bitmapSource = BitmapHelper.CreateBitmapSourceFromBitmap(bitmap);

            var pixelsArray = new byte[bitmapSource.PixelWidth * bitmapSource.PixelHeight * 4];
            bitmapSource.CopyPixels(pixelsArray, bitmapSource.PixelWidth * 4, 0);

            var valuesArray = pixelsArray;

            int strideSize = 512 * 4;
            int dataStartIndex = GetDataStartIndex(valuesArray);
            if (dataStartIndex == -1)
                return null; // No data rect detected.

            int dataStartIndexColumn = dataStartIndex % strideSize;
            int dataEndIndexColumn = dataStartIndexColumn + 512 * 4;
            int dataStartIndexRow = dataStartIndex / strideSize;
            int dataEndIndexRow = dataStartIndexRow + 512 * 4;


            var dataLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(valuesArray, 0));
            int dataLengthBytesLength = 4;
            var sb = new StringBuilder();
            for (int i = dataStartIndex + dataLengthBytesLength; i < dataStartIndex + 512 * 512 * 4; i++)
            {
                int column = i % strideSize;
                int row = i / strideSize;
                if (column < dataStartIndexColumn || column > dataEndIndexColumn
                    || row < dataStartIndexRow || row > dataEndIndexRow)
                    continue; // Out of data rect bounds.

                byte val = valuesArray[i];

                char c = (char)val;
                sb.Append(c);

                if (sb.Length == dataLength)
                    break;
            }
            string data = sb.ToString();

            return new Result(data, new byte[0], new ResultPoint[0], BarcodeFormat.UPC_EAN_EXTENSION); // TODO: Change to returning just data string.
        }






        private static readonly byte[] pattern = QRMessageScannerHelper.pattern;
        public static BitmapSource EncodeDataThroughScreenPixelsToBitmapSource(string data)
        {
            var dataLengthBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
            int dataLengthBytesLength = 4;

            int patternBytesIndex = 0;
            int dataLengthBytesIndex = 0;

            var pixelValuesArray = new byte[512*512*4];
            for (int i = 0; i < pixelValuesArray.Length; i++)
            {
                byte val;
                if (i < pattern.Length)
                    val = pattern[patternBytesIndex++]; // Prepend pattern.
                else if (i < pattern.Length + dataLengthBytesLength)
                    val = dataLengthBytes[dataLengthBytesIndex++]; // Prepend data length.
                else if (i < pattern.Length + dataLengthBytesLength + data.Length)
                    val = (byte)data[i - pattern.Length - dataLengthBytesLength]; // Write data after pattern.
                else
                    val = (byte)(i % 4 == 3 ? 225 : i % 8 < 4 ? 0 : 255);

                pixelValuesArray[i] = val;
            }

            var writableBitmap = BitmapHelper.CreateBitmapSourceFromPixelValuesArray(pixelValuesArray, 512, 512);
            return writableBitmap;
        }
		
		


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
		
