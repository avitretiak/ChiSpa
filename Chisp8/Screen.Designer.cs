namespace Chisp8
{
    partial class Screen
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.screenPicture = new PixelBox();
            ((System.ComponentModel.ISupportInitialize)(this.screenPicture)).BeginInit();
            this.SuspendLayout();
            // 
            // screenPicture
            // 
            this.screenPicture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.screenPicture.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            this.screenPicture.Location = new System.Drawing.Point(0, 0);
            this.screenPicture.Name = "screenPicture";
            this.screenPicture.Size = new System.Drawing.Size(640, 320);
            this.screenPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            this.screenPicture.TabIndex = 0;
            this.screenPicture.TabStop = false;
            // 
            // Screen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 320);
            this.Controls.Add(this.screenPicture);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(64, 32);
            this.Name = "Screen";
            this.Text = "Chisp8 - Chip8 Interpreter";
            this.Load += new System.EventHandler(this.Screen_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.screenPicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private PixelBox screenPicture;
    }
}