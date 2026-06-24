using Cysharp.Threading.Tasks;
using Services;
using TMPro;
using UI.Base;
using UnityEngine;
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
        public UiButtonView RestartButton;
        public UiButtonView MainMenuButton;
    }

    public class GameOverWindowController : BaseWindowController<GameOverWindowView, GameOverWindowData>, ITickable
    {
        [Inject] private GameCycleService _gameCycleService;
        private bool _listeningInput;

        [Inject]
        public GameOverWindowController(GameOverWindowView view, GameOverWindowData data) : base(view, data) { }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (View.MessageText != null)
                View.MessageText.text = Data.Message;

            if (View.RestartButton != null)
                Disposables.Add(View.RestartButton.Subscribe(HandleRestartClicked));

            if (View.MainMenuButton != null)
                Disposables.Add(View.MainMenuButton.Subscribe(HandleMainMenuClicked));
        }


        protected override void OnAfterShow()
        {
            _listeningInput = true;
        }

        protected override void OnBeforeHide()
        {
            _listeningInput = false;
        }

        public void Tick()
        {
            if (!_listeningInput)
                return;

            if (Input.anyKeyDown)
                HandleRestartClicked();
        }

        private void HandleRestartClicked() => _gameCycleService.TransitionTo(GameState.Game).Forget();

        private void HandleMainMenuClicked() => _gameCycleService.TransitionTo(GameState.Menu).Forget();
    }
}