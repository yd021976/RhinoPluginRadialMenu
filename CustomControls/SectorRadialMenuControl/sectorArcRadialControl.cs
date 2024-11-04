using System;
using System.Collections.Generic;
using Eto.Drawing;
using Eto.Forms;

namespace customControls
{
    public class SectorArcRadialControl : Drawable
    {
        protected static int defaultInnerRadius = 70;
        protected static int defaultThickness = 40;
        private PixelLayout layout;
        private List<RadialMenuLevel> menuLevels = new List<RadialMenuLevel>();

        /// <summary>
        /// Button click event
        /// </summary>
        public event buttonClickEvent onclickEvent;
        /// <summary>
        /// Event for button click
        /// </summary>
        /// <param name="sender"></param>
        public delegate void buttonClickEvent(SectorArcRadialControl sender);
        
        /// <summary>
        /// event to ensure main window has focus
        /// </summary>
        public event requestedFocusEvent onRequestedFocus;
        /// <summary>
        /// delegate for event to ensure main window has focus
        /// </summary>
        /// <param name="sender"></param>
        public delegate void requestedFocusEvent(SectorArcRadialControl sender);


        public SectorArcRadialControl() : base()
        {
            initLevels();
            Size = new Size((defaultInnerRadius + defaultThickness) * 2, (defaultInnerRadius + defaultThickness) * 2);
            var level = menuLevels.Find(l => l.level == 1);
            level.sectorData = buildSectors(8, level);

            layout = new PixelLayout();

            // Draw buttons
            foreach (var sd in level.sectorData)
            {
                var btn = new SectorArcButton(sd);

                // Get focus requested event
                btn.onRequestFocusEvent += (s) =>
                {
                    onRequestedFocus.Invoke(this);
                };
                // handle click event and raise click event
                // -> Should be handled by main form window to hide when a Rhino command is executed
                btn.onclickEvent += (s) =>
                {
                    onclickEvent.Invoke(this);
                };
                layout.Add(btn, (int)sd.bounds.Left, (int)sd.bounds.Top);
            }

            Content = layout;
        }
       
        private void initLevels()
        {
            for (int i = 0; i < 3; i++)
            {
                var prevLevel = i > 0 ? menuLevels[i - 1] : null;
                int? innerRadius = prevLevel != null ? prevLevel.innerRadius + prevLevel.thickness + 20 : defaultInnerRadius;

                menuLevels.Add(new RadialMenuLevel(i + 1, (int)innerRadius, defaultThickness));
            }
        }
        private List<SectorData> buildSectors(int sectorsNumber, RadialMenuLevel level)
        {
            int angleStart;
            List<SectorData> sectors = new List<SectorData>();

            var sweepAngle = 360 / sectorsNumber;

            for (int i = 0; i < sectorsNumber; i++)
            {
                // Draw one sector
                angleStart = i * sweepAngle;
                var _graphicsPath = new GraphicsPath();
                var sectorDrawer = new ArcSectorDrawer();
                var sectorData = sectorDrawer.drawSector(_graphicsPath, Size.Width / 2, Size.Height / 2, level.innerRadius, level.thickness, angleStart, sweepAngle);

                // add sector infos to list
                sectorData.levelRef = level;
                sectors.Add(sectorData);

                // Release resources
                _graphicsPath.Dispose();
            }
            return sectors;
        }
        ~SectorArcRadialControl()
        {
            //TODO: Dispose all graphics and images
        }


    }
}