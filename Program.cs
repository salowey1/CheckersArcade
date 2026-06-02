using System;
using CheckersArcade;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        using Game1 game = new();
        game.Run();
    }
}
