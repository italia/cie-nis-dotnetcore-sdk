using CIE.NIS.SDK;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Example for reading and validate the NIS code from an Italian Electronic Identity Card (CIE)\n");            
            using (var pr = new Processor())
            {
                pr.OnException += OnException;
                pr.OnReadComplete += OnReadComplete;
                pr.Start();
                Console.WriteLine("Put your CIE on smartcard reader to start.");
                Console.WriteLine("Press any key to end program");
                Console.ReadLine();
                pr.Stop();
            }               
        }

        public static void OnException(object sender, Exception e)
        {
            Console.WriteLine(e.Message);
        }

        public static void OnReadComplete(object sender, string e)
        {
            if (e != null)
                Console.WriteLine($"NIS {e} verified");
            else
                Console.WriteLine("NIS is not valid");            
        }
    }


}
