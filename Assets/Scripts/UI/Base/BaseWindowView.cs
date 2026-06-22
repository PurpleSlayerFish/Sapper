using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.Scripting;

public abstract class WindowData
{
}

public abstract class BaseWindowView : MonoBehaviour
{
    public UiButtonView CloseButtonView;
    [RequiredMember, SerializeField] public CanvasGroup CanvasGroup;
    [SerializeField] private float _showDuration = 0.25f;
    [SerializeField] private float _hideDuration = 0.2f;

    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform != null
        ? _rectTransform
        : _rectTransform = (RectTransform)transform;

    public virtual void Initialize()
    {
        CanvasGroup.alpha = 0f;
        CanvasGroup.interactable = false;
        CanvasGroup.blocksRaycasts = false;
    }

    public virtual async UniTask PlayShowAsync(CancellationToken token)
    {
        gameObject.SetActive(true);
        CanvasGroup.blocksRaycasts = true;

        await CanvasGroup
            .DOFade(1f, _showDuration)
            .SetUpdate(UpdateType.Normal, true)
            .ToUniTask(cancellationToken: token);

        CanvasGroup.interactable = true;
    }

    public virtual async UniTask PlayHideAsync(CancellationToken token)
    {
        CanvasGroup.interactable = false;

        await CanvasGroup
            .DOFade(0f, _hideDuration)
            .SetUpdate(UpdateType.Normal, true)
            .ToUniTask(cancellationToken: token);

        CanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public virtual void Dispose()
    {
        DOTween.Kill(CanvasGroup);
        if (this != null)
            Destroy(gameObject);
    }
}

public interface IWindowController : IDisposable
{
    RectTransform ViewTransform { get; }
    UniTask Show(CancellationToken token);
    UniTask Hide(CancellationToken token);
    UniTask Close(CancellationToken token);
}

public abstract class BaseWindowController<TView, TData> : IWindowController
    where TView : BaseWindowView
    where TData : WindowData
{
    protected readonly TView View;
    protected readonly TData Data;
    protected List<IDisposable> Disposables { get; } = new();

    private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();

    private bool _isShown;
    private bool _isDisposed;
    
    public RectTransform ViewTransform => View.RectTransform;

    protected BaseWindowController(TView view, TData data)
    {
        View = view;
        Data = data;

        View.Initialize();
        OnInitialize();
    }

    protected BaseWindowController()
    {
    }

    protected virtual void OnInitialize()
    {
        if (View.CloseButtonView)
            View.CloseButtonView.Subscribe(HandleCloseRequested);
    }

    private void HandleCloseRequested() => Close(_lifetimeCts.Token).Forget();

    public async UniTask Show(CancellationToken token)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        if (_isShown)
            return;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _lifetimeCts.Token);

        OnBeforeShow();
        await View.PlayShowAsync(linkedCts.Token);
        _isShown = true;
        OnAfterShow();
    }

    public async UniTask Hide(CancellationToken token)
    {
        if (_isDisposed || !_isShown)
            return;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _lifetimeCts.Token);

        OnBeforeHide();
        await View.PlayHideAsync(linkedCts.Token);
        _isShown = false;
        OnAfterHide();
    }

    public async UniTask Close(CancellationToken token)
    {
        if (_isDisposed)
            return;

        if (_isShown)
            await Hide(token);

        Dispose();
    }

    protected virtual void OnBeforeShow() { }
    protected virtual void OnAfterShow() { }
    protected virtual void OnBeforeHide() { }
    protected virtual void OnAfterHide() { }
    protected virtual void OnDispose() { }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();

        foreach (var disposable in Disposables) 
            disposable.Dispose();
        Disposables.Clear();
        
        OnDispose();
        View.Dispose();
    }
}