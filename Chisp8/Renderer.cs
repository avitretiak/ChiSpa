using System;

namespace Chisp8
{
    internal class Renderer
    {
        public static int Width = 64,
            Height = 32;
        public readonly bool[,] buffer = new bool[Width, Height];
        public readonly bool[,] redrawBuffer = new bool[Width, Height];

        public bool redraw;

        public void Clear() => Array.Clear(buffer);
    }
}
