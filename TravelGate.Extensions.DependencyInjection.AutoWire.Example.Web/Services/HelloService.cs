using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TravelGate.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Options;

namespace TravelGate.Extensions.DependencyInjection.AutoWire.Example.Web.Services
{
    public class HelloService
    {
        private readonly ILogger<HelloService> _logger;
        private readonly HelloServiceAdditional _helloServiceAdditional;

        private string _helloValue;

        public HelloService(ILogger<HelloService> logger, HelloServiceAdditional helloServiceAdditional, IOptionsSnapshot<ConfigOptions> options)
        {
            _helloValue = options.Value.Key1;
            _logger = logger;
            _helloServiceAdditional = helloServiceAdditional;
        }

        public string GetHelloValue()
        {
            _logger.LogInformation($"Hello value is: {_helloValue}");

            return _helloValue;
        }
        
        public string GetAdditionalHelloValue()
        {
            _logger.LogInformation($"Hello value is: {_helloValue}");

            return _helloServiceAdditional.GetHelloValue();
        }
    }
}