using Common;
using Services;
using Zenject;

namespace Installers.Project
{
    public sealed class ProjectInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Токен уровня приложения
            Container.BindInterfacesAndSelfTo<AppLifetimeTokenService>()
                .AsSingle()
                .NonLazy();

            // Сигнал бас
            SignalBusInstaller.Install(Container);
            
            Container.Bind<ResourcesService>().AsSingle();
            Container.Bind<PrefabsService>().AsSingle();
            Container.Bind<WindowsAssetService>().AsSingle();
            Container.Bind<UiAssetService>().AsSingle();

            // Конфиги — грузятся один раз, биндятся по конкретному типу
            ConfigsInstaller.Install(Container);

            // Фабрика контроллеров — универсальная
            Container.Bind<IControllerFactory>()
                .To<DiControllerFactory>()
                .AsSingle();

            // Камера
            Container.BindInterfacesAndSelfTo<UiCameraService>()
                .AsSingle()
                .NonLazy();
        }
    }
}