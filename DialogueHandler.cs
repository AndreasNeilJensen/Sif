using System;
using System.Windows.Forms;
using System.IO;

namespace FullyFreeSteganography
{
    /// <summary>
    /// Opens a dialogue for the user to communicate with the program through, 
    /// due to the sequential nature of the 'UI' the code is pretty dang 'un-sexy' to look at, but it will do for now.
    /// </summary>
    class DialogueHandler
    {
        FolderBrowserDialog folderBrowserDialog;
        OpenFileDialog fileBrowserDialog;
        CryptoHandler cryptoHandler;

        public DialogueHandler()
        {
            cryptoHandler = new CryptoHandler();
        }

        /// <summary>
        /// Main Dialogue starter.
        /// </summary>
        public void StartDialogue()
        {
            string folderToActOn;
            string fileToActOn;
            string reply;
            int requestedKeysize;

            bool keepRunning = true;

            Console.WriteLine("Welcome user!");
            while (keepRunning)
            {
                //Show menu.
                Console.WriteLine(cryptoHandler.InformRSAKeyStatus());
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("1) Load RSA Keys.");
                Console.WriteLine("2) Encrypt a file.");
                Console.WriteLine("3) Decrypt a file.");
                Console.WriteLine("4) Generate RSA Keys.");
                Console.WriteLine("9) Exit");
                reply = Console.ReadLine();

                //Choose action based on reply.
                switch (reply)
                {
                    case "1":
                        // Load RSAParameter from RSAKey file.
                        Console.WriteLine("Please write the path of the RSA Key File.\nAlternatively type \"-fb\" to browse to the location of the RSAKey file.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            fileToActOn = OpenFileBrowserDialog();
                        }
                        else
                        {
                            fileToActOn = reply;
                        }

                        try
                        {
                            if (cryptoHandler.RSALoadKeys(fileToActOn))
                            {
                                Console.Out.WriteLine("Success!");
                            }
                        }
                        catch (Exception e)
                        {
                            //Catch exceptions in case the loading of RSAKey did not succeed.
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "2":
                        // Encryption
                        if (!(cryptoHandler.keyPairStatus == KeyPairStatus.PUBLIC) && !(cryptoHandler.keyPairStatus == KeyPairStatus.PUBLIC_AND_PRIVATE))
                        {
                            Console.WriteLine("No valid RSA key has been loaded.");
                            break;
                        }

                        Console.WriteLine("Please write the path of the file you wish to encrypt.\nAlternatively type \"-fb\" to browse to the location of the desired file.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            fileToActOn = OpenFileBrowserDialog();
                        }
                        else
                        {
                            fileToActOn = reply;
                        }

                        Console.WriteLine("Please write the path of the target folder.\nAlternatively type \"-fb\" to browse to the location of the target folder.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            folderToActOn = OpenFolderBrowserDialog();
                        }
                        else
                        {
                            folderToActOn = reply;
                        }

                        try
                        {
                            if (cryptoHandler.RSAEncrypt(fileToActOn, folderToActOn, false))
                            {
                                Console.Out.WriteLine("Success!");
                            }
                        }
                        catch (Exception e)
                        {
                            //Catch exceptions in case the encryption did not succeed.
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "3":
                        // Decryption
                        if (!(cryptoHandler.keyPairStatus == KeyPairStatus.PUBLIC_AND_PRIVATE))
                        {
                            Console.WriteLine("No valid RSA key has been loaded.");
                            break;
                        }
                        Console.WriteLine("Please write the path of the file you wish to decrypt.\nAlternatively type \"-fb\" to browse to the location of the desired image.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            fileToActOn = OpenFileBrowserDialog();
                        }
                        else
                        {
                            fileToActOn = reply;
                        }

                        Console.WriteLine("Please write the path of the target folder.\nAlternatively type \"-fb\" to browse to the location of the target folder.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            folderToActOn = OpenFolderBrowserDialog();
                        }
                        else
                        {
                            folderToActOn = reply;
                        }

                        try
                        {
                            if (cryptoHandler.RSADecrypt(fileToActOn, folderToActOn, false))
                            {
                                Console.Out.WriteLine("Success!");
                            }
                        }
                        catch (Exception e)
                        {
                            //Catch exceptions in case the decryption did not succeed.
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "4":
                        // Generate new RSA keys to file.
                        Console.WriteLine("Please write the path of the target folder.\nAlternatively type \"-fb\" to browse to the location of the target folder.");
                        reply = Console.ReadLine();
                        if (reply == "-fb")
                        {
                            folderToActOn = OpenFolderBrowserDialog();
                        }
                        else
                        {
                            folderToActOn = reply;
                        }

                        Console.WriteLine("Please write the prefered size of the key.\n(Write a non number for a default 4096 Key size)");
                        reply = Console.ReadLine();
                        if (!int.TryParse(reply, out requestedKeysize))
                        {
                            requestedKeysize = 4096;
                        }

                        try
                        {
                            if (cryptoHandler.RSAGenerateKeys(folderToActOn, requestedKeysize))
                            {
                                Console.Out.WriteLine("Success!");
                            }
                        }
                        catch (Exception e)
                        {
                            //Catch exceptions in case the loading of RSAKey did not succeed.
                            Console.WriteLine(e.Message);
                        }
                        break;
                    case "9":
                        // Exit Program
                        keepRunning = false;
                        break;
                    default:
                        Console.WriteLine("Wrong Input.");
                        break;
                }

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Opens a file browsing dialog box and returns the path string.
        /// </summary>
        private string OpenFileBrowserDialog()
        {
            fileBrowserDialog = new OpenFileDialog();
            if (fileBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("File Name: " + Path.GetFileName(fileBrowserDialog.FileName)); // file name
            }

            return fileBrowserDialog.FileName;
        }

        /// <summary>
        /// Opens a folder browsing dialog box and returns the path string.
        /// </summary>
        private string OpenFolderBrowserDialog()
        {
            folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("File Path: " + folderBrowserDialog.SelectedPath); // full path
            }

            return folderBrowserDialog.SelectedPath;
        }
    }
}
