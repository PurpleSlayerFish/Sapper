using System;
using TMPro;
using UnityEngine;
using Zenject;

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

public class TimerView : BaseUiElementView<TimerData>
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

public class TimerController : BaseUiElementController<TimerView, TimerData>, ITickable
{
    private TimeSpan _currentTime;
    private bool _isIncreasing;
    private bool _isRunning;
    private double _accumulatedSeconds;
    private double _sinceLastRedraw;

    protected override void OnInitialize(TimerData data)
    {
        _currentTime = data.InitialTime;
        _isIncreasing = data.IsIncreasing;
        _isRunning = false;
        _accumulatedSeconds = 0d;
        _sinceLastRedraw = 0d;

        View.SetTime(_currentTime);
    }

    public void Start() => _isRunning = true;

    public void Pause() => _isRunning = false;

    public void Reset()
    {
        _isRunning = false;
        _accumulatedSeconds = 0d;
        _sinceLastRedraw = 0d;
        View.SetTime(_currentTime);
    }

    public void Tick()
    {
        if (!_isRunning)
            return;

        var delta = Time.deltaTime;
        _accumulatedSeconds += delta;
        _sinceLastRedraw += delta;

        if (_sinceLastRedraw < View.UpdateThreshold)
            return;

        var elapsed = TimeSpan.FromSeconds(_accumulatedSeconds);
        _currentTime = _isIncreasing
            ? _currentTime + elapsed
            : _currentTime - elapsed;

        if (!_isIncreasing && _currentTime < TimeSpan.Zero)
            _currentTime = TimeSpan.Zero;

        _accumulatedSeconds = 0d;
        _sinceLastRedraw = 0d;

        View.SetTime(_currentTime);
    }
}