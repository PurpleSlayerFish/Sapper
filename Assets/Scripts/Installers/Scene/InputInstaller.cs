using Model.Processors;
using Services;
using Zenject;

namespace Installers.Scene
{
    public sealed class InputInstaller : Installer<InputInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<InputService>()
                .AsSingle();
        }
    }
}