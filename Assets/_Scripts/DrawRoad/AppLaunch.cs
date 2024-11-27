using GMVC.Core;
using UnityEngine;

public class AppLaunch : MonoBehaviour
{
    public DrawingBoardManager DrawingBoardManager;
    public AudioManager AudioManager;
    public UiManager UiManager;
    void Start()
    {
        App.Run(OnGameStart, AudioManager);
    }

    void OnGameStart()
    {
        DrawingBoardManager.Init();
        UiManager.Init();
    }
}