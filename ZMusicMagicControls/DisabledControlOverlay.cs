using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZMusicMagicControls
{
    public class DisabledControlOverlay : Control
    {
        public bool DrawDisabledOverlay { get; set; }

        public DisabledControlOverlay()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);

            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if(DrawDisabledOverlay)
            {
                using (var brush = new SolidBrush(Color.FromArgb(50, Color.DarkGray))) //new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(50, Color.Fuchsia), Color.FromArgb(0, Color.DarkGray)))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

        //protected override void OnPaintBackground(PaintEventArgs e)
        //{
        //    //base.OnPaintBackground(e);
        //}
    }
}
