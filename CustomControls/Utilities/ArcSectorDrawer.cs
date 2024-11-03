using System;
using Eto.Drawing;
using ObjCRuntime;

namespace customControls
{

    public class ArcSectorDrawer
    {
        /// <summary>
        /// Draw  an arc sector in graphic path object, generate images for button and return a SectorData object
        /// </summary>
        /// <param name="g"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="innerRadius"></param>
        /// <param name="thickness"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns>SectorData</returns>
        public SectorData drawSector(GraphicsPath g, int x, int y, int innerRadius, int thickness, int startAngle, int sweepAngle, RadialButtonStateColors theme)
        {
            var center = new Point(x, y);
            var outerR = innerRadius + thickness;
            var outerRect = new Rectangle
                            (center.X - outerR, center.Y - outerR, 2 * outerR, 2 * outerR);
            var innerRect = new Rectangle
                            (center.X - innerRadius, center.Y - innerRadius, 2 * innerRadius, 2 * innerRadius);

            g.AddArc(outerRect, startAngle, sweepAngle);
            g.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
            g.CloseFigure();
            return buildSectorImages(g, x, y, innerRadius, thickness, startAngle, sweepAngle, theme);
        }

        protected SectorData buildSectorImages(GraphicsPath gp, int x, int y, int innerRadius, int thickness, int startAngle, int sweepAngle, RadialButtonStateColors theme)
        {
            var pen = new Pen(theme.normal.pen, 1);
            var fillColor = theme.normal.fill;

            var pathSize = new Size((int)gp.Bounds.Size.Width + 3, (int)gp.Bounds.Size.Height + 3);
            // Create button image for normal state
            var normalStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            var _graphics = new Graphics(normalStateImage);
            _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
            _graphics.FillPath(fillColor, gp);
            _graphics.DrawPath(pen, gp);
            _graphics.Dispose();
            pen.Dispose();

            // Create button image for over state
            pen = new Pen(theme.hover.pen, 1);
            fillColor = theme.hover.fill;
            var overStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(overStateImage);
            _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
            _graphics.FillPath(fillColor, gp);
            _graphics.DrawPath(pen, gp);
            _graphics.Dispose();
            pen.Dispose();

            // Create mask image
            pen = new Pen(Colors.Blue, 1);
            var maskImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(maskImage);
            _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
            _graphics.FillPath(Colors.Blue, gp);
            _graphics.DrawPath(pen, gp);
            _graphics.Dispose();
            pen.Dispose();

            // REMARK: no specific "drag" image
            var sData = new SectorData()
            {
                buttonID = generateButtonID(),
                // Arc data
                arcCenter = new Point(x, y),
                bounds = gp.Bounds,
                size = pathSize,
                startAngle = startAngle,
                sweepAngle = sweepAngle,
                // Images
                normalStateImage = normalStateImage,
                overStateImage = overStateImage,
                dragStateImage = normalStateImage,
                sectorMask = maskImage,

            };
            return sData;
            
        }
        private string generateButtonID()
        {
            return Guid.NewGuid().ToString();
        }
    }
}