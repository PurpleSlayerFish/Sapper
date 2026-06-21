using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class PauseWindowData : WindowData
{
}

public class PauseWindowView : BaseWindowView
{
    public Button RestartButton;
    public Button MainMenuButton;
    public ReferenceButtonInterceptorView ReferenceButtonInterceptorView;
}

public class PauseWindowController : BaseWindowController<PauseWindowView, PauseWindowData>
{
    [Inject] private IUiElementFactory<ReferenceButtonInterceptorController, ReferenceButtonInterceptorView> _interceptorFactory;

    private ReferenceButtonInterceptorController _interceptorController;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (View.RestartButton != null)
            View.RestartButton.onClick.AddListener(HandleRestartClicked);

        if (View.MainMenuButton != null)
            View.MainMenuButton.onClick.AddListener(HandleMainMenuClicked);

        if (View.ReferenceButtonInterceptorView != null) 
            _interceptorController = _interceptorFactory.Init(View.ReferenceButtonInterceptorView);
    }

    private void HandleRestartClicked()
    {
        // TODO: рестарт уровня
    }

    private void HandleMainMenuClicked()
    {
        // TODO: переход в главное меню
    }

    protected override void OnDispose()
    {
        if (View.RestartButton != null)
            View.RestartButton.onClick.RemoveListener(HandleRestartClicked);

        if (View.MainMenuButton != null)
            View.MainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);

        if (_interceptorController != null)
            _interceptorFactory.Dispose(_interceptorController);

        base.OnDispose();
    }
}