using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZMusicMagicLibrary;
using ZMusicMagicLibrary.NSPC;

namespace ZMusicMagicControls
{
    public partial class NoteEditor : Control
    {
        public Channel Channel { get; set; }

        PointF scrollPosition;
        public PointF ScrollPosition
        {
            get { return scrollPosition; }
            set
            {
                scrollPosition = value;

                Invalidate();
            }
        }

        int noteThickness = 20;
        public int NoteThickness
        {
            get { return noteThickness; }
            set
            {
                noteThickness = value;

                Invalidate();
            }
        }

        public NoteEditor()
        {
            //InitializeComponent();

            this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            g.DrawString($"{ScrollPosition}", this.Font, new SolidBrush(this.ForeColor), 0, 0);

            DrawLines(g);
        }

        void DrawLines(Graphics g)
        {
            // canvas size
            int noteHeightWithLine = noteThickness + 1;

            int width = 2000;
            int height = 72 * noteHeightWithLine;

            int verticalOffset = (int)(scrollPosition.Y / 100.0 * height);
            Debug.WriteLine($"verticalOffset {verticalOffset}");
            int y = noteThickness;
            int x = 0;
            for(int i = (int)Track.Command._C7_B6; i > (int)Track.Command._80_C1 - 1; --i)
            {
                Debug.WriteLine($"i: y {i} {y}");
                int topY = y - verticalOffset - noteThickness;
                int offsetY = y - verticalOffset;

                if ((topY >= 0 && topY < this.Bottom) || (offsetY > 0 && offsetY < this.Bottom))
                {
                    g.FillRectangle(Brushes.Blue, x, topY, 40, noteThickness);
                }
                x += 40;

                if (offsetY > 0 && offsetY < this.Bottom)
                {
                    g.DrawLine(Pens.Black, 0, y - verticalOffset, this.Right, y - verticalOffset);
                }
                y += noteHeightWithLine;
            }
        }

        public void HorizontalScroll_ValueChanged(object sender, EventArgs e)
        {
            if (sender is ScrollBar)
            {
                ScrollPosition = new PointF(((ScrollBar)sender).Value, ScrollPosition.Y);
            }
        }

        public void VerticalScroll_ValueChanged(object sender, EventArgs e)
        {
            if (sender is ScrollBar)
            {
                ScrollPosition = new PointF(ScrollPosition.X, ((ScrollBar)sender).Value);
            }
        }
    }
}
