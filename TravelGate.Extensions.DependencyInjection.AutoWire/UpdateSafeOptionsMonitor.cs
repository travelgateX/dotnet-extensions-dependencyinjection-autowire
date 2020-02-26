using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace TravelGate.Extensions.DependencyInjection.AutoWire
{
    /// <summary>
    /// Implementation of <see cref="IOptionsMonitor{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions">Options type.</typeparam>
    internal class UpdateSafeOptionsMonitor<TOptions> : IOptionsMonitor<TOptions>, IDisposable where TOptions : class, new()
    {
        private readonly IOptionsMonitorCache<TOptions> _cache;
        private readonly OptionsMonitorBindingExceptionNotifier _optionsMonitorBindingExceptionNotifier;
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        internal event Action<TOptions, string> _onChange;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="factory">The factory to use to create options.</param>
        /// <param name="sources">The sources used to listen for changes to the options instance.</param>
        /// <param name="cache">The cache used to store options.</param>
        /// <param name="optionsMonitorBindingExceptionNotifier">The action to execute in case of binding exception.</param>
        public UpdateSafeOptionsMonitor(
            IOptionsFactory<TOptions> factory,
            IEnumerable<IOptionsChangeTokenSource<TOptions>> sources,
            IOptionsMonitorCache<TOptions> cache,
            OptionsMonitorBindingExceptionNotifier optionsMonitorBindingExceptionNotifier
        )
        {
            _factory = factory;
            _cache = cache;
            _optionsMonitorBindingExceptionNotifier = optionsMonitorBindingExceptionNotifier;

            foreach (var source in sources)
            {
                var registration = ChangeToken.OnChange(
                      () => source.GetChangeToken(),
                      InvokeChanged,
                      source.Name);

                _registrations.Add(registration);
            }
        }

        private void InvokeChanged(string name)
        {
            name ??= Options.DefaultName;
            var currentOptions = Get(name);
            _cache.TryRemove(name);
            TOptions options;
            try
            {
                options = _factory.Create(name);
                _cache.TryAdd(name, options);
            }
            catch(Exception ex)
            {
                _cache.TryAdd(name, currentOptions);
                options = currentOptions;
                _optionsMonitorBindingExceptionNotifier.NotifyException?.Invoke(options, options.GetType(), ex);
            }
            _onChange?.Invoke(options, name);
        }

        /// <summary>
        /// The present value of the options.
        /// </summary>
        public TOptions CurrentValue => Get(Options.DefaultName);

        /// <summary>
        /// Returns a configured <typeparamref name="TOptions"/> instance with the given <paramref name="name"/>.
        /// </summary>
        public virtual TOptions Get(string name)
        {
            name ??= Options.DefaultName;
            return _cache.GetOrAdd(name, () => _factory.Create(name));
        }

        /// <summary>
        /// Registers a listener to be called whenever <typeparamref name="TOptions"/> changes.
        /// </summary>
        /// <param name="listener">The action to be invoked when <typeparamref name="TOptions"/> has changed.</param>
        /// <returns>An <see cref="IDisposable"/> which should be disposed to stop listening for changes.</returns>
        public IDisposable OnChange(Action<TOptions, string> listener)
        {
            var disposable = new ChangeTrackerDisposable(this, listener);
            _onChange += disposable.OnChange;
            return disposable;
        }

        /// <summary>
        /// Removes all change registration subscriptions.
        /// </summary>
        public void Dispose()
        {
            // Remove all subscriptions to the change tokens
            foreach (var registration in _registrations)
            {
                registration.Dispose();
            }

            _registrations.Clear();
        }

        internal class ChangeTrackerDisposable : IDisposable
        {
            private readonly Action<TOptions, string> _listener;
            private readonly UpdateSafeOptionsMonitor<TOptions> _monitor;

            public ChangeTrackerDisposable(UpdateSafeOptionsMonitor<TOptions> monitor, Action<TOptions, string> listener)
            {
                _listener = listener;
                _monitor = monitor;
            }

            public void OnChange(TOptions options, string name) => _listener.Invoke(options, name);

            public void Dispose() => _monitor._onChange -= OnChange;
        }
    }
}