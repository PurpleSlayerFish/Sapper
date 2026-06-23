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
    [Inject] private IControllerFactory _interceptorFactory;

    private ReferenceButtonInterceptorUiElementController _interceptorUiElementController;

    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (View.RestartButtonView != null)
            Disposables.Add(View.RestartButtonView.Subscribe(HandleRestartClicked));

        if (View.MainMenuButtonView != null)
            Disposables.Add(View.MainMenuButtonView.Subscribe(HandleMainMenuClicked));

        if (View.ReferenceButtonInterceptorView != null)
        {
            _interceptorUiElementController = _interceptorFactory.Create<ReferenceButtonInterceptorUiElementController>();
            _interceptorUiElementController.Init(View.ReferenceButtonInterceptorView);
            Disposables.Add(_interceptorUiElementController);
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