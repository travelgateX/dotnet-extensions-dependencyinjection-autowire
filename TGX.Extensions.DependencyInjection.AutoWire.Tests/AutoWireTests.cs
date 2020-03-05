using System;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Domain;
using TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Options;
using TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Others;
using TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace TGX.Extensions.DependencyInjection.AutoWire.Tests
{
    public class AutoWireTests : BaseTest
    {

        [Fact]
        public void AutoWireAll()
        {
            var config = GetConfiguration();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AutoWire(config, options => options.EntryAssembly = typeof(AutoWireTests).Assembly);
            var sp = serviceCollection.BuildServiceProvider();

	        Assert.NotNull(sp.GetRequiredService<BaseService>());
	        Assert.NotNull(sp.GetRequiredService<DerivedService>());
	        Assert.NotNull(sp.GetRequiredService<InterfacedService>());
	        var iSampleInterfaceImpl = sp.GetRequiredService<ISampleInterface>();
	        Assert.True(typeof(InterfacedService) == iSampleInterfaceImpl.GetType() && typeof(ISampleInterface).IsInstanceOfType(iSampleInterfaceImpl));
        }

		[Fact]
        public void AutoWireByImplementingAutoWireAttribute()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, options => options.RegisterByAttribute = true);
            var sp = sc.BuildServiceProvider();

	        Assert.NotNull(sp.GetRequiredService<InterfacedService>());
	        Assert.NotNull(sp.GetRequiredService<BaseService>());
            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<DerivedService>());
        }

        [Fact]
        public void AutoWireByPrefix()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, options => options.IncludeNamespaceOf<OtherClass>());
            var sp = sc.BuildServiceProvider();

            Assert.NotNull(sp.GetRequiredService<OtherClass>());
        }

        [Fact]
        public void AutoWireIgnoresIgnoredServicesByAttribute()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config);
            var sp = sc.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IgnoredDomainClass>());
        }

        [Fact]
        public void AutoWireRegistersGenericClasses()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, options => options.EntryAssembly = typeof(AutoWireTests).Assembly);
            var sp = sc.BuildServiceProvider();

            Assert.Equal(
                "System.String",
                sp.GetRequiredService<GenericClass<string>>().ToString());
            
            Assert.StartsWith(
                "Microsoft.Extensions.Options.IOptionsMonitor`1[[TGX.Extensions.DependencyInjection.AutoWire.Tests.Dummy.Options.ConfigOptions",
                sp.GetRequiredService<GenericClass<IOptionsMonitor<ConfigOptions>>>().ToString());
        }

        [Fact]
        public void AutoWireIgnoresItsOwnClasses()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, options => options.EntryAssembly = typeof(AutoWireTests).Assembly);
            var sp = sc.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<AutoWireAttribute>());
        }

        [Fact]
        public void AutoWireIgnoresServiceByDerivedInterface()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, options => options.EntryAssembly = typeof(AutoWireTests).Assembly);
            var sp = sc.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => sp.GetRequiredService<IgnoredServiceByDerivedInterface>());
        }

        [Fact]
        public void AutoWireRegistersServiceOnce()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();

            var service = new BaseService();
            
            sc.AddSingleton(service);
            
            sc.AutoWire(config, options => options.EntryAssembly = typeof(AutoWireTests).Assembly);
            var sp = sc.BuildServiceProvider();

            Assert.Equal(sp.GetService<BaseService>(), service);
        }

        [Fact]
        public void AutoWireConfiguresOptions()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, autoWireOptions =>
            {
                autoWireOptions.EntryAssembly = typeof(AutoWireTests).Assembly;
                autoWireOptions.MakeOptionsMonitorUpdateSafe = true;
                autoWireOptions.ReloadConfigurationInterval = TimeSpan.FromSeconds(1);
            });

            var sp = sc.BuildServiceProvider();

            var options = sp.GetRequiredService<IOptions<ConfigOptions>>();
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ConfigOptions>>();

            optionsMonitor.OnChange((configOptions, s) => { Assert.NotNull(configOptions); });
            
            Assert.NotNull(options.Value.Key1);
            Assert.NotNull(optionsMonitor.CurrentValue.Key1);
            
            Environment.SetEnvironmentVariable("CONFIGOPTIONS__KEY1", "replaced_value");

            var backgroundService = new ConfigReloaderBackgroundService(config, sp.GetRequiredService<AutoWireOptions>(), new NullLogger<ConfigReloaderBackgroundService>());

            backgroundService.StartAsync(new CancellationToken());
            
            Thread.Sleep(5000);

            options = sp.GetRequiredService<IOptions<ConfigOptions>>();
            optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ConfigOptions>>();
            Assert.Equal("value1", options.Value.Key1);
            Assert.Equal("replaced_value", optionsMonitor.CurrentValue.Key1);

            var optionsValue = options.Value;
            Assert.NotNull(optionsValue);
            Assert.Equal("value1", optionsValue.Key1);
            Assert.Equal("value2", optionsValue.Key2);
            Assert.Equal(1, optionsValue.Key3);
            Assert.True(optionsValue.Key4);
            Assert.NotNull(optionsValue.SubKey);
            Assert.Contains(optionsValue.SubKey.Key2, x => x == "value2.1");
        }

        [Fact]
        public void AutoWiredConfiguredOptionsDoNotBreakWhenChangingConfiguration()
        {
            var config = GetConfiguration();

            var sc = new ServiceCollection();
            sc.AutoWire(config, autoWireOptions =>
            {
                autoWireOptions.EntryAssembly = typeof(AutoWireTests).Assembly;
                autoWireOptions.ReloadConfigurationInterval = TimeSpan.FromSeconds(1);
                autoWireOptions.MakeOptionsMonitorUpdateSafe = true;
                autoWireOptions.OnOptionsMonitorUpdateException = (o, type, ex) => { Assert.NotNull(o); };
            });

            var sp = sc.BuildServiceProvider();
            
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ConfigOptions>>();
            
            Assert.True(optionsMonitor.CurrentValue.Key4);
            
            Environment.SetEnvironmentVariable("CONFIGOPTIONS__KEY4", "invalid_key_value_because_is_not_boolean");

            var backgroundService = new ConfigReloaderBackgroundService(config, sp.GetRequiredService<AutoWireOptions>(), new NullLogger<ConfigReloaderBackgroundService>());

            backgroundService.StartAsync(new CancellationToken());
            
            Thread.Sleep(5000);

            optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ConfigOptions>>();

            Assert.True(optionsMonitor.CurrentValue.Key4);
        }
    }
}

