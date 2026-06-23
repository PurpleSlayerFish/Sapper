using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UI;
using Zenject;

public abstract class WindowData
{
}

public abstract class BaseWindowView : MonoBehaviour
{
    [SerializeField] private UiButtonView _closeButtonView;
    [RequiredMember, SerializeField] private CanvasGroup _canvasGroup;
    [RequiredMember, SerializeField] private Canvas _canvas;
    [RequiredMember, SerializeField] private GraphicRaycaster _graphicRaycaster;
    [SerializeField] private float _showDuration = 0.25f;
    [SerializeField] private float _hideDuration = 0.2f;

    private RectTransform _rectTransform;

    public Canvas Canvas => _canvas;
    public GraphicRaycaster GraphicRaycaster => _graphicRaycaster;
    public UiButtonView CloseButtonView => _closeButtonView;

    public RectTransform RectTransform => _rectTransform != null
        ? _rectTransform
        : _rectTransform = (RectTransform)transform;

    public virtual void Initialize()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void SetCamera(Camera uiCamera)
    {
        if (_canvas == null)
            return;

        _canvas.worldCamera = uiCamera;
        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
    }

    public virtual async UniTask PlayShowAsync(CancellationToken token)
    {
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        if (_graphicRaycaster != null)
            _graphicRaycaster.enabled = true;

        await _canvasGroup
            .DOFade(1f, _showDuration)
            .SetUpdate(UpdateType.Normal, true)
            .ToUniTask(cancellationToken: token);

        _canvasGroup.interactable = true;
    }

    public virtual async UniTask PlayHideAsync(CancellationToken token)
    {
        _canvasGroup.interactable = false;

        await _canvasGroup
            .DOFade(0f, _hideDuration)
            .SetUpdate(UpdateType.Normal, true)
            .ToUniTask(cancellationToken: token);

        _canvasGroup.blocksRaycasts = false;

        if (_graphicRaycaster != null)
            _graphicRaycaster.enabled = false;

        gameObject.SetActive(false);
    }

    public virtual void Dispose()
    {
        DOTween.Kill(_canvasGroup);
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
    [Inject] private UiCameraService _uiCameraService;

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
        // Устанавливаем UI-камеру для канваса окна
        if (_uiCameraService?.Camera != null)
            View.SetCamera(_uiCameraService.Camera);

        if (View.CloseButtonView)
            Disposables.Add(View.CloseButtonView.Subscribe(HandleCloseRequested));
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