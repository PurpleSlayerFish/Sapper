using Common;
using Cysharp.Threading.Tasks;
using Services;
using UI.Base;
using UI.Elements;
using Zenject;

namespace UI.Windows
{
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
        [Inject] private GameCycleService _gameCycleService;
        private ReferenceButtonInterceptorController _interceptorController;
        
        [Inject]
        public PauseWindowController(PauseWindowView view, PauseWindowData data) : base(view, data) { }
        
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

        private void HandleRestartClicked() => _gameCycleService.TransitionTo(GameState.Game).Forget();
    
        private void HandleMainMenuClicked() => _gameCycleService.TransitionTo(GameState.Menu).Forget();
    }
}