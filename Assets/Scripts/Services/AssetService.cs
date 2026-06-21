using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public abstract class AssetService : IDisposable
{
    // Раздел Addressables, задаётся константой в наследнике
    protected abstract string Section { get; }

    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedHandles = new();

    // Инициализация ассет-вью, имя ассета строго совпадает с именем компонента TAsset
    public UniTask<TAsset> InitializeAsset<TAsset>(Transform parent, CancellationToken token)
        where TAsset : Component
        => InitializeAsset<TAsset>(typeof(TAsset).Name, parent, token);

    // Инициализация ассет-вью по явно переданному имени
    public async UniTask<TAsset> InitializeAsset<TAsset>(string assetName, Transform parent, CancellationToken token)
        where TAsset : Component
    {
        var address = BuildAddress(assetName);

        if (!_loadedHandles.TryGetValue(address, out var handle))
        {
            handle = Addressables.LoadAssetAsync<GameObject>(address);
            _loadedHandles[address] = handle;
        }

        var prefab = await handle.ToUniTask(cancellationToken: token);

        var instance = parent != null
            ? UnityEngine.Object.Instantiate(prefab, parent)
            : UnityEngine.Object.Instantiate(prefab);

        if (!instance.TryGetComponent(out TAsset asset))
            throw new InvalidOperationException($"Asset '{address}' has no component {typeof(TAsset).Name}");

        return asset;
    }

    private string BuildAddress(string assetName) => $"{Section}/{assetName}";

    public void Dispose()
    {
        foreach (var handle in _loadedHandles.Values)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        _loadedHandles.Clear();
    }
}

public sealed class UiAssetService : AssetService
{
    private const string SectionName = "UI";

    protected override string Section => SectionName;
}

public sealed class WindowsAssetService : AssetService
{
    private const string SectionName = "Windows";

    protected override string Section => SectionName;
}