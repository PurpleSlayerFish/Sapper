using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public sealed class UiCameraView : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    public Camera Camera => _camera;

    private void Awake()
    {
        // Overlay: камера рендерит UI поверх остальных камер
        _camera.clearFlags = CameraClearFlags.Depth;
        _camera.cullingMask = LayerMask.GetMask("UI");
        _camera.depth = 10;
    }
}

public sealed class UiCameraService : IInitializable, IDisposable
{
    [Inject] private CommonAssetService _commonAssetService;
    [Inject(Id = "UiRoot")] private Transform _uiRoot;

    private readonly CancellationTokenSource _lifetimeCts = new CancellationTokenSource();

    private UiCameraView _cameraView;

    public Camera Camera => _cameraView != null ? _cameraView.Camera : null;

    public void Initialize() => InitCamera(_lifetimeCts.Token).Forget();

    private async UniTaskVoid InitCamera(CancellationToken token)
    {
        // Имя ассета == имя вьюшки "UiCameraView"
        _cameraView = await _commonAssetService.InitializeAsset<UiCameraView>(_uiRoot, token);
    }

    public void Dispose()
    {
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();

        if (_cameraView != null)
        {
            UnityEngine.Object.Destroy(_cameraView.gameObject);
            _cameraView = null;
        }
    }
}