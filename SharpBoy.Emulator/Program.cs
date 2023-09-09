using SharpBoy.Core.Processor;
using SharpBoy.Core;
using Raylib_cs;

internal class Program
{
    private static void Main(string[] args)
    {
        //var gb = new GameBoy();

        Raylib.InitWindow(800, 480, "Hello World");

        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.WHITE);

            Raylib.DrawText("Hello, world!", 12, 12, 20, Color.BLACK);

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}