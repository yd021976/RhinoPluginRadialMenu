using System;
using Eto.Drawing;

namespace customControls
{
    public enum DragSourceTypes
    {
        self = 0,
        rhinoItem = 1,
        unknown = 3
    }

    /// <summary>
    /// Menu level class
    /// </summary>
    public class RadialMenuLevel
    {
        /// <summary>
        /// Level number
        /// </summary>
        public int level;

        /// <summary>
        /// Inner radius of level
        /// </summary>
        public int innerRadius;

        /// <summary>
        /// Sector thickness fro drawing this level
        /// </summary>
        public int thickness;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level"></param>
        /// <param name="innerRadius"></param>
        /// <param name="thickness"></param>
        public RadialMenuLevel(int level, int innerRadius, int thickness) { this.level = level; this.innerRadius = innerRadius; this.thickness = thickness; }
    }


    public class ButtonColor
    {
        public Color pen;
        public Color fill;

        public ButtonColor(Color pen, Color fill) { this.pen = pen; this.fill = fill; }
    }
    public class RadialButtonStateColors
    {
        public ButtonColor normal = new ButtonColor(Colors.Beige, Colors.DarkGray);
        public ButtonColor hover = new ButtonColor(Colors.Beige, Colors.LightGrey);
        public ButtonColor selected = new ButtonColor(Colors.Beige, Colors.WhiteSmoke);
        public ButtonColor disabled = new ButtonColor(Colors.LightGrey, Colors.SlateGray);
        public ButtonColor drag = new ButtonColor(Colors.Beige, Colors.DarkKhaki);
    }
    public struct ButtonStateImages
    {
        public Image normalStateImage;
        public Image overStateImage;
        public Image disabledStateImage;
        public Image selectedStateImage;
        public Image dragStateImage;
        // Mask image to detect if a point is hover a sector image
        public Bitmap sectorMask;
    }
    public class SectorData
    {
        /// <summary>
        /// Size of the sector image
        /// </summary>
        public Size size;

        /// <summary>
        /// Sector arc bounds in real coordinates, i.e Parent coordinates and/or in real enclosing circle (based on inner Radius)
        /// Usefull to set icon location in enclosing control (form for example)
        /// </summary>
        public RectangleF bounds;

        // Center of the arc in "real" coordinates
        public Point arcCenter;
        public int startAngle;
        public int sweepAngle;

        /// <summary>
        /// State images
        /// </summary>
        public ButtonStateImages images;

        #region Arc radius and thickness
        protected int innerRadius = 50;
        protected int thickness = 30;
        #endregion

        /// <summary>
        /// Center of the sector in world coordinates (usefull to place icon in sector button) 
        /// </summary>
        public PointF sectorCenter()
        {
            var centerRadius = innerRadius + (thickness / 2);
            var bisectorAngle = startAngle + (sweepAngle / 2);
            float X = (float)(arcCenter.X + centerRadius * Math.Cos(bisectorAngle * (Math.PI / 180)));
            float Y = (float)(arcCenter.Y + centerRadius * Math.Sin(bisectorAngle * (Math.PI / 180)));
            return new PointF(X, Y);
        }

        public int endAngle { get { return startAngle + sweepAngle; } }

        public SectorData() { }
        public SectorData(RadialMenuLevel level)
        {
            innerRadius = level.innerRadius;
            thickness = level.thickness;
        }

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
            var bmData = images.sectorMask.Lock();
            var p = Point.Round(location);
            try
            {
                // var color = images.sectorMask.GetPixel(p);
                var color = bmData.GetPixel(p);
                return color.B == 1 ? true : false;
            }
            catch
            {
                return false;
            }
            finally
            {
                bmData.Dispose();
            }
        }
    }
}