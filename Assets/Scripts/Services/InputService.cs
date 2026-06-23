using UnityEngine;
using Zenject;

namespace Services
{
    public sealed class InputService : ITickable
    {
        [Inject] private SignalBus _signalBus;

        // Кэшируем сигнал — не аллоцируем каждый тик
        private OnPointerSignal _signal;

        // Два кнопки: 0 = left, 1 = right
        private static readonly int[] Buttons = { 0, 1 };

        public void Tick()
        {
            foreach (var button in Buttons)
            {
                if (Input.GetMouseButtonDown(button))
                    Fire(button, PointerPhase.Down);
                else if (Input.GetMouseButton(button))
                    Fire(button, PointerPhase.Hold);
                else if (Input.GetMouseButtonUp(button))
                    Fire(button, PointerPhase.Up);
            }
        }

        private void Fire(int button, PointerPhase phase)
        {
            _signal.Button = button;
            _signal.Phase = phase;
            _signal.ScreenPosition = Input.mousePosition;
            _signalBus.Fire(_signal);
        }
    }
    
    public enum PointerPhase { Down, Hold, Up }

    public struct OnPointerSignal
    {
        public int Button;
        public PointerPhase Phase;
        public Vector2 ScreenPosition;
    }

    public struct OnCellPointerSignal
    {
        public int Column;
        public int Row;
        public int Button;
        public PointerPhase Phase;
    }
}