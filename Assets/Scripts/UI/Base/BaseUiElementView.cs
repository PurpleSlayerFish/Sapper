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

public abstract class BaseUiElementController<TView> : IUiElementController<TView>, IDisposable
    where TView : BaseUiElementView
{
    protected List<IDisposable> Disposables { get; } = new();
    protected TView View { get; set; }

    public void SetActive(bool value)
    {
        if (View)
            View.gameObject.SetActive(value);
    }

    private bool _isDisposed;

    public void Init(TView view)
    {
        View = view;
        View.Init();
        Disposables.Add(view);
        OnInit();
        Refresh();
    }

    public virtual void Refresh() => OnRefresh();
    protected virtual void OnInit(){}
    protected virtual void OnRefresh(){}

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
    
    void IUiElementController.AddToDisposables(IDisposable disposable) => Disposables.Add(disposable);
}

public abstract class BaseUiElementController<TView, TData>
    : BaseUiElementController<TView>
    where TView : BaseUiElementView
{ 
    protected TData Data { get; set; }
    public void Init(TView view, TData data)
    {
        View = view;
        View.Init();
        Data = data;
        Disposables.Add(view);
        OnInit(data);
        Refresh();
    }
    protected virtual void OnInit(TData data) { }
}

public interface IUiElementController
{
    void SetActive(bool value);
    internal void AddToDisposables(IDisposable disposable);
}

public interface IUiElementController<in TView>
    : IUiElementController
    where TView : BaseUiElementView
{
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