using GMVC.Core;
using GMVC.Utls;
using UnityEngine;

public class GameController : IController
{
    World World => App.World;
    public void DrawingUi(bool up)
    {
        App.Setting.DrawingPadAlign += up ? 10 : -10;
        SaveSettings();
        AlignDrawingPad();
    }

    public void StartGame()
    {
        World.StartGame();
    }

    public void AlignDrawingPad()
    {
        App.SendEvent(App.DrawingBoard_Align);
        World.AlignDrawingBoard();
    }

    public void ToggleRoadDraw(bool enable)
    {
        App.Setting.CircuitIsRoad = enable;
        SaveSettings();
    }
    public void ToggleErrorCutDraw(bool enable)
    {
        App.Setting.GenerateCutPathOnError = enable;
        SaveSettings();
    }
    static void SaveSettings()
    {
        var json = Json.Serialize(App.Setting);
        PlayerPrefs.SetString(App.Pref_Settings, json);
        App.SendEvent(App.Game_Save);
    }
}