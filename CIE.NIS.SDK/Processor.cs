
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
        public event EventHandler<string> OnReadComplete;
        public event EventHandler<Exception> OnException;

        private bool _disposed = false;
        private bool _stopped = false;

        public Processor() {            
            //Smartcard initialization
            _smartcardFactory = new SmartcardFactory();            
            _smartcardReaders = _smartcardFactory.GetSmartcardReaders();
            if (_smartcardReaders.Length == 0)
                OnException(this, new NullReferenceException("No smartcard reader plugged"));                        
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
                OnException(this, ex);
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
                OnException(this, ex);
            }
        }
        private void OnSmartcardRead(string reader) {
            {
                try
                {
                    //Read card data from reader
                    string nis = _smartcardFactory.ReadIdentifier(reader);
                    OnReadComplete(this, nis);                    
                }
                catch (Exception ex)
                {
                    OnException(this, ex);
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
                OnException(this, ex);
            }
        }
    }
}
