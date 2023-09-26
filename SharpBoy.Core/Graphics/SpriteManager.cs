﻿using SharpBoy.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    public class SpriteManager
    {
        private readonly Sprite[] sprites = new Sprite[40];
        private List<Sprite> visibleSprites = new List<Sprite>();

        public SpriteManager(IReadableMemory oam, IReadableMemory vram)
        {
            var tileData = new TileData(vram, 0x8000);
            for (int i = 0; i < sprites.Length; i++)
            {
                int index = i * 4; // Each sprite has 4 bytes in OAM
                sprites[i] = new Sprite(oam, index, tileData);
            }
        }

        public IReadOnlyCollection<Sprite> GetVisibleSprites(int currentScanline, int spriteHeight)
        {
            visibleSprites.Clear();
            int spriteCount = 0;

            foreach (var sprite in sprites)
            {
                if (IsSpriteVisibleOnScanline(sprite, currentScanline, spriteHeight))
                {
                    visibleSprites.Add(sprite);
                    spriteCount++;

                    if (spriteCount >= 10)
                    {
                        break;
                    }
                }
            }

            // Sort sprites based on their X position and priority rules
            // Sorting could be important for sprite overlap
            visibleSprites.Sort((a, b) =>
            {
                int xPosComparison = a.XPos.CompareTo(b.XPos);
                return xPosComparison != 0 ? xPosComparison : a.OamIndex.CompareTo(b.OamIndex);
            });

            return visibleSprites;
        }

        private bool IsSpriteVisibleOnScanline(Sprite sprite, int scanline, int spriteHeight)
        {
            // Check the vertical visibility of the sprite
            int spriteTop = sprite.YPos - 16;
            int spriteBottom = spriteTop + spriteHeight - 1;

            return scanline >= spriteTop && scanline <= spriteBottom;
        }
    }

    public class Sprite
    {
        public int OamIndex { get; } // Starting index in OAM for this sprite
        private readonly IReadableMemory oam; // Object Attribute Memory array        
        private readonly TileData tileData;

        public Sprite(IReadableMemory oam, int index, TileData tileData)
        {
            this.oam = oam;
            OamIndex = index;
            this.tileData = tileData;
        }

        public byte YPos => oam.Read(OamIndex);
        public byte XPos => oam.Read(OamIndex + 1);
        public byte TileNumber => oam.Read(OamIndex + 2);
        public SpriteAttributes Attributes => (SpriteAttributes)oam.Read(OamIndex + 3);

        public bool YFlip => Attributes.HasFlag(SpriteAttributes.YFlip);
        public bool XFlip => Attributes.HasFlag(SpriteAttributes.XFlip);
        public bool BgAndWindowHasPriority => Attributes.HasFlag(SpriteAttributes.BgAndWindowHasPriority);
        public bool UseObp1Palette => Attributes.HasFlag(SpriteAttributes.UseObp1Palette);

        // Game Boy Color specific attributes
        public byte CgbPalette => (byte)(Attributes & SpriteAttributes.CgbPalette); // Mask to get bits 0-2
        public bool UseVramBank1 => Attributes.HasFlag(SpriteAttributes.UseVramBank1);

        public int GetColorIndex(int x, int y)
        {
            return tileData.GetTile(TileNumber).GetColorIndex(x, y);
        }

        public byte[] GetLineToRender(int y)
        {
            byte[] lineData = new byte[8];
            var tile = tileData.GetTile(TileNumber);

            for (int x = 0; x < 8; x++)
            {
                lineData[XFlip ? 7 - x : x] = (byte)tile.GetColorIndex(x, y);
            }

            return lineData;
        }
    }

    [Flags]
    public enum SpriteAttributes : byte
    {
        UseObp1Palette = 1 << 4,
        XFlip = 1 << 5,
        YFlip = 1 << 6,
        BgAndWindowHasPriority = 1 << 7,

        // Game Boy Color specific
        UseVramBank1 = 1 << 3,
        CgbPalette = 0x07  // Combining bits 0, 1, 2
    }

}