using GMVC.Core;
using GMVC.Utls;
using GMVC.Views;
using UnityEngine.Events;
using UnityEngine.UI;

public class Page_Settings : PageUiBase
{
    View_Calibration view_Calibration;
    View_DrawingPad view_DrawingPad;
    View_Menu view_menu;
    static GameController GameController => App.GetController<GameController>();

    public Page_Settings(IView v, bool display = false) : base(v, display)
    {
        view_DrawingPad = new View_DrawingPad(v.Get<View>("view_drawingPad"), () => view_menu?.Show(),
            GameController.ResetDrawingPadUi,
            GameController.ToggleRoadDraw, GameController.ToggleErrorCutDraw);
        view_Calibration = new View_Calibration(v.Get<View>("view_calibration"), ()=>view_menu?.Show());
        view_menu = new View_Menu(v.Get<View>("view_menu"), GameController.StartGame, view_Calibration.Show, view_DrawingPad.Show);
        App.RegEvent(App.Game_Start, _ => Hide());
        App.RegEvent(App.Game_Save, _ => Settings_Update());
    }

    public void Settings_Update()
    {
        view_DrawingPad.Settings_Update();
    }

    public void ShowMainMenu()
    {
        Show();
        view_menu.Show();
        Settings_Update();
    }
    class View_Calibration : UiBase
    {
        Button btn_close;
        public View_Calibration(IView v,UnityAction onCloseAction) : base(v, false)
        {
            btn_close = v.Get<Button>("btn_close");
            btn_close.onClick.AddListener(() =>
            {
                onCloseAction();
                Hide();
            });
        }
    }
    class View_DrawingPad : UiBase
    {
        Button btn_x;
        Button btn_uiUp;
        Button btn_uiDown;
        //Button btn_uiScaleUp;
        //Button btn_uiScaleDown;
        Button btn_reset;
        Toggle toggle_road;
        Toggle toggle_errorCut;

        public View_DrawingPad(IView v,UnityAction onCloseAction,UnityAction onResetAction,
            UnityAction<bool> onToggleRoadAction,
            UnityAction<bool> onToggleErrorCutAction) : base(v, false)
        {
            btn_x = v.Get<Button>("btn_x");
            btn_uiUp = v.Get<Button>("btn_uiUp");
            btn_uiDown = v.Get<Button>("btn_uiDown");
            //btn_uiScaleUp = v.Get<Button>("btn_uiScaleUp");
            //btn_uiScaleDown = v.Get<Button>("btn_uiScaleDown");
            toggle_road = v.Get<Toggle>("toggle_road");
            toggle_errorCut = v.Get<Toggle>("toggle_errorCut");
            btn_reset = v.Get<Button>("btn_reset");
            btn_reset.onClick.AddListener(onResetAction);
            toggle_road.onValueChanged.AddListener(onToggleRoadAction);
            toggle_errorCut.onValueChanged.AddListener(onToggleErrorCutAction);
            btn_x.onClick.AddListener(() =>
            {
                onCloseAction();
                Hide();
            });
            btn_uiUp.onClick.AddListener(() => GameController.DrawingUiHeightAlign(true));
            btn_uiDown.onClick.AddListener(() => GameController.DrawingUiHeightAlign(false));
            //btn_uiScaleUp.onClick.AddListener(() => GameController.DrawingUiScale(true));
            //btn_uiScaleDown.onClick.AddListener(() => GameController.DrawingUiScale(false));
        }

        public void Settings_Update()
        {
            var set = App.Setting;
            toggle_road.isOn = set.CircuitIsRoad;
            toggle_errorCut.isOn = set.GenerateCutPathOnError;
        }
    }
    class View_Menu : UiBase
    {
        Button btn_start;
        Button btn_calibration;
        Button btn_uiAlign;

        public View_Menu(IView v, UnityAction onStartAction, UnityAction onCalibration, UnityAction onUiAlign,
            bool display = true) : base(v, display)
        {
            btn_start = v.Get<Button>("btn_start");
            btn_calibration = v.Get<Button>("btn_calibration");
            btn_uiAlign = v.Get<Button>("btn_uiAlign");
            btn_start.onClick.AddListener(() =>
            {
                onStartAction();
                Hide();
            });
            btn_calibration.onClick.AddListener(() =>
            {
                onCalibration();
                Hide();
            });
            btn_uiAlign.onClick.AddListener(() =>
            {
                onUiAlign();
                Hide();
            });
        }
    }
}