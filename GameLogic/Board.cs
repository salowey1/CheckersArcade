public event Action<Vector2, TeamColor> OnPieceCaptured;

private void ExecuteCapture(int fromX, int fromY, int toX, int toY, int capturedX, int capturedY)
{
    var pos = GridToWorld(capturedX, capturedY); 
    OnPieceCaptured?.Invoke(pos, Grid[capturedX, capturedY].Team);
    Grid[capturedX, capturedY] = null;

}