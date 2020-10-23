using CIE.NIS.SDK;
using System;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Example for reading and validate the NIS code from an Italian Electronic Identity Card (CIE)\n");            
            using (var pr = new SmartcardProcessor())
            {
                pr.OnException += ThrowException;
                pr.OnDataRead += ShowResult;
                pr.OnListenerStarted += ListenerStarted;
                pr.OnSmartcardInserted += CardInserted;
                pr.OnSmartcardRemoved += CardRemoved;
                pr.Start();                
                Console.WriteLine("Press any key to end program");
                Console.ReadLine();
                pr.Stop();
            }               
        }

        public static void ListenerStarted(object sender, List<string> e)
        {
            foreach (var reader in e)
            {
                Console.WriteLine("Listener started for reader " + reader);
            }
            Console.WriteLine("Put your card on reader to acquire and verify NIS");
        }

        public static void CardInserted(object sender, string e)
        {
            Console.WriteLine("Smartcard connected to reader " + e);            
        }

        public static void CardRemoved(object sender, string e)
        {
            Console.WriteLine("Smartcard disconnected from reader " + e);
        }

        public static void ThrowException(object sender, Exception e)
        {
            Console.WriteLine(e.Message);
        }

        public static void ShowResult(object sender, string e)
        {
            if (e != null)
                Console.WriteLine($"NIS {e} verified");
            else
                Console.WriteLine("NIS is not valid");            
        }        
    }
}
