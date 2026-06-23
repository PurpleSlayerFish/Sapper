using UI;
using UnityEngine;
using Zenject;

public class MainMenuWindowData : WindowData
{
}

public class MainMenuWindowView : BaseWindowView
{
    public UiButtonView StartButton;
    public UiButtonView ExitButton;
}

public class MainMenuWindowController : BaseWindowController<MainMenuWindowView, MainMenuWindowData>
{
    protected override void OnInitialize()
    {
        base.OnInitialize();

        if (View.StartButton != null)
            Disposables.Add(View.StartButton.Subscribe(HandleStartClicked));

        if (View.ExitButton != null)
            Disposables.Add(View.ExitButton.Subscribe(HandleExitClicked));
    }

    private void HandleStartClicked()
    {
        // TODO: запуск игры
    }

    private void HandleExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}