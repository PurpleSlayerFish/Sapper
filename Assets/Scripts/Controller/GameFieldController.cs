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
    public sealed class GameFieldController : IDisposable
    {
        [Inject] private IControllerFactory _controllerFactory;
        [Inject] private AssetService _assetService;
        [Inject] private GameFieldSettings _settings;
        [Inject] private SignalBus _signalBus;
        [Inject] private DiContainer _diContainer;
        [Inject(Id = "[GameContainer]")] private Transform _gameContainer;

        private CellController[] _cellControllers;
        private CellSprites _cellSprites;
        private CancellationTokenSource _cts = new();

        public async UniTask InitAsync(CancellationToken token)
        {
            await LoadSprites(token);

            _cellControllers = new CellController[_settings.Columns * _settings.Rows];

            var cellData = new CellData {Sprites = _cellSprites};

            for (var col = 0; col < _settings.Columns; col++)
            {
                for (var row = 0; row < _settings.Rows; row++)
                {
                    var worldPos = new Vector3(
                        _settings.FieldStartPosition.x + col * _settings.CellSize,
                        _settings.FieldStartPosition.y + row * _settings.CellSize,
                        0f);

                    var cellView = await _assetService.Instantiate<CellView>(_gameContainer, token);
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
                Hidden = await _assetService.Load<Sprite>(_settings.SpriteHidden, token),
                Flagged = await _assetService.Load<Sprite>(_settings.SpriteFlagged, token),
                Mine = await _assetService.Load<Sprite>(_settings.SpriteMine, token),
                CurrentMine = await _assetService.Load<Sprite>(_settings.SpriteCurrentMine, token),
                Revealed = await _assetService.Load<Sprite>(_settings.SpriteRevealed, token),
                Numbers = new Sprite[8]
            };

            for (var i = 0; i < 8; i++)
                _cellSprites.Numbers[i] = await _assetService.Load<Sprite>(_settings.SpriteNumbers[i], token);
        }

        private void HandleCellStateChanged(OnCellStateChangedSignal signal)
        {
            _cellControllers[signal.Col * _settings.Rows + signal.Row]
                .ApplyState(signal.State, signal.AdjacentMines, signal.IsMine, signal.IsCurrent);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            _signalBus.TryUnsubscribe<OnCellStateChangedSignal>(HandleCellStateChanged);

            if (_cellControllers != null)
                foreach (var c in _cellControllers)
                    c?.Dispose();

        }
    }
}