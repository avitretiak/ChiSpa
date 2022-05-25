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
        private Renderer renderer = new();
        HashSet<byte> pressedKeys = new HashSet<byte>();
        Action<int> beep;

        private byte stackPointer, delayTimer = 0, soundTimer = 0;
        private Random random = new Random();

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

        private void ClearOrReturn(OpCode opcode)
        {
            if (opcode.NN == 0xE0)
                renderer.Clear();
            else if (opcode.NN == 0xEE)
                programCounter = Pop();
        }

        private void Jump(OpCode opcode) => programCounter = opcode.NNN;
        private void Call(OpCode opcode) {
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
                    registers[0xF] = (byte)(registers[opcode.X] + registers[opcode.Y] > 0xFF ? 1 : 0); // handle overflow
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
                    registers[opcode.Y] -= registers[opcode.X]; // should we store this in X?
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
        private void JumpWithOffset(OpCode opcode) => programCounter = (ushort)(opcode.NNN + registers[0]);
        private void Random(OpCode opcode) => registers[opcode.X] = (byte)(random.Next(0, 255) & opcode.NN);
        private void Draw(OpCode opcode)
        {
            var startX = registers[opcode.X];
            var startY = registers[opcode.Y];

            // TODO
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
            memory[index] = (byte)(registers[opcode.X] / 100 % 10);
            memory[index + 1] = (byte)(registers[opcode.X] / 10 % 10);
            memory[index + 2] = (byte)(registers[opcode.X] % 10);
        }
        private void SaveX(OpCode opcode)
        {
            for (int i = 0; i < opcode.X; i++)
            {
                memory[index + 1] = registers[i];
            }
        }
        private void LoadX(OpCode opcode)
        {
            for (int i = 0; i < opcode.X; i++)
            {
                registers[i] = memory[index + 1];
            }
        }




        private void Push(ushort value) => stack[stackPointer++] = value;

        private ushort Pop() => stack[--stackPointer];

        private ushort Fetch() => memory[programCounter];
        

    }
}
