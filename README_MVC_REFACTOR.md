# CheckersArcade MVC refactor

This patch restructures the gameplay into MVC without changing the core arcade idea.

## Model

Files in `Models/`:

- `CheckersBoardModel.cs` — board state, move rules, capture rules, kings, chain captures, and the curling collision state for living pieces.
- `CheckerPiece.cs` — a single live checker, including grid state and visual/velocity state.
- `PieceSide.cs` — red/blue side enum.
- `BoardLayout.cs` — board constants and screen/cell conversion.
- `PieceAnimation.cs` — move animation state.
- `CaptureEffectInfo.cs` — event payload for capture visuals.

The model owns the game state. It does not draw and does not read mouse input.

## Controller

Files in `Controllers/`:

- `CheckersGameController.cs` — converts mouse clicks into model actions: select, move, chain capture continuation, clear selection.

The controller owns input decisions. It does not draw and does not store the board rules.

## View

Files in `Views/`:

- `GameView.cs` — draws the board, pieces, valid move markers, kings, HUD, and delegates visual effects drawing.

The view reads model state and draws it. It does not mutate the board.

## Screen composition

`Screens/GameScreen.cs` wires model, controller, view, and effects together. It is now a composition root, not the place where game logic lives.

## Effects

`Core/PhysicsSystem.cs` now contains `VisualEffectsSystem`.

Important: effects are visual-only. They no longer remove pieces from the board. The living-piece curling collision is handled by the model because it affects the actual game state.

## Deprecated old files

`GameLogic/Board.cs` and `GameLogic/Piece.cs` are replaced by empty deprecated namespaces so old non-MVC code is not accidentally used.

## How to apply

Copy the files from this archive into the repository root, replacing files when asked.

Then run:

```bash
dotnet clean
dotnet build
dotnet run
```

## Tuning curling feel

Tweak constants in `Models/CheckersBoardModel.cs`:

- `ImpactRadius`
- `ImpactForce`
- `FrictionPerFrame`
- `Restitution`
- `StopSpeed`
- `SettleDelay`
