using GMVC.Core;
using GMVC.Utls;
using UnityEngine;

public class GameController : IController
{
    World World => App.World;
    public void DrawingUiHeightAlign(bool up)
    {
        App.Setting.DrawingPadAlign += up ? 10 : -10;
        SaveSettings();
        UpdateDrawingPad();
    }
    public void DrawingUiScale(bool up)
    {
        var value = App.Setting.DrawingPadScale + (up ? 10 : -10);
        //default is 150 max 200 min 150
        if (value < 0) return;
        if (value > 50) return;
        App.Setting.DrawingPadScale = value;
        SaveSettings();
        UpdateDrawingPad();
    }

    public void StartGame()
    {
        World.StartGame();
    }

    public void UpdateDrawingPad()
    {
        App.SendEvent(App.DrawingBoard_Align);
    }

    public void AlignDrawingPad()
    {
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
    public void ResetDrawingPadUi()
    {
        App.Setting.DrawingPadAlign = 0;
        SaveSettings();
        UpdateDrawingPad();
    }
    static void SaveSettings()
    {
        var json = Json.Serialize(App.Setting);
        PlayerPrefs.SetString(App.Pref_Settings, json);
        App.SendEvent(App.Game_Save);
    }
}