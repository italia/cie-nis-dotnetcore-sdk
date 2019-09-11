using CIE.NIS.SDK;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Identificazione del documento tramite il Numero Identificativo per i Servizi\n");            
            using (var pr = new Processor())
            {
                pr.Start();
                Console.WriteLine("Appoggia la cie sul lettore NFC per iniziare il controllo del nis.");
                Console.WriteLine("Premi un tasto per terminare l'applicazione");
                Console.ReadLine();
                pr.Stop();
            }               
        }
    }
}
