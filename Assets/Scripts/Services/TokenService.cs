using System;
using System.Threading;
using Zenject;

namespace Services
{
    public sealed class AppLifetimeTokenService : IInitializable, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public CancellationToken Token => _cts.Token;

        public void Initialize() { }
        public void Dispose() => _cts.Cancel();
    }

    public sealed class SessionLifetimeTokenService : IInitializable, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        public CancellationToken Token => _cts.Token;

        public void Initialize() { }
        public void Dispose() => _cts.Cancel();
    }
}