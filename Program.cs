using System;
using CheckersArcade;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            using var game = new Game1();
            game.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ КРИТИЧЕСКАЯ ОШИБКА ПРИ ЗАПУСКЕ:");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("\nНажмите Enter для выхода...");
            Console.ReadLine();
        }
    }
}