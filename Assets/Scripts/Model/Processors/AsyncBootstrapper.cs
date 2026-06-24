using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services;
using Zenject;

namespace Model.Processors
{
    public sealed class AsyncBootstrapper : IInitializable, IDisposable
    {
        [Inject] private UiCameraService  _uiCameraService;
        [Inject] private WindowService    _windowService;
        [Inject] private GameCycleService _gameCycleService;
        [Inject] private AppLifetimeTokenService _appToken;

        public void Initialize() => BootstrapAsync(_appToken.Token).Forget();

        private async UniTaskVoid BootstrapAsync(CancellationToken token)
        {
            // Строго последовательно — каждый шаг ждёт предыдущий
            await _uiCameraService.InitializeAsync(token);
            await _windowService.InitializeAsync(token);
            await _gameCycleService.InitializeAsync(token);
        }

        public void Dispose() { }
    }
}