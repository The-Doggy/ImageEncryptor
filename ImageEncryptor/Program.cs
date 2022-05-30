using System;
using System.IO;
using System.Security.Cryptography;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageEncryptor
{
    class Program
    {
        private static Aes aes;
        private static readonly string key = "770A8A65DA156D24EE2A093277530142";
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Usage: ImageEncryptor.exe <bitmap file>");
                return;
            }

            if(!args[0].EndsWith(".bmp"))
            {
                Console.WriteLine($"{Path.GetFileName(args[0])} is not a .bmp file.");
                return;
            }

            using (FileStream fin = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            {
                // The first two bytes of the file header for bitmap images will always be 0x42
                // and 0x4D which translates to BM in ASCII.
                if (fin.ReadByte() != 0x42 || fin.ReadByte() != 0x4D)
                {
                    Console.WriteLine($"{Path.GetFileName(args[0])} is not a valid bitmap image.");
                    return;
                }
            }

            // Initalize the AES object and set the key by converting our key string into a byte array.
            aes = Aes.Create();
            aes.Key = Convert.FromHexString(key);

            // Encrypt and write the data from our bitmap image as a jpeg image, using a
            // different cipher mode each time.
            EncryptBitmapToJpeg(args[0], CipherMode.ECB);
            EncryptBitmapToJpeg(args[0], CipherMode.CBC);
            EncryptBitmapToJpeg(args[0], CipherMode.CFB);
        }

        private static void EncryptBitmapToJpeg(string inputFile, CipherMode mode)
        {
            // THIS IS ALL BROKEN STUFF THAT DOESNT WORK BUT IS STILL HERE FOR POSTERITY AND BECAUSE IM EXTREMELY DUMB AND WILL PROBABLY FORGET THAT THIS STUFF DOESNT WORK IN 2 DAYS
            /*            // Create the file streams to handle the input and output files.
                        FileStream fin = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
                        FileStream fout = new FileStream($"output_{mode}.jpg", FileMode.OpenOrCreate, FileAccess.Write);
                        fout.SetLength(0);

                        // Create variables to help with read and write.
                        byte[] bin = new byte[16];   // This is intermediate storage for the encryption.
                        long rdlen = 0;              // This is the total number of bytes written.
                        long totlen = fin.Length;    // This is the total length of the input file.
                        int len;                     // This is the number of bytes to be written at a time.

                        CryptoStream encStream = new CryptoStream(fout, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write);

                        Console.WriteLine($"Encrypting file {Path.GetFileName(fin.Name)}...");

                        // Read from the input file, then encrypt and write to the output file.
                        while (rdlen < totlen)
                        {
                            len = fin.Read(bin, 0, 16);
                            encStream.Write(bin, 0, len);
                            rdlen += len;
                            Console.WriteLine("{0} bytes processed", rdlen);
                        }

                        encStream.Close();
                        fout.Close();
                        fin.Close();*/

            /*            FileStream fin = new FileStream(inputFile, FileMode.Open, FileAccess.ReadWrite);
                        byte[] imageOffset = new byte[4];
                        fin.Position = 10;
                        fin.Read(imageOffset, 0, 4);
                        fin.Position = BitConverter.ToInt32(imageOffset);*/

            /*            byte[] imageData;
                        Bitmap img = new Bitmap(inputFile);
                        using (MemoryStream from = new MemoryStream())
                        {
                            img.Save(from, ImageFormat.Bmp);
                            using (MemoryStream to = new MemoryStream())
                            {
                                using (CryptoStream cs = new CryptoStream(to, aes.CreateEncryptor(), CryptoStreamMode.Write))
                                {
                                    from.Position = 0;
                                    byte[] buffer = new byte[16];
                                    long curLen = 0;
                                    long maxLen = from.Length;
                                    int len = 0;

                                    Console.WriteLine($"Encrypting file {Path.GetFileName(inputFile)}...");

                                    from.Position = 10;
                                    from.Read(buffer, 0, 4); // READ OFFSET FOR STARTING ADDRESS OF PIXEL ARRAY
                                    from.Position = BitConverter.ToInt32(buffer); // SET POSITION TO STARTING ADDRESS OF PIXEL ARRAY
                                    maxLen -= from.Position;

                                    while (curLen < maxLen)
                                    {
                                        len = from.Read(buffer, 0, 16);
                                        cs.Write(buffer, 0, len);
                                        curLen += len;

                                        Console.WriteLine($"Processing byte {curLen}");
                                    }

                                    to.Position = 0;
                                    Image aaa = Image.Load(to, new BmpDecoder());
                                    aaa.SaveAsJpeg($"output_{mode}.jpeg", new JpegEncoder());
                                }

                            }
                        }*/

            // Set the CipherMode to the mode passed into the method and create
            // the encryptor object
            aes.Mode = mode;
            ICryptoTransform enc = aes.CreateEncryptor();

            // Load the image from our file path using Rgba32 pixel format
            using Image<Rgba32> image = Image.Load<Rgba32>(inputFile);

            // We can use this method to iterate over every pixel row and subsequently
            // every pixel
            image.ProcessPixelRows(accessor =>
            {
                // Iterate over every pixel row
                for (int y = 0; y < accessor.Height; y++)
                {
                    // Get the current row from accessor
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    // We can use a foreach loop to iterate over a reference of each pixel
                    // in the row and write the encrypted pixel data to a MemoryStream
                    foreach (ref Rgba32 pixel in pixelRow)
                    {
                        // Setting up the streams we need to encrypt our pixel data
                        using MemoryStream ms = new MemoryStream();
                        using CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write);
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            // This encrypts and writes the pixel data to our MemoryStream object
                            sw.Write(pixel);
                        }

                        // Get the encrypted data from the MemoryStream as a byte array and then
                        // create a new Rgba32 object using the first 4 bytes of the array as RGBA
                        // values and assign it to the pixel reference
                        byte[] encPixel = ms.ToArray();
                        pixel = new Rgba32(encPixel[0], encPixel[1], encPixel[2], encPixel[3]);
                    }
                }
            });

            // Save the encrypted image as a new JPEG image file
            image.SaveAsJpeg($"output_{mode}.jpeg", new JpegEncoder());
        }
    }
}
