﻿namespace SharpBoy.Core.Graphics
{
    public interface IRenderer
    {
        void ClearBuffers();
        void PushFrame();
        void RenderScanline(PpuRegisters registers);
        void ResetWindowLineCounter();
        void SetActiveTileMapAndTileData(LcdcFlags lcdc);
        void UpdateBgColorMapping(byte value);
        void UpdateObp0ColorMapping(byte value);
        void UpdateObp1ColorMapping(byte value);
    }
}