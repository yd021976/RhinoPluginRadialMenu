using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace RadialMenuPlugin.Data
{
    /// <summary>
    /// Class to convert Rhino keyboard "key" (int) value to ETO Keys value
    /// </summary>
    public static class RhinoKeyToEto
    {
        private static readonly Dictionary<int, Keys> match = new Dictionary<int, Keys>
        {
            {55,Keys.Application},
            {58,Keys.Alt},
            {59,Keys.Control},
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rhinoKey"></param>
        /// <returns></returns>
        public static Keys toEtoKey(int rhinoKey)
        {
            if (match.ContainsKey(rhinoKey))
            {
                return match[rhinoKey];
            }
            return Keys.None;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class SettingsDomain
    {
        public string Name { get; }
        private SettingsDomain(string key) { Name = key; }
        public static SettingsDomain RadialButtonsConfig => new SettingsDomain("Buttons");
        public static SettingsDomain Theme => new SettingsDomain("Theme");
        public static SettingsDomain GeneralSettings => new SettingsDomain("General");

        public override string ToString()
        {
            return Name;
        }
    }
    public enum DragSourceType
    {
        radialMenuItem = 0,
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
        public int Level;

        /// <summary>
        /// Inner radius of level
        /// </summary>
        public int InnerRadius;

        /// <summary>
        /// Sector thickness fro drawing this level
        /// </summary>
        public int Thickness;

        /// <summary>
        /// Start angle for this level
        /// </summary>
        public int StartAngle;

        /// <summary>
        /// # of sectors for this level
        /// </summary>
        public int SectorsNumber;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="level"></param>
        /// <param name="innerRadius"></param>
        /// <param name="thickness"></param>
        /// <param name="startAngle"></param>
        /// <param name="sectorsNumber"></param>
        public RadialMenuLevel(int level, int innerRadius, int thickness = 30, int startAngle = 0, int sectorsNumber = 8)
        {
            Level = level; InnerRadius = innerRadius;
            Thickness = thickness; StartAngle = startAngle;
            SectorsNumber = sectorsNumber;
        }
    }


    public class ButtonColor
    {
        public Color Pen;
        public Color Fill;

        public ButtonColor(Color pen, Color fill) { Pen = pen; Fill = fill; }
    }
    public class RadialButtonStateColors
    {
        public ButtonColor Normal = new ButtonColor(Colors.Beige, Colors.DarkGray);
        public ButtonColor Hover = new ButtonColor(Colors.Beige, Colors.LightGrey);
        public ButtonColor Selected = new ButtonColor(Colors.Beige, Colors.WhiteSmoke);
        public ButtonColor Disabled = new ButtonColor(Colors.LightGrey, Colors.SlateGray);
        public ButtonColor Drag = new ButtonColor(Colors.Beige, Colors.DarkKhaki);
    }
    public struct ButtonStateImages
    {
        public Image NormalStateImage;
        public Image OverStateImage;
        public Image DisabledStateImage;
        public Image SelectedStateImage;
        public Image dragStateImage;
        // Mask image to detect if a point is hover a sector image
        public Bitmap SectorMask;
    }
    public class SectorData
    {
        /// <summary>
        /// Size of the sector image
        /// </summary>
        public Size Size;

        /// <summary>
        /// Sector arc bounds in real coordinates, i.e Parent coordinates and/or in real enclosing circle (based on inner Radius)
        /// Usefull to set icon location in enclosing control (form for example)
        /// </summary>
        public RectangleF Bounds;

        // Center of the arc in "real" coordinates
        public Point ArcCenter;
        public int StartAngle;
        public int SweepAngle;
        public int InnerRadius = 50;
        public int Thickness = 30;

        /// <summary>
        /// State images
        /// </summary>
        public ButtonStateImages Images;

        #region Arc radius and thickness
        #endregion

        /// <summary>
        /// Center of the sector in world coordinates (usefull to place icon in sector button) 
        /// </summary>
        public PointF SectorCenter()
        {
            var centerRadius = InnerRadius + (Thickness / 2);
            var bisectorAngle = StartAngle + (SweepAngle / 2);
            float X = (float)(ArcCenter.X + centerRadius * Math.Cos(bisectorAngle * (Math.PI / 180)));
            float Y = (float)(ArcCenter.Y + centerRadius * Math.Sin(bisectorAngle * (Math.PI / 180)));
            return new PointF(X, Y);
        }
        /// <summary>
        /// Get a point in the sector
        /// </summary>
        /// <param name="angleFromStart">Angle from start angle of sector</param>
        /// <param name="radiusFromInner">Radius from inner radius</param>
        /// <returns></returns>
        public PointF GetPoint(int angleFromStart, int radiusFromInner)
        {
            var atAngle = StartAngle + angleFromStart;
            var atRadius = InnerRadius + radiusFromInner;
            float X = (float)(ArcCenter.X + atRadius * Math.Cos(atAngle * (Math.PI / 180)));
            float Y = (float)(ArcCenter.Y + atRadius * Math.Sin(atAngle * (Math.PI / 180)));
            return new PointF(X, Y);
        }
        public int EndAngle { get { return StartAngle + SweepAngle; } }

        public SectorData() { }
        public SectorData(RadialMenuLevel level)
        {
            InnerRadius = level.InnerRadius;
            Thickness = level.Thickness;
        }

        /// <summary>
        /// Convert a local location (i.e. control coordinates) to world arc coordinates (because a control only displays the arc and not the whole circle)
        /// </summary>
        /// <param name="localLocation"></param>
        /// <returns></returns>
        public PointF ConvertLocalToWorld(PointF localLocation)
        {
            // convert mouse location to real circle size (i.e. Parent form size)
            var localX = Bounds.TopLeft.X + localLocation.X;
            var localY = Bounds.TopLeft.Y + localLocation.Y;
            return new PointF(localX, localY);
        }
        /// <summary>
        /// convert real world coordinates to local (i.e. control local coordinates) coordinates
        /// </summary>
        /// <param name="worldLocaltion"></param>
        /// <returns></returns>
        public PointF ConvertWorldToLocal(PointF worldLocaltion)
        {
            var worldX = worldLocaltion.X - Bounds.TopLeft.X;
            var worldY = worldLocaltion.Y - Bounds.TopLeft.Y;
            return new PointF(worldX, worldY);
        }
        /// <summary>
        /// <para>Check if a Point is in the arc shape</para>
        /// <para>IMPORTANT: Point should be in control local coordinates.</para>
        /// </summary>
        /// <param name="location">Local control coordinates</param>
        /// <returns></returns>
        public bool IsPointInShape(PointF location)
        {
            var bmData = Images.SectorMask.Lock();
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