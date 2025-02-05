using System;
using Eto.Drawing;

namespace RadialMenuPlugin.Utilities
{

    public class ArcSectorDrawer
    {
        Data.RadialButtonStateColors theme = new Data.RadialButtonStateColors();

        public ArcSectorDrawer(Data.RadialButtonStateColors theme = null)
        {
            if (theme != null) { this.theme = theme; }
        }
        /// <summary>
        /// Draw  an arc sector in graphic path object, generate images for button and return a SectorData object
        /// </summary>
        /// <param name="g">Graphics path object</param>
        /// <param name="arcID">ID, most of the time a Button ID</param>
        /// <param name="x">Arc center X</param>
        /// <param name="y">Arc center Y</param>
        /// <param name="level">Level data</param>
        /// <param name="startAngle">Start angle</param>
        /// <param name="sweepAngle">Sweep angle</param>
        /// <returns></returns>
        // public Data.SectorData DrawSector(GraphicsPath g, int x, int y, Data.RadialMenuLevel level, int startAngle, int sweepAngle)
        // {
        //     var center = new Point(x, y);
        //     var outerR = level.InnerRadius + level.Thickness;
        //     var outerRect = new Rectangle
        //                     (center.X - outerR, center.Y - outerR, 2 * outerR, 2 * outerR);
        //     var innerRect = new Rectangle
        //                     (center.X - level.InnerRadius, center.Y - level.InnerRadius, 2 * level.InnerRadius, 2 * level.InnerRadius);


        //     g.AddArc(outerRect, startAngle, sweepAngle);
        //     g.AddArc(innerRect, startAngle + sweepAngle, -sweepAngle);
        //     g.CloseFigure();
        //     return buildSectorImages(g, x, y, level, startAngle, sweepAngle);
        // }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="level"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        // protected Data.SectorData buildSectorImages(GraphicsPath gp, int x, int y, Data.RadialMenuLevel level, int startAngle, int sweepAngle)
        // {
        //     var pen = new Pen(theme.Normal.Pen, 1);
        //     var fillColor = theme.Normal.Fill;

        //     var pathSize = new Size((int)gp.Bounds.Size.Width + 3, (int)gp.Bounds.Size.Height + 3);

        //     // Create button image for normal state
        //     var normalStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
        //     var _graphics = new Graphics(normalStateImage);
        //     _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
        //     _graphics.FillPath(fillColor, gp);
        //     _graphics.DrawPath(pen, gp);
        //     _graphics.Dispose();
        //     pen.Dispose();

        //     // Create button image for over state
        //     pen = new Pen(theme.Hover.Pen, 1);
        //     fillColor = theme.Hover.Fill;
        //     var overStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
        //     _graphics = new Graphics(overStateImage);
        //     _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
        //     _graphics.FillPath(fillColor, gp);
        //     _graphics.DrawPath(pen, gp);
        //     _graphics.Dispose();
        //     pen.Dispose();


        //     // Create button image for disable state
        //     pen = new Pen(theme.Disabled.Pen, 1);
        //     fillColor = theme.Disabled.Fill;
        //     var disabledImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
        //     _graphics = new Graphics(disabledImage);
        //     _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
        //     _graphics.FillPath(fillColor, gp);
        //     _graphics.DrawPath(pen, gp);
        //     _graphics.Dispose();
        //     pen.Dispose();

        //     // Create button image for selected state
        //     pen = new Pen(theme.Selected.Pen, 1);
        //     fillColor = theme.Selected.Fill;
        //     var selectedImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
        //     _graphics = new Graphics(selectedImage);
        //     _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
        //     _graphics.FillPath(fillColor, gp);
        //     _graphics.DrawPath(pen, gp);
        //     _graphics.Dispose();
        //     pen.Dispose();

        //     // Create mask image
        //     pen = new Pen(Colors.Blue, 1);
        //     var maskImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
        //     _graphics = new Graphics(maskImage);
        //     _graphics.TranslateTransform(new PointF(-gp.Bounds.Left, -gp.Bounds.Top));
        //     _graphics.FillPath(Colors.Blue, gp);// no path drawn because of in UI 2 buttons can be "hovered", so only inner paint is part of mask image
        //     // _graphics.DrawPath(pen, gp);
        //     _graphics.Dispose();
        //     pen.Dispose();

        //     // REMARK: no specific "drag" image. It is same as "normal" state
        //     return new Data.SectorData(level)
        //     {
        //         // Arc data
        //         ArcCenter = new Point(x, y),
        //         Bounds = gp.Bounds,
        //         Size = pathSize,
        //         StartAngle = startAngle,
        //         SweepAngle = sweepAngle,
        //         // Images
        //         Images = new Data.ButtonStateImages()
        //         {
        //             NormalStateImage = normalStateImage,
        //             OverStateImage = overStateImage,
        //             DisabledStateImage = disabledImage,
        //             dragStateImage = normalStateImage,
        //             SelectedStateImage = selectedImage,
        //             SectorMask = maskImage,
        //         },

