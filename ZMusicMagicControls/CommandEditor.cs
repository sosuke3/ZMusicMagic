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
    public partial class CommandEditor : Control
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

        int canvasHeight = 200;
        public int CanvasHeight
        {
            get { return canvasHeight; }
            set
            {
                canvasHeight = value;

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

        public CommandEditor()
        {
            //InitializeComponent();

            this.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            DrawCommands(g);
        }

        private void DrawCommands(Graphics g)
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

            int x = 0;

            DrawCommands(g, ref x, ref duration, ref noteWidth, Channel.Commands);

        }

        private void DrawCommands(Graphics g, ref int x, ref int duration, ref int noteWidth, List<ChannelCommand> commands, bool isLoop = false)
        {
            if(commands == null || commands.Count == 0)
            {
                return;
            }

            int loopStartX = x;

            int fullNoteDuration = 0x60;
            float pixelsPerDuration = (float)fullNoteWidth / (float)fullNoteDuration;

            int width = canvasWidth;
            int horizontalOffset = (int)(scrollPosition.X / 100.0 * width);

            int verticalOffset = (int)(scrollPosition.Y / 100.0 * canvasHeight);

            int y = 0;
            foreach (var c in commands)
            {
                var cmd = (Track.Command)c.Command;

                if (c is DurationCommand)
                {
                    duration = c.Command;
                    noteWidth = (int)(duration * pixelsPerDuration);
                }

                if (c is SettingCommand)
                {
                    g.DrawString(cmd.GetDescription(), this.Font, Brushes.Black, x, y - verticalOffset);
                    y += 10;
                }

                if (c is CallLoopCommand)
                {
                    var loop = c as CallLoopCommand;
                    if(loop != null)
                    {
                        for (int i = 0; i < loop.LoopCount; ++i)
                        {
                            DrawCommands(g, ref x, ref duration, ref noteWidth, loop.LoopPart.Commands, true);
                        }
                    }
                }

                if (c is NoteCommand)
                {
                    x += noteWidth;
                    y = 0;
                }
            }

            //int loopEndX = x;

            //if (isLoop)
            //{
            //    int startX = loopStartX - horizontalOffset;
            //    int offsetX = loopEndX - horizontalOffset;

            //    if ((startX >= 0 && startX <= this.Width) || (offsetX >= 0 && offsetX <= this.Width))
            //    {
            //        var loopRectangle = new Rectangle(startX, 0, loopEndX - loopStartX, this.Height);
            //        g.FillRectangle(new SolidBrush(Color.FromArgb(40, 0, 0, 0)), loopRectangle);
            //    }
            //}
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
