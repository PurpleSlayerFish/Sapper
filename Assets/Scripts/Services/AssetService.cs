using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AssetReferenceExtensions
{
    public static string GetAddress(this AssetReference reference)
        => reference.RuntimeKey.ToString();
}

public abstract class BaseAssetService : IDisposable
{
    protected abstract string Section { get; }

    // Кэш загруженных ассетов по (тип, адрес)
    private readonly Dictionary<(Type, string), AsyncOperationHandle> _assetHandles =
        new Dictionary<(Type, string), AsyncOperationHandle>();

    // Трекинг инстансов: инстанс -> хэндл, чтобы можно было точечно релизить
    private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles =
        new Dictionary<GameObject, AsyncOperationHandle<GameObject>>();

    public async UniTask<T> Load<T>(string address, CancellationToken token) where T : UnityEngine.Object
    {
        var fullAddress = BuildAddress(address);
        var key         = (typeof(T), fullAddress);

        if (!_assetHandles.TryGetValue(key, out var rawHandle))
        {
            var handle = Addressables.LoadAssetAsync<T>(fullAddress);
            _assetHandles[key] = handle;
            rawHandle = handle;
        }

        // Ждём уже запущенный хэндл без повторной загрузки
        if (!rawHandle.IsDone)
            await rawHandle.ToUniTask(cancellationToken: token);

        return (T)rawHandle.Result;
    }

    public UniTask<T> Load<T>(AssetReference reference, CancellationToken token) where T : UnityEngine.Object
        => Load<T>(reference.GetAddress(), token);

    // Async Instantiate через Addressables — хэндл инстанса трекается отдельно
    public async UniTask<T> Instantiate<T>(string address, Transform parent, CancellationToken token)
        where T : Component
    {
        var fullAddress    = BuildAddress(address);
        var instanceHandle = Addressables.InstantiateAsync(fullAddress, parent);
        var go             = await instanceHandle.ToUniTask(cancellationToken: token);

        if (!go.TryGetComponent<T>(out var component))
        {
            // Если компонента нет — сразу релизим инстанс и бросаем исключение
            Addressables.ReleaseInstance(instanceHandle);
            throw new InvalidOperationException($"Prefab '{fullAddress}' has no component {typeof(T).Name}");
        }

        _instanceHandles[go] = instanceHandle;
        return component;
    }

    public UniTask<T> Instantiate<T>(AssetReference reference, Transform parent, CancellationToken token)
        where T : Component
        => Instantiate<T>(reference.GetAddress(), parent, token);
    
    public UniTask<T> Instantiate<T>(Transform parent, CancellationToken token)
        where T : Component
        => Instantiate<T>(typeof(T).Name, parent, token);

    // Ручная выгрузка конкретного инстанса
    public void ReleaseInstance(GameObject instance)
    {
        if (!_instanceHandles.TryGetValue(instance, out var handle))
            return;

        if (handle.IsValid())
            Addressables.ReleaseInstance(handle);

        _instanceHandles.Remove(instance);
    }

    public void ReleaseInstance<T>(T component) where T : Component
        => ReleaseInstance(component.gameObject);

    // Ручная выгрузка загруженного ассета (не инстанса)
    public void Release<T>(string address) where T : UnityEngine.Object
    {
        var key = (typeof(T), BuildAddress(address));

        if (!_assetHandles.TryGetValue(key, out var handle))
            return;

        if (handle.IsValid())
            Addressables.Release(handle);

        _assetHandles.Remove(key);
    }

    public void Release<T>(AssetReference reference) where T : UnityEngine.Object
        => Release<T>(reference.GetAddress());

    public void Dispose()
    {
        foreach (var handle in _instanceHandles.Values)
            if (handle.IsValid())
                Addressables.ReleaseInstance(handle);
        _instanceHandles.Clear();

        foreach (var handle in _assetHandles.Values)
            if (handle.IsValid())
                Addressables.Release(handle);
        _assetHandles.Clear();
    }

    private string BuildAddress(string address)
    {
        if (string.IsNullOrEmpty(Section) || address.Contains("/"))
            return address;

        return $"{Section}/{address}";
    }
}

public sealed class UiAssetService      : BaseAssetService { protected override string Section => "UI";     }
public sealed class WindowsAssetService : BaseAssetService { protected override string Section => "Windows"; }
public sealed class CommonAssetService  : BaseAssetService { protected override string Section => "Common";  }
public sealed class ResourcesService    : BaseAssetService { protected override string Section => "Game";    }
public sealed class ConfigAssetService  : BaseAssetService { protected override string Section => string.Empty; }