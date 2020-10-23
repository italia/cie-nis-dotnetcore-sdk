using System;
using System.Collections.Generic;

namespace CIE.NIS.SDK.Interfaces
{
    public interface ISmartcardEventHandlers : IProcessor
    {
        event EventHandler<List<string>> OnListenerStarted;
        event EventHandler<List<string>> OnListenerStopped;
        event EventHandler<string> OnSmartcardInserted;
        event EventHandler<string> OnSmartcardRemoved;
        event EventHandler<string> OnDataRead;        
        event EventHandler<Exception> OnException;
    }
}
