using TMPro;
using UI;
using UnityEngine;
using Zenject;

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

    // Проверка нажатия любой клавиши через ITickable подключается в инсталлере,
    // здесь используем Update через хук OnAfterShow/OnBeforeHide через флаг
    private bool _listeningInput;

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

    private void HandleRestartClicked()
    {
        // TODO: рестарт уровня
    }

    private void HandleMainMenuClicked()
    {
        // TODO: переход в главное меню
    }
}