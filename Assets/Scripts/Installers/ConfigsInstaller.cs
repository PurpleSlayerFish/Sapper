using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Installers
{
    public sealed class ConfigsInstaller : MonoInstaller
    {
        private const string ConfigsLabel = "Configs";

        public override void InstallBindings()
        {
            var handle = Addressables.LoadAssetsAsync<ScriptableObject>(ConfigsLabel, null);
            var configs = handle.WaitForCompletion();

            foreach (var config in configs)
                BindConfig(config);
        }

        private void BindConfig(ScriptableObject config)
        {
            var concreteType = config.GetType();
            Container
                .Bind(concreteType)
                .FromInstance(config)
                .AsSingle()
                .NonLazy();
        }
    }
}