using GMVC.Core;
using GMVC.Utls;

public class World 
{
    DrawingBoardManager DrawingBoard;
    public bool IsDrawingBoardActive { get; private set; }

    public World(DrawingBoardManager drawingBoard)
    {
        DrawingBoard = drawingBoard;
    }
    public void Init()
    {
        DrawingBoard_Enable(false);
    }
    public void DrawingBoard_Enable(bool enable)
    {
        IsDrawingBoardActive = enable;
        DrawingBoard.Display(enable);
        App.SendEvent(App.DrawingBoard_Activation);
    }
}