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

        public CryptoHandler() { }

        internal void initProgram()
        {
            try
            {
                cspp = new CspParameters();
                cspp.Flags |= CspProviderFlags.UseMachineKeyStore; //set CspProvider to use Machine Key Container, not User Key Container

                cspp.KeyContainerName = securyptKeyName;
                rsa = new RSACryptoServiceProvider(cspp);
                rsa.PersistKeyInCsp = true;

                if (doesKeyExist(securyptKeyName))
                {
                    Console.WriteLine("Keys found... Importing...");
                    Console.WriteLine("Key pair retrieved from container : {0}", rsa.ToXmlString(true));
                }
                else
                {
                    Console.WriteLine("No Securypt keys found");

                    //Console.WriteLine("Key pair generated and stored in container from container : {0}", rsa.ToXmlString(true));
                }

                //tests the RSA algorithm by encrypting a short string, printing the bytes, and decrypting the bytes
                //testRSAEncrypt(textToEncrypt);

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

        internal void archiveFiles()
        {
            //check if file is already archived
            //if not, open archive file
            //add files to archive and close file
        }

        //simple function to return Public Key XML string
        public string PublicKeyToString()
        {
            return rsa.ToXmlString(false); //false returns only Public Key
        }

        //example from https://docs.microsoft.com/en-us/dotnet/standard/security/walkthrough-creating-a-cryptographic-application
        internal void DecryptFile(string inFile)
        {
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
            string outFile = inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

            // Use FileStream objects to read the encrypted
            // file (inFs) and save the decrypted file (outFs).
            using (FileStream inFs = new FileStream(inFile, FileMode.Open))
            {

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
                // from the FileSteam of the encrypted
                // file (inFs) into the FileStream
                // for the decrypted file (outFs).
                using (FileStream outFs = new FileStream(outFile, FileMode.Create))
                {

                    int count = 0;
                    int offset = 0;

                    // blockSizeBytes can be any arbitrary size.
                    int blockSizeBytes = rjndl.BlockSize / 8;
                    byte[] data = new byte[blockSizeBytes];


                    // By decrypting a chunk a time,
                    // you can save memory and
                    // accommodate large files.

                    // Start at the beginning
                    // of the cipher text.
                    inFs.Seek(startC, SeekOrigin.Begin);
                    using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
                    {
                        do
                        {
                            count = inFs.Read(data, 0, blockSizeBytes);
                            offset += count;
                            outStreamDecrypted.Write(data, 0, count);

                        }
                        while (count > 0);

                        outStreamDecrypted.FlushFinalBlock();
                        outStreamDecrypted.Close();
                    }
                    outFs.Close();
                }
                inFs.Close();
            }
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

        internal void testRSAEncrypt(string stringToEncrypt)
        {
            try
            {
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                Console.WriteLine("String to encrypt: " + stringToEncrypt);

                byte[] bytesToEncrypt = ByteConverter.GetBytes(stringToEncrypt);    //gets bytes from string
                byte[] encryptedBytes = rsa.Encrypt(bytesToEncrypt, false);         //encrypts bytes with RSA
                Console.WriteLine("Encrypted Bytes: " + Convert.ToBase64String(encryptedBytes));

                byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, false);         //decrypts bytes with RSA 
                Console.WriteLine("Decrypted Bytes: " + Convert.ToBase64String(decryptedBytes));

                String decryptedText = ByteConverter.GetString(decryptedBytes);     //decodes bytes in Unicode
                Console.WriteLine("Decrypted text: " + decryptedText);              //prints resulting text
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /*
        //opens file for EncryptFile(string inFile)
        internal void EncryptFileDialog()
        {
            if (rsa == null) ;
            //MessageBox.Show("Key not set.");
            else
            {
                // Display a dialog box to select a file to encrypt.
                OpenFileDialog dlg = new OpenFileDialog();
                //dlg.InitialDirectory = SrcFolder;

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string fName = dlg.FileName;
                    if (fName != null)
                    {
                        FileInfo fInfo = new FileInfo(fName);
                        // Pass the file name without the path.
                        string name = fInfo.FullName;
                        EncryptFile(name);
                    }
                }
                else return; //file not selected, so return to page
                
            }
        }
        */

        /*
        //helper for DecryptFile(string inFile)
        internal void DecryptFileDialog()
        {
            
            if (rsa == null)
                MessageBox.Show("Key not set.");
            else
            {
                // Display a dialog box to select the encrypted file.
                OpenFileDialog dlg = new OpenFileDialog();
                //dlg.InitialDirectory = EncrFolder;

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string fName = dlg.FileName;
                    if (fName != null)
                    {
                        FileInfo fi = new FileInfo(fName);
                        string name = fi.Name;
                        DecryptFile(name);
                    }
                }
            }
        }
        */
    }
}
