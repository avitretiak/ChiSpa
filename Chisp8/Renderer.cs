using System;

namespace Chisp8
{
    internal class Renderer
    {
        public const int SCREEN_W = 64,
            SCREEN_H = 32;
        private readonly bool[,] Screen = new bool[SCREEN_W, SCREEN_H];
        private bool redraw;

        public void Clear() => Array.Clear(Screen);
    }
}
