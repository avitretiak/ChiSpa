using System;
using System.Collections.Generic;
using System.Text;

namespace Chisp8
{
    class Cpu
    {
        private readonly byte[] RAM = new byte[0x1000];
        private readonly byte[] V = new byte[16];
        private ushort I = 0, PC = 0x200;
        private ushort[] Stack = new ushort[16];

        private byte _dt = 0;
        private byte _st = 0;

        private Dictionary<byte, Action<OpCode>> OpCodes;
        public void Tick()
        {

        }

    }
}
