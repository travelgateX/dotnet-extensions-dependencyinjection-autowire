using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace TGX.Extensions.DependencyInjection.AutoWire
{

    /// <summary>
    /// Options for customizing how to the AutoWire discover services
    /// </summary>
    public class AutoWireOptions
    {

        /// <summary>
        /// Specifies if auto register should be applied only to classes or interfaces that implement or
        /// derive from a class ore interface that implements <see cref="AutoWireAttribute" />
        /// </summary>
        public bool RegisterByAttribute { get; set; }

        /// <summary>
        /// Specifies a collection of string prefixes by which all classes that match them
        /// by their full name will be registered as services.
        /// </summary>
        public IEnumerable<string> IncludePrefixed { get; set; } = new List<string>();
        
        /// <summary>
        /// Specifies an interval at which IConfigurationRoot will be reloaded if <see cref="ReloadConfiguration"/> is true.
        /// Defaults to 1 minute.
        /// </summary>
        public TimeSpan? ReloadConfigurationInterval { get; set; }

        /// <summary>
        /// Whether AutoWire should manage <see cref="IConfiguration"/> reload or not. Defaults to true.
        /// </summary>
        public bool ReloadConfiguration { get; set; } = true;
        
        /// <summary>
        /// Should AutoWire make sure that, when a <see cref="IOptionsMonitor{TOptions}"/> is bound,
        /// any invalid change in <see cref="IConfiguration"/> will not throw when re-binding, but instead
        /// return the cached instance. 
        /// </summary>
        public bool MakeOptionsMonitorUpdateSafe { get; set; }

        /// <summary>
        /// Action to execute if an <see cref="IOptionsMonitor{TOptions}"/> binding exception occurs.
        /// </summary>
        public Action<object, Type, Exception> OnOptionsMonitorUpdateException { get; set; }
        
        internal Assembly EntryAssembly { get; set; }
    }

}
