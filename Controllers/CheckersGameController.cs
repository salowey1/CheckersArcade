#nullable disable

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CheckersArcade.Models;

namespace CheckersArcade.Controllers;

public class CheckersGameController
{
    private readonly CheckersBoardModel _board;

    public CheckersGameController(CheckersBoardModel board)
    {
        _board = board;
    }

    public void Update(float deltaSeconds)
    {
        _board.Update(deltaSeconds);
    }

    public void HandleMouse(MouseState mouse, bool isLeftClick, bool isDoubleClick)
    {
        if (!isLeftClick || _board.IsBusy || _board.IsGameOver)
        {
            return;
        }

        if (!BoardLayout.TryScreenToCell(mouse.X, mouse.Y, out Point clickedCell))
        {
            return;
        }

        if (isDoubleClick && _board.TryExplodePiece(clickedCell))
        {
            return;
        }

        HandleBoardClick(clickedCell);
    }

    private void HandleBoardClick(Point clickedCell)
    {
        if (_board.SelectedCell != null && _board.TryMoveSelectedPiece(clickedCell))
        {
            return;
        }

        if (_board.CanSelect(clickedCell))
        {
            _board.SelectPiece(clickedCell);
            return;
        }

        _board.ClearSelection();
    }
}
