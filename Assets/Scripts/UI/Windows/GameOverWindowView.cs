using Cysharp.Threading.Tasks;
using Services;
using TMPro;
using UI.Base;
using Zenject;

namespace UI.Windows
{
    public class GameOverWindowData : WindowData
    {
        public readonly string Message;

        public GameOverWindowData(string message)
        {
            Message = message;
        }
    }

    public class GameOverWindowView : BaseWindowView
    {
        public TMP_Text MessageText;
        public UiButtonView MainMenuButton;
        public UiButtonView RestartButton;
    }

    public class GameOverWindowController : BaseWindowController<GameOverWindowView, GameOverWindowData>
    {
        [Inject] private GameCycleService _gameCycleService;

        [Inject]
        public GameOverWindowController(GameOverWindowView view, GameOverWindowData data) : base(view, data) { }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            View.MessageText.text = Data.Message;

            if (View.MainMenuButton != null)
                Disposables.Add(View.MainMenuButton.Subscribe(HandleMainMenuClicked));
            
            if (View.RestartButton != null)
                Disposables.Add(View.RestartButton.Subscribe(HandleRestartClicked));
        }

        private void HandleMainMenuClicked() => _gameCycleService.TransitionTo(GameState.Menu).Forget();
        
        private void HandleRestartClicked() => _gameCycleService.RestartGame().Forget();
    }
}
