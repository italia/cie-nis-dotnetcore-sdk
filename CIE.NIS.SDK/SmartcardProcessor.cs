
namespace CIE.NIS.SDK
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;
    using Smartcard;
    public class SmartcardProcessor : ISmartcardEventHandlers, IDisposable
    {       
        private readonly SmartcardFactory _smartcardFactory;
        private string[] _smartcardReaders;        
        public event EventHandler<Exception> OnException;
        public event EventHandler<List<string>> OnListenerStarted;
        public event EventHandler<List<string>> OnListenerStopped;
        public event EventHandler<string> OnSmartcardInserted;
        public event EventHandler<string> OnSmartcardRemoved;
        public event EventHandler<string> OnDataRead;

        private bool _disposed = false;
        private bool _stopped = true;

        public SmartcardProcessor() {            
            //Smartcard initialization
            _smartcardFactory = new SmartcardFactory();            
            _smartcardReaders = _smartcardFactory.GetSmartcardReaders();                                   
        }

        public void Start()
        {
            try
            {
                if(_stopped)
                {
                    if (_smartcardReaders.Length == 0)
                        throw new NullReferenceException("No smartcard reader plugged");

                    _smartcardFactory.OnSmartcardInserted += SmartcardInserted;
                    _smartcardFactory.OnSmartcardRemoved += SmartcardRemoved;
                    _smartcardFactory.StartListeners(_smartcardReaders);
                    if (OnListenerStarted != null)
                        OnListenerStarted(this, _smartcardReaders.ToList());
                    _stopped = false;
                }                
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
                if(!_stopped)
                {
                    _smartcardFactory.OnSmartcardInserted -= SmartcardInserted;
                    _smartcardFactory.OnSmartcardRemoved -= SmartcardRemoved;
                    _smartcardFactory.StopListeners();
                    _smartcardFactory.Dispose();
                    if (OnListenerStopped != null)
                        OnListenerStopped(this, _smartcardReaders.ToList());
                    _stopped = true;
                }                
            }
            catch (Exception ex)
            {
                OnException(this, ex);
            }
        }
        private void SmartcardInserted(string reader) {
            {
                try
                {
                    if(OnSmartcardInserted != null)
                        OnSmartcardInserted(this, reader);
                    //Read card data from reader
                    string nis = _smartcardFactory.ReadIdentifier(reader);
                    OnDataRead(this, nis);                    
                }
                catch (Exception ex)
                {
                    OnException(this, ex);
                }
            }
        }
        private void SmartcardRemoved(string reader)
        {
            {
                try
                {
                    if(OnSmartcardRemoved != null)
                        OnSmartcardRemoved(this, reader);                                        
                }
                catch (Exception ex)
                {
                    OnException(this, ex);
                }
            }
        }
        ~SmartcardProcessor()
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
                        this.Stop();                                               
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
