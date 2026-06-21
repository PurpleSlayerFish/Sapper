using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class WindowsInstaller : MonoInstaller
{
    [Inject] private WindowsAssetService _assetService;
    
    public override void InstallBindings()
    {
        Container.Bind<WindowControllerFactory>()
            .To<WindowControllerFactoryById>()
            .AsSingle();

        BindWindow<GameWindowController, GameWindowView, GameWindowData>();
        BindWindow<LoadingWindowController, LoadingWindowView, LoadingWindowData>();
        // ... остальные окна
    }

    private void BindWindow<TController, TView, TData>()
        where TController : BaseWindowController<TView, TData>
        where TView : BaseWindowView
        where TData : WindowData
    {
        Container.BindInstance<Func<TData, IWindowController>>(data =>
        {
            var view = _assetService
                .InitializeAsset<TView>(Container.Resolve<Transform>(), CancellationToken.None)
                .GetAwaiter().GetResult();

            return Container.Instantiate<TController>(new object[] { view, data });
        }).WithId(typeof(TData));
    }
}