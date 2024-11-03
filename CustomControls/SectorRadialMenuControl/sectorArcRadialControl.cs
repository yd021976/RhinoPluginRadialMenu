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


        public SectorArcRadialControl() : base()
        {
            var buttonTheme = new RadialButtonStateColors();
            buttonTheme.normal = new ButtonColor(Colors.White, Colors.DarkKhaki);
            buttonTheme.hover = new ButtonColor(Colors.White, Colors.Khaki);
            buttonTheme.drag = new ButtonColor(Colors.White, Colors.DarkKhaki);

            initLevels();
            var level = menuLevels.Find(l => l.level == 1);
            level.sectorData = buildSectors(8, level, buttonTheme);

            Size = new Size(500, 500);
            layout = new PixelLayout();

            // Draw buttons
            foreach (var sd in level.sectorData)
            {
                var btn = new SectorArcButton(sd);
                btn.onclickEvent += (s) =>
                {
                    onclickEvent.Invoke(this);
                };
                layout.Add(btn, (int)sd.bounds.Left, (int)sd.bounds.Top);
            }

            this.Content = layout;
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
        private List<SectorData> buildSectors(int sectorsNumber, RadialMenuLevel level, RadialButtonStateColors theme)
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
                var sectorData = sectorDrawer.drawSector(_graphicsPath, 250, 250, level.innerRadius, level.thickness, angleStart, sweepAngle, theme);

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