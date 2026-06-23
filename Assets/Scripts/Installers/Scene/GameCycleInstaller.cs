using Services;
using Zenject;

namespace Installers.Scene
{
    public sealed class GameCycleInstaller : Installer<GameCycleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GameCycleService>()
                .AsSingle()
                .NonLazy();
        }
    }
}