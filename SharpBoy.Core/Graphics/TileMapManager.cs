using SharpBoy.Core.Memory;

namespace SharpBoy.Core.Graphics
{
    /// <summary>
    /// Manages the two tile maps for the background and window.
    /// </summary>
    public class TileMapManager
    {
        private readonly TileMap tileMap9800;
        private readonly TileMap tileMap9C00;
        private TileMap activeBgTileMap;
        private TileMap activeWindowTileMap;

        public TileMapManager(IReadableMemory vram)
        {
            tileMap9800 = new TileMap(vram, 0x9800);
            tileMap9C00 = new TileMap(vram, 0x9C00);
            activeBgTileMap = tileMap9800;
            activeWindowTileMap = tileMap9800;
        }

        public void SetActiveBgTileMap(bool useTileMap9C00)
        {
            activeBgTileMap = useTileMap9C00 ? tileMap9C00 : tileMap9800;
        }

        public void SetActiveWindowTileMap(bool useTileMap9C00)
        {
            activeWindowTileMap = useTileMap9C00 ? tileMap9C00 : tileMap9800;
        }

        public void SetActiveTileData(bool useTileData8000)
        {
            activeBgTileMap.SetActiveTileData(useTileData8000);
            activeWindowTileMap.SetActiveTileData(useTileData8000);
        }

        public int GetBgColorIndex(int x, int y)
        {
            return activeBgTileMap.GetColorIndex(x, y);
        }

        public int GetWindowColorIndex(int x, int y)
        {
            return activeWindowTileMap.GetColorIndex(x, y);
        }
    }

    /// <summary>
    /// Represents a tile map, which consists of tile indices.
    /// </summary>
    public class TileMap
    {
        private readonly IReadableMemory vram;
        private readonly int baseAddress;

        private readonly TileData tileData8800;
        private readonly TileData tileData8000;

        private TileData activeTileData;

        public TileMap(IReadableMemory vram, int baseAddress)
        {
            this.vram = vram;
            this.baseAddress = baseAddress;

            tileData8000 = new TileData(vram, 0x8000);
            tileData8800 = new TileData(vram, 0x8800);
            activeTileData = tileData8000;
        }

        public void SetActiveTileData(bool useTileData8000)
        {
            activeTileData = useTileData8000 ? tileData8000 : tileData8800;
        }

        public int GetColorIndex(int x, int y)
        {
            return GetTile(x, y).GetColorIndex(x, y, 8);
        }

        private Tile GetTile(int x, int y)
        {
            int tileNumber = GetTileNumber(x >>> 3, y >>> 3);
            return activeTileData.GetTile(tileNumber, 8);
        }

        // Get the tile index number from the tile map based on coordinates.
        private int GetTileNumber(int xIndex, int yIndex)
        {
            // Calculate the address in memory for the desired tile index.
            int address = baseAddress + (yIndex * 32) + xIndex;
            int value = vram.Read(address);
            if (activeTileData.Address == 0x8800)
            {
                value -= 128;
            }
            return value;
        }
    }

    /// <summary>
    /// Represents a set of tiles available at a specific memory address.
    /// </summary>
    public class TileData
    {
        public int Address { get; }

        private const int TotalTiles = 256;
        private readonly Tile[] tiles = new Tile[TotalTiles];

        public TileData(IReadableMemory vram, int baseAddress)
        {
            Address = baseAddress;

            for (int tileNumber = 0; tileNumber < TotalTiles; tileNumber++)
            {
                int tileAddress = Address + (tileNumber << 4);
                tiles[tileNumber] = new Tile(vram, tileAddress);
            }
        }

        public Tile GetTile(int tileNumber, int tileHeight)
        {
            // Normalize the tile number based on sprite height
            // Ignore bit 0 if it's 8x16
            int normalizedTileNumber = tileNumber & (tileHeight == 16 ? 0xFE : 0xFF);
            return tiles[normalizedTileNumber];
        }
    }

    /// <summary>
    /// Represents an individual 8x8 pixel tile.
    /// </summary>
    public class Tile
    {
        private readonly IReadableMemory vram;
        private readonly int baseAddress;

        public Tile(IReadableMemory vram, int baseAddress)
        {
            this.vram = vram;
            this.baseAddress = baseAddress;
        }

        // Fetch the color index for a specific pixel within the tile.
        public int GetColorIndex(int x, int y, int tileHeight)
        {
            int xPixelInTile = x & (tileHeight - 1);  // Extract the x position within the tile.
            int yPixelInTile = y & (tileHeight - 1);  // Extract the y position within the tile.

            int yOffset = yPixelInTile * 2; // 2 bytes per row
            byte data1 = vram.Read(baseAddress + yOffset);
            byte data2 = vram.Read(baseAddress + yOffset + 1);

            // Find the correct pixel within the tile.
            int colorBitIndex = 7 - xPixelInTile;

            // Combine bits from data1 and data2 to create the color index for the pixel.
            int colorIndex = ((data2 >>> colorBitIndex) & 1) << 1;
            colorIndex |= (data1 >>> colorBitIndex) & 1;

            return colorIndex;
        }
    }
}
