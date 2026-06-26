using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Model.Signals;
using UnityEngine;
using Zenject;

namespace Services
{
    public sealed class InputService : IInitializable
    {
        [Inject] private SignalBus _signalBus;
        private static readonly int[] Buttons = { 0, 1 };
        private OnPointerSignal _signal;
        private CancellationTokenSource _cts;

        private const float InputCooldown = 0.3f;
        
        public bool IsActive { get; set; }
        
        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            TickAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid TickAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2))
                {
                    _signalBus.Fire<OnAnyKeySignal>();
                    await UniTask.Delay(TimeSpan.FromSeconds(InputCooldown), cancellationToken: token);
                    continue;
                }
                
                if (!IsActive)
                    continue;
                foreach (var button in Buttons)
                {
                    if (Input.GetMouseButtonDown(button))
                    {
                        Fire(button, PointerPhase.Down);
                    }
                    else if (Input.GetMouseButton(button))
                    {
                        Fire(button, PointerPhase.Hold);
                    }
                    else if (Input.GetMouseButtonUp(button))
                    {
                        Fire(button, PointerPhase.Up);
                    }
                }
            }
        }

        private void Fire(int button, PointerPhase phase)
        {
            _signal.Button = button;
            _signal.Phase = phase;
            _signal.ScreenPosition = Input.mousePosition;
            _signalBus.Fire(_signal);
        }
        
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}