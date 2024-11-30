using GMVC.Core;
using GMVC.Utls;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;

public class AppSetting
{
    public bool CircuitIsRoad = false;
    public float DrawingPadAlign = 0;
    public bool GenerateCutPathOnError = true;
}
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
        var json = PlayerPrefs.GetString(App.Pref_Settings);
        var settings = Json.Deserialize<AppSetting>(json) ?? new AppSetting();
        App.Run(OnGameStart, world, AudioManager, MainThreadDispatcher, settings);
        ExceptionHandler.OnError += ApplicationError;

        void OnGameStart()
        {
            EnhancedTouchSupport.Enable();
            DrawingBoardManager.Init();
            UiManager.Init();
            world.Init();
            var controller = App.GetController<GameController>();
            controller.AlignDrawingPad();
            UiManager.StartSettings();
        }
    }

    void ApplicationError(string errorMessage)
    {
        MainThreadDispatcher.Enqueue(() => UiManager.OnLogMessage(errorMessage));
    }
}