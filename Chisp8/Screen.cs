using System.Diagnostics;

namespace Chisp8
{
    public partial class Screen : Form
    {


        readonly Bitmap screen;

        readonly Stopwatch timer = Stopwatch.StartNew();
        readonly TimeSpan elapsed60hz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 60);
        readonly TimeSpan elapsedMhz = TimeSpan.FromTicks(TimeSpan.TicksPerSecond / 1000);
        TimeSpan lastTime;

        public Screen()
        {
            InitializeComponent();

            screen = new Bitmap(Renderer.SCREEN_W, Renderer.SCREEN_H);
            screenPicture.Image = screen;

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Screen_Load(object sender, EventArgs e)
        {

        }

        private void Screen_Load_1(object sender, EventArgs e)
        {

        }
    }
}