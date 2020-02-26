using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TravelGate.Extensions.DependencyInjection.AutoWire
{
    internal static class RegisterOptionsExtensions
    {
        private static readonly MethodInfo ConfigureMethodInfo;

        static RegisterOptionsExtensions() => ConfigureMethodInfo = GetConfigureMethodInfo();

        private static MethodInfo GetConfigureMethodInfo() => typeof(OptionsConfigurationServiceCollectionExtensions).GetMethod
        (
            nameof(OptionsConfigurationServiceCollectionExtensions.Configure),
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(IServiceCollection), typeof(IConfiguration) },
            null
        );

        internal static IServiceCollection RegisterOptions(this IServiceCollection services, IEnumerable<Type> types, IConfiguration configuration)
        {
            services.AddSingleton(ServiceDescriptor.Singleton(typeof(IOptionsMonitor<>), typeof(UpdateSafeOptionsMonitor<>)));

            var candidateTypes = types.Where(it => it.IsClass).Where(type => type.GetConstructor(Type.EmptyTypes) != null);
            
            configuration.GetChildren().Select(
                    section =>
                    {
                        var sectionCorrespondingType = candidateTypes.FirstOrDefault(type => type.Name.Equals(section.Key, StringComparison.OrdinalIgnoreCase));

                        return new
                        {
                            section,
                            sectionCorrespondingType
                        };
                    }
                )
                .Where(x => x.sectionCorrespondingType != null)
                .ToList()
                .ForEach(x => ConfigureOption(services, configuration, x.sectionCorrespondingType, x.section.Key));

            return services;
        }

        private static void ConfigureOption(this IServiceCollection services, IConfiguration configuration, Type optionType, string configurationSectionKey)
        {
            var genericConfigureMethodInfo = ConfigureMethodInfo.MakeGenericMethod(optionType);
            genericConfigureMethodInfo.Invoke(null, new object[] { services, configuration.GetSection(configurationSectionKey) });
        }
    }
}
