using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace FullyFreeSteganography
{
    // Informs UI providers of the RSA Key status.
    enum KeyPairStatus
    {
        NOT_LOADED,
        PUBLIC,
        PUBLIC_AND_PRIVATE
    }

    /// <summary>
    /// Called CryptoHandler, a more appropriate name for the CryptoHandler may have been RSACryptoHandler, 
    /// but for future development I may choose to implement symmetrical encryption like AES encryption in which case Cryptohandler would be fitting and so, CryptoHandler it is.
    /// CryptoHandler in its current state, is windows specific mostly due to the way directory strings are formatted, this is something that may change in the future.
    /// </summary>
    class CryptoHandler
    {
        RSACryptoServiceProvider rSACryptoServiceProvider;
        string rSAKeyExtPublic = "/PUBLIC_RSA_Key.xml";
        string rSAKeyExtPublicAndPrivate = "/PUBLIC_AND_PRIVATE_RSA_Keys.xml";
        string encryptedFileExt = ".encrypted";
        public KeyPairStatus keyPairStatus = KeyPairStatus.NOT_LOADED;

        /// <summary>
        /// Start RSA encryption.
        /// 'fileToEncrypt' is encrypted, resulting encrypted file is saved at 'folderToActOn'.
        /// OAEP padding is only available on Microsoft Windows XP or later. 
        /// OAEP padding will not be used in the final version of FFE.
        /// </summary>
        /// <param name="fileToEncrypt"></param>
        /// <param name="folderToActOn"></param>
        /// <param name="DoOAEPPadding"></param>
        public bool RSAEncrypt(string fileToEncrypt, string folderToActOn, bool DoOAEPPadding)
        {
            try
            {
                byte[] dataToEncrypt = ConvertFileToByteArray(fileToEncrypt);
                byte[] encryptedData;

                // Encrypt the converted byte array and specify OAEP padding.  
                encryptedData = rSACryptoServiceProvider.Encrypt(dataToEncrypt, DoOAEPPadding);

                // Saves the encrypted file at the chosen folder.
                // To be frank, I'm not crazy about the patchwork needed to get the correct address string, but it will do for now.
                SaveByteArrayToFile(folderToActOn + "\\" + Path.GetFileName(fileToEncrypt) + encryptedFileExt, encryptedData);
            }
            // Catch and display a CryptographicException to the console.
            catch (CryptographicException e)
            {
                throw new CryptographicException(e.Message + "\nAre you sure the selected file is not exceeding the maximum size?");
            }
            // Catch and display a DirectoryNotFoundException to the console.
            catch (DirectoryNotFoundException e)
            {
                throw new CryptographicException(e.Message + "\nAre you sure the selected path is valid?");
            }
            return true;
        }

        /// <summary>
        /// Start RSA decryption.
        /// 'fileToDecrypt' is decrypted, resulting decrypted file is saved at 'folderToActOn'.
        /// OAEP padding is only available on Microsoft Windows XP or later. 
        /// OAEP padding will not be used in the final version of FFE.
        /// </summary>
        /// <param name="fileToDecrypt"></param>
        /// <param name="folderToActOn"></param>
        /// <param name="doOAEPPadding"></param>
        public bool RSADecrypt(string fileToDecrypt, string folderToActOn, bool doOAEPPadding)
        {
            try
            {
                byte[] dataToDecrypt = ConvertFileToByteArray(fileToDecrypt);
                byte[] decryptedData;

                // Decrypt the passed byte array and specify OAEP padding.
                decryptedData = rSACryptoServiceProvider.Decrypt(dataToDecrypt, doOAEPPadding);

                // Saves the decrypted file at the chosen folder.
                // To be frank, I'm not crazy about the patchwork needed to get the correct address string, but it will do for now.
                SaveByteArrayToFile(folderToActOn + "\\" + Path.GetFileName(fileToDecrypt.Replace(encryptedFileExt, "")), decryptedData);
            }
            // Catch and display a CryptographicException to the console.
            catch (CryptographicException e)
            {
                Console.WriteLine(e.Message + "\nAre you sure the selected file is encrypted?");
            }
            // Catch and display a DirectoryNotFoundException to the console.
            catch (DirectoryNotFoundException e)
            {
                throw new CryptographicException(e.Message + "\nAre you sure the selected path is valid?");
            }
            return true;
        }

        /// <summary>
        /// Generates RSA parameter objects (refered to throughout the program as RSA Keys) and saves them as two files, 
        /// one containing the public key used for encryption, and one containing both the public AND the private key meant for encryption AND decryption.
        /// The generated keys needs a keysize specifying both the complexity AND the capacity of the encryption.
        /// </summary>
        /// <param name="folderToActOn"></param>
        /// <param name="dwKeySize"></param>
        public bool RSAGenerateKeys(string folderToActOn, int dwKeySize)
        {
            if (!ValidateKeySize(dwKeySize))
            {
                throw new CryptographicException("Key size not appropriate.");
            }
            try
            {
                rSACryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
                SaveRSAParametersToFile(rSACryptoServiceProvider.ExportParameters(true), folderToActOn, rSAKeyExtPublicAndPrivate);
                SaveRSAParametersToFile(rSACryptoServiceProvider.ExportParameters(false), folderToActOn, rSAKeyExtPublic);
                keyPairStatus = KeyPairStatus.PUBLIC_AND_PRIVATE;
            }
            // Catch and display a CryptographicException to the console.
            catch (CryptographicException e)
            {
                throw new CryptographicException("Something unexpected happened during RSA Key generation: " + e.Message);
            }
            // Catch and display a DirectoryNotFoundException to the console.
            catch (DirectoryNotFoundException e)
            {
                throw new CryptographicException(e.Message + "\nAre you sure the selected path is valid?");
            }
            return true;
        }

        /// <summary>
        /// Loads deserialized RSAParameters from a file.
        /// </summary>
        /// <param name="path"></param>
        public bool RSALoadKeys(string path)
        {
            try
            {
                // Make sure that the CryptoServiceProvider is instantiated.
                rSACryptoServiceProvider = new RSACryptoServiceProvider();

                // Load RSAParameters from the serializable parameter object.
                rSACryptoServiceProvider.ImportParameters(DeSerializeObject<RSAPSerializable>(path).RSAParameters);

                // Update local bool to inform the dialogue handler that the key/keypair is loaded.
                if (rSACryptoServiceProvider.PublicOnly)
                {
                    keyPairStatus = KeyPairStatus.PUBLIC;
                }
                else
                {
                    keyPairStatus = KeyPairStatus.PUBLIC_AND_PRIVATE;
                }
            }
            // Catch and display a NullReferenceException to the console.
            catch (NullReferenceException e)
            {
                throw new NullReferenceException(e.Message + "\nAre you sure the selected file is an appropriate RSAKey file?");
            }
            return true;
        }

        /// <summary>
        /// Reads from a file at a given path using a filestream, the result is a byte array.
        /// </summary>
        /// <param name="path"></param>
        private byte[] ConvertFileToByteArray(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                //Create a byte array of file stream length
                byte[] byteArray = File.ReadAllBytes(path);
                //Read block of bytes from stream into the byte array
                fileStream.Read(byteArray, 0, Convert.ToInt32(fileStream.Length));
                //Close the File Stream
                fileStream.Close();
                //Return the resulting byte array.
                return byteArray;
            }
        }

        /// <summary>
        /// Saves a given byte array to a file located at a given path, if the file does not already exist, it is created, if it does it is overwritten.
        /// If I am to be completely pedantic, then this method isn't strictly necessary seeing as it's just an extension of 'File.WriteAllBytes()'
        /// but I like the structure it provides for me, and in the future I may think of a better way to write the byte array to a file,
        /// especially seeing as byte arrays are limited in size in .NET.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="byteArray"></param>
        private void SaveByteArrayToFile(string path, byte[] byteArray)
        {
            File.WriteAllBytes(path, byteArray);
        }

        /// <summary>
        /// Saves the given RSAParameters as a serialized XML file.
        /// </summary>
        /// <param name="rSAKeyInfo"></param>
        /// <param name="path"></param>
        private void SaveRSAParametersToFile(RSAParameters rSAKeyInfo, string path, string keyExt)
        {
            SerializeObject(new RSAPSerializable(rSAKeyInfo), path + keyExt);
        }

        /// <summary>
        /// Returns wether or not the RSAParameters have been loaded into the CryptoHandler,
        /// as well as the capacity of any loaded keys.
        /// </summary>
        public string InformRSAKeyStatus()
        {
            switch (keyPairStatus)
            {
                case KeyPairStatus.NOT_LOADED:
                    return "RSA KEYS: NOT LOADED!";
                case KeyPairStatus.PUBLIC_AND_PRIVATE:
                    return "RSA KEYS: PRIVATE AND PUBLIC! Capacity: " + CalculateKeyEncryptionCapacity(rSACryptoServiceProvider.KeySize) + " bytes.";
                case KeyPairStatus.PUBLIC:
                    return "RSA KEYS: ONLY PUBLIC! Capacity: " + CalculateKeyEncryptionCapacity(rSACryptoServiceProvider.KeySize) + " bytes.";
                default:
                    return "RSA KEYS: ERROR";
            }
        }

        /// <summary>
        /// The following documentaion is from the RSACryptoServiceProvider documentation at microsoft: 
        /// The RSACryptoServiceProvider supports key sizes from 384 bits to 16384 bits in increments of 8 bits if you have the Microsoft Enhanced Cryptographic Provider installed. It supports key sizes from 384 bits to 512 bits in increments of 8 bits if you have the Microsoft Base Cryptographic Provider installed.
        /// </summary>
        /// <param name="keySize"></param>
        private bool ValidateKeySize(int keySize)
        {
            //Validates the keysize, it must be a multiple of 8, and between 384 and 16384 as specified in Microsoft Enhanced Cryptographic Provider.
            if ((keySize % 8 == 0) && (keySize >= 384) && (keySize <= 16384))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the amount of bytes a given RSA key is able to encrypt.
        /// </summary>
        /// <param name="keySize"></param>
        private int CalculateKeyEncryptionCapacity(int keySize)
        {
            return ((keySize - 384) / 8) + 37;
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="fileName"></param>
        private void SerializeObject<T>(T serializableObject, string fileName)
        {
            if (serializableObject == null) { return; }

            try
            {
                // Open a new XML doc and serialize a object into the doc using a MemoryStream.
                XmlDocument xmlDocument = new XmlDocument();
                XmlSerializer serializer = new XmlSerializer(serializableObject.GetType());
                // Using using in order to more cleanly control the scope and garbage handling. 
                using (MemoryStream stream = new MemoryStream())
                {
                    serializer.Serialize(stream, serializableObject);
                    stream.Position = 0;
                    xmlDocument.Load(stream);
                    xmlDocument.Save(fileName);
                }
            }
            // Catch and display a Exception to the console.
            catch (Exception e)
            {
                throw new Exception(e.Message + "\nError happened during serialization.");
            }
        }

        /// <summary>
        /// Deserializes an xml file into an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        private T DeSerializeObject<T>(string fileName)
        {

            if (string.IsNullOrEmpty(fileName)) { return default(T); }

            T objectOut = default(T);

            try
            {
                // Open a new XML doc and load the passed file as a string.
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(fileName);
                string xmlString = xmlDocument.OuterXml;

                // Using using in order to more cleanly control the scope and garbage handling. 
                using (StringReader read = new StringReader(xmlString))
                {
                    Type outType = typeof(T);

                    XmlSerializer serializer = new XmlSerializer(outType);
                    using (XmlReader reader = new XmlTextReader(read))
                    {
                        objectOut = (T)serializer.Deserialize(reader);
                    }
                }
            }
            // Catch and display a Exception to the console.
            catch (Exception e)
            {
                throw new Exception(e.Message + "\nError happened during deserialization.");
            }

            return objectOut;
        }
    }
}
