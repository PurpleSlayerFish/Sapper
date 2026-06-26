using Model.Signals;
using Zenject;

namespace Installers.Scene
{
    public sealed class SignalsInstaller : Installer<SignalsInstaller>
    {
        public override void InstallBindings()
        {
            Container.DeclareSignal<OnPointerSignal>();
            Container.DeclareSignal<OnCellPointerSignal>();
            Container.DeclareSignal<OnCellStateChangedSignal>();
            Container.DeclareSignal<OnGameOverSignal>();
            Container.DeclareSignal<OnAnyKeySignal>();
        }
    }
}