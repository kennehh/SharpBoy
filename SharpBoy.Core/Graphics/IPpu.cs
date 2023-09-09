﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBoy.Core.Graphics
{
    internal interface IPpu
    {
        byte ReadVram(ushort address);
        byte ReadOam(byte address);
        void WriteVram(ushort address, byte value);
        void WriteOam(ushort address, byte value);
    }
}