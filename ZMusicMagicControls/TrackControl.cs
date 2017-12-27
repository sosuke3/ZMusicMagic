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

            int x = 5;
            int y = 5;
            int maxY = 0;
            foreach (var c in m_part.Channels)
            {
                int maxX = 0;

                foreach (var command in c.Commands)
                {
                    var cmd = (ZMusicMagicLibrary.NSPC.Track.Command)command.Command;
                    var text = $"{cmd.GetDescription()}";
                    if(command.Parameters.Count > 0)
                    {
                        text += $" ( {String.Join(",", command.Parameters.Select(o => o.ToString("X2")).ToArray())} )";
                    }
                    e.Graphics.DrawString(text, Font, new SolidBrush(ForeColor), x, y, style);

                    var stringSize = e.Graphics.MeasureString(text, Font);
                    y += (int)stringSize.Height + 2;

                    if((int)stringSize.Width > maxX)
                    {
                        maxX = (int)stringSize.Width + 10;
                    }
                }

                if(y > maxY)
                {
                    maxY = y;
                }

                x += maxX;
                y = 5;
            }

            this.AutoScrollMinSize = new Size(400, maxY+10);
        }
    }
}
