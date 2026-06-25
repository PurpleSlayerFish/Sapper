using System;
using Model.Configs;
using Model.Signals;
using Services;
using UnityEngine;
using Zenject;

namespace Model.Processors
{
    public sealed class GameInputProcessorService : IInitializable, IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private UiCameraService _uiCameraService;
        [Inject] private GameFieldSettings _settings;

        private readonly Plane _fieldPlane = new Plane(Vector3.back, Vector3.zero);
        private OnCellPointerSignal _cellSignal;

        private bool _isActive;

        public void Initialize()
        {
            _isActive = true;
            _signalBus.Subscribe<OnPointerSignal>(HandlePointerSignal);
        }

        private void HandlePointerSignal(OnPointerSignal signal)
        {
            if (!_isActive || signal.Phase == PointerPhase.Hold)
                return;

            var camera = _uiCameraService.Camera;
            if (camera == null) return;

            var ray = camera.ScreenPointToRay(signal.ScreenPosition);
            if (!_fieldPlane.Raycast(ray, out var distance)) return;

            var worldPoint = ray.GetPoint(distance);
            var col = Mathf.FloorToInt((worldPoint.x - _settings.FieldStartPosition.x) / _settings.CellSize);
            var row = Mathf.FloorToInt((worldPoint.y - _settings.FieldStartPosition.y) / _settings.CellSize);

            if (col < 0 || col >= _settings.Columns || row < 0 || row >= _settings.Rows)
                return;

            _cellSignal.Column = col;
            _cellSignal.Row    = row;
            _cellSignal.Button = signal.Button;
            _cellSignal.Phase  = signal.Phase;

            _signalBus.Fire(_cellSignal);
        }

        public void Dispose()
        {
            _isActive = false;
            _signalBus.TryUnsubscribe<OnPointerSignal>(HandlePointerSignal);
        }
    }
}