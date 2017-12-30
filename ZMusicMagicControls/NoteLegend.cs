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
    public partial class NoteLegend : Control
    {
        public event EventHandler<MouseEventArgs> MouseScroll;

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

        int textAreaWidth = 30;
        public int TextAreaWidth
        {
            get { return textAreaWidth; }
            set
            {
                textAreaWidth = value;

                Invalidate();
            }
        }

        int keyAreaWidth = 10;
        public int KeyAreaWidth
        {
            get { return keyAreaWidth; }
            set
            {
                keyAreaWidth = value;

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

        public NoteLegend()
        {
            //InitializeComponent();

            this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            var visibleAreaRectangle = MakeVisibleNotesRectangle();

            var visibleNoteList = MakeVisibleNoteList(visibleAreaRectangle);

            DrawLines(g, visibleAreaRectangle);
            DrawNotes(g, visibleAreaRectangle, visibleNoteList);
        }

        private void DrawNotes(Graphics g, Rectangle visibleAreaRectangle, Dictionary<Track.Command, CommandLineInfo> visibleNoteList)
        {
            //if (Channel == null)
            //{
            //    // nothing to draw
            //    return;
            //}

            int keysX = this.TextAreaWidth + 1;
            var brush = Brushes.Black;

            foreach(var info in visibleNoteList.Values)
            {
                if(ChannelCommand.NoteIsSharp(info.Command))
                {
                    brush = Brushes.Black;
                }
                else
                {
                    brush = Brushes.White;
                }
                g.FillRectangle(brush, keysX, info.LineArea.Y, keyAreaWidth, info.LineArea.Height);

                g.DrawString(info.Command.GetDescription(), 
                    this.Font, 
                    Brushes.Black, 
                    2, 
                    info.LineArea.Y + (info.LineArea.Height / 2) - (g.MeasureString(info.Command.GetDescription(), this.Font).Height / 2));
            }
        }

        Rectangle MakeVisibleNotesRectangle()
        {
            // canvas height size
            int noteHeightWithLine = noteThickness + 1;
            int canvasHeight = 72 * noteHeightWithLine;
            int verticalOffset = (int)(scrollPosition.Y / 100.0 * canvasHeight);


            int left;
            int top;
            int width;
            int height;

            left = 0;
            width = this.Width;

            top = verticalOffset;
            height = this.Height;

            return new Rectangle(left, top, width, height);
        }


        Dictionary<Track.Command, CommandLineInfo> MakeVisibleNoteList(Rectangle visibleNoteArea)
        {
            int noteHeightWithLine = noteThickness + 1;

            var ret = new Dictionary<Track.Command, CommandLineInfo>();

            int y = noteThickness;

            for (int i = (int)Track.Command._C7_B6; i > (int)Track.Command._80_C1 - 1; --i)
            {
                int topY = y - visibleNoteArea.Y - noteThickness;
                int offsetY = y - visibleNoteArea.Y;

                if ((topY >= 0 && topY <= this.Height) || (offsetY >= 0 && offsetY <= this.Height))
                {
                    Rectangle area = new Rectangle(0, topY, this.Width, noteThickness);
                    
                    ret.Add((Track.Command)i, new CommandLineInfo() { Command = (Track.Command)i, LineArea = area });
                }

                y += noteHeightWithLine;
            }

            return ret;
        }

        void DrawLines(Graphics g, Rectangle visibleArea)
        {
            // horizontal lines
            int noteHeightWithLine = noteThickness + 1;
            int height = 72 * noteHeightWithLine;

            int verticalOffset = (int)(scrollPosition.Y / 100.0 * height);
            //Debug.WriteLine($"verticalOffset {verticalOffset}");
            int y = noteThickness;
            for(int i = (int)Track.Command._C7_B6; i > (int)Track.Command._80_C1 - 1; --i)
            {
                //Debug.WriteLine($"i: y {i} {y}");
                int topY = y - verticalOffset - noteThickness;
                int offsetY = y - verticalOffset;

                if (offsetY >= 0 && offsetY <= this.Height)
                {
                    g.DrawLine(Pens.Black, 0, y - verticalOffset, this.Right, y - verticalOffset);
                }
                y += noteHeightWithLine;
            }

            g.DrawLine(Pens.Black, this.textAreaWidth, 0, this.textAreaWidth, this.Height);
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
