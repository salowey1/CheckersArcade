using System;
using System.IO;
using CheckersArcade;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            Console.WriteLine("🚀 Запуск игры...");
            using var game = new Game1();
            Console.WriteLine("✅ Game1 создан, вызываем Run()...");
            game.Run();
            Console.WriteLine("🔚 Игра завершена нормально");
        }
        catch (Exception ex)
        {
            Console.WriteLine("\n❌ === КРИТИЧЕСКАЯ ОШИБКА ===");
            Console.WriteLine($"Тип: {ex.GetType().Name}");
            Console.WriteLine($"Сообщение: {ex.Message}");
            Console.WriteLine($"\nСтек-трейс:\n{ex.StackTrace}");

            // Если это ошибка контента — даём подсказку
            if (ex.Message.Contains("Content") || ex is Microsoft.Xna.Framework.Content.ContentLoadException)
            {
                Console.WriteLine("\n💡 Подсказка: проверь, что:");
                Console.WriteLine("  1. Content/Content.mgcb существует");
                Console.WriteLine("  2. Arial.spritefont добавлен в pipeline");
                Console.WriteLine("  3. Ты запустил 'dotnet build' перед 'dotnet run'");
            }
            Console.WriteLine("\nНажми Enter для выхода...");
            Console.ReadLine();
        }
    }
}