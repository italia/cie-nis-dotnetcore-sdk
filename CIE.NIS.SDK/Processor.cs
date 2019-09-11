
namespace CIE.NIS.SDK
{    
    using System;    
    using Extensions;    
    using Interfaces;
    using Smartcard;
    

    public class Processor : IProcessor, IDisposable
    {       
        private readonly SmartcardFactory _smartcardFactory;
        private string[] _smartcardReaders;        
        
        private bool _disposed = false;
        private bool _stopped = false;

        public Processor() {            
            //Smartcard initialization
            _smartcardFactory = new SmartcardFactory();            
            _smartcardReaders = _smartcardFactory.GetSmartcardReaders();
            if (_smartcardReaders.Length != 0)
            {
                foreach (var smartcardReader in _smartcardReaders)
                {
                    Console.WriteLine("Lettore {0} in ascolto", smartcardReader);
                }
            }            
        }

        public void Start()
        {
            try
            {                
                _smartcardFactory.OnSmartcardRead += OnSmartcardRead;
                _smartcardFactory.StartListeners(_smartcardReaders);

                _stopped = false;
            }
            catch (Exception ex)
            {
                //Log error
                Console.WriteLine(ex.ToUniqueMessage());
            }
        }
        public void Stop()
        {
            try
            {
                _smartcardFactory.OnSmartcardRead -= OnSmartcardRead;
                _smartcardFactory.StopListeners();

                _stopped = true;
            }
            catch (Exception ex)
            {
                //Log error
                Console.WriteLine(ex.ToUniqueMessage());
            }
        }
        public void OnSmartcardRead(string reader) {
            {
                try
                {
                    //Read card data from reader
                    string nis = _smartcardFactory.ReadIdentifier(reader);
                    if(nis != null)
                    {
                        //Do some operation with valid nis
                        Console.WriteLine("Il tuo nis ({0}) è stato verificato", nis);                       
                    }
                    else
                        Console.WriteLine("Verifica del nis fallita");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToUniqueMessage());
                }
            }
        }       
        ~Processor()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        //Cleanup managed objects                     
                        if (!_stopped)
                            this.Stop();

                        _smartcardFactory.Dispose();                        
                    }
                    // Cleanup unmanaged objects
                }
                _disposed = true;
            }
            catch (Exception ex)
            {
                //Log error
                Console.WriteLine(ex.ToUniqueMessage());
            }
        }
    }
}
