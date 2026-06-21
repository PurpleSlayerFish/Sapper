using Zenject;

public class LoadingWindowData : WindowData
{
}

public class LoadingWindowView : BaseWindowView
{
}

public class LoadingWindowController : BaseWindowController<LoadingWindowView, LoadingWindowData>
{
    [Inject] private WindowService _windowService;
}