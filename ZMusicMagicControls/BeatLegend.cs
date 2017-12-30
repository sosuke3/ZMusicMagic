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
    public partial class BeatLegend : Control
    {
        public event EventHandler<MouseEventArgs> MouseScroll;

        Color beatLineColor = Color.Black;
        int beatLineThickness = 1;
        Pen beatLinePen = new Pen(Color.Black, 1);
        public Color BeatLineColor
        {
            get { return beatLineColor; }
            set
            {
                beatLineColor = value;
                beatLinePen = new Pen(beatLineColor, beatLineThickness);

                Invalidate();
            }
        }
        public int BeatLineThickness
        {
            get { return beatLineThickness; }
            set
            {
                beatLineThickness = value;
                beatLinePen = new Pen(beatLineColor, beatLineThickness);

                Invalidate();
            }
        }

        Color subBeatLineColor = Color.DarkGray;
        int subBeatLineThickness = 1;
        Pen subBeatLinePen = new Pen(Color.DarkGray, 1);
        public Color SubBeatLineColor
        {
            get { return subBeatLineColor; }
            set
            {
                subBeatLineColor = value;
                subBeatLinePen = new Pen(subBeatLineColor, subBeatLineThickness);

                Invalidate();
            }
        }
        public int SubBeatLineThickness
        {
            get { return subBeatLineThickness; }
            set
            {
                subBeatLineThickness = value;
                subBeatLinePen = new Pen(subBeatLineColor, subBeatLineThickness);

                Invalidate();
            }
        }

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

        int canvasWidth = 2000;
        public int CanvasWidth
        {
            get { return canvasWidth; }
            set
            {
                canvasWidth = value;

                Invalidate();
            }
        }

        int fullNoteWidth = 120;
        public int FullNoteWidth
        {
            get { return fullNoteWidth; }
            set
            {
                fullNoteWidth = value;

                Invalidate();
            }
        }

        public BeatLegend()
        {
            //InitializeComponent();

            this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            DrawLines(g);
        }

        void DrawLines(Graphics g)
        {
            // vertical lines
            int width = canvasWidth;
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);
            int x = 0;
            for (int i = 0; i < width; i+=fullNoteWidth)
            {
                int startX = x - horizontalOffset;
                int offsetX = x - horizontalOffset + fullNoteWidth;

                if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                {
                    DrawBeatLines(g, startX, 0, fullNoteWidth, this.Height);

                    var str = $"{1 + i / fullNoteWidth}";
                    g.DrawString(str, this.Font, Brushes.Black, x - horizontalOffset + 2, this.Height - g.MeasureString(str, this.Font).Height);
                }

                x += fullNoteWidth;
            }
        }

        void DrawBeatLines(Graphics g, int x, int y, int width, int height)
        {
            g.DrawLine(beatLinePen, x, y, x, height);

            int halfX = x + (width / 2);
            g.DrawLine(subBeatLinePen, halfX, y + height / 2, halfX, height);
            var str = $"½";
            var font = new Font(this.Font.Name, 8, this.Font.Style, this.Font.Unit);
            g.DrawString(str, font, Brushes.Black, halfX + 2, height - g.MeasureString(str, font).Height);

            int quarterX = x + (width / 4);
            str = $"¼";
            g.DrawLine(subBeatLinePen, quarterX, y + height / 2, quarterX, height);
            g.DrawString(str, font, Brushes.Black, quarterX + 2, height - g.MeasureString(str, font).Height);
            int threeQuarterX = x + 3 * (width / 4);
            str = $"¾";
            g.DrawLine(subBeatLinePen, threeQuarterX, y + height / 2, threeQuarterX, height);
            g.DrawString(str, font, Brushes.Black, threeQuarterX + 2, height - g.MeasureString(str, font).Height);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.MouseScroll?.Invoke(this, e);

            base.OnMouseWheel(e);
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
