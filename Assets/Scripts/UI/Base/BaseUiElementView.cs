using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public abstract class BaseUiElementView : MonoBehaviour, IDisposable
{
    public CanvasGroup CanvasGroup;
    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform != null
        ? _rectTransform
        : _rectTransform = (RectTransform)transform;

    public virtual void Init()
    {
    }

    public virtual void Dispose()
    {
        if (CanvasGroup != null)
            DOTween.Kill(CanvasGroup);

        if (this != null)
            Destroy(gameObject);
    }
}

public abstract class AbstractUiElementController<TView> : IUiElementController<TView>, IDisposable
    where TView : BaseUiElementView
{
    private bool _isDisposed;
    protected List<IDisposable> Disposables { get; } = new();
    public TView View { get; protected set; }

    public virtual void Refresh() => OnRefresh();
    protected virtual void OnRefresh(){}
    public abstract void OnAfterInit();


    public void SetActive(bool value)
    {
        if (View)
            View.gameObject.SetActive(value);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        foreach (var disposable in Disposables) 
            disposable.Dispose();
        Disposables.Clear();
        
        View.Dispose();
    }
}

public abstract class BaseUiElementController<TView> : AbstractUiElementController<TView>, IUiElementControllerWithView<TView>
    where TView : BaseUiElementView
{
    public void Init(TView view)
    {
        View = view;
        View.Init();
        OnAfterInit();
        Refresh();
    }
}

public abstract class BaseUiElementController<TView, TData> : AbstractUiElementController<TView>, IUiElementControllerWithViewAndData<TView,TData>
    where TView : BaseUiElementView
{ 
    public TData Data { get; protected set; }

    public void Init(TView view, TData data)
    {
        Data = data;
        View = view;
        View.Init();
        OnAfterInit();
        Refresh();
    }
}

public interface IUiElementController
{
}

public interface IUiElementController<TView> : IUiElementController
    where TView : BaseUiElementView
{
    TView View { get; }
    void SetActive(bool value);
    void OnAfterInit();
}

public interface IUiElementControllerWithView<TView> : IUiElementController<TView>
    where TView : BaseUiElementView
{
    void Init(TView  view);
}

public interface IUiElementControllerWithViewAndData<TView, TData> : IUiElementController<TView>
    where TView : BaseUiElementView
{
    TData Data { get; }
    void Init(TView  view, TData data);
}

public interface IUiElementControllerFactory
{
    T Create<T>() where T : class, IUiElementController;
    IUiElementController Create(Type type);
}

public sealed class UiElementControllerFactory : IUiElementControllerFactory
{
    private readonly DiContainer _container;

    public UiElementControllerFactory(DiContainer container)
    {
        _container = container;
    }

    public T Create<T>() where T : class, IUiElementController => Create(typeof(T)) as T;

    public IUiElementController Create(Type type)
    {
        return _container.Instantiate(type) as IUiElementController;
    }
}