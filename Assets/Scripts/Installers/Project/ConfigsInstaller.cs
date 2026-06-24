using Installers.Scene;
using Model.Configs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Installers.Project
{
    public sealed class ConfigsInstaller : Installer<ConfigsInstaller>
    {
        private static readonly string[] ConfigAddresses =
        {
            nameof(GameFieldSettings),
            // сюда добавляем новые конфиги по мере появления
        };

        public override void InstallBindings()
        {
            foreach (var address in ConfigAddresses)
            {
                var handle = Addressables.LoadAssetAsync<ScriptableObject>(address);
                var config = handle.WaitForCompletion();
                BindConfig(config);
            }
        }

        private void BindConfig(ScriptableObject config)
        {
            Container
                .Bind(config.GetType())
                .FromInstance(config)
                .AsSingle()
                .NonLazy();
        }
    }
}