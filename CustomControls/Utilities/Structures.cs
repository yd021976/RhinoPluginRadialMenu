using System;
using System.Collections.Generic;
using Eto.Drawing;
using Rhino;

namespace customControls
{
    enum sourceTypes
    {
        self = 0,
        rhinoItem = 1,
        unknown = 3
    }
    public class RadialMenuLevel
    {
        public int level;
        public int innerRadius;
        public int thickness;

        public List<SectorData> sectorData;

        public RadialMenuLevel(int level, int innerRadius, int thickness) { this.level = level; this.innerRadius = innerRadius; this.thickness = thickness; }
    }

    public class ButtonColor
    {
        public Color pen;
        public Color fill;

        public ButtonColor(Color pen, Color fill) { this.pen = pen; this.fill = fill; }
    }
    public struct RadialButtonStateColors
    {
        public ButtonColor normal;
        public ButtonColor hover;
        public ButtonColor drag;
    }
    public class SectorData
    {
        public string buttonID;
        // Size of the sector image
        public Size size;

        // Sector arc bounds in real coordinates, i.e Parent coordinates and/or in real enclosing circle (based on inner Radius)
        // Usefull to set icon location in enclosing control (form for example)
        public RectangleF bounds;

        // Center of the arc in "real" coordinates
        public Point arcCenter;
        public int startAngle;
        public int sweepAngle;
        public RadialMenuLevel levelRef;

        /// <summary>
        /// Center of the sector in world coordinates (usefull to place icon in sector button) 
        /// </summary>
        public PointF sectorCenter()
        {
            var centerRadius = levelRef.innerRadius + (levelRef.thickness / 2);
            var bisectorAngle = startAngle + (sweepAngle / 2);
            float X = (float)(arcCenter.X + centerRadius * Math.Cos(bisectorAngle * (Math.PI / 180)));
            float Y = (float)(arcCenter.Y + centerRadius * Math.Sin(bisectorAngle * (Math.PI / 180)));
            return new PointF(X, Y);
        }
        public Image normalStateImage;
        public Image overStateImage;
        public Image dragStateImage;
        // Mask image to detect if a point is hover a sector image
        public Bitmap sectorMask;

        public int endAngle { get { return startAngle + sweepAngle; } }
        public SectorData() { }

        /// <summary>
        /// Convert a local location (i.e. control coordinates) to world arc coordinates (because a control only displays the arc and not the whole circle)
        /// </summary>
        /// <param name="localLocation"></param>
        /// <returns></returns>
        public PointF convertLocalToWorld(PointF localLocation)
        {
            // convert mouse location to real circle size (i.e. Parent form size)
            var localX = bounds.TopLeft.X + localLocation.X;
            var localY = bounds.TopLeft.Y + localLocation.Y;
            return new PointF(localX, localY);
        }
        /// <summary>
        /// convert real world coordinates to local (i.e. control local coordinates) coordinates
        /// </summary>
        /// <param name="worldLocaltion"></param>
        /// <returns></returns>
        public PointF convertWorldToLocal(PointF worldLocaltion)
        {
            var worldX = worldLocaltion.X - bounds.TopLeft.X;
            var worldY = worldLocaltion.Y - bounds.TopLeft.Y;
            return new PointF(worldX, worldY);
        }
        /// <summary>
        /// <para>Check if a Point is in the arc shape</para>
        /// <para>IMPORTANT: Point should be in control local coordinates.</para>
        /// </summary>
        /// <param name="location">Local control coordinates</param>
        /// <returns></returns>
        public bool isPointInShape(PointF location)
        {
            var bmData = sectorMask.Lock();
            var p = Point.Round(location);
            var color = bmData.GetPixel(p);
            bmData.Dispose();
            return color.B == 1 ? true : false;
        }
    }
}