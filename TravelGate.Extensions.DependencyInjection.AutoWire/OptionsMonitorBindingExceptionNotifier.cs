using System;

namespace TravelGate.Extensions.DependencyInjection.AutoWire
{
    internal class OptionsMonitorBindingExceptionNotifier
    {
        internal Action<object, Type, Exception> NotifyException { get; }

        internal OptionsMonitorBindingExceptionNotifier(Action<object, Type, Exception> notifyExceptionAction)
        {
            NotifyException = notifyExceptionAction;
        }
    }
}