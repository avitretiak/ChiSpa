using System;
using System.Collections.Generic;
using System.Text;

namespace Chisp8
{
    class CPU
    {
        private readonly byte[] memory = new byte[0x1000],
            registers = new byte[16]; // 4kb of ram and 16 8-bit registers
        private ushort index = 0,
            programCounter = 0x200;
        private ushort[] stack = new ushort[16]; // stack for 16-bit addresses
        private Renderer renderer = new();
        HashSet<byte> pressedKeys = new HashSet<byte>();
        Action<bool[,]> draw;
        Action<int> beep;

        private byte stackPointer,
            delayTimer = 0,
            soundTimer = 0;
        private Random random = new Random();

        private Dictionary<byte, Action<OpCode>> opCodes;
        private Dictionary<byte, Action<OpCode>> opCodesMisc;

        public void LoadRom(byte[] romData) => Array.Copy(romData, 0, memory, 0x200, romData.Length);

        public CPU(Action<bool[,]> draw, Action<int> beep)
        {
            this.draw = draw;
            this.beep = beep;

            WriteFont();

            opCodes = new Dictionary<byte, Action<OpCode>>
            {
                { 0x0, ClearOrReturn },
                { 0x1, Jump },
                { 0x2, Call },
                { 0x3, SkipIfXEqual },
                { 0x4, SkipIfXNotEqual },
                { 0x5, SkipIfXandYEqual },
                { 0x6, SetX },
                { 0x7, AddX },
                { 0x8, Arithmetic },
                { 0x9, SkipIfXandYDifferent },
                { 0xA, SetIndex },
                { 0xB, JumpWithOffset },
                { 0xC, Random },
                { 0xD, DrawSprite },
                { 0xE, SkipIfPressed },
                { 0xF, Misc },
            };

            opCodesMisc = new Dictionary<byte, Action<OpCode>>
            {
                { 0x07, SetXToDelay },
                { 0x0A, WaitForKeyPress },
                { 0x15, SetDelayToX },
                { 0x18, SetSoundToX },
                { 0x1E, AddXToIndex },
                { 0x29, SetIndexForChar },
                { 0x33, BinaryCodedDecimal },
                { 0x55, SaveX },
                { 0x65, LoadX },
            };
        }

        void Misc(OpCode opcode)
        {
            if (opCodesMisc.ContainsKey(opcode.NN))
                opCodesMisc[opcode.NN](opcode);
        }

        public void Tick()
        {
            ushort data = (ushort)(memory[programCounter++] << 8 | memory[programCounter++]); // load next opCode
            OpCode opCode = new OpCode()
            {
                Data = data,
                NNN = (ushort)(data & 0x0FFF),
                NN = (byte)(data & 0x00FF),
                N = (byte)(data & 0x000F),
                X = (byte)((data & 0x0F00) >> 8),
                Y = (byte)((data & 0x00F0) >> 4),
            };

            opCodes[(byte)(data >> 12)](opCode);
        }

        public void Tick60hz()
        {
            if (delayTimer > 0)
                delayTimer--;
            if (renderer.redraw)
            {
                renderer.redraw = false;
                draw(renderer.buffer);
            }

        }

        void WriteFont()
        {
            var offset = 0x0;
            WriteFont(5 * offset++, Font.D0);
            WriteFont(5 * offset++, Font.D1);
            WriteFont(5 * offset++, Font.D2);
            WriteFont(5 * offset++, Font.D3);
            WriteFont(5 * offset++, Font.D4);
            WriteFont(5 * offset++, Font.D5);
            WriteFont(5 * offset++, Font.D6);
            WriteFont(5 * offset++, Font.D7);
            WriteFont(5 * offset++, Font.D8);
            WriteFont(5 * offset++, Font.D9);
            WriteFont(5 * offset++, Font.DA);
            WriteFont(5 * offset++, Font.DB);
            WriteFont(5 * offset++, Font.DC);
            WriteFont(5 * offset++, Font.DD);
            WriteFont(5 * offset++, Font.DE);
            WriteFont(5 * offset++, Font.DF);
        }

        void WriteFont(int address, long fontData)
        {
            memory[address + 0] = (byte)((fontData & 0xF000000000) >> (8 * 4));
            memory[address + 1] = (byte)((fontData & 0x00F0000000) >> (8 * 3));
            memory[address + 2] = (byte)((fontData & 0x0000F00000) >> (8 * 2));
            memory[address + 3] = (byte)((fontData & 0x000000F000) >> (8 * 1));
            memory[address + 4] = (byte)((fontData & 0x00000000F0) >> (8 * 0));
        }

        private void ClearOrReturn(OpCode opcode)
        {
            if (opcode.NN == 0xE0)
                renderer.Clear();
            else if (opcode.NN == 0xEE)
                programCounter = Pop();
        }

        private void Jump(OpCode opcode) => programCounter = opcode.NNN;

        private void Call(OpCode opcode)
        {
            Push(programCounter);
            programCounter = opcode.NNN;
        }

        private void SkipIfXEqual(OpCode opcode)
        {
            if (registers[opcode.X] == opcode.NN)
                programCounter += 2;
        }

        private void SkipIfXNotEqual(OpCode opcode)
        {
            if (registers[opcode.X] != opcode.NN)
                programCounter += 2;
        }

        private void SkipIfXandYEqual(OpCode opcode)
        {
            if (registers[opcode.X] == registers[opcode.Y])
                programCounter += 2;
        }

