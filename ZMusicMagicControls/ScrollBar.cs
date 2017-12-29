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
    public class ScrollBar
    {
        public enum ScrollBarDirection { Horizonal, Vertical }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Minimum { get; set; } = 0;
        public int Maximum { get; set; } = 100;

        int value;
        int largeChange = 10;
        int smallChange = 1;

        public int Value
        {
            get { return value; }
            set
            {
                this.value = ZMusicMagicLibrary.Utilities.Clamp(value, this.Minimum, this.Maximum);

                PositionChanged?.Invoke(this, new EventArgs());
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

        Rectangle controlPosition;
        public Rectangle Position
        {
            get { return controlPosition; }
            set
            {
                controlPosition = value;

                //PositionChanged?.Invoke(this, new EventArgs());
            }
        }

        public Size AnchorEndRelativeOffset { get; set; } = new Size(10, 10);
        public int ScrollFrameThickness { get; set; } = 10;

        Rectangle parentRectange;
        public Rectangle ParentRectangle
        {
            get { return parentRectange; }
            set
            {
                parentRectange = value;

                Resize();
                //PositionChanged?.Invoke(this, new EventArgs());
            }
        }

        public AnchorStyles Anchor { get; set; } = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        public ScrollBarDirection Orientation { get; set; }

        public event EventHandler<EventArgs> ValueChanged;
        public event EventHandler<EventArgs> Scroll;
        public event EventHandler<EventArgs> PositionChanged;

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

        public ScrollBar(Rectangle position)
        {
            Position = position;
        }

        //public void OnMouseCaptureChanged(MouseEventArgs e)
        //{
        //}
        public void OnMouseClick(MouseEventArgs e)
        {
            // scroll to where they clicked
        }
        public void OnMouseDoubleClick(MouseEventArgs e)
        {
            OnMouseClick(e);
        }
        public void OnMouseDown(MouseEventArgs e)
        {
            // start dragging
            if (this.Position.Contains(e.Location))
            {
                previousLocation = e.Location;
                isDragging = true;
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
        Point previousLocation;
        public void OnMouseMove(MouseEventArgs e)
        {
            if(this.isDragging)
            {
                // move
                if(Orientation == ScrollBarDirection.Horizonal)
                {
                    Value = this.Position.Left + 2 - e.X;

                    Debug.WriteLine($"e.X {e.X}");
                    Value += e.X - previousLocation.X;
                }
                else
                {
                    Value += e.Y - previousLocation.Y;
                }

                previousLocation = e.Location;
            }
        }
        public void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
        }

        private void Resize()
        {
            int x = this.Position.X;
            int y = this.Position.Y;
            int width = this.Position.Width;
            int height = this.Position.Height;

            if (Orientation == ScrollBarDirection.Horizonal)
            {
                height = this.ScrollFrameThickness;
            }
            else
            {
                width = this.ScrollFrameThickness;
            }

            if ((Anchor & AnchorStyles.Left) == AnchorStyles.Left)
            {
                // don't change X
            }

            if((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                // update width
                if (Orientation == ScrollBarDirection.Horizonal)
                {
                    width = this.parentRectange.Right - x - this.AnchorEndRelativeOffset.Width;
                }
                else if ((Anchor & AnchorStyles.Left) != AnchorStyles.Left)
                {
                    // vertical scroll bar without anchor left
                    x = this.parentRectange.Right - this.AnchorEndRelativeOffset.Width;
                }
            }

            if((Anchor & AnchorStyles.Top) == AnchorStyles.Top)
            {
                // don't change Y
            }

            if((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
            {
                if(Orientation == ScrollBarDirection.Vertical)
                {
                    height = this.parentRectange.Bottom - y - this.AnchorEndRelativeOffset.Height;
                }
                else if((Anchor & AnchorStyles.Top) != AnchorStyles.Top)
                {
                    // horizontal scroll bar without anchor top
                    y = this.parentRectange.Bottom - this.AnchorEndRelativeOffset.Height;
                }
            }

            this.controlPosition = new Rectangle(x, y, width, height);
        }

        public void Draw(Graphics g)
        {
            // draw frame
            g.FillRectangle(scrollFrameBrush, Position.Left, Position.Top, Position.Width, Position.Height);

            // draw handle
            if (Orientation == ScrollBarDirection.Horizonal)
            {
                DrawHorizontalScrollBar(g);
                //g.FillRectangle(Brushes.Magenta, this.parentRectange.Right - 30 - ScrollFrameThickness - 2, this.parentRectange.Bottom - 20, 30, 10);
            }
            else
            {
                DrawVerticalScrollBar(g);
            }
        }

        private void DrawHorizontalScrollBar(Graphics g)
        {


            int maxHandleWidth = this.controlPosition.Width - 4; // 2 pixel padding
            if(maxHandleWidth < 1)
            {
                // nothing to draw
                return;
            }

            int range = Maximum - Minimum;
            if (range <= 0)
            {
                // wtf did you do!
                return;
            }

            int barWidth = maxHandleWidth < 30 ? maxHandleWidth : 30; // TODO: maybe do something different with this to scale it

            float barHeadMaxRelativePosition = maxHandleWidth - barWidth;
            if(barHeadMaxRelativePosition < 1)
            {
                barHeadMaxRelativePosition = 1;
            }

            float pixelsPerDivision = (barHeadMaxRelativePosition) / range;

            int offsetPosition = value - Minimum;
            int barStart = (int)(pixelsPerDivision * offsetPosition);

            int barThickness = this.ScrollFrameThickness - 4;
            int barOffset = 2;
            if(barThickness < 0)
            {
                barThickness = this.ScrollFrameThickness;
                barOffset = 0;
            }

            g.FillRectangle(this.scrollBarBrush, 
                this.controlPosition.Left + barStart + 2, 
                this.controlPosition.Top + barOffset, 
                barWidth, 
                barThickness);

            //// handle
            //int scrollStartY = 11; // TODO: calculate this
            //int scrollHandleLength = 20; // TODO: calculate this

            //g.FillRectangle(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + 2 + halfScrollWidth,
            //    scrollBarWidth,
            //    scrollHandleLength - halfScrollWidth - 2);
            //// top rounded cap
            //g.FillEllipse(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + 2,
            //    scrollBarWidth,
            //    scrollBarWidth);

            //// bottom rounded cap
            //g.FillEllipse(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + scrollHandleLength - halfScrollWidth,
            //    scrollBarWidth,
            //    scrollBarWidth);
        }

        private void DrawVerticalScrollBar(Graphics g)
        {
            //// handle
            //int scrollStartY = 11; // TODO: calculate this
            //int scrollHandleLength = 20; // TODO: calculate this

            //g.FillRectangle(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + 2 + halfScrollWidth,
            //    scrollBarWidth,
            //    scrollHandleLength - halfScrollWidth - 2);
            //// top rounded cap
            //g.FillEllipse(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + 2,
            //    scrollBarWidth,
            //    scrollBarWidth);

            //// bottom rounded cap
            //g.FillEllipse(scrollBarHandleBrush,
            //    x + 2,
            //    y + scrollStartY + scrollHandleLength - halfScrollWidth,
            //    scrollBarWidth,
            //    scrollBarWidth);
        }

    }
}
