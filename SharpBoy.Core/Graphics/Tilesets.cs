using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    internal class Tilesets
    {
    }

    internal class Tile
    {
        private TilePixelValue[] tiles;

        public Tile()
        {
            tiles = new TilePixelValue[8 * 8];
        }
    }

    internal enum TilePixelValue
    {
        Zero,
        One,
        Two,
        Three,
    }
}
