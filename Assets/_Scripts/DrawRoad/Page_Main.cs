using GMVC.Core;
using GMVC.Views;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

public class Page_Main : PageUiBase
{
    View_Calibration view_Calibration;
    View_DrawingBoard view_drawingBoard;
    View_Exception view_exception;
    public Page_Main(IView v) : base(v, true)
    {
        view_Calibration = new View_Calibration(v.Get<View>("view_calibration"));
        view_drawingBoard = new View_DrawingBoard(v.Get<View>("view_drawingBoard"));
        view_exception = new View_Exception(v.Get<View>("view_exception"));
        App.RegEvent(App.DrawingBoard_Activation, _ => view_drawingBoard.Display(App.World.IsDrawingBoardActive));
    }

    public void StartCalibration(UnityAction onCloseAction) => view_Calibration.StartCalibration(onCloseAction);
    public void ShowException(string exception) => view_exception.SetMessage(exception);

    class View_Calibration : UiBase
    {
        Button btn_close;
        public View_Calibration(IView v) : base(v, false)
        {
            btn_close = v.Get<Button>("btn_close");
        }
        public void StartCalibration(UnityAction onCloseAction)
        {
            Show();
            btn_close.onClick.AddListener(() =>
            {
                onCloseAction();
                Hide();
            });
        }
    }
    class View_DrawingBoard : UiBase
    {
        public View_DrawingBoard(IView v, bool display = true) : base(v, display)
        {
        }
    }
    class View_Exception : UiBase
    {
        TMP_Text tmp_message;
        Button btn_x;
        public View_Exception(IView v) : base(v, false)
        {
            tmp_message = v.Get<TMP_Text>("tmp_message");
            btn_x = v.Get<Button>("btn_x");
            btn_x.onClick.AddListener(() =>
            {
                App.Pause(false);
                Hide();
            });
        }
        public void SetMessage(string message)
        {
            tmp_message.text = message;
            Show();
        }
    }
}