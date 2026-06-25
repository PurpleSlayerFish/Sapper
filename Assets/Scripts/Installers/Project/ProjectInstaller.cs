using Common;
using Installers.Scene;
using Model.Processors;
using Services;
using Zenject;

namespace Installers.Project
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // AsyncBootstrapper — единственный IInitializable в цепочке
            Container.BindInterfacesAndSelfTo<AsyncBootstrapper>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<AppLifetimeTokenService>()
                .AsSingle();

            // Фабрика окон
            Container.Bind<WindowControllerFactory>()
                .To<WindowControllerFactoryById>()
                .AsSingle();
            
            ConfigsInstaller.Install(Container);
            // todo перенести когда будут отдельные сцены
            SignalsInstaller.Install(Container);
            InputInstaller.Install(Container);
            Container.Bind<UiCameraService>().AsSingle();
            Container.Bind<WindowService>().AsSingle();
            // todo перенести когда будут отдельные сценыы
            Container.Bind<GameCycleService>().AsSingle();

            SignalBusInstaller.Install(Container);
            Container.Bind<AssetService>().AsSingle();
            Container.Bind<IControllerFactory>().To<DiControllerFactory>().AsSingle();

            WindowsInstaller.Install(Container);
        }
    }
}