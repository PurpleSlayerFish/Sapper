using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Services
{
    public sealed class UiCameraView : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Transform _uiRoot;

        public Camera Camera => _camera;
        public Transform UiRoot => _uiRoot;

        private void Awake()
        {
            // Overlay: камера рендерит UI поверх остальных камер
            _camera.clearFlags = CameraClearFlags.Depth;
            _camera.cullingMask = LayerMask.GetMask("UI");
            _camera.depth = 10;
            
            DontDestroyOnLoad(this);
        }
    }

    public sealed class UiCameraService : IDisposable
    {
        [Inject] private AssetService _assetService;
        [Inject] private DiContainer    _container;

        private UiCameraView _cameraView;

        public Camera    Camera  => _cameraView?.Camera;
        public Transform UiRoot  => _cameraView?.UiRoot;

        public async UniTask InitializeAsync(CancellationToken token)
        {
            _cameraView = await _assetService.Instantiate<UiCameraView>(null, token);
            
            _container.Bind<Transform>()
                .WithId("WindowsRoot")
                .FromInstance(_cameraView.UiRoot)
                .AsSingle();
        }

        public void Dispose()
        {
            if (_cameraView != null)
                UnityEngine.Object.Destroy(_cameraView.gameObject);
        }
    }
}