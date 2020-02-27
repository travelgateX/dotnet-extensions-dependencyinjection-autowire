using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace TGX.Extensions.DependencyInjection.AutoWire
{
    internal static class RegisterServicesExtensions
    {
        internal static void RegisterServices(this IServiceCollection serviceCollection, IEnumerable<Type> types, AutoWireOptions options)
        {
	        var candidateTypes = GetTypes(types, options);
	        
	        var typesFromAutoWire = typeof(AutoWireAttribute).Assembly.GetExportedTypes();

			foreach (var type in candidateTypes.Where(x => typesFromAutoWire.All(y => x != y)))
			{
				serviceCollection.TryAdd(ServiceDescriptor.Transient(type, type));

				var interfaces = type.GetTypeInfo().ImplementedInterfaces.Where(x => x != typeof(IDisposable));

				foreach (var @interface in interfaces)
				{
					if (serviceCollection.Any(y => y.ServiceType == @interface)) continue;

					if (options.RegisterByAttribute && @interface.GetCustomAttribute<AutoWireAttribute>() == null) continue;

					serviceCollection.TryAdd(new ServiceDescriptor(@interface, type, ServiceLifetime.Transient));
				}
            }
        }
        
        private static IReadOnlyList<Type> GetTypes(IEnumerable<Type> types, AutoWireOptions options)
	    {
		    var candidateTypes = types
			    .Where(x =>
				    x.GetCustomAttribute<IgnoreAutoWireAttribute>() == null
					&& x.GetInterfaces().All(y => y.GetCustomAttribute<IgnoreAutoWireAttribute>() == null)
				    && x.IsClass
				    && !x.IsAbstract
				    && !x.IsNested
				    && !x.Name.EndsWith("Options", StringComparison.Ordinal)
			    );

		    if (options.RegisterByAttribute)
		    {
			    candidateTypes = candidateTypes.Where(x =>
				    x.CustomAttributes.Any(y => y.AttributeType == typeof(AutoWireAttribute)) ||
				    x.GetInterfaces().Any(y => y.GetCustomAttribute<AutoWireAttribute>() != null));
		    }

		    return candidateTypes.ToList();
	    }
	}
}
