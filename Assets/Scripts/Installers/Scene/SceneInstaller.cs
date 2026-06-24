using Services;
using UnityEngine;
using Zenject;

namespace Installers.Scene
{
    public sealed class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalsInstaller.Install(Container);

            // Токен уровня сессии
            Container.BindInterfacesAndSelfTo<SessionLifetimeTokenService>()
                .AsSingle()
                .NonLazy();

            // Инсталлеры подсистем
            WindowsInstaller.Install(Container);
            InputInstaller.Install(Container);
        }
    }
}