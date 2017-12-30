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

            DrawNotes(g, ref x, ref duration, ref noteWidth, Channel.Commands, visibleAreaRectangle, visibleNoteList);

        }

        private void DrawNotes(Graphics g, ref int x, ref int duration, ref int noteWidth, List<ChannelCommand> commands, Rectangle visibleAreaRectangle, Dictionary<Track.Command, CommandLineInfo> visibleNoteList, bool isLoop = false)
        {
            if(commands == null || commands.Count == 0)
            {
                return;
            }

            var unselectedBrush = Brushes.Purple;
            var selectedBrush = Brushes.Orange;
            if (isLoop)
            {
                unselectedBrush = Brushes.LightSlateGray;
            }
            var outlineBrush = Brushes.Black;

            int loopStartX = x;

            int fullNoteDuration = 0x60;
            float pixelsPerDuration = (float)fullNoteWidth / (float)fullNoteDuration;

            int width = canvasWidth;
            int canvasX = visibleAreaRectangle.X;
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);
            foreach (var c in commands)
            {
                var cmd = (Track.Command)c.Command;

                if (c is DurationCommand)
                {
                    duration = c.Command;
                    noteWidth = (int)(duration * pixelsPerDuration);
                }

                if (c is CallLoopCommand)
                {
                    var loop = c as CallLoopCommand;
                    if(loop != null)
                    {
                        for (int i = 0; i < loop.LoopCount; ++i)
                        {
                            DrawNotes(g, ref x, ref duration, ref noteWidth, loop.LoopPart.Commands, visibleAreaRectangle, visibleNoteList, true);
                        }
                    }
                }

                if (c is NoteCommand)
                {
                    if (cmd == Track.Command._C8_Tie || cmd == Track.Command._C9_Rest)
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

                                var noteRectangle = new Rectangle(startX, info.LineArea.Top, noteWidth, info.LineArea.Height);
                                g.FillRectangle(unselectedBrush, noteRectangle);
                                g.DrawRectangle(Pens.Black, noteRectangle);
                            }


                        }
                    }

                    x += noteWidth;
                }
            }

            int loopEndX = x;

            if (isLoop)
            {
                int startX = loopStartX - horizontalOffset;
                int offsetX = loopEndX - horizontalOffset;

                if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                {
                    var loopRectangle = new Rectangle(startX, 0, loopEndX - loopStartX, this.Height);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 0, 0, 0)), loopRectangle);
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
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);
            int x = 0;
            for (int i = 0; i < width; i+=fullNoteWidth)
            {
                int startX = x - horizontalOffset;
                int offsetX = x - horizontalOffset + fullNoteWidth;

                if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                {
                    DrawBeatLines(g, startX, 0, fullNoteWidth, this.Height);
                    //g.DrawLine(Pens.Black, x - horizontalOffset, 0, x - horizontalOffset, this.Height);
                }

                x += fullNoteWidth;
            }
        }

        void DrawBeatLines(Graphics g, int x, int y, int width, int height)
        {
            g.DrawLine(beatLinePen, x, y, x, height);

            int halfX = x + (width / 2);
            g.DrawLine(subBeatLinePen, halfX, y, halfX, height);

            int quarterX = x + (width / 4);
            g.DrawLine(subBeatLinePen, quarterX, y, quarterX, height);
            int threeQuarterX = x + 3 * (width / 4);
            g.DrawLine(subBeatLinePen, threeQuarterX, y, threeQuarterX, height);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            this.MouseScroll?.Invoke(this, e);
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
