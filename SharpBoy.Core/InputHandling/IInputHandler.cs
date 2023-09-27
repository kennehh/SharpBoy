﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.InputHandling
{
    public interface IInputHandler
    {
        bool IsButtonPressed(GameBoyButton btn);
    }

    [Flags]
    public enum GameBoyButton : byte
    {
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        A = 1 << 4,
        B = 1 << 5,
        Select = 1 << 6,
        Start = 1 << 7,
    }
}