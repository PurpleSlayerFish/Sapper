using Services;
using UI.Base;
using Zenject;

namespace UI.Windows
{
    public class LoadingWindowData : WindowData
    {
    }

    public class LoadingWindowView : BaseWindowView
    {
    }

    public class LoadingWindowController : BaseWindowController<LoadingWindowView, LoadingWindowData>
    {
        [Inject] private WindowService _windowService;
        
        [Inject]
        public LoadingWindowController(LoadingWindowView view, LoadingWindowData data) : base(view, data) { }

    }
}