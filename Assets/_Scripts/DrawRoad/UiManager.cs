using GMVC.Core;
using GMVC.Views;

public class UiManager : UiManagerBase
{
    public View page_mainView;
    public View page_settingsView;
    Page_Main Page_Main { get; set; }
    Page_Settings Page_Settings { get; set; }
    public override void Init()
    {
        Page_Main = new Page_Main(page_mainView);
        Page_Settings = new Page_Settings(page_settingsView);
    }

    public void StartSettings() => Page_Settings.ShowMainMenu();
    public void OnLogMessage(string errorMessage) => Page_Main.ShowException(errorMessage);
}