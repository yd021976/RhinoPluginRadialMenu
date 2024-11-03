using Eto.Drawing;
using Rhino;
using Rhino.PlugIns;

namespace customControls
{
    public class SectorRadialMenuForm : TransparentForm
    {
        public SectorRadialMenuForm(PlugIn plugin) : base(plugin)
        {
            Size = new Size(500, 500);
            var layout = new Eto.Forms.PixelLayout();

            // Handle mouse over to get focus
            MouseMove += this.onMouseMove;
            
            // Get ressource icon for close button
            var img = Bitmap.FromResource("RadialMenu.Bitmaps.close-icon-13612.png");
            var icon = img.WithSize(16, 16);

            // Create close button
            var closeBtn = new RoundButton();
            closeBtn.Size = new Size(32, 32);
            closeBtn.setButtonIcon(icon);
            closeBtn.onclickEvent += onCloseClickEvent;
            layout.Add(closeBtn, (Size.Width / 2) - closeBtn.Size.Width / 2, (Size.Height / 2) - (closeBtn.Size.Height / 2));

            // Create and add RadialMenu Control
            var ctrl = new SectorArcRadialControl();
            ctrl.onclickEvent += (s) =>
            {
                this.Visible = false;
                RhinoApp.SetFocusToMainWindow();
            };
            layout.Add(ctrl, 0, 0);
            Content = layout;
        }
    }
}