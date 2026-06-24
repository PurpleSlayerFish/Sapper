using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Windows;
using UnityEngine;
using Zenject;

namespace Services
{
    public enum GameState
    {
        Menu,
        Game
    }

    public abstract class BaseGameState : IDisposable
    {
        protected readonly Transform Container;
        protected readonly WindowService WindowService;
        protected readonly CancellationToken LifetimeToken;

        protected BaseGameState(Transform container, WindowService windowService, CancellationToken lifetimeToken)
        {
            Container = container;
            WindowService = windowService;
            LifetimeToken = lifetimeToken;
        }

        public abstract UniTask Enter();
        public abstract UniTask Exit();

        public virtual void Dispose()
        {
            // Деактивируем контейнер при выходе из стейта
            if (Container != null)
                Container.gameObject.SetActive(false);
        }
    }

    public sealed class MenuGameState : BaseGameState
    {
        public MenuGameState(Transform container, WindowService windowService, CancellationToken lifetimeToken)
            : base(container, windowService, lifetimeToken)
        {
        }

        public override async UniTask Enter()
        {
            Container.gameObject.SetActive(true);

            await WindowService.Show(
                new MainMenuWindowData(),
                needLoadingScreen: true,
                token: LifetimeToken);
        }

        public override async UniTask Exit()
        {
            await WindowService.Close<MainMenuWindowData>(LifetimeToken);
            Container.gameObject.SetActive(false);
        }
    }

    public sealed class GameplayGameState : BaseGameState
    {
        public GameplayGameState(Transform container, WindowService windowService, CancellationToken lifetimeToken)
            : base(container, windowService, lifetimeToken)
        {
        }

        public override async UniTask Enter()
        {
            Container.gameObject.SetActive(true);

            // TODO: инициализация игрового поля
            // TODO: инициализация контроллера ввода

            await WindowService.Show(
                new GameWindowData(),
                needLoadingScreen: true,
                token: LifetimeToken);
        }

        public override async UniTask Exit()
        {
            await WindowService.Close<GameWindowData>(LifetimeToken);

            // TODO: выгрузка игрового поля
            // TODO: выгрузка контроллера ввода

            Container.gameObject.SetActive(false);
        }
    }

    public sealed class GameCycleService : IDisposable
    {
        [Inject] private WindowService _windowService;

        private Transform _menuContainer;
        private Transform _gameContainer;

        private BaseGameState _currentState;
        private GameState _currentStateType;
        private bool _isTransitioning;

        private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();

        public async UniTask InitializeAsync(CancellationToken token)
        {
            // Создаём контейнеры кодом — никакого DI, никаких ссылок из инспектора
            _menuContainer = CreateContainer("MenuContainer");
            _gameContainer = CreateContainer("GameContainer");

            await TransitionTo(GameState.Menu, token);
        }

        private static Transform CreateContainer(string name)
        {
            var go = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(go);
            return go.transform;
        }

        public async UniTask TransitionTo(GameState targetState, CancellationToken token = default)
        {
            if (_isTransitioning || (_currentState != null && _currentStateType == targetState))
                return;

            _isTransitioning = true;

            try
            {
                if (_currentState != null)
                {
                    await _currentState.Exit();
                    _currentState.Dispose();
                    _currentState = null;
                }

                _currentState = CreateState(targetState);
                _currentStateType = targetState;

                await _currentState.Enter();
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        private BaseGameState CreateState(GameState state) => state switch
        {
            GameState.Menu => new MenuGameState(_menuContainer, _windowService, _lifetimeCts.Token),
            GameState.Game => new GameplayGameState(_gameContainer, _windowService, _lifetimeCts.Token),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };

        public void Dispose()
        {
            _lifetimeCts.Cancel();
            _lifetimeCts.Dispose();

            _currentState?.Dispose();

            if (_menuContainer != null) UnityEngine.Object.Destroy(_menuContainer.gameObject);
            if (_gameContainer != null) UnityEngine.Object.Destroy(_gameContainer.gameObject);
        }
    }
}