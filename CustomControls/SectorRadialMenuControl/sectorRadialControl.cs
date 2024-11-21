using System;
using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public class SectorButtonControl : Drawable
    {
        public int innerRadius;
        public int thickness;

        /// <summary>
        /// Rectangle containing the thick arc path
        /// </summary>
        public RectangleF arcBound;
        protected ButtonProperties properties = new ButtonProperties();
        protected int startAngle;
        protected int sweepAngle;
        protected int endAngle { get { return startAngle + sweepAngle; } }

        protected bool isHovering;
        protected bool isDraggingIcon;

        private Bitmap _bitmap;
        private Graphics _graphics;
        private GraphicsPath _graphicsPath;



        public SectorButtonControl(Size parentSize, int innerRadius, int thickness, int startAngle = 0, int sweepAngle = 40) : base()
        {
            this.startAngle = startAngle;
            this.sweepAngle = sweepAngle;

            var s = (innerRadius * 2) + thickness;
            this.innerRadius = innerRadius;
            this.thickness = thickness;
            // Size = new Eto.Drawing.Size(2 * (innerRadius + thickness), 2 * (innerRadius + thickness));

            MouseMove += mouseMoveHandler;
            MouseLeave += mouseLeaveHandler;

            _bitmap = new Bitmap(parentSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(_bitmap);
            _graphicsPath = new GraphicsPath();
            arcBound = drawSector(_graphicsPath, parentSize.Width / 2, parentSize.Height / 2);
            Size = new Size((int)arcBound.Width, (int)arcBound.Height);

            // DragDrop
            AllowDrop = true;
            DragEnter += dragEnterHandler;
            DragOver += dragOverHandler;
            DragLeave += dragLeaveHandler;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var normalStatePen = new Pen(Colors.Blue, 2);
            var normalStateFill = Colors.Black;
            var hoverStateFill = Colors.Yellow;
            var fillcolor = isHovering ? hoverStateFill : normalStateFill;

            e.Graphics.TranslateTransform(new PointF(-arcBound.Left, -arcBound.Top));
            e.Graphics.FillPath(fillcolor, _graphicsPath);
            e.Graphics.DrawPath(normalStatePen, _graphicsPath);
            if (properties.isActive)
            {
                if (properties.icon != null)
                {
                    var location = getIconCenter(properties.icon);
                    e.Graphics.DrawImage(properties.icon, location);
                }
            }
        }
        /// <summary>
        /// Compute location of icon center in the arc sector
        /// </summary>
        /// <returns></returns>
        private PointF getIconCenter(Icon icon)
        {
            var angle = startAngle + (sweepAngle / 2); // bisector angle
            var radius = innerRadius + (thickness / 2); // middle radius between inner radius and thickness
            var innerX = _bitmap.Width / 2 + radius * Math.Cos(angle * (Math.PI / 180));
            var innerY = _bitmap.Height / 2 + radius * Math.Sin(angle * (Math.PI / 180));
            return new PointF((float)innerX - (icon.Width / 2), (float)innerY - (icon.Height / 2));
        }

        /// <summary>
        /// Draw a arc sector
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private RectangleF drawSector(GraphicsPath g, int x, int y)
        {
            var center = new Point(x, y);
            var outerR = innerRadius + thickness;
            var outerRect = new Rectangle
                            (center.X - outerR, center.Y - outerR, 2 * outerR, 2 * outerR);
            var innerRect = new Rectangle
                            (center.X - innerRadius, center.Y - innerRadius, 2 * innerRadius, 2 * innerRadius);

            g.AddArc(outerRect, startAngle, sweepAngle);
            g.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);

            var bounds = g.Bounds;
            g.CloseFigure();
            return bounds;
        }


        private bool isHoveringArc(PointF controlMouseLocation)
        {
            var isHoveringCheck = false;
            var realMouseLocation = convertMouseLocation(controlMouseLocation);

            // Check mouse is in arc enclosing bound rectangle
            if (realMouseLocation.X >= arcBound.Left && realMouseLocation.X <= arcBound.Right && realMouseLocation.Y <= arcBound.Bottom && realMouseLocation.Y >= arcBound.Top)
            {
                // check the location angle is in arc start/end angle
                var mousePosAngle = realMouseLocation.AngleTo(new PointF(_bitmap.Width / 2, _bitmap.Height / 2)) + 180;
                if (mousePosAngle >= startAngle && mousePosAngle <= endAngle)
                {
                    // check location is inside the arc thickness
                    var r = computeInnerSegmentAtAngle(mousePosAngle);
                    if (realMouseLocation.X >= r.Left && realMouseLocation.X <= r.Right && realMouseLocation.Y >= r.Top && realMouseLocation.Y <= r.Bottom)
                    {
                        isHoveringCheck = true;
                    }
                }
            }
            return isHoveringCheck;
        }
        private PointF convertMouseLocation(PointF controlMouseLocation)
        {
            // convert mouse location to real circle size (i.e. Parent form size)
            var realMouseLocation_X = arcBound.TopLeft.X + controlMouseLocation.X;
            var realMouseLocation_Y = arcBound.TopLeft.Y + controlMouseLocation.Y;
            return new PointF(realMouseLocation_X, realMouseLocation_Y);
        }

        private void mouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (Parent == null) return;
            // convert mouse location to real circle size (i.e. Parent form size)
            var isHoveringCheck = isHoveringArc(e.Location);

            // If hovering state changed, redraw control
            if (isHoveringCheck != isHovering)
            {
                isHovering = isHoveringCheck;
                Invalidate();
            }
        }
        private void mouseLeaveHandler(object sender, EventArgs e)
        {
            isHovering = false;
            Invalidate();
        }

        private RectangleF computeInnerSegmentAtAngle(float angle)
        {
            var innerX = _bitmap.Width / 2 + innerRadius * Math.Cos(angle * (Math.PI / 180));
            var innerY = _bitmap.Height / 2 + innerRadius * Math.Sin(angle * (Math.PI / 180));

            var outerX = _bitmap.Width / 2 + (innerRadius + thickness) * Math.Cos(angle * (Math.PI / 180));
            var outerY = _bitmap.Height / 2 + (innerRadius + thickness) * Math.Sin(angle * (Math.PI / 180));

            return new RectangleF(new PointF((float)innerX, (float)innerY), new PointF((float)outerX, (float)outerY));

        }

        private void dragEnterHandler(object sender, DragEventArgs e)
        {
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (isHovering)
            {
                isDraggingIcon = true;
            }
        }

        private void dragLeaveHandler(object sender, DragEventArgs e)
        {
            OnMouseLeave(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (!isHovering)
            {
                isDraggingIcon = false;
                e.Effects = DragEffects.None;
            }

            var dSource = dragSourceType(e.Source);
            if ((dSource == DragSourceTypes.self && e.Source.ID == ID) || dSource == DragSourceTypes.rhinoItem)
            {
                properties.isActive = false;
                Invalidate();
            }
        }
        private void dragOverHandler(object sender, DragEventArgs e)
        {
            OnMouseMove(new MouseEventArgs(e.Buttons, e.Modifiers, e.Location));
            if (!isHovering)
            {
                isDraggingIcon = false;
            }
            else
            {
                e.Effects = DragEffects.Copy;
            }
        }
        private void dragDropHandler(object sender, DragEventArgs e)
        {
            switch (dragSourceType(e.Source))
            {
                case DragSourceTypes.self:
                    if (e.Source.ID == ID)
                    {
                        // properties.icon != null ? properties.isActive = true : properties.isActive = false;
                    }
                    break;
                case DragSourceTypes.rhinoItem:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Get type of object dragged into this control
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private DragSourceTypes dragSourceType(Control source)
        {
            if (source.GetType() == typeof(SectorButtonControl))
            {
                return DragSourceTypes.self;
            }
            else if (source.GetType().ToString() == "Rhino.UI.Internal.TabPanels.Controls.ToolBarControlItem")
            {
                return DragSourceTypes.rhinoItem;
            }
            else
            {
                return DragSourceTypes.unknown;
            }
        }
    }
}