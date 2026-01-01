
            var bytes = File.ReadAllBytes("123");

            Directory.CreateDirectory("tmp");
            File.Copy("123", "tmp/123");
            System.IO.Compression.ZipFile.CreateFromDirectory("tmp", "1.zip", System.IO.Compression.CompressionLevel.Optimal, false);


            GZipRead();
            GZipWrite();

            static void GZipRead()
            {
                using (Stream fs = File.OpenRead("gj.txt"))
                using (Stream fd = File.Create("gj.zip"))
                using (Stream csStream = new System.IO.Compression.GZipStream(fd, System.IO.Compression.CompressionMode.Compress))
                {
                    byte[] buffer = new byte[1024];
                    int nRead;
                    while ((nRead = fs.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        csStream.Write(buffer, 0, nRead);
                    }
                }
            }

            static void GZipWrite()
            {
                using (Stream fd = File.Create("gj.new.txt"))
                using (Stream fs = File.OpenRead("gj.zip"))
                using (Stream csStream = new System.IO.Compression.GZipStream(fs, System.IO.Compression.CompressionMode.Decompress))
                {
                    byte[] buffer = new byte[1024];
                    int nRead;
                    while ((nRead = csStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fd.Write(buffer, 0, nRead);
                    }
                }
            }


            ICSharpCode.SharpZipLib.GZip.GZip.Compress(File.OpenRead("123"), File.Create("x.gzip"), true, 512, 9);
            ICSharpCode.SharpZipLib.GZip.GZip.Decompress(File.OpenRead("x.gzip"), File.Create("123_new"), true);



            {
                using var zipOutputStream = new ICSharpCode.SharpZipLib.Zip.ZipOutputStream(File.Create("x.zip"));
                using var sourceFileStream = File.OpenRead("123");
                zipOutputStream.SetLevel(9);
                var entry = new ICSharpCode.SharpZipLib.Zip.ZipEntry(Path.GetFileName("x.zip"));
                entry.DateTime = DateTime.Now;
                zipOutputStream.PutNextEntry(entry);
                byte[] buffer = new byte[4096];
                int sourceBytes;
                do
                {
                    sourceBytes = sourceFileStream.Read(buffer, 0, buffer.Length);
                    zipOutputStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }


            {
                using var zipInputStream = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead("x.zip"));
                using var targetFileStream = File.Create("123_new_zip");
                var theEntry = zipInputStream.GetNextEntry();
                byte[] data = new byte[4096];
                int size = zipInputStream.Read(data, 0, data.Length);
                while (size > 0)
                {
                    targetFileStream.Write(data, 0, size);
                    size = zipInputStream.Read(data, 0, data.Length);
                }
            }



            {
                using var zip = new Ionic.Zip.ZipFile();
                zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level9;
                zip.AddFile("123");
                zip.Save("x_dn.zip");
            }

            {
                var fs = File.Create("x_dn_unzipped");
                var zip = Ionic.Zip.ZipFile.Read("x_dn.zip");
                foreach (Ionic.Zip.ZipEntry entry in zip)
                {
                    if (entry.FileName == "123")
                        entry.Extract(fs);
                }
            }