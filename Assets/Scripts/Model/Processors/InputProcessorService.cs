using Model.Configs;
using Services;
using UnityEngine;
using Zenject;

namespace Model.Processors
{
    public sealed class InputProcessorService : IInitializable, System.IDisposable
    {
        [Inject] private SignalBus _signalBus;
        [Inject] private UiCameraService _uiCameraService;
        [Inject] private GameFieldSettings _fieldSettings;

        // Плейн в мировых координатах, перпендикулярный камере Z=0 для 2D
        private readonly Plane _fieldPlane = new Plane(Vector3.back, Vector3.zero);

        private OnCellPointerSignal _cellSignal;

        public void Initialize()
        {
            _signalBus.Subscribe<OnPointerSignal>(HandlePointerSignal);
        }

        private void HandlePointerSignal(OnPointerSignal signal)
        {
            // Down и Up обрабатываем, Hold — только если нужно (можно расширить)
            if (signal.Phase == PointerPhase.Hold)
                return;

            var camera = _uiCameraService.Camera;
            if (camera == null)
                return;

            var ray = camera.ScreenPointToRay(signal.ScreenPosition);

            // Рейкаст по математическому плейну — ноль коллайдеров, ноль GC
            if (!_fieldPlane.Raycast(ray, out var distance))
                return;

            var worldPoint = ray.GetPoint(distance);

            // Вычисляем координаты клетки из мировой позиции
            var localX = worldPoint.x - _fieldSettings.OriginWorld.x;
            var localY = worldPoint.y - _fieldSettings.OriginWorld.y;

            var col = Mathf.FloorToInt(localX / _fieldSettings.CellSize);
            var row = Mathf.FloorToInt(localY / _fieldSettings.CellSize);

            // Отбрасываем клики вне поля
            if (col < 0 || col >= _fieldSettings.Columns || row < 0 || row >= _fieldSettings.Rows)
                return;

            _cellSignal.Column = col;
            _cellSignal.Row = row;
            _cellSignal.Button = signal.Button;
            _cellSignal.Phase = signal.Phase;

            _signalBus.Fire(_cellSignal);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<OnPointerSignal>(HandlePointerSignal);
        }
    }
}