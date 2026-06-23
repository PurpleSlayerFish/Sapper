using UI;
using Zenject;

public class GameWindowData : WindowData
{
}

public class GameWindowView : BaseWindowView
{
    public TimerView TimerView;
    public UiButtonView MenuButtonView;
}

public class GameWindowController : BaseWindowController<GameWindowView, GameWindowData>
{
    [Inject] private IControllerFactory _timerFactory;
    [Inject] private WindowService _windowService;

    private TimerController _timerController;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        _timerController = _timerFactory.Create<TimerController>();
        _timerController.Init(View.TimerView, new TimerData());

        if (View.MenuButtonView != null)
            Disposables.Add(View.MenuButtonView.Subscribe(HandleMenuButtonClicked));
    }

    private void HandleMenuButtonClicked()
    {
        // TODO: окно меню будет реализовано позже
        // _windowService.Show<MenuWindowData>(new MenuWindowData()).Forget();
    }

    protected override void OnAfterShow()
    {
        base.OnAfterShow();
        _timerController.Start();
    }

    protected override void OnBeforeHide()
    {
        base.OnBeforeHide();
        _timerController.Pause();
    }

    protected override void OnDispose()
    {
        if (!View.MenuButtonView)
            View.MenuButtonView.Unsubscribe(HandleMenuButtonClicked);

        _timerController.Dispose();

        base.OnDispose();
    }
}