using System;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using Model.Configs;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI.Elements
{
    public readonly struct TimerData
    {
        public readonly TimeSpan InitialTime;
        public readonly bool IsIncreasing;

        public TimerData(TimeSpan initialTime, float updateThreshold, bool isIncreasing = true)
        {
            InitialTime = initialTime;
            IsIncreasing = isIncreasing;
        }
    }

    public class TimerView : BaseUiElementView
    {
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private string _timeFormat = @"hh\:mm\:ss";
        public float UpdateThreshold = 1f;

        public string TimeFormat => _timeFormat;

        public void SetTime(TimeSpan time)
        {
            if (_timeText != null)
                _timeText.text = time.ToString(_timeFormat);
        }
    }

    public sealed class TimerController : BaseControllerWithViewAndData<TimerView, TimerData>
    {
        private CancellationTokenSource _cts;
        private bool _isRunning;
        private TimeSpan _currentTime;
        private double _sinceLastRedraw;

        public override void OnAfterInit()
        {
            _currentTime = Data.InitialTime;
            _cts = new CancellationTokenSource();
            View.SetTime(_currentTime);
            RunAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                if (!_isRunning)
                    continue;

                _sinceLastRedraw += Time.deltaTime;

                if (_sinceLastRedraw < View.UpdateThreshold)
                    continue;

                _currentTime = Data.IsIncreasing
                    ? _currentTime + TimeSpan.FromTicks((long)(_sinceLastRedraw * TimeSpan.TicksPerSecond))
                    : _currentTime - TimeSpan.FromTicks((long)(_sinceLastRedraw * TimeSpan.TicksPerSecond));

                if (!Data.IsIncreasing && _currentTime < TimeSpan.Zero)
                    _currentTime = TimeSpan.Zero;

                _sinceLastRedraw = 0d;
                View.SetTime(_currentTime);
            }
        }

        public void Start() => _isRunning = true;
        public void Pause()      => _isRunning = false;

        public void Reset()
        {
            _isRunning       = false;
            _sinceLastRedraw = 0d;
            _currentTime     = Data.InitialTime;
            View.SetTime(_currentTime);
        }

        public override void OnDispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
    }
}