using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Services;
using UI.Base;
using UI.Windows;
using UnityEngine;
using Zenject;

namespace Installers.Scene
{
    public sealed class WindowsInstaller : Installer<WindowsInstaller>
    {
        public override void InstallBindings()
        {
            // Фабрика окон
            Container.Bind<WindowControllerFactory>()
                .To<WindowControllerFactoryById>()
                .AsSingle();

            // Сервис окон
            Container.BindInterfacesAndSelfTo<WindowService>()
                .AsSingle()
                .NonLazy();

            // Биндим делегаты создания окон по Id = typeof(TData)
            BindWindow<MainMenuWindowController, MainMenuWindowView, MainMenuWindowData>();
            BindWindow<GameWindowController, GameWindowView, GameWindowData>();
            BindWindow<PauseWindowController, PauseWindowView, PauseWindowData>();
            BindWindow<LoadingWindowController, LoadingWindowView, LoadingWindowData>();
            BindWindow<GameOverWindowController, GameOverWindowView, GameOverWindowData>();
        }

        private void BindWindow<TController, TView, TData>()
            where TController : BaseWindowController<TView, TData>
            where TView : BaseWindowView
            where TData : WindowData
        {
            Container
                .Bind<Func<TData, CancellationToken, UniTask<IWindowController>>>()
                .WithId(typeof(TData))
                .FromInstance(async (data, token) =>
                {
                    var assetService = Container.Resolve<WindowsAssetService>();
                    var root = Container.ResolveId<Transform>("WindowsRoot");
                    var view = await assetService.Instantiate<TView>(root, token);
                    var controller = Container.Instantiate<TController>(new object[] {view, data});
                    return (IWindowController) controller;
                });
        }
    }
}