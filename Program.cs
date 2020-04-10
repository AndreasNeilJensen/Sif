using System;
using FullyFreeSteganography;

namespace FullyFreeEncryption
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            DialogueHandler dialogueHandler = new DialogueHandler();
            dialogueHandler.StartDialogue();
        }
    }
}
