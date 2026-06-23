using Services;
using UnityEngine;
using Zenject;

namespace Installers.Scene
{
    public sealed class SceneInstaller : MonoInstaller
    {
        [SerializeField] private Transform _windowsRoot;
        [SerializeField] private Transform _menuContainer;
        [SerializeField] private Transform _gameContainer;
        [SerializeField] private Transform _uiRoot;

        public override void InstallBindings()
        {
            SignalsInstaller.Install(Container);

            // Корневые трансформы
            Container.Bind<Transform>()
                .WithId("WindowsRoot")
                .FromInstance(_windowsRoot);

            Container.Bind<Transform>()
                .WithId("MenuContainer")
                .FromInstance(_menuContainer);

            Container.Bind<Transform>()
                .WithId("GameContainer")
                .FromInstance(_gameContainer);

            Container.Bind<Transform>()
                .WithId("UiRoot")
                .FromInstance(_uiRoot);

            // Токен уровня сессии
            Container.BindInterfacesAndSelfTo<SessionLifetimeTokenService>()
                .AsSingle()
                .NonLazy();

            // Инсталлеры подсистем
            WindowsInstaller.Install(Container);
            InputInstaller.Install(Container);
            GameCycleInstaller.Install(Container);
        }
    }
}