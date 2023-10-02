using SharpBoy.Core.Graphics.Interfaces;
using SharpBoy.Core.Interrupts;
using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Graphics
{

    public class Ppu : IPpu
    {
        private PpuRegisters registers = new PpuRegisters();
        

        private int cycles;
        private readonly IInterruptManager interruptManager;
        private readonly IPpuMemory memory;
        private readonly IPpuRenderer renderer;
        
        private bool LastLcdEnabledStatus = false;


        public Ppu(IInterruptManager interruptManager, IPpuMemory memory, IPpuRenderer renderer)
        {
            this.interruptManager = interruptManager;
            this.memory = memory;
            this.renderer = renderer;
        }

        public void Tick()
        {
            if (!registers.LCDC.HasFlag(LcdcFlags.LcdEnable))
            {
                if (LastLcdEnabledStatus)
                {
                    LastLcdEnabledStatus = false;
                    ResetLcd();
                }
                return;
            }

            LastLcdEnabledStatus = true;
            var previousStatus = registers.CurrentStatus;
            cycles += 4;

            if (registers.LY <= 143)
            {
                HandleModeSwitching(previousStatus);
            }
            else
            {
                HandleVBlank(previousStatus);
            }
        }

        public byte ReadVram(ushort address)
        {
            if (registers.CurrentStatus != PpuStatus.Drawing)
            {
                return memory.Vram.Read(address);
            }
            return 0xff;
        }

        public byte ReadOam(ushort address)
        {
            if (registers.CurrentStatus != PpuStatus.Drawing && registers.CurrentStatus != PpuStatus.SearchingOam)
            {
                return memory.Oam.Read(address);
            }
            return 0xff;
        }

        public void WriteVram(ushort address, byte value)
        {
            if (registers.CurrentStatus != PpuStatus.Drawing)
            {
                memory.Vram.Write(address, value);
            }
        }

        public void WriteOam(ushort address, byte value)
        {
            if (registers.CurrentStatus != PpuStatus.Drawing && registers.CurrentStatus != PpuStatus.SearchingOam)
            {
                memory.Oam.Write(address, value);
            }
        }

        public byte ReadRegister(ushort address)
        {
            return address switch
            {
                0xff40 => (byte)registers.LCDC,
                0xff41 => registers.STAT,
                0xff42 => registers.SCY,
                0xff43 => registers.SCX,
                0xff44 => registers.LY,
                0xff45 => registers.LYC,
                0xff46 => registers.DMA,
                0xff47 => registers.BGP,
                0xff48 => registers.OBP0,
                0xff49 => registers.OBP1,
                0xff4a => registers.WY,
                0xff4b => registers.WX,
                _ => throw new NotImplementedException(),
            };
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xff40: UpdateLcdc(value); break;
                case 0xff41: registers.STAT = value; break;
                case 0xff42: registers.SCY = value; break;
                case 0xff43: registers.SCX = value; break;
                case 0xff44: break;
                case 0xff45: UpdateLyc(value); break;
                case 0xff46: registers.DMA = value; break;
                case 0xff47:
                    registers.BGP = value;
                    renderer.UpdateBgColorMapping(value);
                    break;
                case 0xff48:
                    registers.OBP0 = value;
                    renderer.UpdateObp0ColorMapping(value);
                    break;
                case 0xff49:
                    registers.OBP1 = value;
                    renderer.UpdateObp1ColorMapping(value);
                    break;
                case 0xff4a: registers.WY = value; break;
                case 0xff4b: registers.WX = value; break;
                default: throw new NotImplementedException();
            }
        }

        public void DoOamDmaTransfer(byte[] sourceData)
        {
            memory.Oam.Copy(sourceData);
        }

        public void ResetState(bool bootRomLoaded)
        {
            registers = new PpuRegisters();
            renderer.ClearBuffers();

            if (!bootRomLoaded)
            {
                registers.LCDC = (LcdcFlags)0x91;
                registers.BGP = 0xfc;
                registers.OBP0 = 0xff;
                registers.OBP1 = 0xff;
            }
        }

        private void ResetLcd()
        {
            registers.LY = 0;
            cycles = 0;
            registers.CurrentStatus = PpuStatus.HorizontalBlank;
        }

        private void UpdateLcdc(byte newValue)
        {
            registers.LCDC = (LcdcFlags)newValue;
            renderer.SetActiveTileMapAndTileData(registers.LCDC);
        }

        private void UpdateLyc(byte newValue)
        {
            registers.LYC = newValue;
            CheckLyEqualsLycInterrupt();
        }

        private void IncrementLy()
        {
            if (registers.LY >= 153)
            {
                registers.LY = 0;
                renderer.ResetWindowLineCounter();
            }
            else
            {
                registers.LY++;
            }
            CheckLyEqualsLycInterrupt();
        }

        private void CheckLyEqualsLycInterrupt()
        {
            if (registers.LyCompareFlag)
            {
                HandleStatInterrupt(StatInterruptSourceFlags.LyEqualsLyc);
            }
        }

        private void HandleModeSwitching(PpuStatus previousStatus)
        {
            switch (cycles)
            {
                case < 80:
                    registers.CurrentStatus = PpuStatus.SearchingOam;
                    HandleStatInterrupt(StatInterruptSourceFlags.SearchingOam);
                    break;
                case < 252:
                    // could take from 172 to 289 cycles, defaulting to 172 for now
                    registers.CurrentStatus = PpuStatus.Drawing;
                    if (registers.CurrentStatus != previousStatus)
                    {
                        renderer.RenderScanline(registers);
                    }
                    break;
                case < 456:
                    // could take from 87 to 204 cycles, defaulting to 204 for now
                    registers.CurrentStatus = PpuStatus.HorizontalBlank;
                    HandleStatInterrupt(StatInterruptSourceFlags.HorizontalBlank);
                    break;
                default:
                    IncrementLy();
                    cycles -= 456;
                    break;
            }
        }

        private void HandleVBlank(PpuStatus previousStatus)
        {
            registers.CurrentStatus = PpuStatus.VerticalBlank;
            HandleStatInterrupt(StatInterruptSourceFlags.VerticalBlank);

            if (registers.CurrentStatus != previousStatus)
            {
                interruptManager.RequestInterrupt(InterruptFlags.VBlank);
                renderer.PushFrame();
            }

            if (cycles >= 456)
            {
                IncrementLy();
                cycles -= 456;
            }
        }

        private void HandleStatInterrupt(StatInterruptSourceFlags flagToCheck)
        {
            if (registers.StatInterruptSource != StatInterruptSourceFlags.None && registers.StatInterruptSource.HasFlag(flagToCheck))
            {
                interruptManager.RequestInterrupt(InterruptFlags.LcdStat);
            }
        }
    }
}
