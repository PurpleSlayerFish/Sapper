using Controller;
using Model.Entities;
using Model.Processors;
using Zenject;

namespace Installers.Scene
{
    // TODO пока не надо, если будет 
    // public sealed class GameFieldInstaller : Installer<GameFieldInstaller>
    // {
    //     public override void InstallBindings()
    //     {
    //         Container.Bind<GameFieldModel>()
    //             .AsSingle();
    //
    //         Container.BindInterfacesAndSelfTo<GameFieldController>()
    //             .AsSingle()
    //             .NonLazy();
    //
    //         Container.BindInterfacesAndSelfTo<GameFieldProcessor>()
    //             .AsSingle()
    //             .NonLazy();
    //     }
    // }
}