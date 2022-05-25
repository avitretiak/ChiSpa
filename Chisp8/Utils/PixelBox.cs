using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Chisp8
{
    internal class PixelBox : PictureBox
    {
        // http://stackoverflow.com/a/13484101/25124
        public InterpolationMode InterpolationMode { get; set; }

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode;
            base.OnPaint(paintEventArgs);
        }
    }
}
