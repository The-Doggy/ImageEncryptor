using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using SixLabors.ImageSharp;

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
            aes.Mode = mode;

            byte[] imageData;
            Bitmap img = new Bitmap(inputFile);
            using (MemoryStream from = new MemoryStream())
            {
                img.Save(from, ImageFormat.Bmp);
                using (MemoryStream to = new MemoryStream())
                {

                }
            }

            
        }
    }
}
