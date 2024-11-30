using GMVC.Core;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class AppLaunch : MonoBehaviour
{
    public DrawingBoardManager DrawingBoardManager;
    public AudioManager AudioManager;
    public UiManager UiManager;
    public MainThreadDispatcher MainThreadDispatcher;
    public ExceptionHandler ExceptionHandler;
    World world;
    void Start()
    {
        world = new World(DrawingBoardManager);
        App.Run(OnGameStart, world, AudioManager, MainThreadDispatcher);
        ExceptionHandler.OnError += ApplicationError;
    }

    void ApplicationError(string errorMessage)
    {
        MainThreadDispatcher.Enqueue(() => UiManager.OnLogMessage(errorMessage));
    }

    void OnGameStart()
    {
        EnhancedTouchSupport.Enable();
        DrawingBoardManager.Init();
        UiManager.Init();
        world.Init();
    }
}