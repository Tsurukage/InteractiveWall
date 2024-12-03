using System.Collections;
using GMVC.Core;
using GMVC.Utls;
using GMVC.Views;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utls;

public class Page_Main : PageUiBase
{
    View_DrawingBoard view_drawingBoard;
    View_Exception view_exception;
    static GameController GameController => App.GetController<GameController>();
    public Page_Main(IView v) : base(v, true)
    {
        view_drawingBoard = new View_DrawingBoard(v.Get<View>("view_drawingBoard"));
        view_exception = new View_Exception(v.Get<View>("view_exception"));
        //App.RegEvent(App.DrawingBoard_Activation, _ => view_drawingBoard.Display(App.World.IsDrawingBoardActive));
        App.RegEvent(App.DrawingBoard_Align, view_drawingBoard.UiAlign);
    }

    public void ShowException(string exception) => view_exception.SetMessage(exception);

    class View_DrawingBoard : UiBase
    {
        Vector3? local;
        RectTransform Rect { get; }
        View[] drawingPads { get; }

        public View_DrawingBoard(IView v, bool display = true) : base(v, display)
        {
            Rect = v.RectTransform;
            drawingPads = new View[6];
            for (var i = 0; i < 6; i++) 
                drawingPads[i] = v.Get<View>($"element_drawingPad_{i+1}");
        }

        public void UiAlign(DataBag bag)
        {
            local ??= Rect.localPosition;
            var size = AppSetting.DrawingPadUi_Size + App.Setting.DrawingPadScale;
            foreach (var pad in drawingPads)
            {
                pad.Display(false);
                pad.RectTransform.sizeDelta = new Vector2(size, size);
                pad.Display(true);
            }
            Rect.localPosition = local.Value.ChangeY(local.Value.y + App.Setting.DrawingPadAlign);
            StartCoroutine(UpdateOnNextFrame());
        }

        IEnumerator UpdateOnNextFrame()
        {
            yield return null;
            GameController.AlignDrawingPad();
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