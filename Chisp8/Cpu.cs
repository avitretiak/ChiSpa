using System;
using System.Collections.Generic;
using System.Text;

namespace Chisp8
{
    class CPU
    {
        private readonly byte[] memory = new byte[0x1000], registers = new byte[16]; // 4kb of ram and 16 8-bit registers
        private ushort index = 0, programCounter = 0x200;
        private ushort[] stack = new ushort[16]; // stack for 16-bit addresses

        private byte delayTimer = 0, soundTimer = 0;

        private Dictionary<byte, Action<OpCode>> OpCodes;
        public void Tick()
        {
            ushort data = (ushort)(memory[programCounter++] << 8 | memory[programCounter++]); // load next opCode
            OpCode opCode = new OpCode()
            {
                Data = data,
                NNN = (ushort)(data & 0x0FFF),
                NN = (byte)(data & 0x00F),
                N = (byte)(data & 0x000F),
                X = (byte)((data & 0x0F00) >> 8),
                Y = (byte)((data & 0x00F0) >> 4),
            };
        }
    }
}
