using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TGX.Extensions.DependencyInjection.AutoWire
{
    internal class ConfigReloaderBackgroundService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfigReloaderBackgroundService> _logger;
        private readonly TimeSpan? _interval;

        public ConfigReloaderBackgroundService(
            IConfiguration configuration, AutoWireOptions autoWireOptions,
            ILogger<ConfigReloaderBackgroundService> logger
        )
        {
            _configuration = configuration;
            _logger = logger;
            _interval = autoWireOptions.ReloadConfigurationInterval ?? TimeSpan.FromMinutes(1);
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                ((IConfigurationRoot)_configuration).Reload();
                _logger.LogInformation($"Successfully reloaded configuration, sleeping {_interval.Value.TotalSeconds} seconds...");
                await Task.Delay(_interval.Value, stoppingToken);
            }
        }
    }
}
