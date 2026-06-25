using Services;
using UnityEngine;
using Zenject;

namespace Installers.Scene
{
    public sealed class SceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Todo когда переделаем на сцены - вернуть нормальный биндинг
            // InputInstaller.Install(Container);
            // GameFieldInstaller.Install(Container);
        }
    }
}