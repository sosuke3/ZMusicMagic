using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
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

        #region Properties and Fields
        public Channel Channel { get; set; }
        List<ChannelCommand> selectedNotes = new List<ChannelCommand>();

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
        int fullNoteDuration = 0x60;
        float pixelsPerDuration { get { return (float)fullNoteWidth / (float)fullNoteDuration; } }

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
        #endregion

        public NoteEditor()
        {
            //InitializeComponent();

            this.DoubleBuffered = true;
            
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Selectable, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            var visibleAreaRectangle = MakeVisibleNotesRectangle();

            var visibleNoteList = MakeVisibleNoteList(visibleAreaRectangle);

            //Debug.WriteLine(String.Join(",", visibleNoteList));

            DrawLines(g, visibleAreaRectangle);
            DrawNotes(g, visibleAreaRectangle, visibleNoteList);

            //g.DrawString($"{visibleAreaRectangle}", this.Font, new SolidBrush(this.ForeColor), 0, 0);
            //g.DrawString($"{String.Join(",", visibleNoteList)}", this.Font, new SolidBrush(this.ForeColor), 0, 20);
        }


        Rectangle MakeVisibleNotesRectangle()
        {
            // canvas height size
            int noteHeightWithLine = noteThickness + 1;
            int canvasHeight = 72 * noteHeightWithLine;
            int verticalOffset = (int)(scrollPosition.Y / 100.0 * canvasHeight);

            int horizontalOffset = (int)(scrollPosition.X / 100.0 * canvasWidth);

            int left;
            int top;
            int width;
            int height;

            // todo: fix this
            left = horizontalOffset;
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
            // vertical lines
            int width = canvasWidth;
            int horizontalOffset = visibleArea.X;
            int x = 0;
            for (int i = 0; i < width; i += fullNoteWidth)
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

            // horizontal lines
            int noteHeightWithLine = noteThickness + 1;

            int verticalOffset = visibleArea.Y;
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

        }

        void DrawBeatLines(Graphics g, int x, int y, int width, int height)
        {
            // beat
            g.DrawLine(beatLinePen, x, y, x, height);
            // 1/2
            int halfX = x + (width / 2);
            g.DrawLine(subBeatLinePen, halfX, y, halfX, height);
            // 1/4
            int quarterX = x + (width / 4);
            g.DrawLine(subBeatLinePen, quarterX, y, quarterX, height);
            // 3/4
            int threeQuarterX = x + 3 * (width / 4);
            g.DrawLine(subBeatLinePen, threeQuarterX, y, threeQuarterX, height);
        }

        private void DrawNotes(Graphics g, Rectangle visibleAreaRectangle, Dictionary<Track.Command, CommandLineInfo> visibleNoteList)
        {
            if (Channel == null || Channel.Commands.Count == 0)
            {
                // nothing to draw, let's just fill it with a color or pattern
                using (var brush = new HatchBrush(HatchStyle.BackwardDiagonal, Color.FromArgb(50, Color.Fuchsia), Color.FromArgb(50, Color.DarkGray)))
                {
                    g.FillRectangle(brush, this.ClientRectangle);
                }
                return;
            }

            DrawNotes(g, Channel.Commands, visibleAreaRectangle, visibleNoteList);

        }

        private void DrawNotes(Graphics g,
            List<ChannelCommand> commands,
            Rectangle visibleAreaRectangle,
            Dictionary<Track.Command, CommandLineInfo> visibleNoteList,
            int loopStartTime = 0,
            int loopTotalDuration = 0,
            int loopStartDuration = 0,
            Track.Command lastNote = 0,
            bool isLoop = false)
        {
            if (commands == null || commands.Count == 0)
            {
                return;
            }

            Brush unselectedBrush = new SolidBrush(Color.FromArgb(190, 76, 233));
            Brush selectedBrush = new SolidBrush(Color.FromArgb(255, 192, 0));
            if (isLoop)
            {
                unselectedBrush = Brushes.Silver;
            }
            var outlineBrush = Brushes.Black;

            int startDuration = loopStartDuration;
            int lastDuration = 0;

            int width = canvasWidth;
            int horizontalOffset = visibleAreaRectangle.X;
            foreach (var c in commands)
            {
                var cmd = (Track.Command)c.Command;

                if (c is DurationCommand)
                {
                    lastDuration = c.Command;
                    startDuration = -1;
                }

                if (c is CallLoopCommand)
                {
                    var loop = c as CallLoopCommand;
                    if (loop != null)
                    {
                        for (int i = 0; i < loop.LoopCount; ++i)
                        {
                            DrawNotes(g, loop.LoopPart.Commands, visibleAreaRectangle, visibleNoteList, c.StartTime + (c.Duration * i), c.Duration, lastDuration, lastNote, true);
                        }
                    }
                }

                if (c is NoteCommand)
                {
                    var noteCommand = c as NoteCommand;
                    var duration = noteCommand.Duration;
                    if(duration == 0)
                    {
                        if (loopStartDuration > -1)
                        {
                            duration = loopStartDuration;
                        }
                        else
                        {
                            Debugger.Break(); // shouldn't hit this.
                        }
                    }
                    var noteWidth = (int)(duration * pixelsPerDuration);
                    int x = (int)((noteCommand.StartTime + loopStartTime) * pixelsPerDuration);

                    if (cmd == Track.Command._C8_Tie)
                    {
                        if (visibleNoteList.ContainsKey(lastNote))
                        {
                            var info = visibleNoteList[lastNote];

                            var mag = new SolidBrush(Color.FromArgb(200, 255, 0, 255));
                            int startX = x - horizontalOffset;
                            int offsetX = x - horizontalOffset + noteWidth;

                            if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                            {
                                int thickness = 4;
                                if(thickness > info.LineArea.Height)
                                {
                                    thickness = info.LineArea.Height;
                                }
                                int y = info.LineArea.Top + info.LineArea.Height / 2 - thickness / 2;
                                var noteRectangle = new Rectangle(startX, y, noteWidth, thickness);
                                var brush = unselectedBrush;
                                if (selectedNotes.Contains(c))
                                {
                                    brush = selectedBrush;
                                }
                                g.FillRectangle(brush, noteRectangle);
                                int endX = noteRectangle.Right - 2;
                                if(endX < noteRectangle.Left)
                                {
                                    endX = noteRectangle.Left;
                                }
                                g.FillRectangle(brush, endX, info.LineArea.Y, 2, info.LineArea.Height);
                            }
                        }
                    }
                    else if(cmd == Track.Command._C9_Rest)
                    {
                        var mag = new SolidBrush(Color.FromArgb(40, 255, 0, 255));
                        g.FillRectangle(mag, x - horizontalOffset, 0, noteWidth, this.Height);
                    }
                    else
                    {
                        lastNote = noteCommand.CommandType;

                        if (visibleNoteList.ContainsKey(cmd))
                        {
                            var info = visibleNoteList[cmd];

                            int startX = x - horizontalOffset;
                            int offsetX = x - horizontalOffset + noteWidth;

                            if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                            {
                                //g.DrawLine(Pens.Black, x - horizontalOffset, 0, x - horizontalOffset, this.Height);

                                var noteRectangle = new Rectangle(startX, info.LineArea.Top, noteWidth, info.LineArea.Height);
                                var brush = unselectedBrush;
                                if(selectedNotes.Contains(c))
                                {
                                    brush = selectedBrush;
                                }
                                g.FillRectangle(brush, noteRectangle);
                                g.DrawRectangle(Pens.Black, noteRectangle);

                                g.SetClip(noteRectangle);
                                var noteText = noteCommand.CommandType.GetDescription();
                                g.DrawString(noteText, this.Font, Brushes.Black, noteRectangle.X + 1, noteRectangle.Y + noteRectangle.Height / 2 - g.MeasureString(noteText, this.Font).Height / 2);
                                g.ResetClip();
                            }
                        }
                    }
                }
            }


            if (isLoop)
            {
                int loopStartX = (int)(loopStartTime * pixelsPerDuration);
                int loopEndX = loopStartX + (int)(loopTotalDuration * pixelsPerDuration);

                int startX = loopStartX - horizontalOffset;
                int offsetX = loopEndX - horizontalOffset;

                if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
                {
                    var loopRectangle = new Rectangle(startX, 0, loopEndX - loopStartX, this.Height);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(40, 0, 0, 0)), loopRectangle);
                    g.DrawRectangle(Pens.Magenta, loopRectangle);
                }
            }
            else
            {
                // draw a nice big line at the end until I fix the canvas size and zoom
                var endPen = new Pen(Color.DarkGreen);
                endPen.Width = 4;

                var x = Channel.EndTime * pixelsPerDuration - horizontalOffset + endPen.Width / 2;
                g.DrawLine(endPen, x, 0, x, this.Height);
            }
        }


        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.MouseScroll?.Invoke(this, e);

            base.OnMouseWheel(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            this.Focus();

            var visibleArea = MakeVisibleNotesRectangle();
            var visibleNotes = MakeVisibleNoteList(visibleArea);
            var selectedBrush = Brushes.Orange;

            int previousCount = selectedNotes.Count;

            // check for ctrl?
            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                selectedNotes.Clear();
            }

            using (var g = this.CreateGraphics())
            {
                foreach (var info in visibleNotes.Values)
                {
                    if (info.LineArea.Contains(e.Location))
                    {
                        var xOffset = this.scrollPosition.X;
                        var notes = this.Channel.Commands
                            .Where(x => x.CommandType == info.Command)
                            .Where(x => x.StartTime * pixelsPerDuration - xOffset <= e.Location.X)
                            .Where(x => x.StartTime * pixelsPerDuration - xOffset + x.Duration * pixelsPerDuration >= e.Location.X)
                            .FirstOrDefault();
                        if (notes != null)
                        {
                            selectedNotes.Add(notes);
                            //g.FillRectangle(selectedBrush, notes.StartTime * pixelsPerDuration - xOffset, info.LineArea.Y, notes.Duration * pixelsPerDuration, info.LineArea.Height);

                        }
                    }
                }
            }

            if (selectedNotes.Count > 0 || previousCount != 0)
            {
                Invalidate();
            }

            base.OnMouseClick(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            switch(e.KeyCode)
            {
                // hack because someone at MS really didn't think through OnKeyDown, etc.
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Track.Command minNote = Track.Command._C7_B6;
            Track.Command maxNote = Track.Command._80_C1;
            bool changed = false;

            foreach(var c in selectedNotes)
            {
                if(c.Command < (int)minNote)
                {
                    minNote = c.CommandType;
                }
                if(c.Command > (int)maxNote)
                {
                    maxNote = c.CommandType;
                }
            }

            if(e.KeyCode == Keys.Up)
            {
                if(maxNote != Track.Command._C7_B6)
                {
                    foreach (var c in selectedNotes)
                    {
                        if (c.CommandType == Track.Command._C7_B6)
                        {

                        }
                        else
                        {
                            c.Command++;
                            changed = true;
                        }
                    }
                }
            }
            if (e.KeyCode == Keys.Down)
            {
                if (minNote != Track.Command._80_C1)
                {
                    foreach (var c in selectedNotes)
                    {
                        if (c.CommandType == Track.Command._80_C1)
                        {

                        }
                        else
                        {
                            c.Command--;
                            changed = true;
                        }
                    }
                }
            }

            base.OnKeyDown(e);

            if(changed)
            {
                Invalidate();
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
