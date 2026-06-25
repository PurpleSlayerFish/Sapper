using System;
using System.Threading;
using Controller;
using Cysharp.Threading.Tasks;
using Model.Configs;
using Model.Entities;
using Model.Processors;
using Model.Signals;
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

            await WindowService.ShowWithLoadingScreen(new MainMenuWindowData(), token: LifetimeToken);
        }

        public override async UniTask Exit()
        {
            await WindowService.Close<MainMenuWindowData>(LifetimeToken);
            Container.gameObject.SetActive(false);
        }
    }

    public sealed class GameplayGameState : BaseGameState
    {
        private readonly DiContainer _container;

        private GameFieldController _gameFieldController;
        private GameFieldProcessor _gameFieldProcessor;
        private GameFieldSettings _settings;
        private GameFieldModel _gameModel;
        private GameInputProcessorService _gameInputProcessorService;
        private SignalBus _signalBus;
        private const string ContainerId = "[GameContainer]";
        private const string WinMessage = "You win";
        private const string LoseMessage = "You lose";

        public GameplayGameState(
            Transform container,
            WindowService windowService,
            CancellationToken lifetimeToken,
            DiContainer diContainer)
            : base(container, windowService, lifetimeToken)
        {
            _container = diContainer;
        }


        public override async UniTask Enter()
        {
            
            await WindowService.ShowWithLoadingScreen(new GameWindowData(), LifetimeToken, AsyncLoad);
        }

        private async UniTask AsyncLoad()
        {
            // биндим трансформ контейнер
            Container.gameObject.SetActive(true);
            _container.Bind<Transform>().WithId(ContainerId).FromInstance(Container).AsCached();
            
            _settings = _container.Resolve<GameFieldSettings>();
            _signalBus = _container.Resolve<SignalBus>();
            _gameModel = new GameFieldModel(_settings.Columns, _settings.Rows);
            _container.Bind<GameFieldModel>().FromInstance(_gameModel).AsCached();
            
            _gameFieldController = _container.Instantiate<GameFieldController>();
            _gameFieldProcessor = _container.Instantiate<GameFieldProcessor>();
            _gameInputProcessorService = _container.Instantiate<GameInputProcessorService>();
            
            _signalBus.Subscribe<OnGameOverSignal>(OnGameOver);
            
            // Инициализируем вручную в нужном порядке
            await _gameFieldController.InitAsync(LifetimeToken);
            _gameFieldProcessor.Initialize();
            _gameInputProcessorService.Initialize();
        }

        private async void OnGameOver(OnGameOverSignal signal)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            await WindowService.Show(new GameOverWindowData(signal.IsWin ? WinMessage: LoseMessage), LifetimeToken);
        }

        public override async UniTask Exit()
        {
            await WindowService.Close<GameWindowData>(LifetimeToken);
            await WindowService.Close<PauseWindowData>(LifetimeToken);

            _gameInputProcessorService.Dispose();
            _gameFieldProcessor.Dispose();
            _gameFieldController.Dispose();
            
            _container.UnbindId<Transform>(ContainerId);
            _container.Unbind<GameFieldModel>();
            _gameModel = null;
            _signalBus.TryUnsubscribe<OnGameOverSignal>(OnGameOver);

            Container.gameObject.SetActive(false);
        }
    }

    public sealed class GameCycleService : IDisposable
    {
        [Inject] private WindowService _windowService;
        [Inject] private DiContainer _container;

        private Transform _menuContainer;
        private Transform _gameContainer;

        private BaseGameState _currentState;
        private GameState _currentStateType;
        private bool _isTransitioning;

        private readonly CancellationTokenSource _lifetimeCts = new();

        public async UniTask InitializeAsync(CancellationToken token)
        {
            // Создаём контейнеры кодом — никакого DI, никаких ссылок из инспектора
            _menuContainer = CreateContainer("MenuContainer");
            _gameContainer = CreateContainer("GameContainer");

            await TransitionTo(GameState.Menu);
        }

        private static Transform CreateContainer(string name)
        {
            var go = new GameObject(name);
            return go.transform;
        }

        public async UniTask TransitionTo(GameState targetState)
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
            GameState.Game => new GameplayGameState(_gameContainer, _windowService, _lifetimeCts.Token, _container),
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