using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

public abstract class BaseUiElementView<TData> : BaseUiElementView
    where TData : struct
{
}

public abstract class BaseUiElementView : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _showDuration = 0.15f;
    [SerializeField] private float _hideDuration = 0.15f;

    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform != null
        ? _rectTransform
        : _rectTransform = (RectTransform)transform;

    public virtual void Initialize()
    {
        if (_canvasGroup == null)
            return;

        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public virtual async UniTask PlayShowAsync(CancellationToken token)
    {
        gameObject.SetActive(true);

        if (_canvasGroup == null)
            return;

        _canvasGroup.blocksRaycasts = true;

        await _canvasGroup
            .DOFade(1f, _showDuration)
            .SetUpdate(UpdateType.Normal, true)
            .ToUniTask(cancellationToken: token);

        _canvasGroup.interactable = true;
    }

    public virtual async UniTask PlayHideAsync(CancellationToken token)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = false;

            await _canvasGroup
                .DOFade(0f, _hideDuration)
                .SetUpdate(UpdateType.Normal, true)
                .ToUniTask(cancellationToken: token);

            _canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    public virtual void Dispose()
    {
        if (_canvasGroup != null)
            DOTween.Kill(_canvasGroup);

        if (this != null)
            Destroy(gameObject);
    }
}

// Базовый контроллер без данных
public abstract class BaseUiElementController<TView> : IDisposable
    where TView : BaseUiElementView
{
    protected TView View { get; private set; }

    private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();
    private bool _isDisposed;

    // Пустой конструктор для наследника с датой, полностью переопределяющего инициализацию
    protected BaseUiElementController()
    {
    }

    protected BaseUiElementController(TView view)
    {
        View = view;

        View.Initialize();
        OnInitialize();
    }

    // Используется наследником с датой для установки вьюшки без повторной инициализации здесь
    protected void SetView(TView view) => View = view;

    // Хук для наследников без данных
    protected virtual void OnInitialize() { }

    public async UniTask Show(CancellationToken token)
    {
        if (_isDisposed)
            throw new ObjectDisposedException(GetType().Name);

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _lifetimeCts.Token);
        await View.PlayShowAsync(linkedCts.Token);
    }

    public async UniTask Hide(CancellationToken token)
    {
        if (_isDisposed)
            return;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _lifetimeCts.Token);
        await View.PlayHideAsync(linkedCts.Token);
    }

    protected virtual void OnDispose() { }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();

        OnDispose();
        View.Dispose();
    }
}

// Контроллер с данными, TData — структура. Полностью переопределяет инициализацию базового класса
public abstract class BaseUiElementController<TView, TData> : BaseUiElementController<TView>
    where TView : BaseUiElementView<TData>
    where TData : struct
{
    protected abstract void OnInitialize(TData data);
}

public interface IUiElementFactory<TController, TView>
    where TController : BaseUiElementController<TView>
    where TView : BaseUiElementView
{
    TController Init(TView view);
    void Dispose(TController controller);
}

public interface IUiElementFactory<TController, TView, TData>
    where TController : BaseUiElementController<TView, TData>
    where TView : BaseUiElementView<TData>
    where TData : struct
{
    TController Init(TView view, TData data);
    void Dispose(TController controller);
}

public class UiElementFactory<TController, TView> : IUiElementFactory<TController, TView>
    where TController : BaseUiElementController<TView>
    where TView : BaseUiElementView
{
    private readonly DiContainer _container;

    public UiElementFactory(DiContainer container)
    {
        _container = container;
    }

    public TController Init(TView view)
        => _container.Instantiate<TController>(new object[] { view });

    public void Dispose(TController controller)
        => controller?.Dispose();
}

public class UiElementFactory<TController, TView, TData> : IUiElementFactory<TController, TView, TData>
    where TController : BaseUiElementController<TView, TData>
    where TView : BaseUiElementView
    where TData : struct
{
    private readonly DiContainer _container;

    public UiElementFactory(DiContainer container)
    {
        _container = container;
    }

    public TController Init(TView view, TData data)
        => _container.Instantiate<TController>(new object[] { view, data });

    public void Dispose(TController controller)
        => controller?.Dispose();
}