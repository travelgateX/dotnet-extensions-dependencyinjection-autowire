using System;
using Microsoft.AspNetCore.Mvc;
using TravelGate.Extensions.DependencyInjection.AutoWire.Example.Web.Services;

namespace TravelGate.Extensions.DependencyInjection.AutoWire.Example.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController : ControllerBase
    {
        private readonly HelloService _helloService;
        private readonly HelloServiceAdditional _helloServiceAdditional;

        public MainController(HelloService helloService, HelloServiceAdditional helloServiceAdditional)
        {
            _helloService = helloService;
            _helloServiceAdditional = helloServiceAdditional;
        }

        [HttpGet("get")]
        public string Get()
        {
            return _helloService.GetHelloValue() + " - " + _helloService.GetAdditionalHelloValue() + " - " + _helloServiceAdditional.GetHelloValue();
        }

        [HttpGet("change/config")]
        public void ChangeConfig(string configKey, string configValue)
        {
            Environment.SetEnvironmentVariable(configKey, configValue);
        }
    }
}