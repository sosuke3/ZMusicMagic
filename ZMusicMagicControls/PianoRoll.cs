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
        Part m_part;
        ScrollBar m_horizontalScroll;
        ScrollBar m_commandVerticalScroll;
        ScrollBar m_notesVerticalScroll;

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

        public PianoRoll()
        {
            InitializeComponent();

            this.DoubleBuffered = true;

            this.SetStyle(ControlStyles.Selectable | ControlStyles.OptimizedDoubleBuffer, true);

            this.SuspendLayout();
            this.m_horizontalScroll = new ScrollBar();
            this.m_horizontalScroll.Location = new Point(upperLeftWidth + guiThickLineWidth, this.ClientRectangle.Bottom - scrollBarFrameWidth);
            this.m_horizontalScroll.Size = new Size(this.ClientRectangle.Right - scrollBarFrameWidth - upperLeftWidth - guiThickLineWidth, scrollBarFrameWidth);
                //new Rectangle(upperLeftWidth + guiThickLineWidth, this.ClientRectangle.Bottom - scrollBarFrameWidth, 2, 2));
            this.m_horizontalScroll.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            this.m_horizontalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_horizontalScroll.ScrollBarColor = scrollBarColor;
            this.m_horizontalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            //this.m_horizontalScroll.AnchorEndRelativeOffset = new Size(scrollBarFrameWidth, scrollBarFrameWidth);
            this.m_horizontalScroll.Orientation = ScrollBar.ScrollBarDirection.Horizonal;
            //this.m_horizontalScroll.PositionChanged += OnHorizontalScrollPositionChanged;
            this.Controls.Add(this.m_horizontalScroll);

            this.m_commandVerticalScroll = new ScrollBar();
                //new Rectangle(this.ClientRectangle.Right - scrollBarFrameWidth, 0, 2, commandAreaHeight - guiThickLineWidth / 2));
            this.m_commandVerticalScroll.Location = new Point(this.ClientRectangle.Right - scrollBarFrameWidth, 0);
            this.m_commandVerticalScroll.Size = new Size(scrollBarFrameWidth, commandAreaHeight - guiThickLineWidth / 2);
            this.m_commandVerticalScroll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.m_commandVerticalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_commandVerticalScroll.ScrollBarColor = scrollBarColor;
            this.m_commandVerticalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            //this.m_commandVerticalScroll.AnchorEndRelativeOffset = new Size(scrollBarFrameWidth, scrollBarFrameWidth);
            this.m_commandVerticalScroll.Orientation = ScrollBar.ScrollBarDirection.Vertical;
            //this.m_commandVerticalScroll.PositionChanged += OnCommandVerticalScrollPositionChanged;
            this.Controls.Add(this.m_commandVerticalScroll);

            this.m_notesVerticalScroll = new ScrollBar();
                //new Rectangle(this.ClientRectangle.Right - scrollBarFrameWidth, upperLeftHeight + guiThickLineWidth, 2, 2));
            this.m_notesVerticalScroll.Location = new Point(this.ClientRectangle.Right - scrollBarFrameWidth, upperLeftHeight + guiThickLineWidth);
            this.m_notesVerticalScroll.Size = new Size(scrollBarFrameWidth, this.ClientRectangle.Bottom - scrollBarFrameWidth - upperLeftHeight - guiThickLineWidth);
            this.m_notesVerticalScroll.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.m_notesVerticalScroll.ScrollFrameColor = scrollFrameColor;
            this.m_notesVerticalScroll.ScrollBarColor = scrollBarColor;
            this.m_notesVerticalScroll.ScrollFrameThickness = scrollBarFrameWidth;
            //this.m_notesVerticalScroll.AnchorEndRelativeOffset = new Size(scrollBarFrameWidth, scrollBarFrameWidth);
            this.m_notesVerticalScroll.Orientation = ScrollBar.ScrollBarDirection.Vertical;
            //this.m_notesVerticalScroll.PositionChanged += OnNotesVerticalScrollPositionChanged;
            this.Controls.Add(this.m_notesVerticalScroll);
            this.ResumeLayout(false);
        }

        private void OnNotesVerticalScrollPositionChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void OnCommandVerticalScrollPositionChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void OnHorizontalScrollPositionChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        const int scrollBarWidth = 6;
        const int halfScrollWidth = scrollBarWidth / 2;
        const int scrollBarFrameWidth = 10;

        const int noteLegendWidth = 14;
        const int noteHeight = 18;

        const int legendWidth = 46;

        const int guiThickLineWidth = 2;
        const int guiThinLineWidth = 1;

        const int upperLeftWidth = 62;
        const int upperLeftHeight = 113;

        const int commandAreaHeight = 95;

        const int barLegendHeight = 16;

        Color noteColorUnselected = Color.FromArgb(192, 76, 233); // purple
        Color noteColorSelected = Color.FromArgb(255, 192, 0); // orange
        Color scrollFrameColor = Color.FromArgb(152, 152, 152); // gray
        Color scrollBarColor = Color.FromArgb(105, 105, 105); // dark gray
        Color backgroundColor = Color.FromArgb(229, 229, 229); // light gray

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
            base.OnMouseWheel(e);

            //var totalNumberOfNotes = (int)ZMusicMagicLibrary.NSPC.Track.Command._C7_B6 - (int)ZMusicMagicLibrary.NSPC.Track.Command._80_C1;
            //var totalHeight = totalNumberOfNotes * noteHeight;
            //var maxPosition = this.ClientRectangle.Height - 3 * noteHeight; // / noteHeight;

            //// why is this always backwards... come on MS, we are not on a phone...
            //yScroll += (e.Delta > 0) ? -noteHeight : noteHeight;
            //yScroll = Utilities.Clamp(yScroll, 0, totalHeight - maxPosition);

            this.Invalidate();
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            //m_horizontalScroll.OnMouseDoubleClick(e);
            //m_commandVerticalScroll.OnMouseDoubleClick(e);
            //m_notesVerticalScroll.OnMouseDoubleClick(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            //m_horizontalScroll.OnMouseClick(e);
            //m_commandVerticalScroll.OnMouseClick(e);
            //m_notesVerticalScroll.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            //m_horizontalScroll.OnMouseDown(e);
            //m_commandVerticalScroll.OnMouseDown(e);
            //m_notesVerticalScroll.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            //m_horizontalScroll.OnMouseMove(e);
            //m_commandVerticalScroll.OnMouseMove(e);
            //m_notesVerticalScroll.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            //m_horizontalScroll.OnMouseUp(e);
            //m_commandVerticalScroll.OnMouseUp(e);
            //m_notesVerticalScroll.OnMouseUp(e);
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

            // bottom scroll bar
            //int bottomScrollMinX = minX + upperLeftWidth + guiThickLineWidth;
            //int bottomScrollY = maxY - scrollBarFrameWidth;
            //int bottomScrollWidth = maxX - bottomScrollMinX - scrollBarFrameWidth;
            //DrawHorizontalScrollBar(g, scrollBarFrameBrush, scrollBarHandleBrush, bottomScrollMinX, bottomScrollY, bottomScrollWidth);

            //m_horizontalScroll.Draw(g);

            // command scroll bar
            //int commandScrollX = maxX - scrollBarFrameWidth;
            //int commandScrollMinY = minY;
            //int commandScrollHeight = commandAreaHeight - halfThickLineWidth;
            //DrawVerticalScrollBar(g, scrollBarFrameBrush, scrollBarHandleBrush, commandScrollX, commandScrollMinY, commandScrollHeight);
            
            //m_commandVerticalScroll.Draw(g);

            // note scroll bar
            //int noteScrollX = maxX - scrollBarFrameWidth;
            //int noteScrollMinY = upperLeftHeight + guiThickLineWidth;
            //int noteScrollHeight = maxY - (upperLeftHeight + guiThickLineWidth) - scrollBarFrameWidth;
            //DrawVerticalScrollBar(g, scrollBarFrameBrush, scrollBarHandleBrush, noteScrollX, noteScrollMinY, noteScrollHeight);

            //m_notesVerticalScroll.Draw(g);




            //g.DrawString(yScroll.ToString(), this.Font, new SolidBrush(ForeColor), 0, 0);

            //g.DrawLine(Pens.Black, this.ClientRectangle.Left + legendWidth, this.ClientRectangle.Top, this.ClientRectangle.Left + legendWidth, this.ClientRectangle.Bottom);

            //// C1 - B6
            //var totalNumberOfNotes = (int)ZMusicMagicLibrary.NSPC.Track.Command._C7_B6 - (int)ZMusicMagicLibrary.NSPC.Track.Command._80_C1;
            //var totalHeight = totalNumberOfNotes * noteHeight;
            //var currentNote = totalNumberOfNotes - (yScroll / noteHeight);
            //Debug.WriteLine($"totalNumberOfNotes: {totalNumberOfNotes}, totalHeight: {totalHeight}, currentNote: {currentNote}, yScroll: {yScroll}, noteHeight: {noteHeight}");

            //var note = (ZMusicMagicLibrary.NSPC.Track.Command)((int)ZMusicMagicLibrary.NSPC.Track.Command._80_C1 + currentNote);

            //for (int i = 1; i < this.ClientRectangle.Height / noteHeight; i++)
            //{
            //    g.DrawString(note.GetDescription(), this.Font, new SolidBrush(ForeColor), this.ClientRectangle.Left, this.ClientRectangle.Top + (noteHeight * i) + (noteHeight / 2) - g.MeasureString(note.GetDescription(), this.Font).Height / 2);
            //    g.DrawLine(Pens.Black, this.ClientRectangle.Left + legendWidth, this.ClientRectangle.Top + (noteHeight * i), this.ClientRectangle.Right, this.ClientRectangle.Top + (noteHeight * i));

            //    note = (ZMusicMagicLibrary.NSPC.Track.Command)((int)note - 1);
            //}

        }

        private static void DrawVerticalScrollBar(Graphics g, Brush scrollBarFrameBrush, Brush scrollBarHandleBrush, int x, int y, int length)
        {
            g.FillRectangle(scrollBarFrameBrush, x, y, scrollBarFrameWidth, length);

            // handle
            int scrollStartY = 11; // TODO: calculate this
            int scrollHandleLength = 20; // TODO: calculate this

            g.FillRectangle(scrollBarHandleBrush,
                x + 2,
                y + scrollStartY + 2 + halfScrollWidth,
                scrollBarWidth,
                scrollHandleLength - halfScrollWidth - 2);
            // top rounded cap
            g.FillEllipse(scrollBarHandleBrush,
                x + 2,
                y + scrollStartY + 2,
                scrollBarWidth,
                scrollBarWidth);

            // bottom rounded cap
            g.FillEllipse(scrollBarHandleBrush,
                x + 2,
                y + scrollStartY + scrollHandleLength - halfScrollWidth,
                scrollBarWidth,
                scrollBarWidth);
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