        private void SetX(OpCode opcode) => registers[opcode.X] = opcode.NN;

        private void AddX(OpCode opcode) => registers[opcode.X] += opcode.NN;

        private void Arithmetic(OpCode opcode)
        {
            switch (opcode.N)
            {
                case 0x0:
                    registers[opcode.X] = registers[opcode.Y];
                    break;
                case 0x1:
                    registers[opcode.X] |= registers[opcode.Y];
                    break;
                case 0x2:
                    registers[opcode.X] &= registers[opcode.Y];
                    break;
                case 0x3:
                    registers[opcode.X] ^= registers[opcode.Y];
                    break;
                case 0x4:
                    registers[0xF] = (byte)(
                        registers[opcode.X] + registers[opcode.Y] > 0xFF ? 1 : 0
                    ); // handle overflow
                    registers[opcode.X] += registers[opcode.Y];
                    break;
                case 0x5:
                    registers[0xF] = (byte)(registers[opcode.X] > registers[opcode.Y] ? 1 : 0);
                    registers[opcode.X] -= registers[opcode.Y];
                    break;
                case 0x6:
                    registers[0xF] = (byte)((registers[opcode.X] & 0x1) != 0 ? 1 : 0);
                    registers[opcode.X] /= 2;
                    break;
                case 0x7:
                    registers[0xF] = (byte)(registers[opcode.Y] > registers[opcode.X] ? 1 : 0);
                    registers[opcode.Y] -= registers[opcode.X];
                    break;
                case 0xE:
                    registers[0xF] = (byte)((registers[opcode.X] & 0x1) != 0 ? 1 : 0);
                    registers[opcode.X] *= 2;
                    break;
            }
        }

        private void SkipIfXandYDifferent(OpCode opcode)
        {
            if (registers[opcode.X] != registers[opcode.Y])
                programCounter += 2;
        }

        private void SetIndex(OpCode opcode) => index = opcode.NNN;

        private void JumpWithOffset(OpCode opcode) =>
            programCounter = (ushort)(opcode.NNN + registers[0]);

        private void Random(OpCode opcode) =>
            registers[opcode.X] = (byte)(random.Next(0, 255) & opcode.NN);

        private void DrawSprite(OpCode opcode)
        {
            var startX = registers[opcode.X];
            var startY = registers[opcode.Y];

            for (int x = 0; x < Renderer.Width; x++)
            {
                for (int y = 0; y < Renderer.Height; y++ )
                {
                    if (renderer.redrawBuffer[x,y])
                    {
                        if (renderer.buffer[x, y])
                            renderer.redraw = true;
                        renderer.redrawBuffer[x, y] = false;
                        renderer.buffer[x, y] = false;
                    }
                }
            }

            registers[0xF] = 0;

            for (int i = 0; i < opcode.N; i++)
            {
                var line = memory[index + i];

                for (var bit = 0; bit < 8; bit++)
                {
                    var x = (startX + bit) % Renderer.Width;
                    var y = (startY + i) % Renderer.Height;

                    var spriteBit = ((line >> (7 - bit)) & 1);
                    var oldBit = renderer.buffer[x, y] ? 1 : 0;

                    if (oldBit != spriteBit)
                        renderer.redraw = true;

                    var newBit = oldBit ^ spriteBit; // XOR

                    if (newBit != 0)
                        renderer.buffer[x, y] = true;
                    else
                        renderer.redrawBuffer[x, y] = true;

                    if (oldBit != 0 && newBit == 0)
                        registers[0xF] = 1;
                }
            }
        }

        private void SkipIfPressed(OpCode opcode)
        {
            if (opcode.NN == 0x9E && pressedKeys.Contains(registers[opcode.X]))
                programCounter += 2;
        }

        private void SkipIfNotPressed(OpCode opcode)
        {
            if (opcode.NN == 0xA1 && !pressedKeys.Contains(registers[opcode.X]))
                programCounter += 2;
        }

        private void SetXToDelay(OpCode opcode) => registers[opcode.X] = delayTimer;

        private void WaitForKeyPress(OpCode opcode)
        {
            if (pressedKeys.Count != 0)
                registers[opcode.X] = pressedKeys.First();
            else
                programCounter -= 2;
        }

        private void SetDelayToX(OpCode opcode) => delayTimer = registers[opcode.X];

        private void SetSoundToX(OpCode opcode) => beep((int)(registers[opcode.X] * (1000f / 60)));

        private void AddXToIndex(OpCode opcode) => index += registers[opcode.X];

        private void SetIndexForChar(OpCode opcode) => index = (ushort)(registers[opcode.X] * 5);

        private void BinaryCodedDecimal(OpCode opcode)
        {
            memory[index] = (byte)(registers[opcode.X] / 100);
            memory[index + 1] = (byte)((registers[opcode.X] / 10) % 10);
            memory[index + 2] = (byte)((registers[opcode.X] % 100) % 10);
        }

        private void SaveX(OpCode opcode)
        {
            for (int i = 0; i <= opcode.X; i++)
                memory[index + i] = registers[i];
        }

        private void LoadX(OpCode opcode)
        {
            for (int i = 0; i <= opcode.X; i++)
                registers[i] = memory[index + i];
        }

        private void Push(ushort value) => stack[stackPointer++] = value;

        private ushort Pop() => stack[--stackPointer];

        private ushort Fetch() => memory[programCounter];
    }
}
