
namespace CIE.NIS.SDK.Smartcard
{
    using System;
    using System.Text;
    using PCSC;
    using PCSC.Exceptions;
    using PCSC.Utils;
    using PCSC.Monitoring;    
    using Extensions;    

    public class SmartcardFactory : IDisposable
    {
        private readonly SCardScope _scope;        
        public delegate void SmartcardReadEventHandler(string readerName);
        public event SmartcardReadEventHandler OnSmartcardRead = null;
        private readonly IContextFactory contextFactory = ContextFactory.Instance;
        private ISCardContext cardFactory;
        private SCardMonitor cardMonitor;
        private bool _disposed = false;
        private bool _stopped = false;
        
        public SmartcardFactory()
            :this(SCardScope.System)
        {

        }
        public SmartcardFactory(SCardScope scope)
        {
            try
            {                
                _scope = scope;
                //Estabilish context
                cardFactory = contextFactory.Establish(_scope);
            }
            catch
            {
                throw;
            }
        }
        public void CardInserted(object sender, CardEventArgs ce)
        {
            if (OnSmartcardRead != null)
                OnSmartcardRead(ce.ReaderName);
        }
        public void CardRemoved(object sender, CardEventArgs ce)
        {

        }
        /// <summary>
        /// Throw card exception
        /// </summary>        
        private void MonitorException(object sender, PCSCException ex)
        {
            throw ex;
        }
        /// <summary>
        /// Start smartcard listeners
        /// </summary>
        /// <param name="reader">Smartacard reader name</param>
        public void StartListeners(string[] readerNames)
        {
            try
            {
                cardMonitor = new SCardMonitor(contextFactory, _scope);
                cardMonitor.CardInserted += (sender, args) => CardInserted(sender, args);
                cardMonitor.CardRemoved += (sender, args) => CardRemoved(sender, args);
                cardMonitor.MonitorException += MonitorException;
                cardMonitor.Start(readerNames);

                _stopped = false;
            }
            catch
            {
                throw new Exception("Unable to start smartcards listeners");
            }
        }
        /// <summary>
        /// Stop smartcard listeners
        /// </summary>        
        public void StopListeners()
        {
            try
            {
                //Remove Events Handlers
                cardMonitor.Cancel();

                _stopped = true;
            }
            catch
            {
                throw new Exception("Unable to stop smartcards listeners");
            }
        }
        /// <summary>
        /// Get a list of available readers
        /// </summary>
        /// <returns>list of readers names</returns>
        public string[] GetSmartcardReaders()
        {
            try
            {
                // Retrieve the names of all installed readers.
                return cardFactory.GetReaders();
            }
            catch
            {
                throw new Exception("Unable to get smartcard readers list");                
            }
        }
        public string ReadIdentifier(string reader) {
            try
            {
                using (var peripheral = new SCardReader(cardFactory))
                {
                    var orchestrator = new CIEOrchestrator(peripheral);
                    try {
                        return Encoding.UTF8.GetString(orchestrator.ReadNIS(reader));
                    }
                    finally { orchestrator = null; }                    
                }
            }
            catch(Exception ex)
            {
                throw new Exception(ex.ToUniqueMessage());                
            }            
        }
        ~SmartcardFactory()
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
                            this.StopListeners();

                        cardMonitor.Dispose();
                        cardMonitor = null;

                        cardFactory.Release();
                        cardFactory.Dispose();
                        cardFactory = null;
                    }
                    // Cleanup unmanaged objects
                }
                _disposed = true;
            }
            catch
            {
                throw new Exception("Unable to dispose smartcard manager");
            }
        }
    }
}
