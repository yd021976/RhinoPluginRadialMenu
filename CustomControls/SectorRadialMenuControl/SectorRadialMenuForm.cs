using Eto.Drawing;
using Rhino.PlugIns;

namespace customControls
{
    public class SectorRadialMenuForm : TransparentForm
    {
        public SectorRadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Eto.Drawing.Size(500, 500);
            var layout = new Eto.Forms.PixelLayout();

            var sectorNumber = 10;
            var sweepAngle = 360 / sectorNumber;

            for (var i = 0; i < sectorNumber; i++)
            {
                var ctrl = new SectorsMenuControl(Size, 70, 40, i * sweepAngle, sweepAngle);
                layout.Add(ctrl, (int)ctrl.arcBound.X, (int)ctrl.arcBound.Y);

            }

            // Get ressource icon for close button
            var img = Bitmap.FromResource("RadialMenu.Bitmaps.close-icon-13612.png");
            var icon = img.WithSize(32, 32);
            // layout.Add(icon, 32, 32);

            // Create close button
            var closeBtn = new RoundedButton("COLSE_BTN");
            closeBtn.setButtonIcon(icon);
            closeBtn.activateButtonInfo(true);
            closeBtn.onclickEvent += onCloseClickEvent;
            layout.Add(closeBtn, (Size.Width / 2) - closeBtn.Size.Width / 2, (Size.Height / 2) - (closeBtn.Size.Height / 2));

            this.Content = layout;
        }
    }
}