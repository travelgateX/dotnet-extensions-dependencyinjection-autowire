using System;
using System.Collections.Generic;
using System.Text;

namespace TravelGate.Extensions.DependencyInjection.AutoWire
{

    /// <summary>
    /// Decorate a class to indicate AutoWire to register the service explicitly
    /// </summary>
    public class AutoWireAttribute : Attribute
    {
    }
}
