using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZMusicMagicControls
{
    public class ScrollBar : Control
    {
        public enum ScrollBarDirection { Horizonal, Vertical }

        public int Minimum { get; set; } = 0;
        public int Maximum { get; set; } = 100;

        float value = 0;
        int largeChange = 10;
        int smallChange = 1;

        public float Value
        {
            get { return value; }
            set
            {
                this.value = ZMusicMagicLibrary.Utilities.Clamp(value, this.Minimum, this.Maximum);

                //PositionChanged?.Invoke(this, new EventArgs());
            }
        }

        public int LargeChange
        {
            get { return largeChange; }
            set
            {
                this.largeChange = value;
            }
        }

        public int SmallChange
        {
            get { return smallChange; }
            set
            {
                this.smallChange = value;
            }
        }

        public int ScrollFrameThickness { get; set; } = 10;

        public ScrollBarDirection Orientation { get; set; }

        public event EventHandler<EventArgs> ValueChanged;
        public event EventHandler<EventArgs> Scroll;
        //public event EventHandler<EventArgs> PositionChanged;

        Color scrollFrameColor;
        Brush scrollFrameBrush;
        public Color ScrollFrameColor
        {
            get { return scrollFrameColor; }
            set
            {
                scrollFrameColor = value;
                scrollFrameBrush = new SolidBrush(scrollFrameColor);
            }
        }
        Color scrollBarColor;
        Brush scrollBarBrush;
        public Color ScrollBarColor
        {
            get { return scrollBarColor; }
            set
            {
                scrollBarColor = value;
                scrollBarBrush = new SolidBrush(scrollBarColor);
            }
        }
        

        bool isDragging = false;

        public ScrollBar()
        {
            this.DoubleBuffered = true;
            SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        //public void OnMouseCaptureChanged(MouseEventArgs e)
        //{
        //}
        protected override void OnMouseClick(MouseEventArgs e)
        {
            // scroll to where they clicked
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            OnMouseClick(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //Debug.WriteLine($"OnMouseDown {e.Location.ToString()}");
            var point = e.Location;
            var barRectangle = CalculateHandleRectangle(true);
            //Debug.WriteLine($"barRectangle {barRectangle.ToString()}");

            if (barRectangle.Contains(point))
            {
                //previousLocation = e.Location;
                if (this.Orientation == ScrollBarDirection.Horizonal)
                {
                    clickOffset = point.X - barRectangle.Left;
                }
                else
                {
                    clickOffset = point.Y - barRectangle.Top;
                }
                isDragging = true;
                //Debug.WriteLine($"clickOffset {clickOffset}");
            }
            else
            {
                // outside the bar
                // was it before or after?
                int clickSpot = 0;
                int beforeBar = 0;
                int afterBar = 0;
                if(this.Orientation == ScrollBarDirection.Horizonal)
                {
                    clickSpot = point.X;
                    beforeBar = barRectangle.Left;
                    afterBar = barRectangle.Right;
                }
                else
                {
                    clickSpot = point.Y;
                    beforeBar = barRectangle.Top;
                    afterBar = barRectangle.Bottom;
                }
                //Debug.WriteLine($"clickSpot {clickSpot}");
                //Debug.WriteLine($"beforeBar {beforeBar}");
                //Debug.WriteLine($"afterBar {afterBar}");
                //Debug.WriteLine($"Value {Value}");

                if (clickSpot < beforeBar)
                {
                    // less
                    Value -= LargeChange;
                }
                else
                {
                    // more
                    Value += LargeChange;
                }
                //Debug.WriteLine($"Value {Value}");
                Application.DoEvents();

                Invalidate();

                this.ValueChanged?.Invoke(this, new EventArgs());
                this.Scroll?.Invoke(this, new EventArgs());
            }
        }
        //public void OnMouseEnter(MouseEventArgs e)
        //{
        //}
        //public void OnMouseHover(MouseEventArgs e)
        //{
        //}
        //public void OnMouseLeave(MouseEventArgs e)
        //{
        //}
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.MouseWheelHandler(this, e);

            base.OnMouseWheel(e);
        }

        public void MouseWheelHandler(object source, MouseEventArgs e)
        {
            this.Value -= ZMusicMagicLibrary.Utilities.Clamp(e.Delta, -1, 1); // todo: make this go faster depending on delta, or something

            Invalidate();

            this.ValueChanged?.Invoke(this, new EventArgs());
            this.Scroll?.Invoke(this, new EventArgs());
        }

        int clickOffset;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if(this.isDragging)
            {
                //Debug.WriteLine($"OnMouseMove {e.Location.ToString()}");

                var point = e.Location;
                var barRectangle = CalculateHandleRectangle();
                //Debug.WriteLine($"barRectangle {barRectangle.ToString()}");
                var valueRange = Maximum - Minimum;
                var pixelRange = 0;
                int newClickOffset = 0;
                if (this.Orientation == ScrollBarDirection.Horizonal)
                {
                    newClickOffset = point.X - clickOffset;
                    pixelRange = this.Width - barRectangle.Width - 4;
                }
                else
                {
                    newClickOffset = point.Y - clickOffset;
                    pixelRange = this.Height - barRectangle.Height - 4;
                }
                //Debug.WriteLine($"newClickOffset {newClickOffset}");
                //Debug.WriteLine($"pixelRange {pixelRange}");

                if (valueRange > 0 && pixelRange > 0)
                {
                    newClickOffset = ZMusicMagicLibrary.Utilities.Clamp(newClickOffset, 0, pixelRange);
                    //Debug.WriteLine($"newClickOffset {newClickOffset}");

                    float perc = (float)newClickOffset / (float)pixelRange;
                    value = perc * (Maximum);
                    //Debug.WriteLine($"value {value}");
                    Application.DoEvents();

                    Invalidate();

                    this.ValueChanged?.Invoke(this, new EventArgs());
                    this.Scroll?.Invoke(this, new EventArgs());
                }
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            // draw frame
            g.FillRectangle(scrollFrameBrush, 0, 0, this.Width, this.Height);

            // draw bar
            var barRectange = CalculateHandleRectangle();
            g.FillRectangle(this.scrollBarBrush, barRectange);
        }


        Rectangle CalculateHandleRectangle(bool fullWidth = false)
        {
            int maxHandleLength = 0;
            if (Orientation == ScrollBarDirection.Horizonal)
            {
                maxHandleLength = this.Width - 4; // 2 pixel padding
            }
            else
            {
                maxHandleLength = this.Height - 4;
            }

            if (maxHandleLength < 1)
            {
                // nothing to draw
                return Rectangle.Empty;
            }

            int range = Maximum - Minimum;
            if (range <= 0)
            {
                // wtf did you do!
                return Rectangle.Empty;
            }

            int barLength = maxHandleLength < 30 ? maxHandleLength : 30; // TODO: maybe do something different with this to scale it

            float barHeadMaxRelativePosition = maxHandleLength - barLength;
            if (barHeadMaxRelativePosition < 1)
            {
                barHeadMaxRelativePosition = 1;
            }

            float pixelsPerDivision = (barHeadMaxRelativePosition) / range;

            float offsetPosition = value - Minimum;
            int barStart = (int)(pixelsPerDivision * offsetPosition);

            int barThickness = this.ScrollFrameThickness - 4;
            int barOffset = 2;
            if (fullWidth || barThickness < 0)
            {
                barThickness = this.ScrollFrameThickness;
                barOffset = 0;
            }

            if(Orientation == ScrollBarDirection.Horizonal)
            {
                return new Rectangle(barStart + 2,
                    barOffset,
                    barLength,
                    barThickness);
            }
            else
            {
                return new Rectangle(barOffset,
                    barStart + 2,
                    barThickness,
                    barLength);
            }
        }
    }
}
