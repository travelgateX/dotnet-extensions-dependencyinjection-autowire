using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("TravelGate.Extensions.DependencyInjection.AutoWire.Tests")]
namespace TravelGate.Extensions.DependencyInjection.AutoWire
{

    /// <summary>
    /// Auto dependency discovery/registration and Options binding
    /// </summary>
    public static class AutoWireExtensions
    {

        /// <summary>
        /// Run the dependency and options wiring
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IServiceCollection AutoWire(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
	        return AutoWire(serviceCollection, configuration, null);
        }

        /// <summary>
        /// Run the dependency and options wiring
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="options">Options for customizing the service discovery</param>
        /// <returns></returns>
        public static IServiceCollection AutoWire(this IServiceCollection serviceCollection, IConfiguration configuration, Action<AutoWireOptions> options)
        {
		    var autoWireOptions = new AutoWireOptions();
		    
		    ConfigurationBinder.Bind(configuration.GetSection("TravelGate:Extensions:DependencyInjection:AutoWire"), options);

		    options?.Invoke(autoWireOptions);

		    serviceCollection.AddOptions();

		    if (autoWireOptions.MakeOptionsMonitorUpdateSafe)
		    {
			    serviceCollection.AddSingleton(provider => new OptionsMonitorBindingExceptionNotifier(autoWireOptions.NotifyExceptionAction));
			    serviceCollection.Add(ServiceDescriptor.Singleton(typeof(IOptionsMonitor<>), typeof(UpdateSafeOptionsMonitor<>)));
		    }

		    serviceCollection.AddSingleton(autoWireOptions);
		    
	        var assemblies = GetAssemblies();

	        var types = assemblies.SelectMany(x => x.GetExportedTypes());
	        
	        if ((autoWireOptions.IncludePrefixed == null || !autoWireOptions.IncludePrefixed.Any()) && !autoWireOptions.RegisterByAttribute)
	        {
		        types = types.Where(x => autoWireOptions.EntryAssembly != null && autoWireOptions.EntryAssembly == x.Assembly || autoWireOptions.EntryAssembly == null && x.Assembly == Assembly.GetEntryAssembly());
	        }
		    
	        if (autoWireOptions.IncludePrefixed != null && autoWireOptions.IncludePrefixed.Any())
	        {
		        types = types
			        .Where(x => autoWireOptions.IncludePrefixed.Any(prefix => x.Namespace != null && x.Namespace.StartsWith(prefix)));
	        }

			serviceCollection.RegisterServices(types, autoWireOptions);

            serviceCollection.RegisterOptions(types, configuration);

            if (autoWireOptions.ReloadConfiguration)
            {
	            serviceCollection.AddHostedService<ConfigReloaderBackgroundService>();
            }
            
            return serviceCollection;
        }

	    private static List<Assembly> GetAssemblies()
	    {
		    var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

		    assemblies = assemblies.Where(x => !IsIgnoredAssembly(x) && !x.IsDynamic).ToList();

		    return assemblies;
	    }

	    private static bool IsIgnoredAssembly(Assembly assembly)
        {

            var assembliesPrefixesToIgnore = new[]
            {
                "netstandard",
                "Microsoft.",
                "System.",
                "System,",
                "CR_ExtUnitTest",
                "CR_VSTest",
                "mscorlib",
                "DevExpress.CodeRush",
                "xunit.",
                "Newtonsoft.Json",
				"Serilog",
				"NLog",
				"SOS.NETCore",
			};

            return assembliesPrefixesToIgnore.Any(x => assembly.FullName.StartsWith(x));

        }

    }

}
