using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    internal class TileMap
    {
        private readonly Tile[] tiles;

        public TileMap()
        {
            tiles = new Tile[32];
        }

        public Tile this[int index]
        {
            get => tiles[index];
            set => tiles[index] = value;
        }
    }

    internal class TileBlock
    {
        private readonly Tile[] tiles;

        public TileBlock()
        {
            tiles = new Tile[128];
            Array.Fill(tiles, new Tile());
        }

        public Tile this[int index] => tiles[index];
    }

    internal class Tile
    {
        private readonly TilePixelValue[] pixels;

        public Tile()
        {
            pixels = new TilePixelValue[8 * 8];
            Array.Fill(pixels, TilePixelValue.Zero);
        }

        public TilePixelValue this[int x, int y]
        {
            get => pixels[x * 8 + y];
            set => pixels[x * 8 + y] = value;
        }
    }

    internal enum TilePixelValue
    {
        Zero = 1 << 0,
        One = 1 << 1,
        Two = 1 << 2,
        Three = 1 << 3
    }
}
