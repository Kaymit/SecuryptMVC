using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace SecuryptMVC.Utility
{
    /// <summary>
    /// Utility class responsible for using the RSA and AES algorithms and related tasks
    /// </summary>
    public class CryptoHandler
    {
        internal string securyptKeyName = "Securypt";

        public string textToEncrypt = "This text will now be encrypted";
        public string textDecrypted;

        internal CspParameters cspp;
        internal RSACryptoServiceProvider rsa;

        //default constructor
        public CryptoHandler() { }

        /// <summary>
        /// Initializes CryptoHandler, either gets or creates Securypt RSA Keypair, and prints info
        /// </summary>
        internal void RegisterKeys()
        {
            try
            {
                cspp = new CspParameters();
                //cspp.Flags |= CspProviderFlags.UseMachineKeyStore; //set CspProvider to use Machine Key Container, not User Key Container

                cspp.KeyContainerName = securyptKeyName;
                rsa = new RSACryptoServiceProvider(cspp);
                rsa.PersistKeyInCsp = true;

                #region Debugging: check if key exists and print result
                if (doesKeyExist(securyptKeyName))
                {
                    Console.WriteLine("Keys found... Importing...");
                    Console.WriteLine("Key pair retrieved from container : {0}", rsa.ToXmlString(true));
                }
                else
                {
                    Console.WriteLine("No Securypt keys found");
                }
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        //example from https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        internal void EncryptFile(string inFile)
        {
            RijndaelManaged rjndl = new RijndaelManaged
            {
                KeySize = 256,
                BlockSize = 256,
                Mode = CipherMode.CBC
            };
            ICryptoTransform transform = rjndl.CreateEncryptor();

            //encrypt the AES key used to encrypt the file, and append it to the start of the encrypted file
            byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

            // Create byte arrays to contain
            // the length values of the key and IV.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            int lKey = keyEncrypted.Length;
            LenK = BitConverter.GetBytes(lKey);
            int lIV = rjndl.IV.Length;
            LenIV = BitConverter.GetBytes(lIV);


            int startFileName = inFile.LastIndexOf("\\") + 1;

            //old method for saving file name //string outFile = inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";
            // Change the file's extension to ".enc"
            string outFile;
            if (!inFile.ToLower().Contains('.'))
                outFile = inFile + ".enc";
            else
                outFile = inFile.Substring(0, inFile.LastIndexOf(".")) + ".enc";
            

            // Write the following to the FileStream
            // for the encrypted file (outFs):
            // - length of the key
            // - length of the IV
            // - encrypted key
            // - the IV
            // - the encrypted cipher content
            using (FileStream outFs = new FileStream(outFile, FileMode.Create))
            {
                //write header for encrypted file
                outFs.Write(LenK, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(keyEncrypted, 0, lKey);
                outFs.Write(rjndl.IV, 0, lIV);

                // Now write the cipher text using
                // a CryptoStream for encrypting.
                using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                {

                    // By encrypting a chunk at
                    // a time, you can save memory
                    // and accommodate large files.
                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];
                    int bytesRead = 0;

                    using (FileStream inFs = new FileStream(inFile, FileMode.Open))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamEncrypted.Write(data, 0, count);
                            bytesRead += blockSizeBytes;
                        }
                        while (count > 0);
                        inFs.Close();
                    }
                    outStreamEncrypted.FlushFinalBlock();
                    outStreamEncrypted.Close();
                }
                outFs.Close();
            }
        }

        //example from https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        internal MemoryStream DecryptFile(string inFileWithExtension)
        {
            // Get file extension from inFile string 
            // (which is the storage path, but with original file extension)
            string inFile;
            if (!inFileWithExtension.ToLower().Contains('.'))
                inFile = inFileWithExtension + ".enc";
            else
                inFile = inFileWithExtension.Substring(0, inFileWithExtension.LastIndexOf(".")) + ".enc";

            // Create instance of Rijndael for
            // symetric decryption of the data.
            RijndaelManaged rjndl = new RijndaelManaged();
            rjndl.KeySize = 256;
            rjndl.BlockSize = 256;
            rjndl.Mode = CipherMode.CBC;

            // Create byte arrays to get the length of
            // the encrypted key and IV.
            // These values were stored as 4 bytes each
            // at the beginning of the encrypted package.
            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];

            // Consruct the file name for the decrypted file.
            //string outFile = inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt"; //*******TODO change returned file type to correct type**************

            // Use FileStream objects to read the encrypted
            // file (inFs) and save the decrypted file (outFs).
            FileStream inFs = new FileStream(inFile, FileMode.Open);
            inFs.Seek(0, SeekOrigin.Begin);
            inFs.Seek(0, SeekOrigin.Begin);
            inFs.Read(LenK, 0, 3);
            inFs.Seek(4, SeekOrigin.Begin);
            inFs.Read(LenIV, 0, 3);

            // Convert the lengths to integer values.
            int lenK = BitConverter.ToInt32(LenK, 0);
            int lenIV = BitConverter.ToInt32(LenIV, 0);

            // Determine the start postition of
            // the ciphter text (startC)
            // and its length(lenC).
            int startC = lenK + lenIV + 8;
            int lenC = (int)inFs.Length - startC;

            // Create the byte arrays for
            // the encrypted Rijndael key,
            // the IV, and the cipher text.
            byte[] KeyEncrypted = new byte[lenK];
            byte[] IV = new byte[lenIV];

            // Extract the key and IV
            // starting from index 8
            // after the length values.
            inFs.Seek(8, SeekOrigin.Begin);
            inFs.Read(KeyEncrypted, 0, lenK);
            inFs.Seek(8 + lenK, SeekOrigin.Begin);
            inFs.Read(IV, 0, lenIV);
            //Directory.CreateDirectory(DecrFolder);

            // Use RSACryptoServiceProvider
            // to decrypt the Rijndael key.
            byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

            // Decrypt the key.
            ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

            // Decrypt the cipher text from
            // from the FileStream of the encrypted
            // file (inFs) into the MemoryStream
            // for the decrypted item (outStream).
            MemoryStream outStream = new MemoryStream();
            int count = 0;
            int offset = 0;

            // blockSizeBytes can be any arbitrary size.
            int blockSizeBytes = rjndl.BlockSize / 8;
            byte[] data = new byte[blockSizeBytes];

            // Start at the beginning
            // of the cipher text.
            inFs.Seek(startC, SeekOrigin.Begin);
            CryptoStream outStreamDecrypted = new CryptoStream(outStream, transform, CryptoStreamMode.Write);
                    
            do
            {
                count = inFs.Read(data, 0, blockSizeBytes);
                offset += count;
                outStreamDecrypted.Write(data, 0, count);
            }
            while (count > 0);

            //unwrapped streams so outStream doesn't get disposed of before 
            //EncryptedItemController.Download(int id) returns file
            //https://stackoverflow.com/questions/10934585/memorystream-cannot-access-a-closed-stream
            outStreamDecrypted.FlushFinalBlock();
            outStream.Position = 0;
            return outStream;
        }

        //https://stackoverflow.com/questions/9995839/how-to-make-random-string-of-numbers-and-letters-with-a-length-of-5
        public string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                Random r = new Random();
                var c = pool[r.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }

        //simple function to return Public Key XML string
        public string PublicKeyToString()
        {
            return rsa.ToXmlString(false); //false returns only Public Key
        }


        //https://docs.microsoft.com/en-us/dotnet/standard/security/how-to-store-asymmetric-keys-in-a-key-container
        internal static void DeleteKeyFromContainer(string ContainerName)
        {
            CspParameters cp = new CspParameters();
            cp.KeyContainerName = ContainerName;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cp);

            rsa.PersistKeyInCsp = false; //doesn't save 

            rsa.Clear();

            Console.WriteLine("Key deleted.");
        }

        //https://stackoverflow.com/questions/17640055/c-sharp-rsacryptoserviceprovider-how-to-check-if-a-key-already-exists-in-contai
        internal static bool doesKeyExist(string containerName)
        {
            var cspParams = new CspParameters
            {
                Flags = CspProviderFlags.UseExistingKey,
                KeyContainerName = containerName
            };

            try
            {
                var provider = new RSACryptoServiceProvider(cspParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }
            return true;
        }
    }
}
