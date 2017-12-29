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
        public event EventHandler<MouseEventArgs> MouseWheel;

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

            var visibleAreaRectangle = MakeVisibleNotesRectangle();

            var visibleNoteList = MakeVisibleNoteList(visibleAreaRectangle);

            Debug.WriteLine(String.Join(",", visibleNoteList));

            DrawLines(g, visibleAreaRectangle);
            DrawNotes(g, visibleAreaRectangle, visibleNoteList);

            //g.DrawString($"{visibleAreaRectangle}", this.Font, new SolidBrush(this.ForeColor), 0, 0);
            //g.DrawString($"{String.Join(",", visibleNoteList)}", this.Font, new SolidBrush(this.ForeColor), 0, 20);
        }

        private void DrawNotes(Graphics g, Rectangle visibleAreaRectangle, Dictionary<Track.Command, CommandLineInfo> visibleNoteList)
        {
            if(Channel == null)
            {
                // nothing to draw
                return;
            }

            int duration = 1;
            int noteWidth = 1;
            int fullNoteDuration = 0x60;
            float pixelsPerDuration = (float)fullNoteWidth / (float)fullNoteDuration;

            int width = canvasWidth;
            int canvasX = visibleAreaRectangle.X;
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);
            int x = 0;
            foreach(var c in Channel.Commands)
            {
                var cmd = (Track.Command)c.Command;

                if (c is DurationCommand)
                {
                    duration = c.Command;
                    noteWidth = (int)(duration * pixelsPerDuration);
                }

                if(c is NoteCommand)
                {
                    if(cmd == Track.Command._C8_Tie || cmd == Track.Command._C9_Rest)
                    {
                        // skip these for now
                    }
                    else
                    {
                        if (visibleNoteList.ContainsKey(cmd))
                        {
                            var info = visibleNoteList[cmd];

                            int startX = x - horizontalOffset;
                            int offsetX = x - horizontalOffset + noteWidth;

                            if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                            {
                                //g.DrawLine(Pens.Black, x - horizontalOffset, 0, x - horizontalOffset, this.Height);

                                g.FillRectangle(Brushes.Purple, x - horizontalOffset, info.LineArea.Top, noteWidth, info.LineArea.Height);
                            }


                        }
                    }

                    x += noteWidth;
                }
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

            // todo: fix this
            left = 0;
            width = this.Width;

            top = verticalOffset;
            height = this.Height;

            return new Rectangle(left, top, width, height);
        }

        public struct CommandLineInfo
        {
            public Track.Command Command { get; set; }
            public Rectangle LineArea { get; set; }
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
            Debug.WriteLine($"verticalOffset {verticalOffset}");
            int y = noteThickness;
            for(int i = (int)Track.Command._C7_B6; i > (int)Track.Command._80_C1 - 1; --i)
            {
                Debug.WriteLine($"i: y {i} {y}");
                int topY = y - verticalOffset - noteThickness;
                int offsetY = y - verticalOffset;

                if (offsetY >= 0 && offsetY <= this.Height)
                {
                    g.DrawLine(Pens.Black, 0, y - verticalOffset, this.Right, y - verticalOffset);
                }
                y += noteHeightWithLine;
            }

            // vertical lines
            int width = canvasWidth;
            int canvasX = visibleArea.X;
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);
            int x = 0;
            for (int i = 0; i < width; i+=fullNoteWidth)
            {
                int startX = x - horizontalOffset - fullNoteWidth;
                int offsetX = x - horizontalOffset;

                if (offsetX >= 0 && offsetX <= this.Width)
                {
                    g.DrawLine(Pens.Black, x - horizontalOffset, 0, x - horizontalOffset, this.Height);
                }

                x += fullNoteWidth;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            this.MouseWheel?.Invoke(this, e);
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