        //     };
        // }
        public Data.SectorData CreateSectorImages(int x, int y, Data.RadialMenuLevel level, int startAngle, int sweepAngle)
        {
            var gp1 = new GraphicsPath();
            var gp2 = new GraphicsPath();
            var center = new Point(x, y);

            // Data for sector path (the one that is displayed)
            var outerRadius = level.InnerRadius + level.Thickness;
            var innerRadius = level.InnerRadius;
            var outerRectangle = new Rectangle(center.X - outerRadius, center.Y - outerRadius, outerRadius * 2, outerRadius * 2);
            var innerRectangle = new Rectangle(center.X - innerRadius, center.Y - innerRadius, innerRadius * 2, innerRadius * 2);

            // Data for inset path
            var insetGap = 4; // in pixels
            var insetInnerRadius = innerRadius + insetGap;
            var insetOuterRadius = outerRadius - insetGap;
            var insetInnerRectangle = new Rectangle(center.X - insetInnerRadius, center.Y - insetInnerRadius, insetInnerRadius * 2, insetInnerRadius * 2);
            var insetOuterRectangle = new Rectangle(center.X - insetOuterRadius, center.Y - insetOuterRadius, insetOuterRadius * 2, insetOuterRadius * 2);

            float innerSin = (float)insetGap / insetInnerRadius;
            var innerAsin = Math.Asin(innerSin);
            var insetInnerAngle = innerAsin * (180 / Math.PI);
            var insetInnerStartAngle = startAngle + insetInnerAngle;
            var insetInnerSweepAngle = sweepAngle - 2 * insetInnerAngle;

            float outerSin = (float)insetGap / insetOuterRadius;
            var outerAsin = Math.Asin(outerSin);
            var insetOuterAngle = outerAsin * (180 / Math.PI);
            var insetOuterStartAngle = startAngle + insetOuterAngle;
            var insetOuterSweepAngle = sweepAngle - 2 * insetOuterAngle;

            gp1.AddArc(outerRectangle, startAngle, sweepAngle);
            gp1.AddArc(innerRectangle, startAngle + sweepAngle, -sweepAngle);
            gp1.CloseFigure();

            gp2.AddArc(insetOuterRectangle.Left, insetOuterRectangle.Top, insetOuterRectangle.Width, insetOuterRectangle.Height, (float)insetOuterStartAngle, (float)insetOuterSweepAngle);
            gp2.AddArc(insetInnerRectangle.Left, insetInnerRectangle.Top, insetInnerRectangle.Width, insetInnerRectangle.Height, (float)(insetInnerStartAngle + insetInnerSweepAngle), (float)-insetInnerSweepAngle);
            gp2.CloseFigure();

            var paths = new GraphicsPath[] { gp1, gp2 };
            return BuildImages(paths, x, y, level, startAngle, sweepAngle);
        }

        protected Data.SectorData BuildImages(GraphicsPath[] graphicPaths, int x, int y, Data.RadialMenuLevel level, int startAngle, int sweepAngle)
        {
            var sectorPath = graphicPaths[0];
            var maskPath = graphicPaths[1];

            var pen = new Pen(theme.Normal.Pen, 1);
            var fillColor = theme.Normal.Fill;

            var pathSize = new Size((int)sectorPath.Bounds.Size.Width + 3, (int)sectorPath.Bounds.Size.Height + 3);

            // Create button image for normal state
            var normalStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            var _graphics = new Graphics(normalStateImage);
            _graphics.TranslateTransform(new PointF(-sectorPath.Bounds.Left, -sectorPath.Bounds.Top));
            _graphics.FillPath(fillColor, sectorPath);
            _graphics.DrawPath(pen, sectorPath);
            _graphics.Dispose();
            pen.Dispose();

            // Create button image for over state
            pen = new Pen(theme.Hover.Pen, 1);
            fillColor = theme.Hover.Fill;
            var overStateImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(overStateImage);
            _graphics.TranslateTransform(new PointF(-sectorPath.Bounds.Left, -sectorPath.Bounds.Top));
            _graphics.FillPath(fillColor, sectorPath);
            _graphics.DrawPath(pen, sectorPath);
            _graphics.Dispose();
            pen.Dispose();


            // Create button image for disable state
            pen = new Pen(theme.Disabled.Pen, 1);
            fillColor = theme.Disabled.Fill;
            var disabledImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(disabledImage);
            _graphics.TranslateTransform(new PointF(-sectorPath.Bounds.Left, -sectorPath.Bounds.Top));
            _graphics.FillPath(fillColor, sectorPath);
            _graphics.DrawPath(pen, sectorPath);
            _graphics.Dispose();
            pen.Dispose();

            // Create button image for selected state
            pen = new Pen(theme.Selected.Pen, 1);
            fillColor = theme.Selected.Fill;
            var selectedImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(selectedImage);
            _graphics.TranslateTransform(new PointF(-sectorPath.Bounds.Left, -sectorPath.Bounds.Top));
            _graphics.FillPath(fillColor, sectorPath);
            _graphics.DrawPath(pen, sectorPath);
            _graphics.Dispose();
            pen.Dispose();

            // Create mask image
            pen = new Pen(Colors.Blue, 1);
            var maskImage = new Bitmap(pathSize, PixelFormat.Format32bppRgba);
            _graphics = new Graphics(maskImage);
            _graphics.TranslateTransform(new PointF(-sectorPath.Bounds.Left, -sectorPath.Bounds.Top));
            _graphics.FillPath(Colors.Blue, maskPath); // no path drawn because of in UI 2 buttons can be "hovered", so only inner paint is part of mask image
            _graphics.Dispose();
            pen.Dispose();

            // REMARK: no specific "drag" image. It is same as "normal" state
            return new Data.SectorData(level)
            {
                // Arc data
                ArcCenter = new Point(x, y),
                Bounds = sectorPath.Bounds,
                Size = pathSize,
                StartAngle = startAngle,
                SweepAngle = sweepAngle,
                // Images
                Images = new Data.ButtonStateImages()
                {
                    NormalStateImage = normalStateImage,
                    OverStateImage = overStateImage,
                    DisabledStateImage = disabledImage,
                    dragStateImage = normalStateImage,
                    SelectedStateImage = selectedImage,
                    SectorMask = maskImage,
                },
            };
        }
    }
}