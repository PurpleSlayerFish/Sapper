using UI;
using UnityEngine.UI;
using Zenject;

public class PauseWindowData : WindowData
{
}

public class PauseWindowView : BaseWindowView
{
    public UiButtonView RestartButtonView;
    public UiButtonView MainMenuButtonView;
    public ReferenceButtonInterceptorView ReferenceButtonInterceptorView;
}

public class PauseWindowController : BaseWindowController<PauseWindowView, PauseWindowData>
{
    [Inject] private IUiElementControllerFactory _interceptorFactory;

    private ReferenceButtonInterceptorController _interceptorController;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (View.RestartButtonView != null)
            Disposables.Add(View.RestartButtonView.Subscribe(HandleRestartClicked));

        if (View.MainMenuButtonView != null)
            Disposables.Add(View.MainMenuButtonView.Subscribe(HandleMainMenuClicked));

        if (View.ReferenceButtonInterceptorView != null)
        {
            _interceptorController = _interceptorFactory.Create<ReferenceButtonInterceptorController>();
            _interceptorController.Init(View.ReferenceButtonInterceptorView);
            Disposables.Add(_interceptorController);
        } 
    }

    private void HandleRestartClicked()
    {
        // TODO: рестарт уровня
    }

    private void HandleMainMenuClicked()
    {
        // TODO: переход в главное меню
    }
}