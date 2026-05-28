# Hotfix: crash on capture/curling explosion

Apply this archive over the real CheckersArcade project root, not as a standalone project.

## What changed

- Added defensive checks against NaN/Infinity positions and velocities in the curling model.
- Fixed exact-overlap piece collisions: two pieces at almost the same position now get a fallback collision normal instead of producing unstable physics.
- Made visual shockwave drawing skip zero-length/invalid draw calls.
- Made board coordinate conversion reject clicks outside the board before integer division.
- Replaced `Random.Shared` in the visual effects with an instance `Random` for broader runtime compatibility.

## How to apply

From the folder that contains `*.csproj`:

```powershell
Expand-Archive C:\Users\jiova\Downloads\checkersarcade_mvc_explosion_hotfix.zip -DestinationPath . -Force
dotnet clean
dotnet build
dotnet run
```

If the game still crashes, copy the red exception text from the console. The most important lines are the exception type and the first file/line in the stack trace.
