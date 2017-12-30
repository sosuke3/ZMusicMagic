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

namespace ZMusicMagicControls
{
    public partial class PianoRoll : UserControl
    {
        Channel m_channel;
        ScrollBar m_horizontalScroll;
        ScrollBar m_commandVerticalScroll;
        ScrollBar m_notesVerticalScroll;
        NoteEditor m_noteEditor;
        NoteLegend m_noteLegend;
        BeatLegend m_beatLegend;
        CommandEditor m_commandEditor;

        public Channel Channel
        {
            get { return m_channel; }
            set
            {
                m_channel = value;

                if (m_noteEditor != null)
                {
                    m_noteEditor.Channel = m_channel;
                }
                if(m_commandEditor != null)
                {
                    m_commandEditor.Channel = m_channel;
                }
                if(m_noteLegend != null)
                {
                    m_noteLegend.Channel = m_channel;
                }

                // repaint
                Invalidate();
            }
        }

        public PianoRoll()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer, true);

            InitializeMyComonents();
        }

        private void InitializeMyComonents()
        {
            this.SuspendLayout();

            this.m_horizontalScroll = new ScrollBar();
            this.m_horizontalScroll.Location = new Point(upperLeftWidth + guiThickLineWidth, this.ClientRectangle.Bottom - scrollBarFrameWidth);
            this.m_horizontalScroll.Size = new Size(this.ClientRectangle.Right - scrollBarFrameWidth - upperLeftWidth - guiThickLineWidth, scrollBarFrameWidth);
            this.m_horizontalScroll.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.m_horizontalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_horizontalScroll.ScrollBarColor = scrollBarColor;
            this.m_horizontalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            this.m_horizontalScroll.Orientation = ScrollBar.ScrollBarDirection.Horizonal;
            this.Controls.Add(this.m_horizontalScroll);

            this.m_commandVerticalScroll = new ScrollBar();
            this.m_commandVerticalScroll.Location = new Point(this.ClientRectangle.Right - scrollBarFrameWidth, 0);
            this.m_commandVerticalScroll.Size = new Size(scrollBarFrameWidth, commandAreaHeight - guiThickLineWidth / 2);
            this.m_commandVerticalScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.m_commandVerticalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_commandVerticalScroll.ScrollBarColor = scrollBarColor;
            this.m_commandVerticalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            this.m_commandVerticalScroll.Orientation = ScrollBar.ScrollBarDirection.Vertical;
            this.Controls.Add(this.m_commandVerticalScroll);

            this.m_notesVerticalScroll = new ScrollBar();
            this.m_notesVerticalScroll.Location = new Point(this.ClientRectangle.Right - scrollBarFrameWidth, upperLeftHeight + guiThickLineWidth);
            this.m_notesVerticalScroll.Size = new Size(scrollBarFrameWidth, this.ClientRectangle.Bottom - scrollBarFrameWidth - upperLeftHeight - guiThickLineWidth);
            this.m_notesVerticalScroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.m_notesVerticalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_notesVerticalScroll.ScrollBarColor = scrollBarColor;
            this.m_notesVerticalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            this.m_notesVerticalScroll.Orientation = ScrollBar.ScrollBarDirection.Vertical;
            this.Controls.Add(this.m_notesVerticalScroll);

            this.m_commandEditor = new CommandEditor();
            this.m_commandEditor.Location = new Point(upperLeftWidth + guiThickLineWidth, 0);
            this.m_commandEditor.Size = new Size(this.Width - upperLeftWidth - guiThickLineWidth - scrollBarFrameWidth, commandAreaHeight - guiThickLineWidth / 2);
            this.m_commandEditor.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.m_commandVerticalScroll.ValueChanged += this.m_commandEditor.VerticalScroll_ValueChanged;
            this.m_horizontalScroll.ValueChanged += this.m_commandEditor.HorizontalScroll_ValueChanged;
            this.m_commandEditor.MouseScroll += this.m_commandVerticalScroll.MouseWheelHandler;
            this.Controls.Add(this.m_commandEditor);

            this.m_beatLegend = new BeatLegend();
            this.m_beatLegend.Location = new Point(upperLeftWidth + guiThickLineWidth, commandAreaHeight + guiThickLineWidth / 2);
            this.m_beatLegend.Size = new Size(this.Width - upperLeftWidth - guiThickLineWidth, barLegendHeight);
            this.m_beatLegend.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            this.m_horizontalScroll.ValueChanged += this.m_beatLegend.HorizontalScroll_ValueChanged;
            this.m_beatLegend.BeatLineColor = barLineColor;
            this.m_beatLegend.BeatLineThickness = barLineThickness;
            this.m_beatLegend.SubBeatLineColor = subBarLineColor;
            this.m_beatLegend.SubBeatLineThickness = subBarLineThickness;
            this.Controls.Add(this.m_beatLegend);

            this.m_noteLegend = new NoteLegend();
            this.m_noteLegend.Location = new Point(0, upperLeftHeight + guiThickLineWidth);
            this.m_noteLegend.Size = new Size(upperLeftWidth, this.Height - upperLeftHeight - guiThickLineWidth);
            this.m_noteLegend.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            this.m_notesVerticalScroll.ValueChanged += this.m_noteLegend.VerticalScroll_ValueChanged;
            this.m_noteLegend.MouseScroll += this.m_notesVerticalScroll.MouseWheelHandler;
            this.m_noteLegend.TextAreaWidth = legendWidth;
            this.m_noteLegend.KeyAreaWidth = upperLeftWidth - legendWidth;
            this.Controls.Add(this.m_noteLegend);

            this.m_noteEditor = new NoteEditor();
            this.m_noteEditor.Location = new Point(upperLeftWidth + guiThickLineWidth, upperLeftHeight + guiThickLineWidth);
            this.m_noteEditor.Size = new Size(this.Width - upperLeftWidth - guiThickLineWidth - scrollBarFrameWidth, 
                this.Height - upperLeftHeight - guiThickLineWidth - scrollBarFrameWidth);
            this.m_noteEditor.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.m_notesVerticalScroll.ValueChanged += this.m_noteEditor.VerticalScroll_ValueChanged;
            this.m_horizontalScroll.ValueChanged += this.m_noteEditor.HorizontalScroll_ValueChanged;
            this.m_noteEditor.MouseScroll += this.m_notesVerticalScroll.MouseWheelHandler;
            this.m_noteEditor.BeatLineColor = barLineColor;
            this.m_noteEditor.BeatLineThickness = barLineThickness;
            this.m_noteEditor.SubBeatLineColor = subBarLineColor;
            this.m_noteEditor.SubBeatLineThickness = subBarLineThickness;
            this.Controls.Add(this.m_noteEditor);

            this.ResumeLayout(false);
        }


        const int scrollBarWidth = 6;
        const int halfScrollWidth = scrollBarWidth / 2;
        const int scrollBarFrameWidth = 10;

        const int noteLegendWidth = 14;
        const int noteHeight = 18;

        const int legendWidth = 46;

        const int guiThickLineWidth = 2;
        const int guiThinLineWidth = 1;

        const int commandAreaHeight = 50;

        const int barLegendHeight = 16;

        const int upperLeftWidth = 62;
        const int upperLeftHeight = commandAreaHeight + barLegendHeight + guiThickLineWidth / 2;

        Color noteColorUnselected = Color.FromArgb(192, 76, 233); // purple
        Color noteColorSelected = Color.FromArgb(255, 192, 0); // orange
        Color scrollFrameColor = Color.FromArgb(152, 152, 152); // gray
        Color scrollBarColor = Color.FromArgb(105, 105, 105); // dark gray
        Color backgroundColor = Color.FromArgb(229, 229, 229); // light gray

        const int barLineThickness = 1;
        Color barLineColor = Color.FromArgb(150, 150, 150);
        const int subBarLineThickness = 1;
        Color subBarLineColor = Color.FromArgb(200, 200, 200);

        const int fontSize = 14;
        const string font = "Arial";

        //int xScroll;
        //int yScroll;

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //m_horizontalScroll.ParentRectangle = this.ClientRectangle;
            //m_commandVerticalScroll.ParentRectangle = this.ClientRectangle;
            //m_notesVerticalScroll.ParentRectangle = this.ClientRectangle;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {

            //var totalNumberOfNotes = (int)ZMusicMagicLibrary.NSPC.Track.Command._C7_B6 - (int)ZMusicMagicLibrary.NSPC.Track.Command._80_C1;
            //var totalHeight = totalNumberOfNotes * noteHeight;
            //var maxPosition = this.ClientRectangle.Height - 3 * noteHeight; // / noteHeight;

            //// why is this always backwards... come on MS, we are not on a phone...
            //yScroll += (e.Delta > 0) ? -noteHeight : noteHeight;
            //yScroll = Utilities.Clamp(yScroll, 0, totalHeight - maxPosition);

            this.Invalidate();

            base.OnMouseWheel(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            //m_horizontalScroll.OnMouseDoubleClick(e);
            //m_commandVerticalScroll.OnMouseDoubleClick(e);
            //m_notesVerticalScroll.OnMouseDoubleClick(e);

            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            //m_horizontalScroll.OnMouseClick(e);
            //m_commandVerticalScroll.OnMouseClick(e);
            //m_notesVerticalScroll.OnMouseClick(e);

            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            //m_horizontalScroll.OnMouseDown(e);
            //m_commandVerticalScroll.OnMouseDown(e);
            //m_notesVerticalScroll.OnMouseDown(e);

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            //m_horizontalScroll.OnMouseMove(e);
            //m_commandVerticalScroll.OnMouseMove(e);
            //m_notesVerticalScroll.OnMouseMove(e);

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            //m_horizontalScroll.OnMouseUp(e);
            //m_commandVerticalScroll.OnMouseUp(e);
            //m_notesVerticalScroll.OnMouseUp(e);

            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            Pen thickLinePenBlack = new Pen(Color.Black, guiThickLineWidth);
            Pen thickLinePenDarkGray = new Pen(Color.DarkGray, guiThickLineWidth);
            thickLinePenBlack.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;
            thickLinePenDarkGray.Alignment = System.Drawing.Drawing2D.PenAlignment.Center;

            Brush scrollBarFrameBrush = new SolidBrush(scrollFrameColor);
            Brush scrollBarHandleBrush = new SolidBrush(scrollBarColor);

            var halfThickLineWidth = guiThickLineWidth / 2;

            int minX = this.ClientRectangle.Left;
            int minY = this.ClientRectangle.Top;
            int maxX = this.ClientRectangle.Right;
            int maxY = this.ClientRectangle.Bottom;

            // top two lines
            g.DrawLine(thickLinePenBlack, minX + upperLeftWidth + halfThickLineWidth, commandAreaHeight, maxX, commandAreaHeight);
            g.DrawLine(thickLinePenBlack, minX, upperLeftHeight + halfThickLineWidth, maxX, upperLeftHeight + halfThickLineWidth);

            // left line
            g.DrawLine(thickLinePenBlack, minX + upperLeftWidth + halfThickLineWidth, minY, minX + upperLeftWidth + halfThickLineWidth, maxY);

        }

        private static void DrawHorizontalScrollBar(Graphics g, Brush scrollBarFrameBrush, Brush scrollBarHandleBrush, int x, int y, int length)
        {
            g.FillRectangle(scrollBarFrameBrush, x, y, length, scrollBarFrameWidth);

            // handle
            int scrollStartX = 20; // TODO: calculate this
            int scrollHandleLength = 100; // TODO: calculate this based on how long our track is

            g.FillRectangle(scrollBarHandleBrush, 
                x + scrollStartX + 2 + halfScrollWidth, 
                y + 2, 
                scrollHandleLength - halfScrollWidth - 2, 
                scrollBarWidth);
            // left rounded cap
            g.FillEllipse(scrollBarHandleBrush, 
                x + scrollStartX + 2, 
                y + 2, 
                scrollBarWidth, 
                scrollBarWidth);
            // right rounded cap
            g.FillEllipse(scrollBarHandleBrush, 
                x + scrollStartX + scrollHandleLength - halfScrollWidth, 
                y + 2, 
                scrollBarWidth, 
                scrollBarWidth);
        }

    }
}
