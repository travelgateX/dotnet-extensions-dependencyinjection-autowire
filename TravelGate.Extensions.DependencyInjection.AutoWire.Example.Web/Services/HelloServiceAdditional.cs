using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelGate.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Options;

namespace TravelGate.Extensions.DependencyInjection.AutoWire.Example.Web.Services
{
    public class HelloServiceAdditional
    {
        private string _helloValue;

        public HelloServiceAdditional()
        {
            _helloValue = "Cualquier cosa";
        }

        public string GetHelloValue()
        {
            return _helloValue;
        }
    }
}