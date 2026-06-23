using System;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using Model.Configs;
using Model.Entities;
using Model.Signals;
using Services;
using UnityEngine;
using View;
using Zenject;

namespace Controller
{
    public sealed class GameFieldController : IInitializable, IDisposable
    {
        [Inject] private IControllerFactory _controllerFactory;
        [Inject] private ResourcesService _resourcesService;
        [Inject] private PrefabsService _prefabsService;
        [Inject] private GameFieldSettings _settings;
        [Inject] private SignalBus _signalBus;
        [Inject] private SessionLifetimeTokenService _sessionToken;
        [Inject(Id = "GameContainer")] private Transform _gameContainer;

        private GameFieldModel _model;
        private CellController[] _cellControllers;
        private CellSprites _cellSprites;


        public void Initialize() => InitAsync(_sessionToken.Token).Forget();

        private async UniTaskVoid InitAsync(CancellationToken token)
        {
            _model = new GameFieldModel(_settings.Columns, _settings.Rows);

            await LoadSprites(token);

            _cellControllers = new CellController[_settings.Columns * _settings.Rows];

            var cellData = new CellData {Sprites = _cellSprites};

            for (var col = 0; col < _settings.Columns; col++)
            {
                for (var row = 0; row < _settings.Rows; row++)
                {
                    var worldPos = new Vector3(
                        _settings.OriginWorld.x + col * _settings.CellSize,
                        _settings.OriginWorld.y + row * _settings.CellSize,
                        0f);

                    var cellView = await _prefabsService.Instantiate<CellView>(_gameContainer, token);
                    cellView.Setup(col, row, worldPos);

                    var controller = _controllerFactory.Create<CellController>();
                    controller.Init(cellView, cellData);

                    _cellControllers[col * _settings.Rows + row] = controller;
                }
            }

            _signalBus.Subscribe<OnCellStateChangedSignal>(HandleCellStateChanged);
        }

        private async UniTask LoadSprites(CancellationToken token)
        {
            _cellSprites = new CellSprites
            {
                Hidden = await _resourcesService.Load<Sprite>(_settings.SpriteHidden, token),
                Flagged = await _resourcesService.Load<Sprite>(_settings.SpriteFlagged, token),
                Mine = await _resourcesService.Load<Sprite>(_settings.SpriteMine, token),
                Revealed = await _resourcesService.Load<Sprite>(_settings.SpriteRevealed, token),
                Numbers = new Sprite[8]
            };

            for (var i = 0; i < 8; i++)
                _cellSprites.Numbers[i] = await _resourcesService.Load<Sprite>(_settings.SpriteNumbers[i], token);
        }

        private void HandleCellStateChanged(OnCellStateChangedSignal signal)
        {
            _cellControllers[signal.Col * _settings.Rows + signal.Row]
                .ApplyState(signal.State, signal.AdjacentMines, signal.IsMine);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<OnCellStateChangedSignal>(HandleCellStateChanged);

            if (_cellControllers != null)
                foreach (var c in _cellControllers)
                    c?.Dispose();

            _resourcesService.Dispose();
        }
    }
}