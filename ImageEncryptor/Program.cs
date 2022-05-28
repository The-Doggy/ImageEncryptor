using System;
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

            // Create the file streams to handle the input and output files.
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
            fin.Close();
        }
    }
}
