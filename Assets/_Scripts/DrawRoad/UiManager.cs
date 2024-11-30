using GMVC.Core;
using GMVC.Views;

public class UiManager : UiManagerBase
{
    public View page_main;
    Page_Main Page_Main { get; set; }
    public override void Init()
    {
        Page_Main = new Page_Main(page_main);
        Page_Main.StartCalibration(() => App.World.DrawingBoard_Enable(true));
    }
    public void OnLogMessage(string errorMessage) => Page_Main.ShowException(errorMessage);
}