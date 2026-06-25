using System;
using Common;
using Cysharp.Threading.Tasks;
using Services;
using UI.Base;
using UI.Elements;
using Zenject;

namespace UI.Windows
{
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
        [Inject] private InputService _inputService;

        private TimerController _timerController;

        [Inject]
        public GameWindowController(GameWindowView view, GameWindowData data) : base(view, data) { }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _timerController = _timerFactory.Create<TimerController>();
            _timerController.Init(View.TimerView, new TimerData(TimeSpan.Zero, 1));

            if (View.MenuButtonView != null)
                Disposables.Add(View.MenuButtonView.Subscribe(HandleMenuButtonClicked));
        }

        private void HandleMenuButtonClicked()
        {
            // todo made pauseService
            _inputService.IsActive = false;
            _windowService.Show(new PauseWindowData(), token: LifetimeCts.Token).Forget();
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
}