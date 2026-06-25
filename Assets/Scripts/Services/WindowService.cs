using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UI.Base;
using UI.Windows;
using Zenject;

namespace Services
{
    public abstract class WindowControllerFactory
    {
        protected readonly DiContainer Container;

        protected WindowControllerFactory(DiContainer container)
        {
            Container = container;
        }

        public abstract UniTask<IWindowController> Create<TData>(TData data, CancellationToken token)
            where TData : WindowData;
    }

    public class WindowControllerFactoryById : WindowControllerFactory
    {
        public WindowControllerFactoryById(DiContainer container) : base(container)
        {
        }

        public override UniTask<IWindowController> Create<TData>(TData data, CancellationToken token)
            => Container.ResolveId<Func<TData, CancellationToken, UniTask<IWindowController>>>(typeof(TData))
                .Invoke(data, token);
    }

    internal sealed class WindowNode
    {
        public readonly IWindowController Controller;
        public readonly Type DataType;
        public readonly bool IsMultiple;

        public WindowNode(IWindowController controller, Type dataType, bool isMultiple)
        {
            Controller = controller;
            DataType = dataType;
            IsMultiple = isMultiple;
        }
    }

    public sealed class WindowService : IDisposable
    {
        [Inject] private WindowControllerFactory _windowFactory;
        [Inject] private AssetService _assetService;
        [Inject] private AppLifetimeTokenService _appToken;
        [Inject] private UiCameraService _uiCameraService;
        [Inject] private DiContainer _container;

        private readonly LinkedList<WindowNode> _windows = new LinkedList<WindowNode>();

        private LoadingWindowController _loadingScreen;

        public async UniTask InitializeAsync(CancellationToken token)
        {
            // WindowsRoot уже готов, потому что UiCameraService отработал до нас
            _loadingScreen = await _windowFactory.Create(new LoadingWindowData(), token) as LoadingWindowController;
        }

        public async UniTask Show<TData>(
            TData data,
            bool isMultiple = false,
            CancellationToken token = default)
            where TData : WindowData
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _appToken.Token);
            await ShowInternal(data, isMultiple, linkedCts.Token);
        }

        // На префабе LoadingWindowView Canvas.sortingOrder = 999
        public async UniTask ShowWithLoadingScreen<TData>(
            TData data,
            bool isMultiple = false,
            Func<UniTask> loadingTask = null,
            CancellationToken token = default)
            where TData : WindowData
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _appToken.Token);
            await _loadingScreen.Show(linkedCts.Token);
            if (loadingTask != null)
                await loadingTask.Invoke();
            await ShowInternal(data, isMultiple, linkedCts.Token);
            await _loadingScreen.Hide(linkedCts.Token);
        }

        private async UniTask ShowInternal<TData>(
            TData data,
            bool isMultiple,
            CancellationToken token)
            where TData : WindowData
        {
            var controller = await PrepareWindow(data, isMultiple, token);
            BringToFront(controller);
            await controller.Show(token);
        }

        private async UniTask<IWindowController> PrepareWindow<TData>(TData data, bool isMultiple,
            CancellationToken token)
            where TData : WindowData
        {
            if (!isMultiple)
            {
                var existingNode = FindNode(typeof(TData));
                if (existingNode != null)
                {
                    if (existingNode != _windows.Last)
                    {
                        _windows.Remove(existingNode);
                        _windows.AddLast(existingNode);
                    }

                    await HidePrevious(existingNode, token);
                    return existingNode.Value.Controller;
                }
            }

            var previousLast = _windows.Last;

            var controller = await _windowFactory.Create(data, token);
            var node = new WindowNode(controller, typeof(TData), isMultiple);

            _windows.AddLast(node);

            if (previousLast != null)
                await previousLast.Value.Controller.Hide(token);

            return controller;
        }

        public async UniTask Close<TData>(CancellationToken token = default) where TData : WindowData
            => await CloseInternal(typeof(TData), token);

        public async UniTask CloseTop(CancellationToken token = default)
        {
            var node = _windows.Last;
            if (node == null)
                return;

            await CloseNode(node, token);
        }

        private async UniTask CloseInternal(Type dataType, CancellationToken token)
        {
            var node = FindLastNode(dataType);
            if (node == null)
                return;

            await CloseNode(node, token);
        }

        private async UniTask CloseNode(LinkedListNode<WindowNode> node, CancellationToken token)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, _appToken.Token);

            var wasLast = node == _windows.Last;
            var previous = node.Previous;

            _windows.Remove(node);
            await node.Value.Controller.Close(linkedCts.Token);

            if (wasLast && previous != null)
            {
                BringToFront(previous.Value.Controller);
                await previous.Value.Controller.Show(linkedCts.Token);
            }
        }

        private async UniTask HidePrevious(LinkedListNode<WindowNode> node, CancellationToken token)
        {
            var previous = node.Previous;
            if (previous != null)
                await previous.Value.Controller.Hide(token);
        }

        private void BringToFront(IWindowController controller)
            => controller.ViewTransform.SetAsLastSibling();

        private LinkedListNode<WindowNode> FindNode(Type dataType)
        {
            for (var node = _windows.First; node != null; node = node.Next)
            {
                if (node.Value.DataType == dataType)
                    return node;
            }

            return null;
        }

        private LinkedListNode<WindowNode> FindLastNode(Type dataType)
        {
            for (var node = _windows.Last; node != null; node = node.Previous)
            {
                if (node.Value.DataType == dataType)
                    return node;
            }

            return null;
        }

        public void Dispose()
        {
            for (var node = _windows.First; node != null; node = node.Next)
                node.Value.Controller.Dispose();

            _windows.Clear();

            _loadingScreen?.Dispose();
            _loadingScreen = null;
        }
    }
}