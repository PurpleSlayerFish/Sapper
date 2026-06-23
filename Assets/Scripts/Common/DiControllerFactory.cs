using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public abstract class BaseUiElementView : MonoBehaviour, IView, IDisposable
{
    public CanvasGroup CanvasGroup;
    private RectTransform _rectTransform;

    public RectTransform RectTransform => _rectTransform != null
        ? _rectTransform
        : _rectTransform = (RectTransform)transform;

    public GameObject GameObject => gameObject;

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

public abstract class BaseGameObjectView : MonoBehaviour, IView, IDisposable
{
    public GameObject GameObject => gameObject;
    
    public virtual void Init()
    {
    }

    public virtual void Dispose()
    {
        if (this != null)
            Destroy(gameObject);
    }
}

public interface IView
{
    GameObject GameObject { get; }
    void Init();
    void Dispose();
}

public abstract class AbstractController<TView> : IController<TView>, IDisposable
    where TView : IView
{
    private bool _isDisposed;
    protected List<IDisposable> Disposables { get; } = new();
    public TView View { get; protected set; }

    public virtual void Refresh() => OnRefresh();
    protected virtual void OnRefresh(){}
    public virtual void OnAfterInit(){}


    public void SetActive(bool value)
    {
        if (View != null)
            View.GameObject.SetActive(value);
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

public abstract class BaseUiElementController<TView> : AbstractController<TView>, IControllerWithView<TView>
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

public abstract class BaseControllerWithView<TView> : AbstractController<TView>, IControllerWithView<TView>
    where TView : BaseGameObjectView
{
    public void Init(TView view)
    {
        View = view;
        View.Init();
        OnAfterInit();
        Refresh();
    }
}

public abstract class BaseControllerWithViewAndData<TView, TData> : AbstractController<TView>, IControllerWithViewAndData<TView,TData>
    where TView : IView
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

public interface IController
{
}

public interface IController<TView> : IController
    where TView : IView
{
    TView View { get; }
    void SetActive(bool value);
}

public interface IControllerWithView<TView> : IController<TView>
    where TView : IView
{
    void Init(TView  view);
}

public interface IControllerWithViewAndData<TView, TData> : IController<TView>
    where TView : IView
{
    TData Data { get; }
    void Init(TView  view, TData data);
}

public interface IControllerFactory
{
    T Create<T>() where T : class, IController;
    IController Create(Type type);
}

public sealed class DiControllerFactory : IControllerFactory
{
    private readonly DiContainer _container;

    public DiControllerFactory(DiContainer container)
    {
        _container = container;
    }

    public T Create<T>() where T : class, IController => Create(typeof(T)) as T;

    public IController Create(Type type)
    {
        return _container.Instantiate(type) as IController;
    }
}