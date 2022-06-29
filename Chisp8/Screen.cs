using System.Diagnostics;

namespace Chisp8
{
    public partial class Screen : Form
    {
        readonly CPU chip8;
        readonly Bitmap screen;
        readonly string ROM = "../../../roms/test_opcode.ch8";


        // Timing
        readonly Stopwatch timer = Stopwatch.StartNew();
        readonly TimeSpan elapsed60hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan elapsedMhz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);
        TimeSpan lastTime;

        public Screen()
        {
            InitializeComponent();

            screen = new Bitmap(Renderer.Width, Renderer.Height);
            screenPicture.Image = screen;

            chip8 = new CPU(Draw, Beep);
            chip8.LoadRom(File.ReadAllBytes(ROM));

        }

        protected override void OnLoad(EventArgs e)
        {
            Task.Run(GameLoop);
        }

        Task GameLoop()
        {
            while (true)
            {
                var currentTime = timer.Elapsed;
                var elapsedTime = currentTime - lastTime;

                while (elapsedTime >= elapsed60hz)
                {
                    Invoke(Tick60hz);
                    elapsedTime -= elapsed60hz;
                    lastTime += elapsed60hz;
                }

                Invoke(Tick);

                Thread.Sleep(elapsedMhz);
            }
        }

        void Tick() => chip8.Tick();

        void Tick60hz()
        {
            chip8.Tick60hz();
            screenPicture.Refresh();
        }

        void Draw(bool[,] buffer)
        {
            var bits = screen.LockBits(new Rectangle(0, 0, screen.Width, screen.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* pointer = (byte*)bits.Scan0;

                for (var y = 0; y < screen.Height; y++)
                {
                    for (var x = 0; x < screen.Width; x++)
                    {
                        pointer[0] = 0; // Blue
                        pointer[1] = buffer[x, y] ? (byte)0x64 : (byte)0; // Green
                        pointer[2] = 0; // Red
                        pointer[3] = 255; // Alpha 

                        pointer += 4; // 4 bytes per pixel
                    }
                }
            }

            screen.UnlockBits(bits);
        }

        void Beep(int milliseconds)
        {
            Console.Beep(500, milliseconds);
        }

        private void pictureBox1_Click(object sender, EventArgs e) { }

        private void Screen_Load(object sender, EventArgs e) { }

        private void Screen_Load_1(object sender, EventArgs e) { }
    }
}
