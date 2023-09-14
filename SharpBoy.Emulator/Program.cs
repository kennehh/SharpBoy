using SharpBoy.Core.Processor;
using SharpBoy.Core;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {
        var romPath = "C:\\Projects\\Tetris (World) (Rev A).gb";
        var bootPath = "Z:\\games\\bios\\gb\\dmg0_rom.bin";
        var gb = new GameBoy();
        gb.LoadBootRom(bootPath);
        gb.LoadCartridge(romPath);
        gb.Run();
    }
}