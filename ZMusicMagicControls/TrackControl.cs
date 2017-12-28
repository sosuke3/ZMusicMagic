using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZMusicMagicLibrary;

namespace ZMusicMagicControls
{
    public partial class TrackControl : Panel
    {

        Part m_part;

        [Browsable(false)]
        [Category("")]
        [Description("")]
        [DisplayName("")]
        public Part Part
        {
            get { return m_part; }
            set
            {
                m_part = value;
                // repaint
                Invalidate();
            }
        }

        public TrackControl()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer, true);


        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
            base.OnPaint(e);

            StringFormat style = new StringFormat();
            style.Alignment = StringAlignment.Near;

            if(m_part == null)
            {
                return;
            }

            List<int> columnX = new List<int>();

            int x = 5;
            int y = 5;
            int maxY = 0;
            foreach (var c in m_part.Channels)
            {
                int maxX = 0;

                foreach (var command in c.Commands)
                {
                    SizeF stringSize = DrawCommand(e, style, x, ref y, command);

                    if ((int)stringSize.Width > maxX)
                    {
                        maxX = (int)stringSize.Width + 10;
                    }

                    if (command is CallLoopCommand)
                    {
                        const int loopOffset = 20;

                        var loop = command as CallLoopCommand;
                        int loopX = x + loopOffset;
                        foreach (var lp in loop.LoopPart.Commands)
                        {
                            var loopSize = DrawCommand(e, style, loopX, ref y, lp);

                            if ((int)loopSize.Width + loopOffset > maxX)
                            {
                                maxX = (int)stringSize.Width + loopOffset + 10;
                            }
                        }
                    }
                }

                if (y > maxY)
                {
                    maxY = y;
                }

                x += maxX;
                y = 5;

                if (maxX == 0)
                {
                    x += 100;
                }

                columnX.Add(x);

                x += 5;
            }

            foreach (var c in columnX.Take(columnX.Count - 1))
            {
                e.Graphics.DrawLine(Pens.Blue, c, 0, c, maxY);
            }

            this.AutoScrollMinSize = new Size(400, maxY+10);
        }

        private SizeF DrawCommand(PaintEventArgs e, StringFormat style, int x, ref int y, ChannelCommand command)
        {
            var cmd = (ZMusicMagicLibrary.NSPC.Track.Command)command.Command;
            var text = $"{cmd.GetDescription()}";
            if (command.Command >= 0x01 && command.Command < 0x80)
            {
                if (command is DurationCommand)
                {
                    text = "Duration";
                }
                else
                {
                    text = "Velocity/Staccato";
                }
                text += $" ( {command.Command.ToString("X2")} )";
            }
            if (command.Parameters.Count > 0)
            {
                text += $" ( {String.Join(",", command.Parameters.Select(o => o.ToString("X2")).ToArray())} )";
            }
            e.Graphics.DrawString(text, Font, new SolidBrush(ForeColor), x, y, style);

            var stringSize = e.Graphics.MeasureString(text, Font);
            y += (int)stringSize.Height + 2;
            return stringSize;
        }

        void DrawCommand(ChannelCommand command, int xBase, int yBase, Graphics g, StringFormat style)
        {
            int x = xBase;
            int y = yBase;

            var cmd = (ZMusicMagicLibrary.NSPC.Track.Command)command.Command;
            var text = $"{cmd.GetDescription()}";
            if (command.Command >= 0x01 && command.Command < 0x80)
            {
                text = $"{command.Command.ToString("X2")}";
            }
            if (command.Parameters.Count > 0)
            {
                text += $" ( {String.Join(",", command.Parameters.Select(o => o.ToString("X2")).ToArray())} )";
            }
            g.DrawString(text, Font, new SolidBrush(ForeColor), x, y, style);

            var stringSize = g.MeasureString(text, Font);
            y += (int)stringSize.Height + 2;

            //if ((int)stringSize.Width > maxX)
            //{
            //    maxX = (int)stringSize.Width + 10;
            //}
        }
    }
}
