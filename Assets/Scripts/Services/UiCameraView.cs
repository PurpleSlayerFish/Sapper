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
        [Inject] private PrefabsService _prefabsService;
        [Inject] private AppLifetimeTokenService _appToken;
        [Inject(Id = "UiRoot")] private Transform _uiRoot;


        private UiCameraView _cameraView;

        public Camera Camera => _cameraView != null ? _cameraView.Camera : null;

        public void Initialize() => InitCamera(_appToken.Token).Forget();

        private async UniTaskVoid InitCamera(CancellationToken token)
        {
            _cameraView = await _prefabsService.Instantiate<UiCameraView>(_uiRoot, token);
        }

        public void Dispose()
        {
            if (_cameraView != null)
            {
                UnityEngine.Object.Destroy(_cameraView.gameObject);
                _cameraView = null;
            }
        }
    }
}