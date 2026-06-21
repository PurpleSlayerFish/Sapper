using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class GameWindowData : WindowData
{
}

public class GameWindowView : BaseWindowView
{
    [SerializeField] private TimerView _timerView;
    [SerializeField] private Button _menuButton;

    public TimerView TimerView => _timerView;
    public Button MenuButton => _menuButton;
}

public class GameWindowController : BaseWindowController<GameWindowView, GameWindowData>
{
    [Inject] private IUiElementFactory<TimerController, TimerView, TimerData> _timerFactory;
    [Inject] private WindowService _windowService;

    private TimerController _timerController;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        _timerController = _timerFactory.Init(View.TimerView, new TimerData());

        if (View.MenuButton != null)
            View.MenuButton.onClick.AddListener(HandleMenuButtonClicked);
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
        if (View.MenuButton != null)
            View.MenuButton.onClick.RemoveListener(HandleMenuButtonClicked);

        _timerFactory.Dispose(_timerController);

        base.OnDispose();
    }
}